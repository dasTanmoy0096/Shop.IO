namespace DataAccess.Configuration;

using System;

internal sealed class AccountUsername {
    internal string Value { get; }
    internal string NormalizedValue { get; }

    internal AccountUsername(
        string value,
        string normalizedValue
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedValue);

        Value = value;
        NormalizedValue = normalizedValue;
    }
}
