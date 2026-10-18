using System.Text.RegularExpressions;

namespace Tekus.Domain.Common;

/// <summary>
/// E-mail address as a value object: normalised to lower case and validated once, so no other
/// part of the domain has to re-implement the rule.
/// </summary>
public sealed partial class EmailAddress : ValueObject
{
    public const int MaxLength = 200;
    public const string Pattern = @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$";

    /// <summary>Required by EF Core's value converter.</summary>
    private EmailAddress() => Value = string.Empty;

    private EmailAddress(string value) => Value = value;

    public string Value { get; private set; }

    public static EmailAddress Create(string? value)
    {
        var normalized = Guard.NotNullOrWhiteSpace(value, "email", MaxLength).ToLowerInvariant();

        return PatternRegex().IsMatch(normalized)
            ? new EmailAddress(normalized)
            : throw new DomainException($"'{value}' is not a valid e-mail address.", ErrorCodes.Validation);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(Pattern, RegexOptions.CultureInvariant)]
    private static partial Regex PatternRegex();
}
