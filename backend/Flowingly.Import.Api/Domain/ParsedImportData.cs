namespace Flowingly.Import.Api.Domain;

/// <summary>
/// Holds all fields extracted from the raw input text.
/// Fields are nullable — absence is handled by validation and defaulting rules.
/// </summary>
public sealed class ParsedImportData
{
    public string? CostCentre { get; init; }
    public string? Total { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Vendor { get; init; }
    public string? Description { get; init; }
    public string? Date { get; init; }
}
