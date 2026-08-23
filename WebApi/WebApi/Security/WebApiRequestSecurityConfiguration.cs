namespace WebApi.Security;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

internal sealed class WebApiRequestSecurityConfiguration {
    private const string CorsSectionPath = "Cors";
    private const string CorsAllowedOriginsConfigurationPath = $"{CorsSectionPath}:AllowedOrigins";
    private const string CorsAllowCredentialsConfigurationPath = $"{CorsSectionPath}:AllowCredentials";
    private const string CorsAllowedMethodsConfigurationPath = $"{CorsSectionPath}:AllowedMethods";
    private const string CorsAllowedHeadersConfigurationPath = $"{CorsSectionPath}:AllowedHeaders";
    private const string CorsExposedHeadersConfigurationPath = $"{CorsSectionPath}:ExposedHeaders";
    private const string CorsPreflightMaxAgeSecondsConfigurationPath = $"{CorsSectionPath}:PreflightMaxAgeSeconds";
    private const string AntiforgerySectionPath = "Security:Antiforgery";
    private const string AntiforgeryCookieNameConfigurationPath = $"{AntiforgerySectionPath}:CookieName";
    private const string AntiforgeryCookiePathConfigurationPath = $"{AntiforgerySectionPath}:CookiePath";
    private const string AntiforgeryCookieDomainConfigurationPath = $"{AntiforgerySectionPath}:CookieDomain";
    private const string AntiforgeryCookieHttpOnlyConfigurationPath = $"{AntiforgerySectionPath}:CookieHttpOnly";
    private const string AntiforgeryCookieSameSiteConfigurationPath = $"{AntiforgerySectionPath}:CookieSameSite";
    private const string AntiforgeryCookieSecurePolicyConfigurationPath = $"{AntiforgerySectionPath}:CookieSecurePolicy";
    private const string AntiforgeryCookieIsEssentialConfigurationPath = $"{AntiforgerySectionPath}:CookieIsEssential";
    private const string AntiforgeryHeaderNameConfigurationPath = $"{AntiforgerySectionPath}:HeaderName";
    private const string AntiforgeryFormFieldNameConfigurationPath = $"{AntiforgerySectionPath}:FormFieldName";
    private const string AntiforgerySuppressReadingTokenFromFormBodyConfigurationPath = $"{AntiforgerySectionPath}:SuppressReadingTokenFromFormBody";
    private const string AntiforgerySuppressXFrameOptionsHeaderConfigurationPath = $"{AntiforgerySectionPath}:SuppressXFrameOptionsHeader";
    private const string RateLimitingSectionPath = "Security:RateLimiting";
    private const string RateLimitingRejectionStatusCodeConfigurationPath = $"{RateLimitingSectionPath}:RejectionStatusCode";
    private const string RateLimitingRejectionMessageConfigurationPath = $"{RateLimitingSectionPath}:RejectionMessage";

    private const string HostCookiePrefix = "__Host-";
    private const bool RequiredAntiforgerySuppressXFrameOptionsHeader = false;
    private const int RequiredRateLimitRejectionStatusCode = StatusCodes.Status429TooManyRequests;

    internal IReadOnlyList<string> CorsAllowedOrigins { get; }
    internal bool CorsAllowCredentials { get; }
    internal IReadOnlyList<string> CorsAllowedMethods { get; }
    internal IReadOnlyList<string> CorsAllowedHeaders { get; }
    internal IReadOnlyList<string> CorsExposedHeaders { get; }
    internal TimeSpan CorsPreflightMaxAge { get; }
    internal string AntiforgeryCookieName { get; }
    internal string AntiforgeryCookiePath { get; }
    internal string? AntiforgeryCookieDomain { get; }
    internal bool AntiforgeryCookieHttpOnly { get; }
    internal SameSiteMode AntiforgeryCookieSameSite { get; }
    internal CookieSecurePolicy AntiforgeryCookieSecurePolicy { get; }
    internal bool AntiforgeryCookieIsEssential { get; }
    internal string AntiforgeryHeaderName { get; }
    internal string AntiforgeryFormFieldName { get; }
    internal bool AntiforgerySuppressReadingTokenFromFormBody { get; }
    internal bool AntiforgerySuppressXFrameOptionsHeader { get; }
    internal int RateLimitRejectionStatusCode { get; }
    internal string RateLimitRejectionMessage { get; }
    internal WebApiRateLimitPolicyConfiguration AntiforgeryTokenRateLimit { get; }
    internal WebApiRateLimitPolicyConfiguration SignInRateLimit { get; }
    internal WebApiRateLimitPolicyConfiguration RegistrationRateLimit { get; }
    internal WebApiRateLimitPolicyConfiguration SearchRateLimit { get; }
    internal WebApiRateLimitPolicyConfiguration PaymentRateLimit { get; }

    public WebApiRequestSecurityConfiguration(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    ) {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        List<string> errors = [];
        IReadOnlyList<string> corsAllowedOrigins = ReadRequiredStringArray(
            configuration,
            CorsAllowedOriginsConfigurationPath,
            errors
        );
        bool corsAllowCredentials = ReadRequiredBoolean(
            configuration,
            CorsAllowCredentialsConfigurationPath,
            errors
        );
        IReadOnlyList<string> corsAllowedMethods = ReadRequiredStringArray(
            configuration,
            CorsAllowedMethodsConfigurationPath,
            errors
        );
        IReadOnlyList<string> corsAllowedHeaders = ReadRequiredStringArray(
            configuration,
            CorsAllowedHeadersConfigurationPath,
            errors
        );
        IReadOnlyList<string> corsExposedHeaders = ReadRequiredStringArray(
            configuration,
            CorsExposedHeadersConfigurationPath,
            errors
        );
        int corsPreflightMaxAgeSeconds = ReadRequiredInteger(
            configuration,
            CorsPreflightMaxAgeSecondsConfigurationPath,
            errors
        );
        string antiforgeryCookieName = ReadRequiredString(
            configuration,
            AntiforgeryCookieNameConfigurationPath,
            errors
        );
        string antiforgeryCookiePath = ReadRequiredString(
            configuration,
            AntiforgeryCookiePathConfigurationPath,
            errors
        );
        string? antiforgeryCookieDomain = ReadOptionalCookieDomain(
            configuration,
            AntiforgeryCookieDomainConfigurationPath,
            errors
        );
        bool antiforgeryCookieHttpOnly = ReadRequiredBoolean(
            configuration,
            AntiforgeryCookieHttpOnlyConfigurationPath,
            errors
        );
        SameSiteMode antiforgeryCookieSameSite = ReadRequiredEnum<SameSiteMode>(
            configuration,
            AntiforgeryCookieSameSiteConfigurationPath,
            errors
        );
        CookieSecurePolicy antiforgeryCookieSecurePolicy = ReadRequiredEnum<CookieSecurePolicy>(
            configuration,
            AntiforgeryCookieSecurePolicyConfigurationPath,
            errors
        );
        bool antiforgeryCookieIsEssential = ReadRequiredBoolean(
            configuration,
            AntiforgeryCookieIsEssentialConfigurationPath,
            errors
        );
        string antiforgeryHeaderName = ReadRequiredString(
            configuration,
            AntiforgeryHeaderNameConfigurationPath,
            errors
        );
        string antiforgeryFormFieldName = ReadRequiredString(
            configuration,
            AntiforgeryFormFieldNameConfigurationPath,
            errors
        );
        bool antiforgerySuppressReadingTokenFromFormBody = ReadRequiredBoolean(
            configuration,
            AntiforgerySuppressReadingTokenFromFormBodyConfigurationPath,
            errors
        );
        bool antiforgerySuppressXFrameOptionsHeader = ReadRequiredBoolean(
            configuration,
            AntiforgerySuppressXFrameOptionsHeaderConfigurationPath,
            errors
        );
        int rateLimitRejectionStatusCode = ReadRequiredInteger(
            configuration,
            RateLimitingRejectionStatusCodeConfigurationPath,
            errors
        );
        string rateLimitRejectionMessage = ReadRequiredString(
            configuration,
            RateLimitingRejectionMessageConfigurationPath,
            errors
        );
        WebApiRateLimitPolicyConfiguration antiforgeryTokenRateLimit = ReadRateLimitPolicy(
            configuration,
            "AntiforgeryToken",
            errors
        );
        WebApiRateLimitPolicyConfiguration signInRateLimit = ReadRateLimitPolicy(
            configuration,
            "SignIn",
            errors
        );
        WebApiRateLimitPolicyConfiguration registrationRateLimit = ReadRateLimitPolicy(
            configuration,
            "Registration",
            errors
        );
        WebApiRateLimitPolicyConfiguration searchRateLimit = ReadRateLimitPolicy(
            configuration,
            "Search",
            errors
        );
        WebApiRateLimitPolicyConfiguration paymentRateLimit = ReadRateLimitPolicy(
            configuration,
            "Payment",
            errors
        );

        ValidateOrigins(
            corsAllowedOrigins,
            hostEnvironment,
            errors
        );
        ValidateHttpTokenArray(
            CorsAllowedMethodsConfigurationPath,
            corsAllowedMethods,
            errors
        );
        ValidateHttpTokenArray(
            CorsAllowedHeadersConfigurationPath,
            corsAllowedHeaders,
            errors
        );
        ValidateHttpTokenArray(
            CorsExposedHeadersConfigurationPath,
            corsExposedHeaders,
            errors
        );
        ValidateNonNegativeInteger(
            CorsPreflightMaxAgeSecondsConfigurationPath,
            corsPreflightMaxAgeSeconds,
            errors
        );
        ValidateAntiforgeryCookieSecurity(
            hostEnvironment,
            antiforgeryCookieName,
            antiforgeryCookiePath,
            antiforgeryCookieDomain,
            antiforgeryCookieHttpOnly,
            antiforgeryCookieSameSite,
            antiforgeryCookieSecurePolicy,
            errors
        );
        ValidateHttpToken(
            AntiforgeryHeaderNameConfigurationPath,
            antiforgeryHeaderName,
            errors
        );
        ValidateCorsAllowsAntiforgeryHeader(
            corsAllowedHeaders,
            antiforgeryHeaderName,
            errors
        );

        if (antiforgerySuppressXFrameOptionsHeader != RequiredAntiforgerySuppressXFrameOptionsHeader) {
            errors.Add($"{AntiforgerySuppressXFrameOptionsHeaderConfigurationPath} must be false.");
        }

        if (rateLimitRejectionStatusCode != RequiredRateLimitRejectionStatusCode) {
            errors.Add($"{RateLimitingRejectionStatusCodeConfigurationPath} must be {RequiredRateLimitRejectionStatusCode}.");
        }

        ThrowIfInvalid(errors);

        CorsAllowedOrigins = corsAllowedOrigins;
        CorsAllowCredentials = corsAllowCredentials;
        CorsAllowedMethods = corsAllowedMethods;
        CorsAllowedHeaders = corsAllowedHeaders;
        CorsExposedHeaders = corsExposedHeaders;
        CorsPreflightMaxAge = TimeSpan.FromSeconds(corsPreflightMaxAgeSeconds);
        AntiforgeryCookieName = antiforgeryCookieName;
        AntiforgeryCookiePath = antiforgeryCookiePath;
        AntiforgeryCookieDomain = antiforgeryCookieDomain;
        AntiforgeryCookieHttpOnly = antiforgeryCookieHttpOnly;
        AntiforgeryCookieSameSite = antiforgeryCookieSameSite;
        AntiforgeryCookieSecurePolicy = antiforgeryCookieSecurePolicy;
        AntiforgeryCookieIsEssential = antiforgeryCookieIsEssential;
        AntiforgeryHeaderName = antiforgeryHeaderName;
        AntiforgeryFormFieldName = antiforgeryFormFieldName;
        AntiforgerySuppressReadingTokenFromFormBody = antiforgerySuppressReadingTokenFromFormBody;
        AntiforgerySuppressXFrameOptionsHeader = antiforgerySuppressXFrameOptionsHeader;
        RateLimitRejectionStatusCode = rateLimitRejectionStatusCode;
        RateLimitRejectionMessage = rateLimitRejectionMessage;
        AntiforgeryTokenRateLimit = antiforgeryTokenRateLimit;
        SignInRateLimit = signInRateLimit;
        RegistrationRateLimit = registrationRateLimit;
        SearchRateLimit = searchRateLimit;
        PaymentRateLimit = paymentRateLimit;
    }

    private static WebApiRateLimitPolicyConfiguration ReadRateLimitPolicy(
        IConfiguration configuration,
        string policyName,
        List<string> errors
    ) {
        string policyConfigurationPath = $"{RateLimitingSectionPath}:{policyName}";
        int permitLimit = ReadRequiredInteger(
            configuration,
            $"{policyConfigurationPath}:PermitLimit",
            errors
        );
        int windowSeconds = ReadRequiredInteger(
            configuration,
            $"{policyConfigurationPath}:WindowSeconds",
            errors
        );
        int segmentsPerWindow = ReadRequiredInteger(
            configuration,
            $"{policyConfigurationPath}:SegmentsPerWindow",
            errors
        );
        int queueLimit = ReadRequiredInteger(
            configuration,
            $"{policyConfigurationPath}:QueueLimit",
            errors
        );
        QueueProcessingOrder queueProcessingOrder = ReadRequiredEnum<QueueProcessingOrder>(
            configuration,
            $"{policyConfigurationPath}:QueueProcessingOrder",
            errors
        );
        bool autoReplenishment = ReadRequiredBoolean(
            configuration,
            $"{policyConfigurationPath}:AutoReplenishment",
            errors
        );

        ValidatePositiveInteger(
            $"{policyConfigurationPath}:PermitLimit",
            permitLimit,
            errors
        );
        ValidatePositiveInteger(
            $"{policyConfigurationPath}:WindowSeconds",
            windowSeconds,
            errors
        );
        ValidatePositiveInteger(
            $"{policyConfigurationPath}:SegmentsPerWindow",
            segmentsPerWindow,
            errors
        );
        ValidateNonNegativeInteger(
            $"{policyConfigurationPath}:QueueLimit",
            queueLimit,
            errors
        );

        if (!autoReplenishment) {
            errors.Add($"{policyConfigurationPath}:AutoReplenishment must be true.");
        }

        return new WebApiRateLimitPolicyConfiguration(
            permitLimit,
            TimeSpan.FromSeconds(windowSeconds),
            segmentsPerWindow,
            queueLimit,
            queueProcessingOrder,
            autoReplenishment
        );
    }

    private static ReadOnlyCollection<string> ReadRequiredStringArray(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        List<string> values = [];

        foreach (IConfigurationSection valueSection in configuration
            .GetSection(configurationPath)
            .GetChildren()) {
            if (string.IsNullOrWhiteSpace(valueSection.Value)) {
                errors.Add($"{configurationPath} must not contain empty values.");
                continue;
            }

            values.Add(valueSection.Value);
        }

        if (values.Count == 0) {
            errors.Add($"{configurationPath} must contain at least one value.");
        }

        return values.AsReadOnly();
    }

    private static string ReadRequiredString(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue)) {
            errors.Add($"{configurationPath} must be a non-empty value.");
            return string.Empty;
        }

        return configuredValue;
    }

    private static int ReadRequiredInteger(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue)
            || !int.TryParse(
                configuredValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int value
        )) {
            errors.Add($"{configurationPath} must be an integer value.");
            return 0;
        }

        return value;
    }

    private static bool ReadRequiredBoolean(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue)
            || !bool.TryParse(
                configuredValue,
                out bool value
        )) {
            errors.Add($"{configurationPath} must be a Boolean value.");
            return false;
        }

        return value;
    }

    private static TEnum ReadRequiredEnum<TEnum>(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) where TEnum : struct, Enum {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue)
            || !Enum.TryParse(
                configuredValue,
                ignoreCase: false,
                out TEnum value
            ) || !Enum.IsDefined(value)) {
            errors.Add($"{configurationPath} must be a valid {typeof(TEnum).Name} value.");
            return default;
        }

        return value;
    }

    private static string? ReadOptionalCookieDomain(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (configuredValue is null || configuredValue.Length == 0) {
            return null;
        }

        if (string.IsNullOrWhiteSpace(configuredValue)) {
            errors.Add($"{configurationPath} must be empty or a valid host name.");
            return null;
        }

        string hostName = configuredValue.StartsWith('.')
            ? configuredValue[ 1.. ]
            : configuredValue;

        if (hostName.Length == 0
            || Uri.CheckHostName(hostName) == UriHostNameType.Unknown) {
            errors.Add($"{configurationPath} must be empty or a valid host name.");
            return null;
        }

        return configuredValue;
    }

    private static void ValidateOrigins(
        IReadOnlyList<string> configuredOrigins,
        IHostEnvironment hostEnvironment,
        List<string> errors
    ) {
        HashSet<string> seenOrigins = new(StringComparer.OrdinalIgnoreCase);

        foreach (string configuredOrigin in configuredOrigins) {
            if (!Uri.TryCreate(
                configuredOrigin,
                UriKind.Absolute,
                out Uri? origin
                ) || origin.UserInfo.Length != 0
                || origin.Query.Length != 0
                || origin.Fragment.Length != 0
                || !string.Equals(
                    origin.AbsolutePath,
                    "/",
                    StringComparison.Ordinal
                ) || configuredOrigin.EndsWith('/')
                || configuredOrigin.Contains('*')
                || !string.Equals(
                    configuredOrigin,
                    origin.GetLeftPart(UriPartial.Authority),
                    StringComparison.Ordinal
                ) || (!string.Equals(
                    origin.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.Ordinal
                ) && !string.Equals(
                    origin.Scheme,
                    Uri.UriSchemeHttp,
                    StringComparison.Ordinal
            ))) {
                errors.Add($"{CorsAllowedOriginsConfigurationPath} contains an invalid exact origin.");
                continue;
            }

            if (string.Equals(
                origin.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.Ordinal
            ) && (!hostEnvironment.IsDevelopment() || !origin.IsLoopback)) {
                errors.Add($"{CorsAllowedOriginsConfigurationPath} permits HTTP only for a Development loopback origin.");
            }

            if (!seenOrigins.Add(configuredOrigin)) {
                errors.Add($"{CorsAllowedOriginsConfigurationPath} contains a duplicate origin.");
            }
        }
    }

    private static void ValidateHttpTokenArray(
        string configurationPath,
        IReadOnlyList<string> configuredValues,
        List<string> errors
    ) {
        HashSet<string> seenValues = new(StringComparer.OrdinalIgnoreCase);

        foreach (string configuredValue in configuredValues) {
            ValidateHttpToken(
                configurationPath,
                configuredValue,
                errors
            );

            if (!seenValues.Add(configuredValue)) {
                errors.Add($"{configurationPath} must not contain duplicate values.");
            }
        }
    }

    private static void ValidateHttpToken(
        string configurationPath,
        string configuredValue,
        List<string> errors
    ) {
        if (!IsHttpToken(configuredValue)
            || string.Equals(configuredValue, "*", StringComparison.Ordinal)) {
            errors.Add($"{configurationPath} must contain explicit valid HTTP tokens.");
        }
    }

    private static bool IsHttpToken(string value) {
        if (value.Length == 0) {
            return false;
        }

        foreach (char character in value) {
            if ((character >= '0' && character <= '9')
                || (character >= 'A' && character <= 'Z')
                || (character >= 'a' && character <= 'z')) {
                continue;
            }

            switch (character) {
                case '!':
                case '#':
                case '$':
                case '%':
                case '&':
                case '\'':
                case '*':
                case '+':
                case '-':
                case '.':
                case '^':
                case '_':
                case '`':
                case '|':
                case '~':
                    continue;
                default:
                    return false;
            }
        }

        return true;
    }

    private static void ValidateCorsAllowsAntiforgeryHeader(
        IReadOnlyList<string> corsAllowedHeaders,
        string antiforgeryHeaderName,
        List<string> errors
    ) {
        foreach (string corsAllowedHeader in corsAllowedHeaders) {
            if (string.Equals(
                corsAllowedHeader,
                antiforgeryHeaderName,
                StringComparison.OrdinalIgnoreCase
            )) {
                return;
            }
        }

        errors.Add($"{CorsAllowedHeadersConfigurationPath} must include {AntiforgeryHeaderNameConfigurationPath}.");
    }

    private static void ValidateAntiforgeryCookieSecurity(
        IHostEnvironment hostEnvironment,
        string cookieName,
        string cookiePath,
        string? cookieDomain,
        bool cookieHttpOnly,
        SameSiteMode cookieSameSite,
        CookieSecurePolicy cookieSecurePolicy,
        List<string> errors
    ) {
        bool usesHostPrefix = cookieName.StartsWith(
            HostCookiePrefix,
            StringComparison.Ordinal
        );
        bool requiresHostScope = !hostEnvironment.IsDevelopment() || usesHostPrefix;

        ValidateHttpToken(
            AntiforgeryCookieNameConfigurationPath,
            cookieName,
            errors
        );

        if (!cookiePath.StartsWith('/')) {
            errors.Add($"{AntiforgeryCookiePathConfigurationPath} must be an absolute path.");
        }

        if (!cookieHttpOnly) {
            errors.Add($"{AntiforgeryCookieHttpOnlyConfigurationPath} must be true.");
        }

        if (cookieSameSite == SameSiteMode.Unspecified) {
            errors.Add($"{AntiforgeryCookieSameSiteConfigurationPath} must be an explicit SameSite mode.");
        }

        if (cookieSameSite == SameSiteMode.None
            && cookieSecurePolicy != CookieSecurePolicy.Always) {
            errors.Add($"{AntiforgeryCookieSecurePolicyConfigurationPath} must be Always when {AntiforgeryCookieSameSiteConfigurationPath} is None.");
        }

        if (!requiresHostScope) {
            return;
        }

        if (!usesHostPrefix) {
            errors.Add($"{AntiforgeryCookieNameConfigurationPath} must start with {HostCookiePrefix} outside Development.");
        }

        if (!string.Equals(cookiePath, "/", StringComparison.Ordinal)) {
            errors.Add($"{AntiforgeryCookiePathConfigurationPath} must be / for a host-only cookie.");
        }

        if (cookieDomain is not null) {
            errors.Add($"{AntiforgeryCookieDomainConfigurationPath} must be empty for a host-only cookie.");
        }

        if (cookieSecurePolicy != CookieSecurePolicy.Always) {
            errors.Add($"{AntiforgeryCookieSecurePolicyConfigurationPath} must be Always for a host-only cookie.");
        }
    }

    private static void ValidatePositiveInteger(
        string configurationPath,
        int configuredValue,
        List<string> errors
    ) {
        if (configuredValue <= 0) {
            errors.Add($"{configurationPath} must be greater than zero.");
        }
    }

    private static void ValidateNonNegativeInteger(
        string configurationPath,
        int configuredValue,
        List<string> errors
    ) {
        if (configuredValue < 0) {
            errors.Add($"{configurationPath} must not be negative.");
        }
    }

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count == 0) {
            return;
        }

        throw new InvalidOperationException($"The WebApi request-security configuration is invalid: {string.Join(" ", errors)}");
    }
}
