using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

public interface IImportValidator
{
    /// <summary>
    /// Validates the extracted data against business rules.
    /// Returns a list of validation errors. An empty list means the data is valid.
    /// </summary>
    IReadOnlyList<ValidationError> Validate(ParsedImportData data);

    /// <summary>
    /// Returns the cost centre value to use in the response.
    /// Defaults to "UNKNOWN" when the extracted value is absent or blank.
    /// </summary>
    string ResolveCostCentre(ParsedImportData data);
}
