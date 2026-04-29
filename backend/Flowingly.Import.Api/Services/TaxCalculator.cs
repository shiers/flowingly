using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Calculates GST-inclusive tax breakdown.
/// Assumes a fixed 15% GST rate applied to a tax-inclusive total.
/// Responsible only for arithmetic — no parsing, no validation.
/// </summary>
public sealed class TaxCalculator : ITaxCalculator
{
    private const decimal GstRate = 0.15m;

    public TaxResult Calculate(string rawTotal)
    {
        // Strip commas to support values like "35,000".
        var normalised = rawTotal.Replace(",", string.Empty);

        // Caller is responsible for ensuring rawTotal is valid before calling.
        // Parse will throw if the value is not numeric — this is intentional.
        var totalIncludingTax = decimal.Parse(normalised);

        var totalExcludingTax = Math.Round(totalIncludingTax / (1 + GstRate), 2);
        var salesTax = Math.Round(totalIncludingTax - totalExcludingTax, 2);

        return new TaxResult
        {
            TotalIncludingTax = totalIncludingTax,
            TotalExcludingTax = totalExcludingTax,
            SalesTax = salesTax
        };
    }
}
