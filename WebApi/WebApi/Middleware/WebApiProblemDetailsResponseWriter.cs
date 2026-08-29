namespace WebApi.Middleware;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

internal sealed class WebApiProblemDetailsResponseWriter {
    private readonly WebApiErrorHandlingConfiguration errorHandlingConfiguration;

    public WebApiProblemDetailsResponseWriter(WebApiErrorHandlingConfiguration errorHandlingConfiguration) {
        ArgumentNullException.ThrowIfNull(errorHandlingConfiguration);

        this.errorHandlingConfiguration = errorHandlingConfiguration;
    }

    internal async Task WriteAsync<TResponse>(
        HttpContext httpContext,
        string contentType,
        int statusCode,
        TResponse responseValue,
        CancellationToken cancellationToken
    ) where TResponse : class {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(responseValue);

        httpContext.Response.Headers[ errorHandlingConfiguration.RequestIdResponseHeaderName ] = httpContext.TraceIdentifier;
        httpContext.Response.ContentType = contentType;
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync<TResponse>(
            responseValue,
            options: null,
            contentType: contentType,
            cancellationToken
        );
    }
}
