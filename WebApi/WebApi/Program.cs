namespace WebApi;

using System.Threading.Tasks;

using DataAccess;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using WebApi.Extensions;

internal static class Program {
    private static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

        // CreateSlimBuilder requires an explicit opt-in for configuration-backed HTTPS endpoints.
        builder.WebHost.UseKestrelHttpsConfiguration();
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        builder.Services.AddDataAccess();
        builder.Services.AddAccountAuthentication();

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns controller/CORS registration.
        if (builder.Environment.IsDevelopment()) {
            builder.Services.AddTemporaryReadinessDemonstration();
        }

        WebApplication app = builder.Build();

        app.UseRouting();

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns health/CORS mapping.
        if (app.Environment.IsDevelopment()) {
            app.UseCors();
        }

        app.UseAuthentication();

        app.MapGet("/", () => "Hello World!");

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns health/CORS mapping.
        if (app.Environment.IsDevelopment()) {
            app.MapControllers();
        }

        await app.RunAsync();
    }
}
