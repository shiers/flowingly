namespace Flowingly.Import.Api.Domain;

public sealed class TaxResult
{
    public decimal TotalIncludingTax { get; init; }
    public decimal TotalExcludingTax { get; init; }
    public decimal SalesTax { get; init; }
}
