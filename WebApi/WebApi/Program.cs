namespace WebApi;

using System.Threading.Tasks;

using DataAccess;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using NLog.Web;

internal static class Program {
    private static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        builder.Services.AddDataAccess();

        WebApplication app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        await app.RunAsync();
    }
}
