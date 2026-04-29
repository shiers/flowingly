using FluentAssertions;
using Flowingly.Import.Api.Services;
using Xunit;

namespace Flowingly.Import.Tests;

/// <summary>
/// Integration-style tests for the full parsing pipeline.
/// Uses real implementations — all services are pure functions with no I/O.
/// </summary>
public sealed class ImportApplicationServiceTests
{
    private readonly ImportApplicationService _service = new(
        new MarkupParser(),
        new ImportValidator(),
        new TaxCalculator(),
        new WorkflowInsightBuilder());

    // -------------------------------------------------------------------------
    // Full successful flow
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_FullValidInput_ReturnsSuccessResponse()
    {
        const string input = """
            Hi Patricia, please create an expense claim
            <expense>
                <cost_centre>DEV632</cost_centre>
                <total>35,000</total>
                <payment_method>personal card</payment_method>
            </expense>
            <vendor>Seaside Steakhouse</vendor>
            <description>development team's project end celebration</description>
            <date>27 April 2022</date>
            """;

        var response = _service.Parse(input);

        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.CostCentre.Should().Be("DEV632");
        response.Data.TotalIncludingTax.Should().Be(35000m);
        response.Data.TotalExcludingTax.Should().Be(30434.78m);
        response.Data.SalesTax.Should().Be(4565.22m);
        response.Data.PaymentMethod.Should().Be("personal card");
        response.Data.Vendor.Should().Be("Seaside Steakhouse");
        response.Data.Description.Should().Be("development team's project end celebration");
        response.Data.Date.Should().Be("27 April 2022");
    }

    [Fact]
    public void Parse_FullValidInput_MetadataIsCorrect()
    {
        const string input = "<expense><total>100</total><cost_centre>DEV632</cost_centre></expense>";

        var response = _service.Parse(input);

        response.Metadata.Parser.Should().Be("deterministic");
        response.Metadata.WorkflowClassification.Should().Be("expense_claim");
        response.Metadata.AiExtensionReady.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Parser failure — unmatched tag
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_UnmatchedTag_ReturnsFailureWithUnmatchedTagError()
    {
        const string input = "<expense><total>100</total><cost_centre>DEV632</expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().HaveCount(1);
        response.Errors[0].Code.Should().Be("UNMATCHED_TAG");
        response.Errors[0].Message.Should().Contain("<cost_centre>");
    }

    [Fact]
    public void Parse_UnmatchedTag_MetadataClassificationIsUnknown()
    {
        const string input = "<total>100<cost_centre>DEV632";

        var response = _service.Parse(input);

        response.Success.Should().BeFalse();
        response.Metadata.WorkflowClassification.Should().Be("unknown");
    }

    // -------------------------------------------------------------------------
    // Validation failure — missing total
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_MissingTotal_ReturnsFailureWithMissingTotalError()
    {
        const string input = "<expense><cost_centre>DEV632</cost_centre></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().HaveCount(1);
        response.Errors[0].Code.Should().Be("MISSING_TOTAL");
    }

    [Fact]
    public void Parse_InvalidTotal_ReturnsFailureWithInvalidTotalError()
    {
        const string input = "<expense><total>not-a-number</total></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeFalse();
        response.Errors.Should().HaveCount(1);
        response.Errors[0].Code.Should().Be("INVALID_TOTAL");
    }

    // -------------------------------------------------------------------------
    // Cost centre defaulting
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_MissingCostCentre_DefaultsToUnknown()
    {
        const string input = "<expense><total>100</total></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeTrue();
        response.Data!.CostCentre.Should().Be("UNKNOWN");
    }

    [Fact]
    public void Parse_PresentCostCentre_UsesExtractedValue()
    {
        const string input = "<expense><total>100</total><cost_centre>OPS99</cost_centre></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeTrue();
        response.Data!.CostCentre.Should().Be("OPS99");
    }

    // -------------------------------------------------------------------------
    // Workflow classification
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_TotalPresent_ClassifiesAsExpenseClaim()
    {
        const string input = "<expense><total>500</total></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeTrue();
        response.Metadata.WorkflowClassification.Should().Be("expense_claim");
    }

    // -------------------------------------------------------------------------
    // Optional fields absent — success with nulls
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_OnlyTotalPresent_SucceedsWithNullOptionalFields()
    {
        const string input = "<expense><total>250</total></expense>";

        var response = _service.Parse(input);

        response.Success.Should().BeTrue();
        response.Data!.PaymentMethod.Should().BeNull();
        response.Data.Vendor.Should().BeNull();
        response.Data.Description.Should().BeNull();
        response.Data.Date.Should().BeNull();
    }
}
