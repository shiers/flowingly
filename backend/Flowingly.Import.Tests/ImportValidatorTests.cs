using FluentAssertions;
using Flowingly.Import.Api.Domain;
using Flowingly.Import.Api.Services;
using Xunit;

namespace Flowingly.Import.Tests;

public sealed class ImportValidatorTests
{
    private readonly ImportValidator _validator = new();

    // -------------------------------------------------------------------------
    // Missing / blank total — MISSING_TOTAL
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_MissingTotal_ReturnsMissingTotalError()
    {
        var data = new ParsedImportData { CostCentre = "DEV632", Total = null };

        var errors = _validator.Validate(data);

        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("MISSING_TOTAL");
    }

    [Fact]
    public void Validate_BlankTotal_ReturnsMissingTotalError()
    {
        var data = new ParsedImportData { Total = "   " };

        var errors = _validator.Validate(data);

        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("MISSING_TOTAL");
    }

    // -------------------------------------------------------------------------
    // Non-numeric total — INVALID_TOTAL
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NonNumericTotal_ReturnsInvalidTotalError()
    {
        var data = new ParsedImportData { Total = "abc" };

        var errors = _validator.Validate(data);

        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("INVALID_TOTAL");
        errors[0].Message.Should().Contain("abc");
    }

    [Fact]
    public void Validate_ZeroTotal_ReturnsInvalidTotalError()
    {
        var data = new ParsedImportData { Total = "0" };

        var errors = _validator.Validate(data);

        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("INVALID_TOTAL");
    }

    [Fact]
    public void Validate_NegativeTotal_ReturnsInvalidTotalError()
    {
        var data = new ParsedImportData { Total = "-100" };

        var errors = _validator.Validate(data);

        errors.Should().HaveCount(1);
        errors[0].Code.Should().Be("INVALID_TOTAL");
    }

    // -------------------------------------------------------------------------
    // Valid total — no errors
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidTotal_ReturnsNoErrors()
    {
        var data = new ParsedImportData { CostCentre = "DEV632", Total = "35000" };

        var errors = _validator.Validate(data);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_CommaFormattedTotal_ReturnsNoErrors()
    {
        var data = new ParsedImportData { Total = "35,000" };

        var errors = _validator.Validate(data);

        errors.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Missing / blank cost_centre — defaults, not rejected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_MissingCostCentre_ReturnsNoErrors()
    {
        var data = new ParsedImportData { Total = "100", CostCentre = null };

        var errors = _validator.Validate(data);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_BlankCostCentre_ReturnsNoErrors()
    {
        var data = new ParsedImportData { Total = "100", CostCentre = "   " };

        var errors = _validator.Validate(data);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ResolveCostCentre_MissingCostCentre_ReturnsUnknown()
    {
        var data = new ParsedImportData { Total = "100", CostCentre = null };

        var result = _validator.ResolveCostCentre(data);

        result.Should().Be("UNKNOWN");
    }

    [Fact]
    public void ResolveCostCentre_BlankCostCentre_ReturnsUnknown()
    {
        var data = new ParsedImportData { Total = "100", CostCentre = "   " };

        var result = _validator.ResolveCostCentre(data);

        result.Should().Be("UNKNOWN");
    }

    [Fact]
    public void ResolveCostCentre_PresentCostCentre_ReturnsValue()
    {
        var data = new ParsedImportData { Total = "100", CostCentre = "DEV632" };

        var result = _validator.ResolveCostCentre(data);

        result.Should().Be("DEV632");
    }
}
