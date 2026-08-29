namespace WebApi.Security;

using System;
using System.Threading.RateLimiting;

internal sealed class WebApiRateLimitPolicyConfiguration {
    private const bool AutoReplenishment = true;

    private int PermitLimit { get; }
    private TimeSpan Window { get; }
    private int SegmentsPerWindow { get; }
    private int QueueLimit { get; }
    private QueueProcessingOrder QueueProcessingOrder { get; }

    internal WebApiRateLimitPolicyConfiguration(
        int permitLimit,
        TimeSpan window,
        int segmentsPerWindow,
        int queueLimit,
        QueueProcessingOrder queueProcessingOrder
    ) {
        PermitLimit = permitLimit;
        Window = window;
        SegmentsPerWindow = segmentsPerWindow;
        QueueLimit = queueLimit;
        QueueProcessingOrder = queueProcessingOrder;
    }

    internal SlidingWindowRateLimiterOptions CreateOptions() {
        return new SlidingWindowRateLimiterOptions {
            PermitLimit = PermitLimit,
            Window = Window,
            SegmentsPerWindow = SegmentsPerWindow,
            QueueLimit = QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder,
            AutoReplenishment = AutoReplenishment,
        };
    }
}
