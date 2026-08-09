namespace WebApi;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

internal sealed class Program {
    internal Program() { }

    internal static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        await app.RunAsync();
    }
}
