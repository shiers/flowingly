using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Applies business validation rules to extracted import data.
/// Responsible for: missing/invalid total, unmatched tags.
/// Not responsible for: tax calculation, field defaulting beyond cost_centre.
/// </summary>
public sealed class ImportValidator : IImportValidator
{
    private const string UnknownCostCentre = "UNKNOWN";

    public IReadOnlyList<ValidationError> Validate(ParsedImportData data)
    {
        var errors = new List<ValidationError>();

        // Rule 1: total must be present and non-blank.
        if (string.IsNullOrWhiteSpace(data.Total))
        {
            errors.Add(new ValidationError
            {
                Code = "MISSING_TOTAL",
                Message = "The required <total> field was not found."
            });

            // No point checking numeric validity if the value is absent.
            return errors;
        }

        // Rule 2: total must be a valid positive number.
        // Strip commas to support values like "35,000".
        var normalised = data.Total.Replace(",", string.Empty);
        if (!decimal.TryParse(normalised, out var parsed) || parsed <= 0)
        {
            errors.Add(new ValidationError
            {
                Code = "INVALID_TOTAL",
                Message = $"The <total> value \"{data.Total}\" is not a valid numeric amount."
            });
        }

        return errors;
    }

    public string ResolveCostCentre(ParsedImportData data) =>
        string.IsNullOrWhiteSpace(data.CostCentre) ? UnknownCostCentre : data.CostCentre;
}
