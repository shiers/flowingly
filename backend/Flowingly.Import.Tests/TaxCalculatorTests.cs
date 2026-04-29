using FluentAssertions;
using Flowingly.Import.Api.Services;
using Xunit;

namespace Flowingly.Import.Tests;

public sealed class TaxCalculatorTests
{
    private readonly TaxCalculator _calculator = new();

    // -------------------------------------------------------------------------
    // Core GST calculation
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_WholeNumberTotal_ReturnsCorrectBreakdown()
    {
        // 35000 / 1.15 = 30434.782608... → rounds to 30434.78
        // 35000 - 30434.78 = 4565.22
        var result = _calculator.Calculate("35000");

        result.TotalIncludingTax.Should().Be(35000m);
        result.TotalExcludingTax.Should().Be(30434.78m);
        result.SalesTax.Should().Be(4565.22m);
    }

    [Fact]
    public void Calculate_CommaFormattedTotal_ParsesAndCalculatesCorrectly()
    {
        var result = _calculator.Calculate("35,000");

        result.TotalIncludingTax.Should().Be(35000m);
        result.TotalExcludingTax.Should().Be(30434.78m);
        result.SalesTax.Should().Be(4565.22m);
    }

    // -------------------------------------------------------------------------
    // Rounding
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_DecimalTotal_RoundsToTwoDecimalPlaces()
    {
        // 100 / 1.15 = 86.9565... → rounds to 86.96
        // 100 - 86.96 = 13.04
        var result = _calculator.Calculate("100");

        result.TotalExcludingTax.Should().Be(86.96m);
        result.SalesTax.Should().Be(13.04m);
    }

    // -------------------------------------------------------------------------
    // TotalIncludingTax is preserved as parsed
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_PreservesTotalIncludingTax()
    {
        var result = _calculator.Calculate("1150");

        result.TotalIncludingTax.Should().Be(1150m);
        result.TotalExcludingTax.Should().Be(1000m);
        result.SalesTax.Should().Be(150m);
    }

    // -------------------------------------------------------------------------
    // SalesTax + TotalExcludingTax reconciles to TotalIncludingTax
    // -------------------------------------------------------------------------

    [Fact]
    public void Calculate_SalesTaxPlusTotalExcludingTax_ReconcilesToTotalIncludingTax()
    {
        var result = _calculator.Calculate("35000");

        (result.TotalExcludingTax + result.SalesTax).Should().Be(result.TotalIncludingTax);
    }
}
