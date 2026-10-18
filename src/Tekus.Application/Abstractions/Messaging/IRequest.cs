namespace Tekus.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for anything that can be sent through the <c>ISender</c> dispatcher:
/// a command (write) or a query (read).
/// </summary>
/// <typeparam name="TResult">Type returned by the handler.</typeparam>
public interface IRequest<TResult>
{
}
