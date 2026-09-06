using System.Linq;
using Xunit;

namespace ShowcaseLabel.Tests;

public class CarverRangeTests
{
    [Theory]
    [InlineData("10", "12")]
    [InlineData(" 3 ", " 7 ")]
    [InlineData("10", "10")]
    public void TryValidateCarverRange_ValidBounds_ReturnsParsedInclusiveBounds(
        string fromText,
        string toText)
    {
        bool result = MainWindow.TryValidateCarverRange(
            fromText,
            toText,
            out int fromCarver,
            out int toCarver,
            out string errorMessage);

        Assert.True(result);
        Assert.Equal(int.Parse(fromText.Trim()), fromCarver);
        Assert.Equal(int.Parse(toText.Trim()), toCarver);
        Assert.Empty(errorMessage);
    }

    [Theory]
    [InlineData("0", "5")]
    [InlineData("-1", "5")]
    [InlineData("not-a-number", "5")]
    [InlineData("", "5")]
    [InlineData(null, "5")]
    public void TryValidateCarverRange_InvalidStart_ReturnsStartError(string? fromText, string toText)
    {
        bool result = MainWindow.TryValidateCarverRange(
            fromText,
            toText,
            out _,
            out _,
            out string errorMessage);

        Assert.False(result);
        Assert.Contains("Start Carver ID", errorMessage);
    }

    [Theory]
    [InlineData("5", "0")]
    [InlineData("5", "-1")]
    [InlineData("5", "not-a-number")]
    [InlineData("5", "")]
    [InlineData("5", null)]
    public void TryValidateCarverRange_InvalidEnd_ReturnsEndError(string fromText, string? toText)
    {
        bool result = MainWindow.TryValidateCarverRange(
            fromText,
            toText,
            out int fromCarver,
            out _,
            out string errorMessage);

        Assert.False(result);
        Assert.Equal(int.Parse(fromText), fromCarver);
        Assert.Contains("End Carver ID", errorMessage);
    }

    [Theory]
    [InlineData("12", "10")]
    [InlineData("2", "1")]
    public void TryValidateCarverRange_ReversedBounds_ReturnsRangeError(string fromText, string toText)
    {
        bool result = MainWindow.TryValidateCarverRange(
            fromText,
            toText,
            out int fromCarver,
            out int toCarver,
            out string errorMessage);

        Assert.False(result);
        Assert.Equal(int.Parse(fromText), fromCarver);
        Assert.Equal(int.Parse(toText), toCarver);
        Assert.Contains("Start Carver ID", errorMessage);
        Assert.Contains("End Carver ID", errorMessage);
    }

    [Fact]
    public void GetCarverSequence_ReturnsInclusiveAscendingSequence()
    {
        int[] result = MainWindow.GetCarverSequence(10, 12).ToArray();

        Assert.Equal([10, 11, 12], result);
    }

    [Fact]
    public void GetCarverSequence_AtInt32Maximum_ReturnsWithoutOverflowing()
    {
        int[] result = MainWindow.GetCarverSequence(int.MaxValue - 1, int.MaxValue).ToArray();

        Assert.Equal([int.MaxValue - 1, int.MaxValue], result);
    }
}
