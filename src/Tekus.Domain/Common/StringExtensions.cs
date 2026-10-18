namespace Tekus.Domain.Common;

/// <summary>Small string helpers shared by the domain invariants.</summary>
public static class StringExtensions
{
    /// <summary>
    /// Collapses consecutive white spaces into a single one and trims the result:
    /// "  Cantidad   de mascotas " becomes "Cantidad de mascotas".
    /// </summary>
    public static string SquashInnerSpaces(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var builder = new System.Text.StringBuilder(value.Length);
        var previousWasSpace = false;

        foreach (var character in value)
        {
            var isSpace = char.IsWhiteSpace(character);

            if (isSpace)
            {
                if (!previousWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                }
            }
            else
            {
                builder.Append(character);
            }

            previousWasSpace = isSpace;
        }

        var result = builder.ToString();
        return result.EndsWith(' ') ? result[..^1] : result;
    }
}
