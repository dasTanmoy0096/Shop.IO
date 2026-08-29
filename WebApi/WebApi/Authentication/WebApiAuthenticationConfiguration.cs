namespace WebApi.Authentication;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

internal sealed class WebApiAuthenticationConfiguration : IDisposable {
    private const string AuthenticationSectionPath = "Security:Authentication";
    private const string CookieNameConfigurationPath = $"{AuthenticationSectionPath}:CookieName";
    private const string CookiePathConfigurationPath = $"{AuthenticationSectionPath}:CookiePath";
    private const string CookieDomainConfigurationPath = $"{AuthenticationSectionPath}:CookieDomain";
    private const string CookieSameSiteConfigurationPath = $"{AuthenticationSectionPath}:CookieSameSite";
    private const string CookieSecurePolicyConfigurationPath = $"{AuthenticationSectionPath}:CookieSecurePolicy";
    private const string CookieIsEssentialConfigurationPath = $"{AuthenticationSectionPath}:CookieIsEssential";
    private const string CookieLifetimeMinutesConfigurationPath = $"{AuthenticationSectionPath}:CookieLifetimeMinutes";
    private const string CookiePersistentConfigurationPath = $"{AuthenticationSectionPath}:CookiePersistent";
    private const string SlidingExpirationConfigurationPath = $"{AuthenticationSectionPath}:SlidingExpiration";
    private const string DataProtectionSectionPath = "Security:DataProtection";
    private const string DataProtectionApplicationNameConfigurationPath = $"{DataProtectionSectionPath}:ApplicationName";
    private const string DataProtectionKeyDirectoryConfigurationPath = $"{DataProtectionSectionPath}:KeyDirectory";
    private const string DataProtectionKeyLifetimeDaysConfigurationPath = $"{DataProtectionSectionPath}:KeyLifetimeDays";
    private const string DataProtectionCertificateFileConfigurationPath = $"{DataProtectionSectionPath}:CertificateFile";
    private const string DataProtectionCertificatePasswordConfigurationPath = $"{DataProtectionSectionPath}:CertificatePassword";

    private const string HostCookiePrefix = "__Host-";
    private const int MinimumDataProtectionKeyLifetimeDays = 7;

    private readonly X509Certificate2? keyEncryptionCertificate;

    internal string CookieName { get; }
    internal string CookiePath { get; }
    internal string? CookieDomain { get; }
    internal SameSiteMode CookieSameSite { get; }
    internal CookieSecurePolicy CookieSecurePolicy { get; }
    internal bool CookieIsEssential { get; }
    internal TimeSpan CookieLifetime { get; }
    internal bool CookiePersistent { get; }
    internal bool SlidingExpiration { get; }
    internal string DataProtectionApplicationName { get; }
    internal DirectoryInfo DataProtectionKeyDirectory { get; }
    internal TimeSpan DataProtectionKeyLifetime { get; }
    internal X509Certificate2? KeyEncryptionCertificate => keyEncryptionCertificate;

    public WebApiAuthenticationConfiguration(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    ) {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        List<string> errors = [];
        string cookieName = ReadRequiredString(
            configuration,
            CookieNameConfigurationPath,
            errors
        );
        string cookiePath = ReadRequiredString(
            configuration,
            CookiePathConfigurationPath,
            errors
        );
        string? cookieDomain = ReadOptionalCookieDomain(
            configuration,
            CookieDomainConfigurationPath,
            errors
        );
        SameSiteMode cookieSameSite = ReadRequiredEnum<SameSiteMode>(
            configuration,
            CookieSameSiteConfigurationPath,
            errors
        );
        CookieSecurePolicy cookieSecurePolicy = ReadRequiredEnum<CookieSecurePolicy>(
            configuration,
            CookieSecurePolicyConfigurationPath,
            errors
        );
        bool cookieIsEssential = ReadRequiredBoolean(
            configuration,
            CookieIsEssentialConfigurationPath,
            errors
        );
        int cookieLifetimeMinutes = ReadRequiredInteger(
            configuration,
            CookieLifetimeMinutesConfigurationPath,
            errors
        );
        bool cookiePersistent = ReadRequiredBoolean(
            configuration,
            CookiePersistentConfigurationPath,
            errors
        );
        bool slidingExpiration = ReadRequiredBoolean(
            configuration,
            SlidingExpirationConfigurationPath,
            errors
        );
        string dataProtectionApplicationName = ReadRequiredString(
            configuration,
            DataProtectionApplicationNameConfigurationPath,
            errors
        );
        string dataProtectionKeyDirectory = ReadRequiredString(
            configuration,
            DataProtectionKeyDirectoryConfigurationPath,
            errors
        );
        int dataProtectionKeyLifetimeDays = ReadRequiredInteger(
            configuration,
            DataProtectionKeyLifetimeDaysConfigurationPath,
            errors
        );
        string? dataProtectionCertificateFile = configuration[ DataProtectionCertificateFileConfigurationPath ];
        string? dataProtectionCertificatePassword = configuration[ DataProtectionCertificatePasswordConfigurationPath ];

        ValidateCookieSecurity(
            hostEnvironment,
            cookieName,
            cookiePath,
            cookieDomain,
            cookieSameSite,
            cookieSecurePolicy,
            errors
        );
        ValidatePositiveInteger(
            CookieLifetimeMinutesConfigurationPath,
            cookieLifetimeMinutes,
            errors
        );
        ValidateMinimumInteger(
            DataProtectionKeyLifetimeDaysConfigurationPath,
            dataProtectionKeyLifetimeDays,
            MinimumDataProtectionKeyLifetimeDays,
            errors
        );

        if (!hostEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(dataProtectionCertificateFile)) {
            errors.Add($"{DataProtectionCertificateFileConfigurationPath} must be supplied outside Development.");
        }

        if (!hostEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(dataProtectionCertificatePassword)) {
            errors.Add($"{DataProtectionCertificatePasswordConfigurationPath} must be supplied outside Development.");
        }

        ThrowIfInvalid(errors);

        CookieName = cookieName;
        CookiePath = cookiePath;
        CookieDomain = cookieDomain;
        CookieSameSite = cookieSameSite;
        CookieSecurePolicy = cookieSecurePolicy;
        CookieIsEssential = cookieIsEssential;
        CookieLifetime = TimeSpan.FromMinutes(cookieLifetimeMinutes);
        CookiePersistent = cookiePersistent;
        SlidingExpiration = slidingExpiration;
        DataProtectionApplicationName = dataProtectionApplicationName;
        DataProtectionKeyDirectory = CreateDirectoryInfo(
            dataProtectionKeyDirectory,
            hostEnvironment.ContentRootPath
        );
        DataProtectionKeyLifetime = TimeSpan.FromDays(dataProtectionKeyLifetimeDays);
        keyEncryptionCertificate = TryLoadKeyEncryptionCertificate(
            dataProtectionCertificateFile,
            dataProtectionCertificatePassword,
            hostEnvironment.ContentRootPath
        );
    }

    void IDisposable.Dispose() {
        keyEncryptionCertificate?.Dispose();
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

        if (hostName.Length == 0 || Uri.CheckHostName(hostName) == UriHostNameType.Unknown) {
            errors.Add($"{configurationPath} must be empty or a valid host name.");
            return null;
        }

        return configuredValue;
    }

    private static void ValidateCookieSecurity(
        IHostEnvironment hostEnvironment,
        string cookieName,
        string cookiePath,
        string? cookieDomain,
        SameSiteMode cookieSameSite,
        CookieSecurePolicy cookieSecurePolicy,
        List<string> errors
    ) {
        bool usesHostPrefix = cookieName.StartsWith(
            HostCookiePrefix,
            StringComparison.Ordinal
        );
        bool requiresHostScope = !hostEnvironment.IsDevelopment() || usesHostPrefix;

        if (!IsHttpToken(cookieName)) {
            errors.Add($"{CookieNameConfigurationPath} must be a valid HTTP token.");
        }

        if (!cookiePath.StartsWith('/')) {
            errors.Add($"{CookiePathConfigurationPath} must be an absolute path.");
        }

        if (cookieSameSite == SameSiteMode.Unspecified) {
            errors.Add($"{CookieSameSiteConfigurationPath} must be an explicit SameSite mode.");
        }

        if (cookieSameSite == SameSiteMode.None && cookieSecurePolicy != CookieSecurePolicy.Always) {
            errors.Add($"{CookieSecurePolicyConfigurationPath} must be Always when {CookieSameSiteConfigurationPath} is None.");
        }

        if (!requiresHostScope) {
            return;
        }

        if (!usesHostPrefix) {
            errors.Add($"{CookieNameConfigurationPath} must start with {HostCookiePrefix} outside Development.");
        }

        if (!string.Equals(
            cookiePath,
            "/",
            StringComparison.Ordinal
        )) {
            errors.Add($"{CookiePathConfigurationPath} must be / for a host-only cookie.");
        }

        if (cookieDomain is not null) {
            errors.Add($"{CookieDomainConfigurationPath} must be empty for a host-only cookie.");
        }

        if (cookieSecurePolicy != CookieSecurePolicy.Always) {
            errors.Add($"{CookieSecurePolicyConfigurationPath} must be Always for a host-only cookie.");
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

    private static void ValidateMinimumInteger(
        string configurationPath,
        int configuredValue,
        int minimumValue,
        List<string> errors
    ) {
        if (configuredValue < minimumValue) {
            errors.Add($"{configurationPath} must be at least {minimumValue}.");
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

    private static DirectoryInfo CreateDirectoryInfo(
        string configuredPath,
        string contentRootPath
    ) {
        string fullPath = Path.IsPathFullyQualified(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);

        return new DirectoryInfo(fullPath);
    }

    private static X509Certificate2? TryLoadKeyEncryptionCertificate(
        string? configuredCertificateFile,
        string? configuredCertificatePassword,
        string contentRootPath
    ) {
        if (string.IsNullOrWhiteSpace(configuredCertificateFile)) {
            return null;
        }

        string certificateFile = Path.IsPathFullyQualified(configuredCertificateFile)
            ? configuredCertificateFile
            : Path.Combine(contentRootPath, configuredCertificateFile);

        try {
            X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
                certificateFile,
                configuredCertificatePassword ?? string.Empty,
                X509KeyStorageFlags.EphemeralKeySet,
                Pkcs12LoaderLimits.Defaults
            );

            if (!certificate.HasPrivateKey
                || certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow
                || certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow) {
                certificate.Dispose();
                throw new InvalidOperationException();
            }

            return certificate;
        } catch {
            throw new InvalidOperationException($"The WebApi authentication configuration is invalid: {DataProtectionCertificateFileConfigurationPath} could not be loaded.");
        }
    }

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count == 0) {
            return;
        }

        throw new InvalidOperationException($"The WebApi authentication configuration is invalid: {string.Join(" ", errors)}");
    }
}
