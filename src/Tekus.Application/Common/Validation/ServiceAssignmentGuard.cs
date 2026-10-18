using Tekus.Application.Common.Exceptions;
using Tekus.Domain.Services;

namespace Tekus.Application.Common.Validation;

/// <summary>
/// Checks that the services referenced by a command really exist before the aggregate links them.
/// Doing it here (instead of relying on the foreign key) produces a helpful 400 response that
/// lists the unknown identifiers.
/// </summary>
internal static class ServiceAssignmentGuard
{
    public static async Task EnsureExistAsync(
        IServiceRepository services,
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken)
    {
        if (serviceIds.Count == 0)
        {
            return;
        }

        var existing = await services.GetExistingIdsAsync(serviceIds, cancellationToken);

        var missing = serviceIds
            .Where(id => !existing.Contains(id))
            .Distinct()
            .ToArray();

        if (missing.Length > 0)
        {
            throw new ValidationException(
                "serviceIds",
                $"The following services do not exist: {string.Join(", ", missing)}.");
        }
    }
}
