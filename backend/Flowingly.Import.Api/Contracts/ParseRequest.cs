using System.ComponentModel.DataAnnotations;

namespace Flowingly.Import.Api.Contracts;

public sealed class ParseRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Text must not be empty.")]
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Optional tax rate as a percentage (e.g. 15 for 15%).
    /// Defaults to 15 when omitted or null.
    /// Must be greater than 0 and no greater than 100.
    /// </summary>
    [Range(0.01, 100, ErrorMessage = "TaxRatePercent must be between 0.01 and 100.")]
    public decimal? TaxRatePercent { get; init; }
}
