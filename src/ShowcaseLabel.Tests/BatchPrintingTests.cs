using System.Linq;
using Xunit;

namespace ShowcaseLabel.Tests;

public class BatchPrintingTests
{
    [Theory]
    [InlineData("15", 15L)]
    [InlineData(" 25 ", 25L)]
    [InlineData("9223372036854775807", long.MaxValue)]
    public void TryValidateMaximumLabels_ValidValue_ReturnsParsedMaximum(string maximumText, long expected)
    {
        bool result = MainWindow.TryValidateMaximumLabels(
            maximumText,
            out long maximumLabels,
            out string errorMessage);

        Assert.True(result);
        Assert.Equal(expected, maximumLabels);
        Assert.Empty(errorMessage);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("9223372036854775808")]
    public void TryValidateMaximumLabels_InvalidValue_ReturnsError(string? maximumText)
    {
        bool result = MainWindow.TryValidateMaximumLabels(
            maximumText,
            out long maximumLabels,
            out string errorMessage);

        Assert.False(result);
        Assert.Contains("maximum label count", errorMessage);
    }

    [Fact]
    public void GetBatchLabelCount_ThreeCarversAndFiveEntries_ReturnsFifteen()
    {
        long total = MainWindow.GetBatchLabelCount(10, 12, 1, 5);

        Assert.Equal(15, total);
    }

    [Fact]
    public void GetBatchLabelCount_SingleCarverAndEntry_ReturnsOne()
    {
        long total = MainWindow.GetBatchLabelCount(10, 10, 1, 1);

        Assert.Equal(1, total);
    }

    [Fact]
    public void GetBatchLabelCount_MaximumIntRanges_UsesLongArithmetic()
    {
        long total = MainWindow.GetBatchLabelCount(1, int.MaxValue, 1, int.MaxValue);

        Assert.Equal((long)int.MaxValue * int.MaxValue, total);
    }

    [Fact]
    public void GetBatchPrintJobs_ReturnsCarverMajorEntryAscendingCartesianProduct()
    {
        (int CarverId, int EntryNumber)[] jobs =
            MainWindow.GetBatchPrintJobs(10, 12, 1, 5).ToArray();

        Assert.Equal(
        [
            (10, 1), (10, 2), (10, 3), (10, 4), (10, 5),
            (11, 1), (11, 2), (11, 3), (11, 4), (11, 5),
            (12, 1), (12, 2), (12, 3), (12, 4), (12, 5),
        ],
        jobs);
        Assert.Equal(15, jobs.Length);
    }

    [Fact]
    public void GetBatchPrintJobs_SingleCarverAndEntry_ReturnsOneJob()
    {
        (int CarverId, int EntryNumber)[] jobs =
            MainWindow.GetBatchPrintJobs(10, 10, 1, 1).ToArray();

        Assert.Equal([(10, 1)], jobs);
    }
}
