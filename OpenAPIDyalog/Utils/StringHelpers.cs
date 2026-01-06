namespace OpenAPIDyalog.Utils;

/// <summary>
/// Helper methods for string manipulation in templates.
/// </summary>
public static class StringHelpers
{
    /// <summary>
    /// Prefixes each line of a multiline string with the APL lamp symbol (⍝).
    /// </summary>
    /// <param name="text">The text to comment.</param>
    /// <returns>The text with each line commented.</returns>
    public static string CommentLines(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join("\n", lines.Select(line => "⍝ " + line));
    }
}
