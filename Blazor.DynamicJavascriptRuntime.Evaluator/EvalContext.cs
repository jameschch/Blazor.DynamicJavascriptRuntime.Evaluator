using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator
{

    public class EvalContext : DynamicObject, IDisposable, IEvalContext
    {

        StringBuilder _script;
        bool _hasInvoked;
        IJSRuntime _runtime;
        private string _escaped;
        private int _functionInvocationCount;

        public Func<dynamic> Expression { get; set; }

        public EvalContext(IJSRuntime runtime)
        {
            _runtime = runtime;
            _script = new StringBuilder();
        }

        private void Append(string value)
        {
            if (_script.Length > 0)
            {
                _script.Append(".");
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
                _script.Append("[");
                _script.Append(Massage(index));
                _script.Append("]");
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
                if (indexes[0] is EvalContext)
                {
                    _script.Append("[");
                    _script.Append(indexes[0].ToString());
                    _script.Append("]");
                }
                else
                {
                    //todo: is this needed?
                    Append(binder.CallInfo.ArgumentNames.Single());
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
                if (_functionInvocationCount > 0)
                {
                    throw new Exception("Only one function invocation is currently supported in a single expression.");
                }
                _functionInvocationCount++;
                if (_script.Length > 0)
                {
                    _script.Append(".");
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
                return $"\"{value.ToString().Replace("'", "\u0027").Replace("\"", "\\u0022")}\"";
            }
            else if (value is DateTime)
            {
                return "new Date(\u0022" + ((DateTime)value).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz") + "\u0022)";
            }
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
        /// Explicitly invokes the Javascript expression and returns a value
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The value returned from Javascript</returns>
        public async virtual Task<T> InvokeAsync<T>()
        {
            if (!_hasInvoked)
            {
                Build();
            }
            return await InvokeAsync<T>(_escaped);
        }

        /// <summary>
        /// Executes an arbitrary string of Javascript
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="script">A string of Javascript</param>
        /// <returns></returns>
        public async virtual Task<T> InvokeAsync<T>(string script)
        {
            _hasInvoked = true;
#if DEBUG
            Debug.WriteLine("BDJR: " + script);
#endif
            return await _runtime.InvokeAsync<T>("BlazorDynamicJavascriptRuntime.evaluate", script);
        }

        public void Dispose()
        {
            if (!_hasInvoked)
            {
                Build();
                Task.Run(async () => await InvokeAsync<dynamic>(_escaped));
            }
        }

        private void Build()
        {
            Expression?.Invoke();
            _hasInvoked = true;
            _escaped = _script.ToString().Replace('_', ' ');
        }

    }

}
