namespace WebApi.Middleware;

using System;

using Microsoft.Extensions.Logging;

internal static partial class WebApiErrorHandlingLog {
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled request failure was converted to a generic problem response."
    )]
    internal static partial void UnhandledRequestFailure(
        ILogger logger,
        Exception exception
    );
}
