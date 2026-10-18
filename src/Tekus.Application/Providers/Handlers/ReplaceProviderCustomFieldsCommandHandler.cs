using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>
/// Replaces the whole set of custom fields of a provider: useful for the "edit provider" screen,
/// which submits the complete form in a single request.
/// </summary>
internal sealed class ReplaceProviderCustomFieldsCommandHandler
    : IRequestHandler<ReplaceProviderCustomFieldsCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ReplaceProviderCustomFieldsCommandHandler(
        IProviderRepository providers,
        IProviderReadRepository readRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providers = providers;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<ProviderDetailDto> HandleAsync(
        ReplaceProviderCustomFieldsCommand request,
        CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.ProviderId, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.ProviderId);

        // Duplicated keys (case-insensitive) are collapsed keeping the last value sent.
        var target = (request.Fields ?? [])
            .GroupBy(field => field.Key?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToArray();

        var obsoleteKeys = provider.CustomFields
            .Select(field => field.Key)
            .Except(target.Select(field => field.Key.Trim()), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var now = _timeProvider.UtcNow();

        foreach (var key in obsoleteKeys)
        {
            provider.RemoveCustomField(key, now);
        }

        foreach (var field in target)
        {
            provider.UpsertCustomField(field.Key, field.Value, now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
