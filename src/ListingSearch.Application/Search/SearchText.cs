using System.Text;

namespace ListingSearch.Application.Search;

/// <summary>Text normalisation shared by validation and filtering, so both agree on what the input means.</summary>
internal static class SearchText
{
    /// <summary>Trims and collapses inner whitespace. Returns null for null, empty or whitespace-only input.</summary>
    public static string? NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Splits text into lower-case words made of letters and digits. Punctuation separates words,
    /// so "top-floor" becomes ["top", "floor"] and "Metro." becomes ["metro"].
    /// </summary>
    public static IReadOnlyList<string> Tokenize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var tokens = new List<string>();
        var current = new StringBuilder();

        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                current.Append(char.ToLowerInvariant(c));
            }
            else if (current.Length > 0)
            {
                tokens.Add(current.ToString());
                current.Clear();
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
