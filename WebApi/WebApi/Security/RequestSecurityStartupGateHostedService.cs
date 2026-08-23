namespace WebApi.Security;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class RequestSecurityStartupGateHostedService : IHostedService {
    private readonly IAntiforgery antiforgery;
    private readonly IOptions<CorsOptions> corsOptions;
    private readonly IOptions<AntiforgeryOptions> antiforgeryOptions;
    private readonly IOptions<MvcOptions> mvcOptions;
    private readonly IOptions<RateLimiterOptions> rateLimiterOptions;

    public RequestSecurityStartupGateHostedService(
        IAntiforgery antiforgery,
        IOptions<CorsOptions> corsOptions,
        IOptions<AntiforgeryOptions> antiforgeryOptions,
        IOptions<MvcOptions> mvcOptions,
        IOptions<RateLimiterOptions> rateLimiterOptions
    ) {
        ArgumentNullException.ThrowIfNull(antiforgery);
        ArgumentNullException.ThrowIfNull(corsOptions);
        ArgumentNullException.ThrowIfNull(antiforgeryOptions);
        ArgumentNullException.ThrowIfNull(mvcOptions);
        ArgumentNullException.ThrowIfNull(rateLimiterOptions);

        this.antiforgery = antiforgery;
        this.corsOptions = corsOptions;
        this.antiforgeryOptions = antiforgeryOptions;
        this.mvcOptions = mvcOptions;
        this.rateLimiterOptions = rateLimiterOptions;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        if (corsOptions.Value.GetPolicy(CorsPolicyNames.Browser) is null) {
            throw new InvalidOperationException("The WebApi browser CORS policy is not registered.");
        }

        _ = antiforgery;
        _ = antiforgeryOptions.Value.Cookie.Name;
        _ = mvcOptions.Value.Filters.Count;
        _ = rateLimiterOptions.Value.RejectionStatusCode;

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
