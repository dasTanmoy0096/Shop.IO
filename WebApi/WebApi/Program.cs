namespace WebApi;

using System.Threading.Tasks;

using DataAccess;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using WebApi.Extensions;

internal static class Program {
    private static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        builder.Services.AddDataAccess();

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns controller/CORS registration.
        if (builder.Environment.IsDevelopment()) {
            builder.Services.AddTemporaryReadinessDemonstration();
        }

        WebApplication app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns health/CORS mapping.
        if (app.Environment.IsDevelopment()) {
            app.UseRouting();
            app.UseCors();
            app.MapControllers();
        }

        await app.RunAsync();
    }
}
