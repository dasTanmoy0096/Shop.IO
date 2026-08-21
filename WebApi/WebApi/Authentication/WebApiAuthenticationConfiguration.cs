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
    private const string CookieHttpOnlyConfigurationPath = $"{AuthenticationSectionPath}:CookieHttpOnly";
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

    private const string RequiredCookieName = "__Host-ShopIO.Api.Auth";
    private const string RequiredCookiePath = "/";
    private const bool RequiredCookieHttpOnly = true;
    private const SameSiteMode RequiredCookieSameSite = SameSiteMode.Strict;
    private const CookieSecurePolicy RequiredCookieSecurePolicy = CookieSecurePolicy.Always;
    private const bool RequiredCookieIsEssential = true;
    private const int RequiredCookieLifetimeMinutes = 480;
    private const bool RequiredCookiePersistent = false;
    private const bool RequiredSlidingExpiration = false;
    private const int RequiredDataProtectionKeyLifetimeDays = 90;

    private readonly X509Certificate2? keyEncryptionCertificate;

    internal string CookieName { get; }
    internal string CookiePath { get; }
    internal bool CookieHttpOnly { get; }
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
        string? cookieDomain = configuration[ CookieDomainConfigurationPath ];
        bool cookieHttpOnly = ReadRequiredBoolean(
            configuration,
            CookieHttpOnlyConfigurationPath,
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

        ValidateFixedValue(
            CookieNameConfigurationPath,
            cookieName,
            RequiredCookieName,
            errors
        );
        ValidateFixedValue(
            CookiePathConfigurationPath,
            cookiePath,
            RequiredCookiePath,
            errors
        );
        ValidateEmptyValue(
            CookieDomainConfigurationPath,
            cookieDomain,
            errors
        );
        ValidateFixedValue(
            CookieHttpOnlyConfigurationPath,
            cookieHttpOnly,
            RequiredCookieHttpOnly,
            errors
        );
        ValidateFixedValue(
            CookieSameSiteConfigurationPath,
            cookieSameSite,
            RequiredCookieSameSite,
            errors
        );
        ValidateFixedValue(
            CookieSecurePolicyConfigurationPath,
            cookieSecurePolicy,
            RequiredCookieSecurePolicy,
            errors
        );
        ValidateFixedValue(
            CookieIsEssentialConfigurationPath,
            cookieIsEssential,
            RequiredCookieIsEssential,
            errors
        );
        ValidateFixedValue(
            CookieLifetimeMinutesConfigurationPath,
            cookieLifetimeMinutes,
            RequiredCookieLifetimeMinutes,
            errors
        );
        ValidateFixedValue(
            CookiePersistentConfigurationPath,
            cookiePersistent,
            RequiredCookiePersistent,
            errors
        );
        ValidateFixedValue(
            SlidingExpirationConfigurationPath,
            slidingExpiration,
            RequiredSlidingExpiration,
            errors
        );
        ValidateFixedValue(
            DataProtectionKeyLifetimeDaysConfigurationPath,
            dataProtectionKeyLifetimeDays,
            RequiredDataProtectionKeyLifetimeDays,
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
        CookieHttpOnly = cookieHttpOnly;
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

    public void Dispose() {
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
        )) {
            errors.Add($"{configurationPath} must be a valid {typeof(TEnum).Name} value.");
            return default;
        }

        return value;
    }

    private static void ValidateFixedValue<T>(
        string configurationPath,
        T configuredValue,
        T requiredValue,
        List<string> errors
    ) {
        if (!EqualityComparer<T>.Default.Equals(
            configuredValue,
            requiredValue
        )) {
            errors.Add($"{configurationPath} must be {requiredValue}.");
        }
    }

    private static void ValidateEmptyValue(
        string configurationPath,
        string? configuredValue,
        List<string> errors
    ) {
        if (configuredValue is null || configuredValue.Length != 0) {
            errors.Add($"{configurationPath} must be explicitly empty.");
        }
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
            throw new InvalidOperationException(
                $"The WebApi authentication configuration is invalid: {DataProtectionCertificateFileConfigurationPath} could not be loaded."
            );
        }
    }

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count == 0) {
            return;
        }

        throw new InvalidOperationException(
            $"The WebApi authentication configuration is invalid: {string.Join(" ", errors)}"
        );
    }
}
