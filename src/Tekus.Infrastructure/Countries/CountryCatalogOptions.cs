namespace Tekus.Infrastructure.Countries;

/// <summary>
/// Settings of the external countries catalogue.
/// </summary>
public sealed class CountryCatalogOptions
{
    public const string SectionName = "CountryCatalog";

    /// <summary>Base URL of the external REST service.</summary>
    public string BaseUrl { get; set; } = "https://restcountries.com/v3.1";

    /// <summary>How long the local snapshot is considered fresh before a new sync is attempted.</summary>
    public int CacheDurationMinutes { get; set; } = 1440;

    /// <summary>Fields requested from the external service (keeps the payload small).</summary>
    public string Fields { get; set; } = "name,cca2,cca3,region,currencies";

    /// <summary>
    /// When <c>false</c> the adapter never calls the external service and only serves the local
    /// snapshot. Useful for demo / offline environments and for the integration test suite.
    /// </summary>
    public bool UseExternalService { get; set; } = true;
}
