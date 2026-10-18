using Tekus.Domain.Common;

namespace Tekus.Domain.Providers;

/// <summary>
/// User defined field attached to a provider (requirement #3 of the challenge).
/// It is an entity *inside* the provider aggregate: it has no repository and can only be
/// changed through <see cref="Provider.UpsertCustomField"/> or <see cref="Provider.RemoveCustomField"/>.
/// </summary>
public sealed class ProviderCustomField : Entity
{
    public const int KeyMaxLength = 60;
    public const int ValueMaxLength = 500;

    /// <summary>Required by EF Core.</summary>
    private ProviderCustomField()
    {
    }

    private ProviderCustomField(Guid id, Guid providerId, string key, string value, DateTime utcNow)
        : base(id)
    {
        ProviderId = providerId;
        Key = key;
        Value = value;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid ProviderId { get; private set; }

    /// <summary>Display name of the field, unique per provider.</summary>
    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    internal static ProviderCustomField Create(Guid providerId, string key, string value, DateTime utcNow)
        => new(Guid.NewGuid(), providerId, key, value, utcNow);

    internal void UpdateValue(string value, DateTime utcNow)
    {
        Value = value;
        UpdatedAtUtc = utcNow;
    }

    internal static string NormalizeValue(string? value, string key)
        => Guard.NotNullOrWhiteSpace(value, $"customFields['{key}'].value", ValueMaxLength)
            .SquashInnerSpaces();
}
