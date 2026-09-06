using System.Linq;
using Xunit;

namespace ShowcaseLabel.Tests;

public class EntryRangeTests
{
    // Valid ranges — multi-entry and same-bound single entry

    [Theory]
    [InlineData("1", "5")]
    [InlineData("10", "10")]     // same-bound single entry
    [InlineData(" 3 ", " 7 ")]   // surrounding whitespace is trimmed
    [InlineData("1", "1")]
    public void TryValidateEntryRange_ValidRange_ReturnsTrueWithParsedBoundsAndNoError(string fromText, string toText)
    {
        bool result = MainWindow.TryValidateEntryRange(fromText, toText, out int fromEntry, out int toEntry, out string errorMessage);

        Assert.True(result);
        Assert.Equal(int.Parse(fromText.Trim()), fromEntry);
        Assert.Equal(int.Parse(toText.Trim()), toEntry);
        Assert.Equal("", errorMessage);
    }

    // Invalid From Entry — non-positive, non-numeric, blank, or null

    [Theory]
    [InlineData("0", "5")]
    [InlineData("-1", "5")]
    [InlineData("abc", "5")]
    [InlineData("", "5")]
    [InlineData(null, "5")]
    public void TryValidateEntryRange_InvalidFromEntry_ReturnsFalseWithFromError(string? fromText, string toText)
    {
        bool result = MainWindow.TryValidateEntryRange(fromText, toText, out int fromEntry, out int toEntry, out string errorMessage);

        Assert.False(result);
        Assert.Equal(0, toEntry);
        Assert.Contains("From Entry", errorMessage);
    }

    // Invalid To Entry — non-positive, non-numeric, blank, or null (From Entry is valid)

    [Theory]
    [InlineData("1", "0")]
    [InlineData("1", "-5")]
    [InlineData("1", "xyz")]
    [InlineData("1", "")]
    [InlineData("1", null)]
    public void TryValidateEntryRange_InvalidToEntry_ReturnsFalseWithToError(string fromText, string? toText)
    {
        bool result = MainWindow.TryValidateEntryRange(fromText, toText, out int fromEntry, out int toEntry, out string errorMessage);

        Assert.False(result);
        Assert.Equal(int.Parse(fromText), fromEntry);
        Assert.Contains("To Entry", errorMessage);
    }

    // Reversed range — From greater than To is rejected, never auto-swapped

    [Theory]
    [InlineData("10", "1")]
    [InlineData("2", "1")]
    public void TryValidateEntryRange_ReversedRange_ReturnsFalseWithRangeError(string fromText, string toText)
    {
        bool result = MainWindow.TryValidateEntryRange(fromText, toText, out int fromEntry, out int toEntry, out string errorMessage);

        Assert.False(result);
        Assert.Equal(int.Parse(fromText), fromEntry);
        Assert.Equal(int.Parse(toText), toEntry);
        Assert.Contains("From Entry", errorMessage);
        Assert.Contains("To Entry", errorMessage);
    }

    // Blank values are never inferred — both must be explicitly supplied

    [Fact]
    public void TryValidateEntryRange_BothBlank_ReturnsFalseWithFromError()
    {
        bool result = MainWindow.TryValidateEntryRange("", "", out int fromEntry, out int toEntry, out string errorMessage);

        Assert.False(result);
        Assert.Equal(0, fromEntry);
        Assert.Equal(0, toEntry);
        Assert.Contains("From Entry", errorMessage);
    }

    // GetEntrySequence — inclusive multi-entry range in ascending order

    [Fact]
    public void GetEntrySequence_MultiEntryRange_ReturnsInclusiveAscendingSequence()
    {
        var result = MainWindow.GetEntrySequence(3, 7).ToList();

        Assert.Equal(new[] { 3, 4, 5, 6, 7 }, result);
    }

    // GetEntrySequence — same-bound range yields exactly one entry

    [Fact]
    public void GetEntrySequence_SameBound_ReturnsSingleEntry()
    {
        var result = MainWindow.GetEntrySequence(10, 10).ToList();

        Assert.Equal(new[] { 10 }, result);
    }

    // GetEntrySequence — no values outside the requested bounds appear in the result

    [Theory]
    [InlineData(1, 5)]
    [InlineData(20, 25)]
    [InlineData(1, 1)]
    public void GetEntrySequence_AnyRange_ContainsNoOutOfRangeValues(int fromEntry, int toEntry)
    {
        var result = MainWindow.GetEntrySequence(fromEntry, toEntry).ToList();

        Assert.All(result, i => Assert.InRange(i, fromEntry, toEntry));
        Assert.Equal(toEntry - fromEntry + 1, result.Count);
    }
}
