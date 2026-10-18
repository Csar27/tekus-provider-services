using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Providers.Dtos;

namespace Tekus.Application.Providers.Commands;

/// <summary>Input shape of a custom field sent by the client.</summary>
public sealed record ProviderCustomFieldInput(string Key, string Value);

/// <summary>Creates a provider, optionally with its custom fields and offered services.</summary>
public sealed record CreateProviderCommand(
    string Nit,
    string Name,
    string Email,
    IReadOnlyCollection<ProviderCustomFieldInput>? CustomFields = null,
    IReadOnlyCollection<Guid>? ServiceIds = null) : IRequest<ProviderDetailDto>;

/// <summary>
/// Edits the data of a provider.
/// <paramref name="ExpectedVersion"/> is filled from the <c>If-Match</c> header: when it does not
/// match the stored version the command fails with <c>409 Conflict</c> instead of silently
/// overwriting another user's change.
/// </summary>
public sealed record UpdateProviderCommand(
    Guid Id,
    string Nit,
    string Name,
    string Email,
    long? ExpectedVersion = null) : IRequest<ProviderDetailDto>;

/// <summary>Activates or deactivates a provider (logical delete / restore).</summary>
public sealed record ChangeProviderStatusCommand(Guid Id, bool IsActive) : IRequest<ProviderDetailDto>;

/// <summary>Logically deletes a provider. Offerings are kept for historical reporting.</summary>
public sealed record DeleteProviderCommand(Guid Id) : IRequest<Unit>;

/// <summary>Creates or updates one custom field of the provider.</summary>
public sealed record UpsertProviderCustomFieldCommand(Guid ProviderId, string Key, string Value)
    : IRequest<ProviderDetailDto>;

/// <summary>Removes one custom field of the provider.</summary>
public sealed record RemoveProviderCustomFieldCommand(Guid ProviderId, string Key) : IRequest<Unit>;

/// <summary>Replaces the whole set of custom fields (an empty list clears them all).</summary>
public sealed record ReplaceProviderCustomFieldsCommand(
    Guid ProviderId,
    IReadOnlyCollection<ProviderCustomFieldInput>? Fields) : IRequest<ProviderDetailDto>;

/// <summary>Replaces the whole set of services offered by the provider.</summary>
public sealed record ReplaceProviderServicesCommand(
    Guid ProviderId,
    IReadOnlyCollection<Guid>? ServiceIds) : IRequest<ProviderDetailDto>;
