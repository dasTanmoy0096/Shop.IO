namespace WebApi.Middleware;

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

internal sealed class WebApiErrorHandlingConfiguration {
    private const string ErrorHandlingSectionPath = "ErrorHandling";
    private const string RequestIdResponseHeaderNameConfigurationPath = $"{ErrorHandlingSectionPath}:RequestIdResponseHeaderName";
    private const string UnexpectedErrorTitleConfigurationPath = $"{ErrorHandlingSectionPath}:UnexpectedError:Title";
    private const string UnexpectedErrorDetailConfigurationPath = $"{ErrorHandlingSectionPath}:UnexpectedError:Detail";
    private const int MaximumClientMessageLength = 512;

    internal const string RequestIdProblemDetailsExtensionName = "requestId";

    internal string RequestIdResponseHeaderName { get; }
    internal string UnexpectedErrorTitle { get; }
    internal string UnexpectedErrorDetail { get; }

    public WebApiErrorHandlingConfiguration(IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        List<string> errors = [];
        string requestIdResponseHeaderName = ReadRequiredHeaderName(
            configuration,
            RequestIdResponseHeaderNameConfigurationPath,
            errors
        );
        string unexpectedErrorTitle = ReadRequiredClientMessage(
            configuration,
            UnexpectedErrorTitleConfigurationPath,
            errors
        );
        string unexpectedErrorDetail = ReadRequiredClientMessage(
            configuration,
            UnexpectedErrorDetailConfigurationPath,
            errors
        );

        ThrowIfInvalid(errors);

        RequestIdResponseHeaderName = requestIdResponseHeaderName;
        UnexpectedErrorTitle = unexpectedErrorTitle;
        UnexpectedErrorDetail = unexpectedErrorDetail;
    }

    private static string ReadRequiredHeaderName(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue) || !IsHttpToken(configuredValue)) {
            errors.Add($"{configurationPath} must be a valid HTTP header name.");
            return string.Empty;
        }

        return configuredValue;
    }

    private static string ReadRequiredClientMessage(
        IConfiguration configuration,
        string configurationPath,
        List<string> errors
    ) {
        string? configuredValue = configuration[ configurationPath ];

        if (string.IsNullOrWhiteSpace(configuredValue)) {
            errors.Add($"{configurationPath} must be a non-empty value.");
            return string.Empty;
        }

        if (configuredValue.Length > MaximumClientMessageLength) {
            errors.Add($"{configurationPath} must not exceed {MaximumClientMessageLength} characters.");
            return string.Empty;
        }

        return configuredValue;
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

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count == 0) {
            return;
        }

        throw new InvalidOperationException($"The WebApi error-handling configuration is invalid: {string.Join(" ", errors)}");
    }
}
