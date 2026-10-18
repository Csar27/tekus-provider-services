namespace Tekus.Application.Countries.Dtos;

/// <summary>Country of the external catalogue, as stored in the local snapshot.</summary>
public sealed record CountryDto(
    string Code,
    string Name,
    string Region,
    string? CurrencyCode,
    string? Alpha3Code,
    DateTime LastSyncedAtUtc);
