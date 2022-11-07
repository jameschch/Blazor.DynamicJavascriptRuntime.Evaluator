﻿using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator
{

    public class EvalContext : DynamicObject, IDisposable, IEvalContext
    {

        private readonly StringBuilder _script;
        private bool _hasInvoked;
        private readonly IJSRuntime _runtime;
        private readonly EvalContextSettings _settings;
        private string _escaped;

        public Func<dynamic> Expression { get; set; }

        public EvalContext(IJSRuntime runtime)
        {
            _runtime = runtime;
            _script = new StringBuilder();
            _settings = new EvalContextSettings();
        }

        public EvalContext(IJSRuntime runtime, EvalContextSettings settings)
        {
            _runtime = runtime;
            _settings = settings;
            _script = new StringBuilder();
        }

        private void Append(string value)
        {
            if (_script.Length > 0)
            {
                _script.Append('.');
            }

            _script.Append(value);
        }

        public string this[string i]
        {
            set
            {
                SetIndexer(i, value);
            }
        }

        private void SetIndexer(string index, string value)
        {
            if (!_hasInvoked)
            {
                _script.Append('[');
                _script.Append(Massage(index));
                _script.Append(']');
                _script.Append(" = ");
                _script.Append(Massage(value));
            }
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (!_hasInvoked)
            {
                Append(binder.CallInfo.ArgumentNames.Single());
            }
            result = this;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_hasInvoked)
            {
                Append(binder.Name);
            }
            result = this;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (!_hasInvoked)
            {
                var isString = indexes[0] is string;
                if (indexes[0] is EvalContext || indexes[0] is int || isString)
                {
                    _script.Append('[');
                    var value = isString ? Massage(indexes[0]) : indexes[0].ToString();
                    _script.Append(value);
                    _script.Append(']');
                }
                else
                {
                    throw new Exception("Unexpected indexer type");
                }
            }

            result = this;
            return true;
        }

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            if (!_hasInvoked)
            {
                Append(binder.CallInfo.ArgumentNames.Single());
            }
            result = this;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (!_hasInvoked)
            {
                if (_script.Length > 0)
                {
                    _script.Append('.');
                }

                var attempts = args.Select(a => Massage(a));

                _script.Append($"{binder.Name}({string.Join(", ", attempts)})");
            }

            result = this;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!_hasInvoked)
            {
                Append(binder.Name);
                _script.Append(" = ");
                _script.Append(Massage(value));
            }
            return true;
        }

        private object Massage(object value)
        {
            if (value == null)
            {
                return "null";
            }
            if (value is string)
            {
                var delimiter = Regex.IsMatch(value.ToString(), "[\r\n]") ? '`' : '\"';
                return $"{delimiter}{value.ToString().Replace("'", "\u0027").Replace("\"", "\\u0022")}{delimiter}";
            }
            else if (value is DateTime)
            {
                return "new Date(\u0022" + ((DateTime)value).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz") + "\u0022)";
            }
            else if (IsAnonymousType(value.GetType()) || _settings.SerializableTypes.Contains(value.GetType()))
            {
                return JsonSerializer.Serialize(value, _settings.JsonSerializerOptions);
            }
            //todo: support enums
            return value;
        }


        /// <summary>
        /// Returns the Javascript expression as a string
        /// </summary>
        /// <returns>the Javascript expression</returns>
        public override string ToString()
        {
            if (!_hasInvoked)
            {
                Build();
            }
            return _escaped;
        }

        /// <summary>
        /// Invokes the Javascript expression and returns a value
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The value returned from Javascript</returns>
        public async virtual ValueTask<T> InvokeAsync<T>()
        {
            if (!_hasInvoked)
            {
                Build();
            }
            return await InvokeAsync<T>(_escaped);
        }

        /// <summary>
        /// Invokes the Javascript expression
        /// </summary>
        public async virtual ValueTask InvokeVoidAsync()
        {
            await InvokeAsync<object>();
        }

        /// <summary>
        /// Invokes the Javascript expression and returns a value
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The value returned from Javascript</returns>
        public virtual T Invoke<T>()
        {
            if (!_hasInvoked)
            {
                Build();
            }
            return Invoke<T>(_escaped);
        }

        /// <summary>
        /// Invokes the Javascript string and returns a value
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The value returned from Javascript</returns>
        public virtual T Invoke<T>(string script)
        {
            _hasInvoked = true;
#if DEBUG
            Debug.WriteLine("BDJR: " + script);
#else
            if (_settings.EnableDebugLogging)
            {
                Debug.WriteLine("BDJR: " + script);
            }
#endif

            return ((IJSInProcessRuntime)_runtime).Invoke<T>("BlazorDynamicJavascriptRuntime.evaluate", script);
        }

        /// <summary>
        /// Executes the provided string of Javascript and returns a value
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="script">A string of Javascript</param>
        /// <returns></returns>
        public async virtual ValueTask<T> InvokeAsync<T>(string script)
        {
            _hasInvoked = true;
#if DEBUG
            Debug.WriteLine("BDJR: " + script);
#else
            if (_settings.EnableDebugLogging)
            {
                Debug.WriteLine("BDJR: " + script);
            }
#endif

            return await _runtime.InvokeAsync<T>("BlazorDynamicJavascriptRuntime.evaluate", script);
        }

        /// <summary>
        /// Executes the provided string of Javascript
        /// </summary>
        /// <param name="script">A string of Javascript</param>
        public async virtual ValueTask InvokeVoidAsync(string script)
        {
            await InvokeAsync<object>(script);
        }

        public void Dispose()
        {
            if (!_hasInvoked)
            {
                Build();
                Task.Run(async () => await InvokeAsync<dynamic>(_escaped));
            }
        }

        /// <summary>
        /// Resets the context allowing for multiple invocations on a single instance
        /// </summary>
        public void Reset()
        {
            Expression = null;
            _script.Clear();
            _hasInvoked = false;
            _escaped = null;
        }

        private void Build()
        {
            Expression?.Invoke();
            _hasInvoked = true;
            _escaped = _settings.EnableSpaceCharacterPlaceholderReplacement ? _script.ToString().Replace(_settings.SpaceCharacterPlaceholder, " ")
                : _script.ToString();

        }

        private static bool IsAnonymousType(Type type)
        {
            return type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0 && type.FullName.Contains("AnonymousType");
        }

    }

}
