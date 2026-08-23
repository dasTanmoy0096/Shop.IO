namespace WebApi.Security;

using System;
using System.Threading.RateLimiting;

internal sealed class WebApiRateLimitPolicyConfiguration {
    private int PermitLimit { get; }
    private TimeSpan Window { get; }
    private int SegmentsPerWindow { get; }
    private int QueueLimit { get; }
    private QueueProcessingOrder QueueProcessingOrder { get; }
    private bool AutoReplenishment { get; }

    internal WebApiRateLimitPolicyConfiguration(
        int permitLimit,
        TimeSpan window,
        int segmentsPerWindow,
        int queueLimit,
        QueueProcessingOrder queueProcessingOrder,
        bool autoReplenishment
    ) {
        PermitLimit = permitLimit;
        Window = window;
        SegmentsPerWindow = segmentsPerWindow;
        QueueLimit = queueLimit;
        QueueProcessingOrder = queueProcessingOrder;
        AutoReplenishment = autoReplenishment;
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
