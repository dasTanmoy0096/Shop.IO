namespace WebApi.Security;

using System;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

using WebApi.Middleware;

internal sealed class WebApiRateLimiterOptionsConfiguration : IConfigureOptions<RateLimiterOptions> {
    private const int RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    private readonly WebApiRequestSecurityConfiguration requestSecurityConfiguration;
    private readonly WebApiProblemDetailsResponseWriter problemDetailsResponseWriter;

    public WebApiRateLimiterOptionsConfiguration(
        WebApiRequestSecurityConfiguration requestSecurityConfiguration,
        WebApiProblemDetailsResponseWriter problemDetailsResponseWriter
    ) {
        ArgumentNullException.ThrowIfNull(requestSecurityConfiguration);
        ArgumentNullException.ThrowIfNull(problemDetailsResponseWriter);

        this.requestSecurityConfiguration = requestSecurityConfiguration;
        this.problemDetailsResponseWriter = problemDetailsResponseWriter;
    }

    void IConfigureOptions<RateLimiterOptions>.Configure(RateLimiterOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.RejectionStatusCode = RejectionStatusCode;
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

        WebApiProblemDetailsResponse responseValue = new(
            type: null,
            title: requestSecurityConfiguration.RateLimitRejectionTitle,
            status: RejectionStatusCode,
            detail: requestSecurityConfiguration.RateLimitRejectionDetail,
            instance: null,
            requestId: context.HttpContext.TraceIdentifier
        );

        await problemDetailsResponseWriter.WriteAsync(
            context.HttpContext,
            MediaTypeNames.Application.ProblemJson,
            RejectionStatusCode,
            responseValue,
            cancellationToken
        );
    }

    private static string GetPartitionKey(HttpContext httpContext) {
        ArgumentNullException.ThrowIfNull(httpContext);

        IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;

        return remoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
    }
}
