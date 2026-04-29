using Flowingly.Import.Api.Domain;

namespace Flowingly.Import.Api.Services;

public interface IWorkflowInsightBuilder
{
    /// <summary>
    /// Derives a workflow classification string from the extracted data.
    /// Returns "unknown" when classification cannot be determined.
    /// </summary>
    string Classify(ParsedImportData data);
}
