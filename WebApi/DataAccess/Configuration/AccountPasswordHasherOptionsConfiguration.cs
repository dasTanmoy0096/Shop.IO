namespace DataAccess.Configuration;

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

internal sealed class AccountPasswordHasherOptionsConfiguration : IConfigureOptions<PasswordHasherOptions> {
    private readonly AccountPolicy accountPolicy;

    public AccountPasswordHasherOptionsConfiguration(AccountPolicy accountPolicy) {
        ArgumentNullException.ThrowIfNull(accountPolicy);

        this.accountPolicy = accountPolicy;
    }

    void IConfigureOptions<PasswordHasherOptions>.Configure(PasswordHasherOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
        options.IterationCount = accountPolicy.PasswordHashIterationCount;
    }
}
