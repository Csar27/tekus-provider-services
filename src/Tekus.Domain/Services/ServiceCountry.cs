using Tekus.Domain.Common;
using Tekus.Domain.Countries;

namespace Tekus.Domain.Services;

/// <summary>
/// Join entity: a country where a <see cref="Service"/> is offered.
/// Belongs to the service aggregate; the country itself is a projection of the external
/// catalogue (see <c>ICountryCatalog</c>).
/// </summary>
public sealed class ServiceCountry : Entity
{
    /// <summary>Required by EF Core.</summary>
    private ServiceCountry()
    {
    }

    private ServiceCountry(Guid id, Guid serviceId, CountryCode countryCode, DateTime utcNow)
        : base(id)
    {
        ServiceId = serviceId;
        CountryCode = countryCode;
        AvailableSinceUtc = utcNow;
    }

    public Guid ServiceId { get; private set; }

    public CountryCode CountryCode { get; private set; } = null!;

    public DateTime AvailableSinceUtc { get; private set; }

    internal static ServiceCountry Offer(Guid serviceId, CountryCode countryCode, DateTime utcNow)
        => new(Guid.NewGuid(), serviceId, countryCode, utcNow);
}
