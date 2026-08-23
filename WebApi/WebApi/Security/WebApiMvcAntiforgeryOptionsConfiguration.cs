namespace WebApi.Security;

using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

internal sealed class WebApiMvcAntiforgeryOptionsConfiguration : IConfigureOptions<MvcOptions> {
    public WebApiMvcAntiforgeryOptionsConfiguration() { }

    void IConfigureOptions<MvcOptions>.Configure(MvcOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
}
