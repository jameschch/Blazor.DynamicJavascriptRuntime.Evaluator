using Microsoft.JSInterop;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.Tests
{
    public class EvalContextTest
    {


        [Fact]
        public void Given_a_dynamic_expression_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.Chart.defaults.global.animation.duration = 0;
            }
            Verify(runtime, "Chart.defaults.global.animation.duration = 0");

            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.Chart.defaults.global.defaultFontColor = "#FFF";
            }
            Verify(runtime, "Chart.defaults.global.defaultFontColor = \"#FFF\"");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_arguments_are_supplied_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                dynamic arg2 = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Chart.config.data.datasets[arg.i].data.push(arg2.config.data.datasets[arg.i].data);
            }

            Verify(runtime, "Chart.config.data.datasets[i].data.push(config.data.datasets[i].data)");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_function_is_called_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Do("stuff");
            }
            Verify(runtime, "Do(\"stuff\")");

            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Do(arg.stuff);
            }
            Verify(runtime, "Do(stuff)");

            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                dynamic arg2 = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Do("document", arg2.@this);
            }
            Verify(runtime, "Do(\"document\", this)");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_function_is_called_And_context_is_reused_When_invoked_Then_should_throw_exception()
        {

            Assert.Throws<Exception>(() =>
            {
                var runtime = new Mock<IJSRuntime>();
                using (dynamic context = new EvalContext(runtime.Object))
                {
                    dynamic arg = new EvalContext(runtime.Object);
                    dynamic arg2 = new EvalContext(runtime.Object);
                    dynamic arg3 = new EvalContext(runtime.Object);
                    (context as EvalContext).Expression = () => context.Do(arg.document, arg2.@this); context.After(arg3.that);
                }
            });
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_string_indexer_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.person["firstname"] = "Joe";
            }
            Verify(runtime, "person[\"firstname\"] = \"Joe\"");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_call_to_window_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.window.location = "www.be-evil\'\".com";
            }
            Verify(runtime, "window.location = \"www.be-evil\'\\u0022.com\"");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_declaration_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.var_instance = arg.new_object();
            }
            Verify(runtime, "var instance = new object()");
        }

        [Fact]
        public async Task Given_a_string_of_javascript_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            var expected = "expected";
            var script = "return " + expected;
            SetupReturnValue<dynamic>(runtime, expected, script);

            using (dynamic context = new EvalContext(runtime.Object))
            {
                var actual = await (context as EvalContext).InvokeAsync<dynamic>(script);
                Assert.Equal(expected, actual);
            }
            Verify(runtime, script);
        }

        [Fact]
        public async Task Given_a_dynamic_expression_And_a_value_is_returned_When_invoked_Then_should_execute_evaluation_And_return_value()
        {
            var runtime = new Mock<IJSRuntime>();
            var expected = "abc123";
            var script = "document.cookie";
            SetupReturnValue<string>(runtime, expected, script);

            dynamic context = new EvalContext(runtime.Object);
            (context as EvalContext).Expression = () => context.document.cookie;
            var actual = await (context as EvalContext).InvokeAsync<string>();

            Assert.Equal(expected, actual);
            runtime.Verify(v => v.InvokeAsync<string>($"BlazorDynamicJavascriptRuntime.evaluate", script), Times.Once);
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_date_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.date = new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            }
            Verify(runtime, "date = new Date(\"2001-01-01T01:01:01.0000000+00:00\")");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_null_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.something = new int?();
            }
            Verify(runtime, "something = null");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_operators_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.sum = 1 + 1 * 2 / 2;
            }
            Verify(runtime, "sum = 2");
        }

        private static void SetupReturnValue<T>(Mock<IJSRuntime> runtime, string expected, string script)
        {
            runtime.Setup(v => v.InvokeAsync<T>($"BlazorDynamicJavascriptRuntime.evaluate", script))
                .Returns(Task.FromResult((T)Convert.ChangeType(expected, typeof(T))));
        }

        private void Verify(Mock<IJSRuntime> runtime, string script)
        {
            runtime.Verify(v => v.InvokeAsync<dynamic>($"BlazorDynamicJavascriptRuntime.evaluate", script), Times.Once);
        }

    }
}
