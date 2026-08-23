namespace WebApi.Security;

using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

internal sealed class WebApiRateLimiterOptionsConfiguration : IConfigureOptions<RateLimiterOptions> {
    private readonly WebApiRequestSecurityConfiguration requestSecurityConfiguration;

    public WebApiRateLimiterOptionsConfiguration(WebApiRequestSecurityConfiguration requestSecurityConfiguration) {
        ArgumentNullException.ThrowIfNull(requestSecurityConfiguration);

        this.requestSecurityConfiguration = requestSecurityConfiguration;
    }

    void IConfigureOptions<RateLimiterOptions>.Configure(RateLimiterOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.RejectionStatusCode = requestSecurityConfiguration.RateLimitRejectionStatusCode;
        options.OnRejected = OnRejectedAsync;
        AddPolicy(
            options,
            RateLimitPolicyNames.AntiforgeryToken,
            requestSecurityConfiguration.AntiforgeryTokenRateLimit
        );
        AddPolicy(
            options,
            RateLimitPolicyNames.SignIn,
            requestSecurityConfiguration.SignInRateLimit
        );
        AddPolicy(
            options,
            RateLimitPolicyNames.Registration,
            requestSecurityConfiguration.RegistrationRateLimit
        );
        AddPolicy(
            options,
            RateLimitPolicyNames.Search,
            requestSecurityConfiguration.SearchRateLimit
        );
        AddPolicy(
            options,
            RateLimitPolicyNames.Payment,
            requestSecurityConfiguration.PaymentRateLimit
        );
    }

    private static void AddPolicy(
        RateLimiterOptions options,
        string policyName,
        WebApiRateLimitPolicyConfiguration policyConfiguration
    ) {
        options.AddPolicy(
            policyName,
            httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                GetPartitionKey(httpContext),
                _ => policyConfiguration.CreateOptions()
            )
        );
    }

    private async ValueTask OnRejectedAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(context);

        HttpResponse response = context.HttpContext.Response;

        response.StatusCode = requestSecurityConfiguration.RateLimitRejectionStatusCode;

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out TimeSpan retryAfter
        )) {
            int retryAfterSeconds = (int)Math.Clamp(
                Math.Ceiling(retryAfter.TotalSeconds),
                1D,
                int.MaxValue
            );

            response.Headers.RetryAfter = retryAfterSeconds.ToString(
                CultureInfo.InvariantCulture
            );
        }

        await response.WriteAsJsonAsync(
            new RateLimitRejectionResponse(requestSecurityConfiguration.RateLimitRejectionMessage),
            cancellationToken
        );
    }

    private static string GetPartitionKey(HttpContext httpContext) {
        ArgumentNullException.ThrowIfNull(httpContext);

        IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;

        return remoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
    }
}
