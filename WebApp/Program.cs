namespace WebApp;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using NLog.Web;

internal sealed class Program {
    internal Program() { }

    internal static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        WebApplication app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapStaticAssets().ShortCircuit();

        await app.RunAsync();
    }
}
