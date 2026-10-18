using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Common.Exceptions;
using Tekus.Domain.Countries;

namespace Tekus.Application.Common.Validation;

/// <summary>
/// Normalises the country codes sent by the client and verifies them against the external
/// catalogue snapshot. If the snapshot is unavailable the codes are accepted as-is: the foreign
/// key between <c>ServiceCountries</c> and <c>Countries</c> remains the last line of defence.
/// </summary>
internal static class CountryAssignmentGuard
{
    public static async Task<IReadOnlyCollection<string>> NormalizeAndEnsureExistAsync(
        ICountryCatalog catalog,
        IReadOnlyCollection<string>? countryCodes,
        CancellationToken cancellationToken)
    {
        var normalized = (countryCodes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(CountryCode.Create)
            .Select(code => code.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            return normalized;
        }

        var available = await catalog.GetAvailableCodesAsync(cancellationToken);

        if (available.Count == 0)
        {
            return normalized;
        }

        var unknown = normalized
            .Where(code => !available.Contains(code))
            .ToArray();

        if (unknown.Length > 0)
        {
            throw new ValidationException(
                "countryCodes",
                $"The following country codes are not part of the catalogue: {string.Join(", ", unknown)}. " +
                "Use GET /api/v1/countries to list the valid ISO 3166-1 alpha-2 codes.");
        }

        return normalized;
    }
}
