using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.Tests.Client.Pages
{
    public class IndexPage : ComponentBase
    {

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        protected override Task OnInitializedAsync()
        {
            using (dynamic context = new EvalContext(JsRuntime))
            {
                var arg = new { Property = "Value", Field = 123, child = new { Member = new DateTime(2001, 1, 1) } };
                (context as EvalContext).Expression = () => context.JsInterop.set(arg);
            }

            var settings = new EvalContextSettings();
            settings.SerializableTypes.Add(typeof(Specified));
            using (dynamic context = new EvalContext(JsRuntime, settings))
            {
                var arg = new Specified { Member = "abc", Empty = null };
                (context as EvalContext).Expression = () => context.JsInterop.setSpecified(arg);
            }

            double value = 1;
            using (dynamic context = new EvalContext(JsRuntime))
            {
                (context as EvalContext).Expression = () => context.JsInterop.callMethod(value);
                value = (context as EvalContext).Invoke<double>();
            }

            using (dynamic context = new EvalContext(JsRuntime))
            {
                (context as EvalContext).Expression = () => context.JsInterop.returnValue = value;
            }

            new EvalContext(JsRuntime).Invoke<dynamic>($"JsInterop.returnValue = {value}");

            return base.OnInitializedAsync();
        }

        private class Specified
        {
            public string Member { get; set; }
            public string Empty { get; set; }
        }

    }

}
