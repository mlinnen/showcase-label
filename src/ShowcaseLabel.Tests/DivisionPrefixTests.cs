using Xunit;

namespace ShowcaseLabel.Tests;

public class DivisionPrefixTests
{
    // Happy path — each division maps to the correct prefix

    [Theory]
    [InlineData("None",         "")]
    [InlineData("Novice",       "N-")]
    [InlineData("Intermediate", "I-")]
    [InlineData("Open",         "O-")]
    public void GetDivisionPrefix_KnownDivisions_ReturnsCorrectPrefix(string division, string expected)
    {
        var result = MainWindow.GetDivisionPrefix(division);
        Assert.Equal(expected, result);
    }

    // Edge cases — unknown/null/empty inputs default to no prefix

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("novice")]      // case-sensitive — "novice" is not "Novice"
    [InlineData("OPEN")]        // case-sensitive
    [InlineData("unknown")]
    public void GetDivisionPrefix_UnknownDivision_ReturnsEmptyPrefix(string division)
    {
        var result = MainWindow.GetDivisionPrefix(division);
        Assert.Equal("", result);
    }

    // Label text composition — verify full formatted label strings

    [Theory]
    [InlineData("None",         10, 1, "C10-1")]
    [InlineData("Novice",       10, 1, "N-C10-1")]
    [InlineData("Intermediate", 10, 1, "I-C10-1")]
    [InlineData("Open",         10, 1, "O-C10-1")]
    [InlineData("Novice",       99, 5, "N-C99-5")]
    [InlineData("None",          1, 1, "C1-1")]
    public void FormatLabelText_DivisionAndCarverAndEntry_ProducesCorrectString(
        string division, int carverId, int entry, string expected)
    {
        // This mirrors the logic: {prefix}C{carverId}-{entry}
        string prefix = MainWindow.GetDivisionPrefix(division);
        string labelText = $"{prefix}C{carverId}-{entry}";
        Assert.Equal(expected, labelText);
    }
}
