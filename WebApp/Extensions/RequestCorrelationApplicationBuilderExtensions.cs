namespace WebApp.Extensions;

using System;

using Microsoft.AspNetCore.Builder;

using WebApp.RequestCorrelation;

internal static class RequestCorrelationApplicationBuilderExtensions {
    internal static IApplicationBuilder UseShopIoRequestCorrelation(this IApplicationBuilder applicationBuilder) {
        ArgumentNullException.ThrowIfNull(applicationBuilder);

        applicationBuilder.UseMiddleware<RequestCorrelationMiddleware>();

        return applicationBuilder;
    }
}
