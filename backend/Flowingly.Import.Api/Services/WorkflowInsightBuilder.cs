using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

/// <summary>
/// Derives a simple workflow classification from extracted import data.
/// Deterministic — no AI, no external calls.
/// </summary>
public sealed class WorkflowInsightBuilder : IWorkflowInsightBuilder
{
    public string Classify(ParsedImportData data) =>
        string.IsNullOrWhiteSpace(data.Total) ? "unknown" : "expense_claim";
}
