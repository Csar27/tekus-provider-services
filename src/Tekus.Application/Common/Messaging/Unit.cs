namespace Tekus.Application.Common.Messaging;

/// <summary>
/// Result of a command that has nothing meaningful to return (e.g. the deletion of a resource).
/// Avoids using <c>Task</c> as a generic argument in the dispatcher contract.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value => default;
}
