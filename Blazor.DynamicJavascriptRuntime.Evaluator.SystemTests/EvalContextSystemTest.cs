using System;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.SystemTests
{
    public class EvalContextSystemTest : IDisposable
    {

        private readonly ChromeDriver _driver;

        public EvalContextSystemTest()
        {
            var options = new ChromeOptions();
            //options.AddArgument("no-sandbox");
            options.AddArguments("headless");
            _driver = new ChromeDriver(".", options);
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
        }

        [Fact]
        public void Given_a_blazor_app_When_passing_anonymous_argument_Then_should_serialize_and_execute()
        {
            _driver.Navigate().GoToUrl("http://localhost:54235");

            object actual = null;

            SpinWait.SpinUntil(() =>
            {
                actual = _driver.ExecuteScript("return JSON.stringify(JsInterop.anonymous)");
                return actual != null && actual.ToString() != "null";
            }, TimeSpan.FromSeconds(3));

            Assert.Equal("{\"property\":\"Value\",\"field\":123,\"child\":{\"member\":\"2001-01-01T00:00:00\"}}", actual);
        }

        [Fact]
        public void Given_a_blazor_app_When_passing_argument_And_specifying_as_serializable_type_Then_should_serialize_and_execute()
        {
            _driver.Navigate().GoToUrl("http://localhost:54235");

            object actual = null;

            SpinWait.SpinUntil(() =>
            {
                actual = _driver.ExecuteScript("return JSON.stringify(JsInterop.specified)");
                return actual != null && actual.ToString() != "null";
            }, TimeSpan.FromSeconds(5));

            Assert.Equal("{\"member\":\"abc\"}", actual);
        }

        [Fact]
        public void Given_a_blazor_app_When_invoking_synchronously_Then_should_execute()
        {
            _driver.Navigate().GoToUrl("http://localhost:54235");

            object actual = null;

            SpinWait.SpinUntil(() =>
            {
                actual = _driver.ExecuteScript("return JsInterop.returnValue");
                return actual != null && actual.ToString() != "null";
            }, TimeSpan.FromSeconds(2));

            Assert.Equal(2, (long)actual);
        }

        [Fact]
        public void Given_a_blazor_app_When_invoking_asynchronously_Then_should_execute()
        {
            _driver.Navigate().GoToUrl("http://localhost:54235");

            object actual = null;

            SpinWait.SpinUntil(() =>
            {
                actual = _driver.ExecuteScript("return JsInterop.anotherReturnValue");
                return actual != null && actual.ToString() != "null";
            }, TimeSpan.FromSeconds(5));

            Assert.Equal(2, (long)actual);
        }

    }
}
