using System.Globalization;

namespace Tekus.Domain.Common;

/// <summary>
/// Small, allocation-friendly argument guard used by the domain factories.
/// Every failure is a <see cref="DomainException"/>, never an <see cref="ArgumentException"/>,
/// because it represents a broken business rule that clients must see as a 4xx response.
/// </summary>
public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string parameterName, int maxLength = 0, int minLength = 1)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"'{parameterName}' is required.", ErrorCodes.Validation);
        }

        var trimmed = value.Trim();

        if (trimmed.Length < minLength)
        {
            throw new DomainException(
                $"'{parameterName}' must have at least {minLength} characters.",
                ErrorCodes.Validation);
        }

        if (maxLength > 0 && trimmed.Length > maxLength)
        {
            throw new DomainException(
                $"'{parameterName}' must not exceed {maxLength} characters.",
                ErrorCodes.Validation);
        }

        return trimmed;
    }

    public static T NotNull<T>(T? value, string parameterName)
        where T : class
        => value ?? throw new DomainException($"'{parameterName}' is required.", ErrorCodes.Validation);

    public static Guid NotEmpty(Guid value, string parameterName)
        => value != Guid.Empty
            ? value
            : throw new DomainException($"'{parameterName}' is required.", ErrorCodes.Validation);

    public static decimal Positive(decimal value, string parameterName, decimal maxValue)
    {
        if (value <= 0m)
        {
            throw new DomainException($"'{parameterName}' must be greater than zero.", ErrorCodes.Validation);
        }

        return value > maxValue
            ? throw new DomainException(
                $"'{parameterName}' must not exceed {maxValue.ToString("N2", CultureInfo.InvariantCulture)}.",
                ErrorCodes.Validation)
            : value;
    }

    public static int InRange(int value, int min, int max, string parameterName)
        => value >= min && value <= max
            ? value
            : throw new DomainException(
                $"'{parameterName}' must be between {min} and {max}.",
                ErrorCodes.Validation);
}
