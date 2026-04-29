namespace Flowingly.Import.Api.Contracts;

public sealed class ParseResponse
{
    public bool Success { get; init; }
    public ParsedDataDto? Data { get; init; }
    public MetadataDto Metadata { get; init; } = new();
    public IReadOnlyList<ValidationErrorDto> Errors { get; init; } = [];
}

public sealed class ParsedDataDto
{
    public string CostCentre { get; init; } = string.Empty;
    public decimal TotalIncludingTax { get; init; }
    public decimal TotalExcludingTax { get; init; }
    public decimal SalesTax { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Vendor { get; init; }
    public string? Description { get; init; }
    public string? Date { get; init; }
}

public sealed class MetadataDto
{
    public string Parser { get; init; } = "deterministic";
    public string WorkflowClassification { get; init; } = "unknown";
    public bool AiExtensionReady { get; init; } = true;
}

public sealed class ValidationErrorDto
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
