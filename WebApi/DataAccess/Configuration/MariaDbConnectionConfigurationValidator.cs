namespace DataAccess.Configuration;

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using MySqlConnector;

internal sealed class MariaDbConnectionConfigurationValidator {
    private const string MariaDbConfigurationSectionPath = "DataAccess:MariaDb";
    private const string DataSourceNameConfigurationKey = "DataSourceName";
    private const string DataSourceNameConfigurationPath = $"{MariaDbConfigurationSectionPath}:{DataSourceNameConfigurationKey}";

    private readonly record struct MariaDbConnectionOptionDefinition(
        string ConfigurationKey,
        bool IsRequired,
        bool RequiresNonEmptyValue
    );

    private readonly record struct MariaDbExpectedConnectionOptionValue(
        string ConfigurationKey,
        string ExpectedValue
    );

    private static readonly MariaDbConnectionOptionDefinition[] connectionOptionDefinitions =
    [
        new("Server", true, true),
        new("Port", true, true),
        new("UserID", true, true),
        new("Password", true, true),
        new("Database", true, true),
        new("LoadBalance", true, true),
        new("ConnectionProtocol", true, true),
        new("SslMode", true, true),
        new("CertificateFile", true, false),
        new("CertificatePassword", false, false),
        new("CertificateStoreLocation", true, true),
        new("CertificateThumbprint", true, false),
        new("SslCert", true, false),
        new("SslKey", true, false),
        new("SslCa", true, false),
        new("SkipCertificateRevocationCheck", true, true),
        new("TlsVersion", true, false),
        new("TlsCipherSuites", true, false),
        new("Pooling", true, true),
        new("ConnectionLifeTime", true, true),
        new("ConnectionReset", true, true),
        new("ConnectionIdleTimeout", true, true),
        new("MinimumPoolSize", true, true),
        new("MaximumPoolSize", true, true),
        new("DnsCheckInterval", true, true),
        new("AllowLoadLocalInfile", true, true),
        new("AllowPublicKeyRetrieval", true, true),
        new("AllowUserVariables", true, true),
        new("AllowZeroDateTime", true, true),
        new("AutoEnlist", true, true),
        new("CancellationTimeout", true, true),
        new("ConnectionTimeout", true, true),
        new("ConvertZeroDateTime", true, true),
        new("DateTimeKind", true, true),
        new("DefaultCommandTimeout", true, true),
        new("GuidFormat", true, true),
        new("IgnoreCommandTransaction", true, true),
        new("InteractiveSession", true, true),
        new("KeepAlive", true, true),
        new("NoBackslashEscapes", true, true),
        new("PersistSecurityInfo", true, true),
        new("Pipelining", true, true),
        new("ServerRedirectionMode", true, true),
        new("ServerRsaPublicKeyFile", true, false),
        new("ServerSPN", true, false),
        new("TreatTinyAsBoolean", true, true),
        new("UseAffectedRows", true, true),
        new("UseCompression", true, true),
        new("UseXaTransactions", true, true),
    ];

    private static readonly MariaDbExpectedConnectionOptionValue[] expectedConnectionOptionValues =
    [
        new("LoadBalance", "RoundRobin"),
        new("CertificateStoreLocation", "None"),
        new("SkipCertificateRevocationCheck", bool.FalseString),
        new("Pooling", bool.TrueString),
        new("ConnectionLifeTime", "0"),
        new("ConnectionReset", bool.TrueString),
        new("DnsCheckInterval", "0"),
        new("AllowLoadLocalInfile", bool.FalseString),
        new("AllowPublicKeyRetrieval", bool.FalseString),
        new("AllowUserVariables", bool.FalseString),
        new("AllowZeroDateTime", bool.FalseString),
        new("AutoEnlist", bool.FalseString),
        new("ConvertZeroDateTime", bool.FalseString),
        new("DateTimeKind", "Utc"),
        new("GuidFormat", "Char36"),
        new("IgnoreCommandTransaction", bool.FalseString),
        new("InteractiveSession", bool.FalseString),
        new("KeepAlive", "0"),
        new("NoBackslashEscapes", bool.FalseString),
        new("PersistSecurityInfo", bool.FalseString),
        new("Pipelining", bool.TrueString),
        new("ServerRedirectionMode", "Disabled"),
        new("TreatTinyAsBoolean", bool.TrueString),
        new("UseAffectedRows", bool.TrueString),
        new("UseCompression", bool.FalseString),
        new("UseXaTransactions", bool.FalseString),
    ];

    private static readonly string[] prohibitedConfigurationKeys =
    [
        "PipeName",
        "DeferConnectionReset",
        "ApplicationName",
        "CharacterSet",
        "IgnorePrepare",
        "OldGuids",
    ];

    private static readonly string[] expectedBlankConfigurationKeys =
    [
        "TlsVersion",
        "TlsCipherSuites",
        "ServerSPN",
    ];

    private readonly IHostEnvironment hostEnvironment;

    public MariaDbConnectionConfigurationValidator(IHostEnvironment hostEnvironment) {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        this.hostEnvironment = hostEnvironment;
    }

    internal MariaDbConnectionSettings Validate(IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        string dataSourceName = configuration[ DataSourceNameConfigurationPath ]?.Trim() ?? string.Empty;
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(dataSourceName)) {
            errors.Add($"{DataSourceNameConfigurationPath} must be a non-empty value.");
        }

        ValidateConfigurationKeys(
            configuration,
            errors
        );

        MySqlConnectionStringBuilder connectionStringBuilder = [];

        foreach (MariaDbConnectionOptionDefinition connectionOptionDefinition in connectionOptionDefinitions) {
            ApplyConnectionOption(
                configuration,
                connectionStringBuilder,
                connectionOptionDefinition,
                errors
            );
        }

        ValidateConnectionOptionPolicy(
            connectionStringBuilder,
            configuration,
            errors
        );

        ThrowIfInvalid(errors);

        return new MariaDbConnectionSettings(
            connectionStringBuilder.ConnectionString,
            dataSourceName
        );
    }

    private static void ValidateConfigurationKeys(
        IConfiguration configuration,
        List<string> errors
    ) {
        IConfigurationSection mariaDbConfigurationSection = configuration.GetSection(MariaDbConfigurationSectionPath);

        foreach (IConfigurationSection childConfigurationSection in mariaDbConfigurationSection.GetChildren()) {
            if (!IsKnownConfigurationKey(childConfigurationSection.Key)) {
                errors.Add($"{MariaDbConfigurationSectionPath}:{childConfigurationSection.Key} is not a supported MariaDB configuration key.");
            }
        }

        foreach (string prohibitedConfigurationKey in prohibitedConfigurationKeys) {
            string prohibitedConfigurationPath = GetConnectionOptionConfigurationPath(prohibitedConfigurationKey);

            if (configuration[ prohibitedConfigurationPath ] is not null) {
                errors.Add($"{prohibitedConfigurationPath} is not permitted by the Shop.IO MariaDB configuration contract.");
            }
        }
    }

    private static bool IsKnownConfigurationKey(string configurationKey) {
        if (string.Equals(
            configurationKey,
            DataSourceNameConfigurationKey,
            StringComparison.Ordinal
        )) {
            return true;
        }

        foreach (MariaDbConnectionOptionDefinition connectionOptionDefinition in connectionOptionDefinitions) {
            if (string.Equals(
                configurationKey,
                connectionOptionDefinition.ConfigurationKey,
                StringComparison.Ordinal
            )) {
                return true;
            }
        }

        foreach (string prohibitedConfigurationKey in prohibitedConfigurationKeys) {
            if (string.Equals(
                configurationKey,
                prohibitedConfigurationKey,
                StringComparison.Ordinal
            )) {
                return true;
            }
        }

        return false;
    }

    private static void ApplyConnectionOption(
        IConfiguration configuration,
        MySqlConnectionStringBuilder connectionStringBuilder,
        MariaDbConnectionOptionDefinition connectionOptionDefinition,
        List<string> errors
    ) {
        string configurationPath = GetConnectionOptionConfigurationPath(connectionOptionDefinition.ConfigurationKey);
        string? configuredValue = configuration[ configurationPath ];

        if (configuredValue is null) {
            if (connectionOptionDefinition.IsRequired) {
                errors.Add($"{configurationPath} must be explicitly configured.");
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(configuredValue)) {
            if (connectionOptionDefinition.RequiresNonEmptyValue) {
                errors.Add($"{configurationPath} must be a non-empty value.");
            }
            return;
        }

        try {
            connectionStringBuilder[ connectionOptionDefinition.ConfigurationKey ] = configuredValue;
        } catch (ArgumentException) {
            errors.Add($"{configurationPath} has an invalid MySqlConnector value.");
        } catch (FormatException) {
            errors.Add($"{configurationPath} has an invalid MySqlConnector value.");
        } catch (OverflowException) {
            errors.Add($"{configurationPath} has an invalid MySqlConnector value.");
        }
    }

    private void ValidateConnectionOptionPolicy(
        MySqlConnectionStringBuilder connectionStringBuilder,
        IConfiguration configuration,
        List<string> errors
    ) {
        ValidateExpectedConnectionOptionValues(
            connectionStringBuilder,
            errors
        );
        ValidateExpectedBlankConfigurationValues(
            configuration,
            errors
        );
        ValidateServerPlaceholder(
            connectionStringBuilder,
            errors
        );
        ValidateUInt32Range(
            connectionStringBuilder,
            "Port",
            1,
            ushort.MaxValue,
            errors
        );
        ValidateConnectionProtocol(
            connectionStringBuilder,
            errors
        );
        ValidateSslMode(
            connectionStringBuilder,
            errors
        );
        ValidateCertificateConfiguration(
            connectionStringBuilder,
            configuration,
            errors
        );
        ValidatePositiveUInt32(
            connectionStringBuilder,
            "ConnectionIdleTimeout",
            errors
        );
        ValidatePoolBounds(
            connectionStringBuilder,
            errors
        );
        ValidatePositiveUInt32(
            connectionStringBuilder,
            "CancellationTimeout",
            errors
        );
        ValidatePositiveUInt32(
            connectionStringBuilder,
            "ConnectionTimeout",
            errors
        );
        ValidatePositiveUInt32(
            connectionStringBuilder,
            "DefaultCommandTimeout",
            errors
        );
    }

    private static void ValidateExpectedConnectionOptionValues(
        MySqlConnectionStringBuilder connectionStringBuilder,
        List<string> errors
    ) {
        foreach (MariaDbExpectedConnectionOptionValue expectedConnectionOptionValue in expectedConnectionOptionValues) {
            ValidateExpectedOptionValue(
                connectionStringBuilder,
                expectedConnectionOptionValue.ConfigurationKey,
                expectedConnectionOptionValue.ExpectedValue,
                errors
            );
        }
    }

    private static void ValidateExpectedBlankConfigurationValues(
        IConfiguration configuration,
        List<string> errors
    ) {
        foreach (string expectedBlankConfigurationKey in expectedBlankConfigurationKeys) {
            if (!string.IsNullOrWhiteSpace(
                GetConfigurationValue(
                    configuration,
                    expectedBlankConfigurationKey
                )
            )) {
                errors.Add($"{GetConnectionOptionConfigurationPath(expectedBlankConfigurationKey)} must be blank under the Shop.IO MariaDB configuration contract.");
            }
        }
    }

    private static void ValidateExpectedOptionValue(
        MySqlConnectionStringBuilder connectionStringBuilder,
        string connectionOptionKey,
        string expectedValue,
        List<string> errors
    ) {
        string actualValue = GetConnectionOptionValue(
            connectionStringBuilder,
            connectionOptionKey
        );

        if (!string.Equals(
            actualValue,
            expectedValue,
            StringComparison.OrdinalIgnoreCase
        )) {
            errors.Add($"{GetConnectionOptionConfigurationPath(connectionOptionKey)} must be {expectedValue}.");
        }
    }

    private void ValidateServerPlaceholder(
            MySqlConnectionStringBuilder connectionStringBuilder,
            List<string> errors
    ) {
        if (hostEnvironment.IsDevelopment()) {
            return;
        }

        string server = GetConnectionOptionValue(
            connectionStringBuilder,
            "Server"
        );

        if (server.EndsWith(".example.invalid", StringComparison.OrdinalIgnoreCase)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("Server")} must override the source-controlled placeholder outside Development.");
        }
    }

    private void ValidateSslMode(
        MySqlConnectionStringBuilder connectionStringBuilder,
        List<string> errors
    ) {
        string sslMode = GetConnectionOptionValue(
            connectionStringBuilder,
            "SslMode"
        );

        if (hostEnvironment.IsDevelopment()) {
            if (string.Equals(
                sslMode,
                "Preferred",
                StringComparison.OrdinalIgnoreCase
            )) {
                errors.Add($"{GetConnectionOptionConfigurationPath("SslMode")} must not use Preferred in Development.");
            }
            return;
        }

        if (!string.Equals(
            sslMode,
            "VerifyFull",
            StringComparison.OrdinalIgnoreCase
        )) {
            errors.Add($"{GetConnectionOptionConfigurationPath("SslMode")} must use VerifyFull outside Development.");
        }
    }

    private static void ValidateConnectionProtocol(
        MySqlConnectionStringBuilder connectionStringBuilder,
        List<string> errors
    ) {
        if (connectionStringBuilder.ConnectionProtocol != MySqlConnectionProtocol.Sockets) {
            errors.Add($"{GetConnectionOptionConfigurationPath("ConnectionProtocol")} must select the cross-platform TCP/IP Socket protocol.");
        }
    }

    private static void ValidatePoolBounds(
        MySqlConnectionStringBuilder connectionStringBuilder,
        List<string> errors
    ) {
        bool hasMinimumPoolSize = TryGetUInt32(
            connectionStringBuilder,
            "MinimumPoolSize",
            out uint minimumPoolSize
        );
        bool hasMaximumPoolSize = TryGetUInt32(
            connectionStringBuilder,
            "MaximumPoolSize",
            out uint maximumPoolSize
        );

        if (!hasMinimumPoolSize || !hasMaximumPoolSize
            || maximumPoolSize == 0
            || minimumPoolSize > maximumPoolSize
        ) {
            errors.Add($"{GetConnectionOptionConfigurationPath("MinimumPoolSize")} and {GetConnectionOptionConfigurationPath("MaximumPoolSize")} must define valid pool bounds.");
        }
    }

    private static void ValidatePositiveUInt32(
        MySqlConnectionStringBuilder connectionStringBuilder,
        string connectionOptionKey,
        List<string> errors
    ) {
        if (!TryGetUInt32(
            connectionStringBuilder,
            connectionOptionKey,
            out uint value
        ) || value == 0) {
            errors.Add($"{GetConnectionOptionConfigurationPath(connectionOptionKey)} must be greater than zero.");
        }
    }

    private static void ValidateUInt32Range(
        MySqlConnectionStringBuilder connectionStringBuilder,
        string connectionOptionKey,
        uint minimumValue,
        uint maximumValue,
        List<string> errors
    ) {
        if (!TryGetUInt32(
            connectionStringBuilder,
            connectionOptionKey,
            out uint value
        ) || value < minimumValue || value > maximumValue) {
            errors.Add($"{GetConnectionOptionConfigurationPath(connectionOptionKey)} must be from {minimumValue} through {maximumValue}.");
        }
    }

    private static bool TryGetUInt32(
        MySqlConnectionStringBuilder connectionStringBuilder,
        string connectionOptionKey,
        out uint value
    ) {
        try {
            value = Convert.ToUInt32(
                connectionStringBuilder[ connectionOptionKey ],
                CultureInfo.InvariantCulture
            );
            return true;
        } catch (FormatException) {
            value = 0;
            return false;
        } catch (InvalidCastException) {
            value = 0;
            return false;
        } catch (OverflowException) {
            value = 0;
            return false;
        }
    }

    private static void ValidateCertificateConfiguration(
        MySqlConnectionStringBuilder connectionStringBuilder,
        IConfiguration configuration,
        List<string> errors
    ) {
        string sslMode = GetConnectionOptionValue(
            connectionStringBuilder,
            "SslMode"
        );
        string certificateFile = GetConfigurationValue(
            configuration,
            "CertificateFile"
        );
        string certificatePassword = GetConfigurationValue(
            configuration,
            "CertificatePassword"
        );
        string certificateThumbprint = GetConfigurationValue(
            configuration,
            "CertificateThumbprint"
        );
        string sslCert = GetConfigurationValue(
            configuration,
            "SslCert"
        );
        string sslKey = GetConfigurationValue(
            configuration,
            "SslKey"
        );
        string sslCa = GetConfigurationValue(
            configuration,
            "SslCa"
        );

        bool hasClientCertificate = !string.IsNullOrWhiteSpace(certificateFile)
            || !string.IsNullOrWhiteSpace(sslCert)
            || !string.IsNullOrWhiteSpace(sslKey);

        if (!string.IsNullOrWhiteSpace(certificateFile) && (!string.IsNullOrWhiteSpace(sslCert) || !string.IsNullOrWhiteSpace(sslKey))) {
            errors.Add($"{GetConnectionOptionConfigurationPath("CertificateFile")}, {GetConnectionOptionConfigurationPath("SslCert")}, and {GetConnectionOptionConfigurationPath("SslKey")} must not configure both PKCS #12 and PEM client certificates.");
        }

        if (string.IsNullOrWhiteSpace(sslCert) != string.IsNullOrWhiteSpace(sslKey)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("SslCert")} and {GetConnectionOptionConfigurationPath("SslKey")} must be supplied together.");
        }

        if (!string.IsNullOrWhiteSpace(certificatePassword) && string.IsNullOrWhiteSpace(certificateFile)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("CertificatePassword")} requires CertificateFile.");
        }

        if (!string.IsNullOrWhiteSpace(certificateThumbprint)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("CertificateThumbprint")} requires a certificate-store topology, which is not cross-platform.");
        }

        if (hasClientCertificate && IsTlsDisabled(sslMode)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("CertificateFile")}, {GetConnectionOptionConfigurationPath("SslCert")}, and {GetConnectionOptionConfigurationPath("SslKey")} require an enabled TLS mode.");
        }

        if (!string.IsNullOrWhiteSpace(sslCa) && !IsCertificateValidatingTlsMode(sslMode)) {
            errors.Add($"{GetConnectionOptionConfigurationPath("SslCa")} requires SslMode=VerifyCA or VerifyFull.");
        }
    }

    private static bool IsTlsDisabled(string sslMode) {
        return string.Equals(
            sslMode,
            "None",
            StringComparison.OrdinalIgnoreCase
        ) || string.Equals(
            sslMode,
            "Disabled",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static bool IsCertificateValidatingTlsMode(string sslMode) {
        return string.Equals(
            sslMode,
            "VerifyCA",
            StringComparison.OrdinalIgnoreCase
        ) || string.Equals(
            sslMode,
            "VerifyFull",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string GetConfigurationValue(
        IConfiguration configuration,
        string connectionOptionKey
    ) {
        return configuration[ GetConnectionOptionConfigurationPath(connectionOptionKey) ] ?? string.Empty;
    }

    private static string GetConnectionOptionConfigurationPath(string connectionOptionKey) {
        return $"{MariaDbConfigurationSectionPath}:{connectionOptionKey}";
    }

    private static string GetConnectionOptionValue(
        MySqlConnectionStringBuilder connectionStringBuilder,
        string connectionOptionKey
    ) {
        return Convert.ToString(
            connectionStringBuilder[ connectionOptionKey ],
            CultureInfo.InvariantCulture
        ) ?? string.Empty;
    }

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count > 0) {
            throw new InvalidOperationException($"Invalid MariaDB configuration. {string.Join(" ", errors)}");
        }
    }
}
