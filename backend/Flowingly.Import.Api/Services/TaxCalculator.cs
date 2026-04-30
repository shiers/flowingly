using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Calculates tax-inclusive breakdown.
/// Defaults to 15% when no rate is supplied.
/// Responsible only for arithmetic — no parsing, no validation.
/// </summary>
public sealed class TaxCalculator : ITaxCalculator
{
    private const decimal DefaultTaxRate = 0.15m;

    public TaxResult Calculate(string rawTotal, decimal? taxRate = null)
    {
        var rate = taxRate ?? DefaultTaxRate;

        // Strip commas to support values like "35,000".
        var normalised = rawTotal.Replace(",", string.Empty);

        // Caller is responsible for ensuring rawTotal is valid before calling.
        // Parse will throw if the value is not numeric — this is intentional.
        var totalIncludingTax = decimal.Parse(normalised);

        var totalExcludingTax = Math.Round(totalIncludingTax / (1 + rate), 2);
        var salesTax = Math.Round(totalIncludingTax - totalExcludingTax, 2);

        return new TaxResult
        {
            TotalIncludingTax = totalIncludingTax,
            TotalExcludingTax = totalExcludingTax,
            SalesTax = salesTax
        };
    }
}
