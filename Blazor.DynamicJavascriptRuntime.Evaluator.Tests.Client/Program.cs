using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Blazor.DynamicJavascriptRuntime.Evaluator.Tests.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddSingleton<IServiceProvider>(builder.Services.BuildServiceProvider());
            //builder.Services.AddBaseAddressHttpClient();

            builder.RootComponents.Add<App>("#app");

            await builder.Build().RunAsync();
        }
    }
}
