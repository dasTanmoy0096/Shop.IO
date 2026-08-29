namespace WebApp.RequestCorrelation;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal sealed class RequestCorrelationMiddleware : IMiddleware {
    private const string RequestIdLogScopePropertyName = "ShopIORequestId";
    private const string RequestIdHttpContextItemName = "ShopIORequestId";

    private readonly WebAppRequestCorrelationConfiguration requestCorrelationConfiguration;
    private readonly ILogger<RequestCorrelationMiddleware> logger;

    public RequestCorrelationMiddleware(
        WebAppRequestCorrelationConfiguration requestCorrelationConfiguration,
        ILogger<RequestCorrelationMiddleware> logger
    ) {
        ArgumentNullException.ThrowIfNull(requestCorrelationConfiguration);
        ArgumentNullException.ThrowIfNull(logger);

        this.requestCorrelationConfiguration = requestCorrelationConfiguration;
        this.logger = logger;
    }

    async Task IMiddleware.InvokeAsync(
        HttpContext httpContext,
        RequestDelegate next
    ) {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(next);

        string requestId = Guid.NewGuid().ToString("N");
        httpContext.TraceIdentifier = requestId;
        httpContext.Items[ RequestIdHttpContextItemName ] = requestId;
        httpContext.Response.Headers[ requestCorrelationConfiguration.ResponseHeaderName ] = requestId;

        KeyValuePair<string, object?>[] scopeProperties = [
            new KeyValuePair<string, object?>(
                RequestIdLogScopePropertyName,
                requestId
            ),
        ];
        using IDisposable? requestScope = logger.BeginScope(scopeProperties);

        await next(httpContext);
    }
}
