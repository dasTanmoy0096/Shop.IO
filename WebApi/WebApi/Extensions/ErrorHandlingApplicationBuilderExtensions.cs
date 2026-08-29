namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Builder;

using WebApi.Middleware;

internal static class ErrorHandlingApplicationBuilderExtensions {
    internal static IApplicationBuilder UseShopIoErrorHandling(this IApplicationBuilder applicationBuilder) {
        ArgumentNullException.ThrowIfNull(applicationBuilder);

        applicationBuilder.UseMiddleware<RequestCorrelationMiddleware>();
        applicationBuilder.UseExceptionHandler();
        applicationBuilder.UseMiddleware<StatusCodeProblemDetailsMiddleware>();

        return applicationBuilder;
    }
}
