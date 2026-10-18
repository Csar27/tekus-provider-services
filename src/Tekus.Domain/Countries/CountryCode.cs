using Tekus.Domain.Common;

namespace Tekus.Domain.Countries;

/// <summary>
/// ISO 3166-1 alpha-2 country code (e.g. <c>CO</c>, <c>PE</c>, <c>MX</c>).
/// A value object guarantees that whatever is stored in a service offering (or sent to the
/// database) is a well formed, upper-cased two letter code.
/// </summary>
public sealed class CountryCode : ValueObject
{
    public const int Length = 2;

    /// <summary>Required by EF Core's value converter.</summary>
    private CountryCode() => Value = string.Empty;

    private CountryCode(string value) => Value = value;

    public string Value { get; private set; }

    public static CountryCode Create(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(normalized)
            || normalized.Length != Length
            || !normalized.All(char.IsAsciiLetter))
        {
            throw new DomainException(
                $"'{value}' is not a valid ISO 3166-1 alpha-2 country code.",
                ErrorCodes.InvalidCountryCode);
        }

        return new CountryCode(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
