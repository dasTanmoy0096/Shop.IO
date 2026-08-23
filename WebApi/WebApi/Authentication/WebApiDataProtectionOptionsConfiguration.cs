namespace WebApi.Authentication;

using System;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

internal sealed class WebApiDataProtectionOptionsConfiguration : IConfigureOptions<DataProtectionOptions> {
    private readonly WebApiAuthenticationConfiguration authenticationConfiguration;

    public WebApiDataProtectionOptionsConfiguration(
        WebApiAuthenticationConfiguration authenticationConfiguration
    ) {
        ArgumentNullException.ThrowIfNull(authenticationConfiguration);

        this.authenticationConfiguration = authenticationConfiguration;
    }

    void IConfigureOptions<DataProtectionOptions>.Configure(DataProtectionOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.ApplicationDiscriminator = authenticationConfiguration.DataProtectionApplicationName;
    }
}
