namespace WebApi;

using System.Threading.Tasks;

using DataAccess;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Web;

using WebApi.Extensions;
using WebApi.Security;

internal static class Program {
    private static async Task Main() {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

        // CreateSlimBuilder requires an explicit opt-in for configuration-backed HTTPS endpoints.
        builder.WebHost.UseKestrelHttpsConfiguration();
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        builder.Services.AddDataAccess();
        builder.Services.AddAccountAuthentication();
        builder.Services.AddShopIoAuthorization();
        builder.Services.AddShopIoErrorHandling();
        builder.Services.AddShopIoRequestSecurity();

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns controller registration.
        if (builder.Environment.IsDevelopment()) {
            builder.Services.AddTemporaryReadinessDemonstration();
        }

        WebApplication app = builder.Build();

        app.UseShopIoErrorHandling();
        app.UseRouting();
        app.UseCors(CorsPolicyNames.Browser);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => "Hello World!");

        // TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns health mapping.
        if (app.Environment.IsDevelopment()) {
            app.MapControllers();
        }

        await app.RunAsync();
    }
}
