namespace Flowingly.Import.Api.Contracts;

public sealed class ParseRequest
{
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Optional tax rate as a percentage (e.g. 15 for 15%).
    /// Defaults to 15 when omitted or null.
    /// Must be greater than 0 and no greater than 100.
    /// </summary>
    public decimal? TaxRatePercent { get; init; }
}
