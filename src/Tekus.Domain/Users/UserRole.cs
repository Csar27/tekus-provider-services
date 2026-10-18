namespace Tekus.Domain.Users;

/// <summary>
/// Roles available out of the box. There is no user-management API (the challenge explicitly
/// says it is not required), so users are provisioned by the SQL seed script / the dev seeder.
/// </summary>
public enum UserRole
{
    /// <summary>Read and write access to every resource.</summary>
    Admin = 1,

    /// <summary>Read-only access: can query providers, services, countries and the dashboard.</summary>
    Analyst = 2,
}
