namespace WebApi.Middleware;

using System;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

internal sealed class StatusCodeProblemDetailsMiddleware : IMiddleware {
    // This is an exclusive validation boundary, not an emitted HTTP status code.
    private const int FirstNonErrorStatusCode = 600;

    private readonly WebApiProblemDetailsResponseWriter problemDetailsResponseWriter;

    public StatusCodeProblemDetailsMiddleware(WebApiProblemDetailsResponseWriter problemDetailsResponseWriter) {
        ArgumentNullException.ThrowIfNull(problemDetailsResponseWriter);

        this.problemDetailsResponseWriter = problemDetailsResponseWriter;
    }

    async Task IMiddleware.InvokeAsync(
        HttpContext httpContext,
        RequestDelegate next
    ) {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(next);

        await next(httpContext);

        HttpResponse response = httpContext.Response;

        if (!ShouldWriteProblemDetails(httpContext, response)) {
            return;
        }

        int statusCode = response.StatusCode;
        WebApiProblemDetailsResponse responseValue = new(
            type: null,
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            status: statusCode,
            detail: null,
            instance: null,
            requestId: httpContext.TraceIdentifier
        );

        await problemDetailsResponseWriter.WriteAsync(
            httpContext,
            MediaTypeNames.Application.ProblemJson,
            statusCode,
            responseValue,
            httpContext.RequestAborted
        );
    }

    private static bool ShouldWriteProblemDetails(
        HttpContext httpContext,
        HttpResponse response
    ) {
        if (response.HasStarted
            || response.StatusCode < StatusCodes.Status400BadRequest
            || response.StatusCode >= FirstNonErrorStatusCode
            || response.ContentLength is not null
            || !string.IsNullOrEmpty(response.ContentType)) {
            return false;
        }

        return !HttpMethods.IsHead(httpContext.Request.Method);
    }
}
