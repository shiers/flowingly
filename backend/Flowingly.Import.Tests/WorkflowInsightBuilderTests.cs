using FluentAssertions;
using Flowingly.Import.Api.Domain;
using Flowingly.Import.Api.Services;
using Xunit;

namespace Flowingly.Import.Tests;

public sealed class WorkflowInsightBuilderTests
{
    private readonly WorkflowInsightBuilder _builder = new();

    [Fact]
    public void Classify_TotalPresent_ReturnsExpenseClaim()
    {
        var data = new ParsedImportData { Total = "35000" };

        var result = _builder.Classify(data);

        result.Should().Be("expense_claim");
    }

    [Fact]
    public void Classify_TotalMissing_ReturnsUnknown()
    {
        var data = new ParsedImportData { Total = null };

        var result = _builder.Classify(data);

        result.Should().Be("unknown");
    }

    [Fact]
    public void Classify_TotalBlank_ReturnsUnknown()
    {
        var data = new ParsedImportData { Total = "   " };

        var result = _builder.Classify(data);

        result.Should().Be("unknown");
    }
}
