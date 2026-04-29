using FluentAssertions;
using Flowingly.Import.Api.Services;
using Xunit;

namespace Flowingly.Import.Tests;

public sealed class MarkupParserTests
{
    private readonly MarkupParser _parser = new();

    // -------------------------------------------------------------------------
    // Extraction — individual fields
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_ValidInput_ExtractsTotal()
    {
        const string input = "<expense><total>35000</total></expense>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.Total.Should().Be("35000");
    }

    [Fact]
    public void TryParse_ValidInput_ExtractsCostCentre()
    {
        const string input = "<expense><cost_centre>DEV632</cost_centre><total>100</total></expense>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.CostCentre.Should().Be("DEV632");
    }

    [Fact]
    public void TryParse_ValidInput_ExtractsPaymentMethod()
    {
        const string input = "<expense><total>100</total><payment_method>personal card</payment_method></expense>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.PaymentMethod.Should().Be("personal card");
    }

    // -------------------------------------------------------------------------
    // Extraction — inline tags outside an expense block
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_InlineVendor_ExtractsVendor()
    {
        const string input = "Please process this. <vendor>Seaside Steakhouse</vendor>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.Vendor.Should().Be("Seaside Steakhouse");
    }

    [Fact]
    public void TryParse_InlineDescription_ExtractsDescription()
    {
        const string input = "<description>development team's project end celebration</description>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.Description.Should().Be("development team's project end celebration");
    }

    [Fact]
    public void TryParse_InlineDate_ExtractsDate()
    {
        const string input = "<date>27 April 2022</date>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.Date.Should().Be("27 April 2022");
    }

    // -------------------------------------------------------------------------
    // Extraction — mixed email body with all fields
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_FullEmailBody_ExtractsAllFields()
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
            Thanks
            """;

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.CostCentre.Should().Be("DEV632");
        result.Total.Should().Be("35,000");
        result.PaymentMethod.Should().Be("personal card");
        result.Vendor.Should().Be("Seaside Steakhouse");
        result.Description.Should().Be("development team's project end celebration");
        result.Date.Should().Be("27 April 2022");
    }

    // -------------------------------------------------------------------------
    // Comma-formatted total — parser returns raw string, no numeric conversion
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_CommaFormattedTotal_ReturnsRawString()
    {
        const string input = "<expense><total>35,000</total></expense>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        // Parser preserves the raw value — tax calculation handles numeric parsing.
        result!.Total.Should().Be("35,000");
    }

    // -------------------------------------------------------------------------
    // Absent fields — parser returns null, not an error
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_MissingCostCentre_ReturnsNullCostCentre()
    {
        const string input = "<expense><total>100</total></expense>";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result!.CostCentre.Should().BeNull();
    }

    [Fact]
    public void TryParse_MissingTotal_ReturnsNullTotal()
    {
        const string input = "<expense><cost_centre>DEV632</cost_centre></expense>";

        var result = _parser.TryParse(input, out var errors);

        // Missing total is NOT a parser error — it is a validator concern.
        errors.Should().BeEmpty();
        result!.Total.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Unmatched opening tags — structural errors
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_UnmatchedOpeningTag_ReturnsNullAndError()
    {
        const string input = "<expense><total>100</total><cost_centre>DEV632</expense>";

        var result = _parser.TryParse(input, out var errors);

        result.Should().BeNull();
        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("UNMATCHED_TAG");
        errors[0].Message.Should().Contain("<cost_centre>");
    }

    [Fact]
    public void TryParse_MultipleUnmatchedTags_ReturnsAllErrors()
    {
        const string input = "<total>100<cost_centre>DEV632";

        var result = _parser.TryParse(input, out var errors);

        result.Should().BeNull();
        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Code == "UNMATCHED_TAG" && e.Message.Contains("<total>"));
        errors.Should().Contain(e => e.Code == "UNMATCHED_TAG" && e.Message.Contains("<cost_centre>"));
    }

    [Fact]
    public void TryParse_OuterExpenseTagWithNoClosingTag_IsIgnored()
    {
        // The <expense> wrapper itself is not treated as a field — an unmatched
        // <expense> opening tag should not produce an error.
        const string input = "<expense><total>100</total><cost_centre>DEV632</cost_centre>";

        var result = _parser.TryParse(input, out var errors);

        // No error for the unmatched <expense> wrapper.
        errors.Should().BeEmpty();
        result!.Total.Should().Be("100");
        result.CostCentre.Should().Be("DEV632");
    }

    // -------------------------------------------------------------------------
    // Edge cases
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParse_EmptyInput_ReturnsEmptyDataNoErrors()
    {
        var result = _parser.TryParse(string.Empty, out var errors);

        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.Total.Should().BeNull();
        result.CostCentre.Should().BeNull();
    }

    [Fact]
    public void TryParse_PlainTextNoTags_ReturnsEmptyDataNoErrors()
    {
        const string input = "Hi Patricia, please process this expense for the team dinner.";

        var result = _parser.TryParse(input, out var errors);

        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.Total.Should().BeNull();
    }
}
