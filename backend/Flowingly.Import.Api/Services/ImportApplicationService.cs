using Flowingly.Import.Api.Contracts;
using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Orchestrates the full import parsing pipeline:
/// parse → validate → resolve defaults → calculate tax → classify → map to response.
/// </summary>
public sealed class ImportApplicationService : IImportApplicationService
{
    private readonly IMarkupParser _parser;
    private readonly IImportValidator _validator;
    private readonly ITaxCalculator _taxCalculator;
    private readonly IWorkflowInsightBuilder _insightBuilder;

    public ImportApplicationService(
        IMarkupParser parser,
        IImportValidator validator,
        ITaxCalculator taxCalculator,
        IWorkflowInsightBuilder insightBuilder)
    {
        _parser = parser;
        _validator = validator;
        _taxCalculator = taxCalculator;
        _insightBuilder = insightBuilder;
    }

    public ParseResponse Parse(string text, decimal? taxRate = null)
    {
        // Step 1: extract fields from raw text.
        var parsed = _parser.TryParse(text, out var parseErrors);

        // Step 2: if the parser found unmatched tags, reject immediately.
        if (parsed is null)
        {
            return FailureResponse(parseErrors, workflowClassification: "unknown");
        }

        // Step 3: apply business validation rules.
        var validationErrors = _validator.Validate(parsed);
        if (validationErrors.Count > 0)
        {
            return FailureResponse(validationErrors, workflowClassification: "unknown");
        }

        // Step 4: resolve cost centre default (missing → "UNKNOWN").
        var costCentre = _validator.ResolveCostCentre(parsed);

        // Step 5: calculate tax — Total is guaranteed non-null and valid at this point.
        // Convert percentage (e.g. 15) to fractional rate (0.15) when provided.
        var fractionalRate = taxRate.HasValue ? taxRate.Value / 100m : (decimal?)null;
        var tax = _taxCalculator.Calculate(parsed.Total!, fractionalRate);

        // Step 6: classify workflow.
        var classification = _insightBuilder.Classify(parsed);

        // Step 7: map to response DTO.
        return new ParseResponse
        {
            Success = true,
            Data = new ParsedDataDto
            {
                CostCentre        = costCentre,
                TotalIncludingTax = tax.TotalIncludingTax,
                TotalExcludingTax = tax.TotalExcludingTax,
                SalesTax          = tax.SalesTax,
                PaymentMethod     = parsed.PaymentMethod,
                Vendor            = parsed.Vendor,
                Description       = parsed.Description,
                Date              = parsed.Date
            },
            Metadata = new MetadataDto
            {
                Parser                 = "deterministic",
                WorkflowClassification = classification,
                AiExtensionReady       = true
            },
            Errors = []
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ParseResponse FailureResponse(
        IReadOnlyList<ValidationError> errors,
        string workflowClassification)
    {
        return new ParseResponse
        {
            Success = false,
            Data    = null,
            Metadata = new MetadataDto
            {
                Parser                 = "deterministic",
                WorkflowClassification = workflowClassification,
                AiExtensionReady       = true
            },
            Errors = errors
                .Select(e => new ValidationErrorDto { Code = e.Code, Message = e.Message })
                .ToList()
        };
    }
}
