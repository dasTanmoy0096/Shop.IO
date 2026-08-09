namespace WebApp;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

internal sealed class Program {
    internal Program() { }

    internal static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapStaticAssets().ShortCircuit();

        await app.RunAsync();
    }
}
