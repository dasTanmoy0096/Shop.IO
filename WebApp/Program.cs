namespace WebApp;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

internal static class Program {
    private static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

        // CreateSlimBuilder requires an explicit opt-in for configuration-backed HTTPS endpoints.
        builder.WebHost.UseKestrelHttpsConfiguration();

        // CreateSlimBuilder does not load the Development static-web-assets manifest by default.
        if (builder.Environment.IsDevelopment()) {
            builder.WebHost.UseStaticWebAssets();
        }

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        WebApplication app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapStaticAssets().ShortCircuit();

        await app.RunAsync();
    }
}
