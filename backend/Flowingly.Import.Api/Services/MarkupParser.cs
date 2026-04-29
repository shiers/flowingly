using System.Text.RegularExpressions;
using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Extracts XML-style tagged values from raw text.
/// Handles both embedded blocks (inside &lt;expense&gt;...&lt;/expense&gt;) and inline tags.
/// Focused solely on extraction and tag-pair integrity — no validation, no tax logic.
/// </summary>
public sealed class MarkupParser : IMarkupParser
{
    // Matches <tagname>content</tagname> where content contains no nested tags.
    // Running this in a loop processes innermost pairs first, then outer wrappers.
    private static readonly Regex InnerTagPairPattern = new(
        @"<([a-z][a-z0-9_]*)>([^<]*)</\1>",
        RegexOptions.Compiled);

    // Matches any remaining opening tag after all matched pairs have been removed.
    private static readonly Regex OpeningTagPattern = new(
        @"<([a-z][a-z0-9_]*)>",
        RegexOptions.Compiled);

    // Tags that act as structural containers — unmatched instances are not errors.
    private static readonly HashSet<string> ContainerTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "expense"
    };

    public ParsedImportData? TryParse(string text, out IReadOnlyList<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            errors = [];
            return new ParsedImportData();
        }

        var extracted = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Step 1: iteratively extract innermost tag pairs until no more matches exist.
        // Each pass removes leaf-level pairs, exposing the next layer for the next pass.
        var remaining = text;
        string previous;
        do
        {
            previous = remaining;
            remaining = InnerTagPairPattern.Replace(remaining, match =>
            {
                var tagName = match.Groups[1].Value;
                var value = match.Groups[2].Value.Trim();

                // Last occurrence wins if the same tag appears more than once.
                extracted[tagName] = value;

                return string.Empty;
            });
        }
        while (remaining != previous);

        // Step 2: scan remaining text for unmatched opening tags.
        var unmatchedErrors = new List<ValidationError>();
        foreach (Match openTag in OpeningTagPattern.Matches(remaining))
        {
            var tagName = openTag.Groups[1].Value;

            if (ContainerTags.Contains(tagName))
                continue;

            unmatchedErrors.Add(new ValidationError
            {
                Code = "UNMATCHED_TAG",
                Message = $"Opening tag <{tagName}> does not have a corresponding closing tag."
            });
        }

        if (unmatchedErrors.Count > 0)
        {
            errors = unmatchedErrors;
            return null;
        }

        errors = [];

        // Step 3: map extracted values to the domain model.
        return new ParsedImportData
        {
            CostCentre    = extracted.GetValueOrDefault("cost_centre"),
            Total         = extracted.GetValueOrDefault("total"),
            PaymentMethod = extracted.GetValueOrDefault("payment_method"),
            Vendor        = extracted.GetValueOrDefault("vendor"),
            Description   = extracted.GetValueOrDefault("description"),
            Date          = extracted.GetValueOrDefault("date")
        };
    }
}
