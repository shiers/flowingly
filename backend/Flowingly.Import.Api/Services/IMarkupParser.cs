using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

public interface IMarkupParser
{
    /// <summary>
    /// Attempts to extract all known XML-style fields from the raw input text.
    /// Returns null and populates errors if the input is structurally invalid
    /// (e.g. unmatched tags).
    /// </summary>
    ParsedImportData? TryParse(string text, out IReadOnlyList<ValidationError> errors);
}
