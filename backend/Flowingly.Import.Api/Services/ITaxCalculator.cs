using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

public interface ITaxCalculator
{
    /// <summary>
    /// Calculates tax-inclusive breakdown from a raw total string.
    /// Handles comma-formatted values (e.g. "35,000").
    /// <paramref name="taxRate"/> is a fractional rate (e.g. 0.15 for 15%).
    /// When null, the default rate of 15% is used.
    /// </summary>
    TaxResult Calculate(string rawTotal, decimal? taxRate = null);
}
