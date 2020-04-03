using Microsoft.JSInterop;
using Moq;
using System;
using System.Threading;
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

        [Theory]
        [InlineData("message", "\"message\"")]
        [InlineData("mess\rage", "`mess\rage`")]
        [InlineData("mess\nage", "`mess\nage`")]
        [InlineData("mess\r\nage", "`mess\r\nage`")]
        public void Given_a_dynamic_expression_And_multiline_string_argument_When_invoked_Then_should_execute_evaluation(string argument, string expected)
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.alert(argument);
            }
            Verify(runtime, $"alert({expected})");
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
            Verify(runtime, "Do(stuff)", 2);

            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                dynamic arg2 = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Do("document", arg2.@this);
            }
            Verify(runtime, "Do(\"document\", this)", 3);
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_function_is_called_And_context_is_reused_When_invoked_Then_should_not_throw_exception()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                dynamic arg2 = new EvalContext(runtime.Object);
                dynamic arg3 = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.Do(arg.document, arg2.@this); context.After(arg3.that);
            }
            //todo: prevent bad syntax
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

        [Theory]
        [InlineData("__", false, "var _instance = new object()")]
        [InlineData("__", true, "var___instance = new__object()")]
        public void Given_a_dynamic_expression_And_settings_Then_should_execute_evaluation(string replacing, bool disabled, string expected)
        {
            var runtime = new Mock<IJSRuntime>();
            var settings = new EvalContextSettings { SpaceCharacterPlaceholder = replacing, DisableSpaceCharacterPlaceholderReplacement = disabled };
            using (dynamic context = new EvalContext(runtime.Object, settings))
            {
                dynamic arg = new EvalContext(runtime.Object, settings);
                //alt+166
                (context as EvalContext).Expression = () => context.var___instance = arg.new__object();
            }
            Verify(runtime, expected);
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
            runtime.Verify(v => v.InvokeAsync<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
        }

        [Fact]
        public async Task Given_a_dynamic_expression_And_a_value_is_returned_by_index_When_invoked_Then_should_execute_evaluation_And_return_value()
        {
            var runtime = new Mock<IJSRuntime>();
            var script = "list[0]";
            var expected = "abc123";
            SetupReturnValue<string>(runtime, expected, script);

            dynamic context = new EvalContext(runtime.Object);
            (context as EvalContext).Expression = () => context.list[0];
            var actual = await (context as EvalContext).InvokeAsync<string>();

            Assert.Equal(expected, actual);
            runtime.Verify(v => v.InvokeAsync<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);

            script = "list[\"index\"]";
            SetupReturnValue<string>(runtime, expected, script);

            context = new EvalContext(runtime.Object);
            (context as EvalContext).Expression = () => context.list["index"];
            actual = await (context as EvalContext).InvokeAsync<string>();

            Assert.Equal(expected, actual);
            runtime.Verify(v => v.InvokeAsync<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
        }

        [Fact]
        public void Given_a_dynamic_expression_And_a_value_is_returned_When_invoked_synchronously_Then_should_execute_evaluation_And_return_value()
        {
            var runtime = new Mock<IJSInProcessRuntime>();
            var script = "method(\"1\")";
            var expected = "2";
            runtime.Setup(v => v.Invoke<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script))).Returns(expected);

            dynamic context = new EvalContext(runtime.Object);
            (context as EvalContext).Expression = () => context.method("1");
            var actual = (context as EvalContext).Invoke<string>();

            Assert.Equal(expected, actual);
            runtime.Verify(v => v.Invoke<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
        }

        [Fact]
        public void Given_a_string_of_javascript_And_a_value_is_returned_When_invoked_synchronously_Then_should_execute_evaluation_And_return_value()
        {
            var runtime = new Mock<IJSInProcessRuntime>();
            var script = "method(\"1\")";
            var expected = "2";
            runtime.Setup(v => v.Invoke<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script))).Returns(expected);

            var actual = new EvalContext(runtime.Object).Invoke<string>(script);

            Assert.Equal(expected, actual);
            runtime.Verify(v => v.Invoke<string>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
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

        [Fact]
        public void Given_a_dynamic_expression_And_floating_point_sum_When_invoked_Then_should_derive_result_in_csharp_And_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.sum = 0.1 + 0.2 * 0.5 / 0.5;
            }
            Verify(runtime, "sum = 0.30000000000000004");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_decimal_sum_When_invoked_Then_should_derive_result_in_csharp_And_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                dynamic arg = new EvalContext(runtime.Object);
                (context as EvalContext).Expression = () => context.sum = 0.1m + 0.2m * 0.5m / 0.5m;
            }
            Verify(runtime, "sum = 0.3");
        }

        [Fact]
        public async Task Given_multiple_dynamic_expressions_When_invoked_And_reset_And_invoked_Then_should_execute_mutliple_evaluations()
        {
            var runtime = new Mock<IJSRuntime>();
            var expected = "value";
            SetupReturnValue<string>(runtime, expected, "instance.property");
            using (dynamic context = new EvalContext(runtime.Object))
            {
                var evalContext = (context as EvalContext);
                dynamic arg = new EvalContext(runtime.Object);
                evalContext.Expression = () => context.var_instance = arg.new_object();
                await evalContext.InvokeAsync<dynamic>();
                evalContext.Reset();
                evalContext.Expression = () => context.instance.property = expected;
                await evalContext.InvokeAsync<dynamic>();
                evalContext.Reset();
                arg = new EvalContext(runtime.Object);
                evalContext.Expression = () => context.instance.property;
                var actual = await evalContext.InvokeAsync<string>();
                Assert.Equal(expected, actual);
            }

            Verify(runtime, "var instance = new object()");
            Verify(runtime, "instance.property = \"value\"");
            Verify<string>(runtime, "instance.property");
        }


        [Fact]
        public void Given_a_dynamic_expression_And_a_function_chain_is_called_When_invoked_Then_should_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {
                (context as EvalContext).Expression = () => context.jQuery("body").css("overflow-y", "hidden");
            }

            Verify(runtime, "jQuery(\"body\").css(\"overflow-y\", \"hidden\")");
        }

        [Fact]
        public void Given_a_dynamic_expression_And_anonymous_type_argument_is_supplied_When_invoked_Then_should_serialize_argument_And_execute_evaluation()
        {
            var runtime = new Mock<IJSRuntime>();
            using (dynamic context = new EvalContext(runtime.Object))
            {

                var arg = new { Property = "Value", Field = 123, child = new { Member = new DateTime(2001, 1, 1) } };
                (context as EvalContext).Expression = () => context.Bind(arg);
            }

            Verify(runtime, "Bind({\"property\":\"Value\",\"field\":123,\"child\":{\"member\":\"2001-01-01T00:00:00\"}})");
        }

        private static void SetupReturnValue<T>(Mock<IJSRuntime> runtime, string expected, string script)
        {
            runtime.Setup(v => v.InvokeAsync<T>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)))
                .Returns(new ValueTask<T>(Task.FromResult((T)Convert.ChangeType(expected, typeof(T)))));
        }

        private void Verify(Mock<IJSRuntime> runtime, string script, int expectedInvocations = 1)
        {
            SpinWait.SpinUntil(() => runtime.Invocations.Count == expectedInvocations, TimeSpan.FromSeconds(1));
            runtime.Verify(v => v.InvokeAsync<dynamic>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
        }

        private void Verify<T>(Mock<IJSRuntime> runtime, string script, int expectedInvocations = 1)
        {
            SpinWait.SpinUntil(() => runtime.Invocations.Count == expectedInvocations, TimeSpan.FromSeconds(1));
            runtime.Verify(v => v.InvokeAsync<T>($"BlazorDynamicJavascriptRuntime.evaluate", It.Is<object[]>(m => m[0].ToString() == script)), Times.Once);
        }

    }
}
