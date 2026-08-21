namespace WebApi.Authentication;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class AuthenticationStartupGateHostedService : IHostedService {
    private readonly IDataProtectionProvider dataProtectionProvider;
    private readonly IOptionsMonitor<CookieAuthenticationOptions> cookieAuthenticationOptionsMonitor;

    public AuthenticationStartupGateHostedService(
        IDataProtectionProvider dataProtectionProvider,
        IOptionsMonitor<CookieAuthenticationOptions> cookieAuthenticationOptionsMonitor
    ) {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        ArgumentNullException.ThrowIfNull(cookieAuthenticationOptionsMonitor);

        this.dataProtectionProvider = dataProtectionProvider;
        this.cookieAuthenticationOptionsMonitor = cookieAuthenticationOptionsMonitor;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        _ = cookieAuthenticationOptionsMonitor.Get(AccountAuthenticationDefaults.Scheme);
        IDataProtector protector = dataProtectionProvider.CreateProtector(
            AccountAuthenticationDefaults.StartupGateProtectorPurpose
        );
        string protectedValue = protector.Protect("authentication-startup-gate");
        _ = protector.Unprotect(protectedValue);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
