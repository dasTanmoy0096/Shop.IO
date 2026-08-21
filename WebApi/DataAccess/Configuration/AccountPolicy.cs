namespace DataAccess.Configuration;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using Microsoft.Extensions.Configuration;

internal sealed class AccountPolicy {
    private const string ConfigurationSectionPath = "DataAccess:Accounts";
    private const string UsernameMinimumLengthConfigurationPath = $"{ConfigurationSectionPath}:UsernameMinimumLength";
    private const string UsernameMaximumLengthConfigurationPath = $"{ConfigurationSectionPath}:UsernameMaximumLength";
    private const string PasswordMinimumLengthConfigurationPath = $"{ConfigurationSectionPath}:PasswordMinimumLength";
    private const string PasswordMaximumLengthConfigurationPath = $"{ConfigurationSectionPath}:PasswordMaximumLength";
    private const string PasswordHashIterationCountConfigurationPath = $"{ConfigurationSectionPath}:PasswordHashIterationCount";
    private const string PasswordBlocklistConfigurationPath = $"{ConfigurationSectionPath}:PasswordBlocklist";

    private const int RequiredUsernameMinimumLength = 3;
    private const int RequiredUsernameMaximumLength = 64;
    private const int RequiredPasswordMinimumLength = 15;
    private const int RequiredPasswordMaximumLength = 128;
    private const int RequiredPasswordHashIterationCount = 220000;

    private readonly FrozenSet<string> passwordBlocklist;

    internal int PasswordHashIterationCount { get; }

    public AccountPolicy(IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        List<string> errors = [];
        int usernameMinimumLength = ReadRequiredInteger(
            configuration,
            UsernameMinimumLengthConfigurationPath,
            errors
        );
        int usernameMaximumLength = ReadRequiredInteger(
            configuration,
            UsernameMaximumLengthConfigurationPath,
            errors
        );
        int passwordMinimumLength = ReadRequiredInteger(
            configuration,
            PasswordMinimumLengthConfigurationPath,
            errors
        );
        int passwordMaximumLength = ReadRequiredInteger(
            configuration,
            PasswordMaximumLengthConfigurationPath,
            errors
        );
        int passwordHashIterationCount = ReadRequiredInteger(
            configuration,
            PasswordHashIterationCountConfigurationPath,
            errors
        );
        FrozenSet<string> configuredPasswordBlocklist = ReadPasswordBlocklist(
            configuration,
            errors
        );

        ValidateFixedInteger(
            UsernameMinimumLengthConfigurationPath,
            usernameMinimumLength,
            RequiredUsernameMinimumLength,
            errors
        );
        ValidateFixedInteger(
            UsernameMaximumLengthConfigurationPath,
            usernameMaximumLength,
            RequiredUsernameMaximumLength,
            errors
        );
        ValidateFixedInteger(
            PasswordMinimumLengthConfigurationPath,
            passwordMinimumLength,
            RequiredPasswordMinimumLength,
            errors
        );
        ValidateFixedInteger(
            PasswordMaximumLengthConfigurationPath,
            passwordMaximumLength,
            RequiredPasswordMaximumLength,
            errors
        );
        ValidateFixedInteger(
            PasswordHashIterationCountConfigurationPath,
            passwordHashIterationCount,
            RequiredPasswordHashIterationCount,
            errors
        );

        if (configuredPasswordBlocklist.Count == 0) {
            errors.Add($"{PasswordBlocklistConfigurationPath} must contain at least one value.");
        }

        ThrowIfInvalid(errors);

        passwordBlocklist = configuredPasswordBlocklist;
        PasswordHashIterationCount = passwordHashIterationCount;
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

    private static FrozenSet<string> ReadPasswordBlocklist(
        IConfiguration configuration,
        List<string> errors
    ) {
        List<string> values = [];
        IConfigurationSection passwordBlocklistSection = configuration.GetSection(PasswordBlocklistConfigurationPath);

        foreach (IConfigurationSection passwordBlocklistEntry in passwordBlocklistSection.GetChildren()) {
            if (!TryNormalize(
                passwordBlocklistEntry.Value,
                NormalizationForm.FormC,
                out string normalizedPassword
            ) || string.IsNullOrWhiteSpace(normalizedPassword)) {
                errors.Add($"{PasswordBlocklistConfigurationPath}:{passwordBlocklistEntry.Key} must be a non-empty Unicode value.");
                continue;
            }

            values.Add(normalizedPassword);
        }

        return values.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private static void ValidateFixedInteger(
        string configurationPath,
        int configuredValue,
        int requiredValue,
        List<string> errors
    ) {
        if (configuredValue != requiredValue) {
            errors.Add($"{configurationPath} must be {requiredValue}.");
        }
    }

    internal static bool TryNormalizeUsername(
        string? value,
        [NotNullWhen(true)] out AccountUsername? accountUsername
    ) {
        accountUsername = null;

        if (!TryNormalize(
            value,
            NormalizationForm.FormKC,
            out string normalizedValue
        )) {
            return false;
        }

        if (normalizedValue.Length is < RequiredUsernameMinimumLength or > RequiredUsernameMaximumLength
            || !IsAsciiLetterOrDigit(normalizedValue[ 0 ])
            || !IsAsciiLetterOrDigit(normalizedValue[ ^1 ])) {
            return false;
        }

        foreach (char character in normalizedValue) {
            if (!IsAllowedUsernameCharacter(character)) {
                return false;
            }
        }

        accountUsername = new AccountUsername(
            normalizedValue,
            normalizedValue.ToUpperInvariant()
        );

        return true;
    }

    internal bool TryNormalizePasswordForRegistration(
        string? value,
        AccountUsername accountUsername,
        out string normalizedPassword
    ) {
        ArgumentNullException.ThrowIfNull(accountUsername);

        normalizedPassword = string.Empty;

        if (!TryNormalizePassword(
            value,
            out string candidatePassword
        )) {
            return false;
        }

        int passwordLength = CountRunes(candidatePassword);

        if (passwordLength < RequiredPasswordMinimumLength
            || passwordLength > RequiredPasswordMaximumLength
            || string.IsNullOrWhiteSpace(candidatePassword)
            || passwordBlocklist.Contains(candidatePassword)
            || string.Equals(
                candidatePassword,
                accountUsername.Value,
                StringComparison.OrdinalIgnoreCase
        )) {
            return false;
        }

        normalizedPassword = candidatePassword;

        return true;
    }

    internal static bool TryNormalizePasswordForVerification(
        string? value,
        out string normalizedPassword
    ) {
        normalizedPassword = string.Empty;

        if (!TryNormalizePassword(
            value,
            out string candidatePassword
        ) || CountRunes(candidatePassword) > RequiredPasswordMaximumLength) {
            return false;
        }

        normalizedPassword = candidatePassword;

        return true;
    }

    private static bool TryNormalizePassword(
        string? value,
        out string normalizedPassword
    ) {
        return TryNormalize(
            value,
            NormalizationForm.FormC,
            out normalizedPassword
        );
    }

    private static bool TryNormalize(
        string? value,
        NormalizationForm normalizationForm,
        out string normalizedValue
    ) {
        normalizedValue = string.Empty;

        if (value is null) {
            return false;
        }

        try {
            normalizedValue = value.Normalize(normalizationForm);
            return true;
        } catch (ArgumentException) {
            return false;
        }
    }

    private static int CountRunes(string value) {
        int count = 0;

        foreach (Rune rune in value.EnumerateRunes()) {
            count++;
        }

        return count;
    }

    private static bool IsAllowedUsernameCharacter(char character) {
        return IsAsciiLetterOrDigit(character)
            || character is '.' or '-' or '_';
    }

    private static bool IsAsciiLetterOrDigit(char character) {
        return character is >= 'A' and <= 'Z'
            or >= 'a' and <= 'z'
            or >= '0' and <= '9';
    }

    private static void ThrowIfInvalid(List<string> errors) {
        if (errors.Count == 0) {
            return;
        }

        throw new InvalidOperationException(
            $"The DataAccess account configuration is invalid: {string.Join(" ", errors)}"
        );
    }
}
