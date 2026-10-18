namespace Tekus.Domain.Common;

/// <summary>
/// Raised when a business invariant is broken. It is a *expected* failure (as opposed to a bug),
/// therefore the API translates it into a <c>400 Bad Request</c> / <c>409 Conflict</c> problem
/// details response instead of a 500 (see <c>GlobalExceptionHandler</c>).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message, string code = ErrorCodes.DomainRuleViolation)
        : base(message) => Code = code;

    public DomainException(string message, Exception innerException, string code = ErrorCodes.DomainRuleViolation)
        : base(message, innerException) => Code = code;

    /// <summary>Stable, machine-readable error code that clients can branch on.</summary>
    public string Code { get; }
}

/// <summary>Machine-readable error codes used across the business rules.</summary>
public static class ErrorCodes
{
    public const string DomainRuleViolation = "domain_rule_violation";
    public const string Validation = "validation_error";
    public const string NotFound = "resource_not_found";
    public const string Conflict = "resource_conflict";
    public const string InvalidCredentials = "invalid_credentials";
    public const string AccountLocked = "account_locked";
    public const string InvalidCountryCode = "invalid_country_code";
    public const string ExternalServiceUnavailable = "external_service_unavailable";
}
