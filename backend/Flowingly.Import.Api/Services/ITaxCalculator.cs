using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

public interface ITaxCalculator
{
    /// <summary>
    /// Calculates GST-inclusive tax breakdown from a raw total string.
    /// Handles comma-formatted values (e.g. "35,000").
    /// </summary>
    TaxResult Calculate(string rawTotal);
}
