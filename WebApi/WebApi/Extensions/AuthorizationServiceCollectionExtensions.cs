namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using WebApi.Authentication;
using WebApi.Authorization;

internal static class AuthorizationServiceCollectionExtensions {
    internal static IServiceCollection AddShopIoAuthorization(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AccountAuthorizationPolicyNames.Customer,
                static policyBuilder => {
                    ConfigureShopIoCookiePolicy(policyBuilder);
                    policyBuilder.Requirements.Add(new AuthenticatedAccountAuthorizationRequirement());
                }
            )
            .AddPolicy(
                AccountAuthorizationPolicyNames.AccountOwner,
                static policyBuilder => {
                    ConfigureShopIoCookiePolicy(policyBuilder);
                    policyBuilder.Requirements.Add(new AccountOwnershipAuthorizationRequirement());
                }
            )
            .AddPolicy(
                AccountAuthorizationPolicyNames.Moderator,
                static policyBuilder => {
                    ConfigureShopIoCookiePolicy(policyBuilder);
                    policyBuilder.Requirements.Add(new AccountRoleAuthorizationRequirement(AccountRoleCodes.Moderator));
                }
            )
            .AddPolicy(
                AccountAuthorizationPolicyNames.Administrator,
                static policyBuilder => {
                    ConfigureShopIoCookiePolicy(policyBuilder);
                    policyBuilder.Requirements.Add(new AccountRoleAuthorizationRequirement(AccountRoleCodes.Administrator));
                }
            );
        services.AddSingleton<IAuthorizationHandler, AuthenticatedAccountAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AccountOwnershipAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AccountRoleAuthorizationHandler>();

        return services;
    }

    private static void ConfigureShopIoCookiePolicy(AuthorizationPolicyBuilder policyBuilder) {
        ArgumentNullException.ThrowIfNull(policyBuilder);

        policyBuilder.AddAuthenticationSchemes(AccountAuthenticationDefaults.Scheme);
        policyBuilder.RequireAuthenticatedUser();
    }
}
