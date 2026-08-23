namespace WebApi.Authentication;

using System;

using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class WebApiDataProtectionKeyManagementOptionsConfiguration : IConfigureOptions<KeyManagementOptions> {
    private readonly WebApiAuthenticationConfiguration authenticationConfiguration;
    private readonly ILoggerFactory loggerFactory;

    public WebApiDataProtectionKeyManagementOptionsConfiguration(
        WebApiAuthenticationConfiguration authenticationConfiguration,
        ILoggerFactory loggerFactory
    ) {
        ArgumentNullException.ThrowIfNull(authenticationConfiguration);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        this.authenticationConfiguration = authenticationConfiguration;
        this.loggerFactory = loggerFactory;
    }

    void IConfigureOptions<KeyManagementOptions>.Configure(KeyManagementOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.XmlRepository = new FileSystemXmlRepository(
            authenticationConfiguration.DataProtectionKeyDirectory,
            loggerFactory
        );
        options.NewKeyLifetime = authenticationConfiguration.DataProtectionKeyLifetime;

        if (authenticationConfiguration.KeyEncryptionCertificate is not null) {
            options.XmlEncryptor = new CertificateXmlEncryptor(
                authenticationConfiguration.KeyEncryptionCertificate,
                loggerFactory
            );
        }
    }
}
