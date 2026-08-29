namespace WebApi.Middleware;

using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal sealed class WebApiExceptionHandler : IExceptionHandler {
    private readonly WebApiErrorHandlingConfiguration errorHandlingConfiguration;
    private readonly WebApiProblemDetailsResponseWriter problemDetailsResponseWriter;
    private readonly ILogger<WebApiExceptionHandler> logger;

    public WebApiExceptionHandler(
        WebApiErrorHandlingConfiguration errorHandlingConfiguration,
        WebApiProblemDetailsResponseWriter problemDetailsResponseWriter,
        ILogger<WebApiExceptionHandler> logger
    ) {
        ArgumentNullException.ThrowIfNull(errorHandlingConfiguration);
        ArgumentNullException.ThrowIfNull(problemDetailsResponseWriter);
        ArgumentNullException.ThrowIfNull(logger);

        this.errorHandlingConfiguration = errorHandlingConfiguration;
        this.problemDetailsResponseWriter = problemDetailsResponseWriter;
        this.logger = logger;
    }

    async ValueTask<bool> IExceptionHandler.TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (httpContext.Response.HasStarted || httpContext.RequestAborted.IsCancellationRequested) {
            return false;
        }

        WebApiErrorHandlingLog.UnhandledRequestFailure(
            logger,
            exception
        );

        WebApiProblemDetailsResponse responseValue = new(
            type: null,
            title: errorHandlingConfiguration.UnexpectedErrorTitle,
            status: StatusCodes.Status500InternalServerError,
            detail: errorHandlingConfiguration.UnexpectedErrorDetail,
            instance: null,
            requestId: httpContext.TraceIdentifier
        );

        await problemDetailsResponseWriter.WriteAsync(
            httpContext,
            MediaTypeNames.Application.ProblemJson,
            StatusCodes.Status500InternalServerError,
            responseValue,
            cancellationToken
        );

        return true;
    }
}
