using Tekus.Domain.Common;

namespace Tekus.Domain.Countries;

/// <summary>
/// Country available for a service offering. Its identity is the ISO alpha-2 code (a natural key)
/// instead of a surrogate Guid, so no extra column is needed to join offerings with countries.
/// </summary>
public sealed class Country : IEquatable<Country>
{
    private Country()
    {
        Code = null!;
        Name = string.Empty;
        Region = string.Empty;
    }

    private Country(CountryCode code, string name, string region, string? currencyCode, string? alpha3Code, DateTime syncedAtUtc)
    {
        Code = code;
        Name = name;
        Region = region;
        CurrencyCode = currencyCode;
        Alpha3Code = alpha3Code;
        LastSyncedAtUtc = syncedAtUtc;
    }

    /// <summary>ISO 3166-1 alpha-2 code, used as the primary key.</summary>
    public CountryCode Code { get; private set; }

    public string Name { get; private set; }

    /// <summary>Region supplied by the external catalog (e.g. <c>Americas</c>).</summary>
    public string Region { get; private set; }

    public string? CurrencyCode { get; private set; }

    public string? Alpha3Code { get; private set; }

    /// <summary>When this row was last reconciled with the external catalog.</summary>
    public DateTime LastSyncedAtUtc { get; private set; }

    public static Country Create(
        string? code,
        string? name,
        string? region,
        string? currencyCode,
        string? alpha3Code,
        DateTime syncedAtUtc)
        => new(
            CountryCode.Create(code),
            Guard.NotNullOrWhiteSpace(name, "name", maxLength: 120),
            string.IsNullOrWhiteSpace(region) ? "Unknown" : Guard.NotNullOrWhiteSpace(region, "region", maxLength: 80),
            string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(alpha3Code) ? null : alpha3Code.Trim().ToUpperInvariant(),
            syncedAtUtc);

    /// <summary>Refreshes the data coming from the external catalog.</summary>
    public void Sync(string? name, string? region, string? currencyCode, string? alpha3Code, DateTime syncedAtUtc)
    {
        Name = Guard.NotNullOrWhiteSpace(name, "name", maxLength: 120);
        Region = string.IsNullOrWhiteSpace(region) ? Region : Guard.NotNullOrWhiteSpace(region, "region", maxLength: 80);
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? CurrencyCode : currencyCode.Trim().ToUpperInvariant();
        Alpha3Code = string.IsNullOrWhiteSpace(alpha3Code) ? Alpha3Code : alpha3Code.Trim().ToUpperInvariant();
        LastSyncedAtUtc = syncedAtUtc;
    }

    public bool Equals(Country? other) => other is not null && Code == other.Code;

    public override bool Equals(object? obj) => Equals(obj as Country);

    public override int GetHashCode() => Code.GetHashCode();
}
