namespace WebApp.RequestCorrelation;

using System;

using Microsoft.Extensions.Configuration;

internal sealed class WebAppRequestCorrelationConfiguration {
    private const string ResponseHeaderNameConfigurationPath = "RequestCorrelation:ResponseHeaderName";

    internal string ResponseHeaderName { get; }

    public WebAppRequestCorrelationConfiguration(IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        string? responseHeaderName = configuration[ ResponseHeaderNameConfigurationPath ];

        if (string.IsNullOrWhiteSpace(responseHeaderName) || !IsHttpToken(responseHeaderName)) {
            throw new InvalidOperationException($"{ResponseHeaderNameConfigurationPath} must be a valid HTTP header name.");
        }

        ResponseHeaderName = responseHeaderName;
    }

    private static bool IsHttpToken(string value) {
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
}
