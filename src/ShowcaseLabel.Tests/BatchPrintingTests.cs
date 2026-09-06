using System.Linq;
using Xunit;

namespace ShowcaseLabel.Tests;

public class BatchPrintingTests
{
    [Fact]
    public void GetBatchLabelCount_TwoCarversAndEightEntries_ReturnsSixteenWithoutBatchCap()
    {
        long total = MainWindow.GetBatchLabelCount(10, 11, 1, 8);

        Assert.Equal(16, total);
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
    public void GetBatchPrintJobs_OverFormerBatchCap_ReturnsAllJobsInCarverMajorEntryAscendingOrder()
    {
        (int CarverId, int EntryNumber)[] jobs =
            MainWindow.GetBatchPrintJobs(10, 11, 1, 8).ToArray();

        Assert.Equal(
        [
            (10, 1), (10, 2), (10, 3), (10, 4),
            (10, 5), (10, 6), (10, 7), (10, 8),
            (11, 1), (11, 2), (11, 3), (11, 4),
            (11, 5), (11, 6), (11, 7), (11, 8),
        ],
        jobs);
        Assert.Equal(16, jobs.Length);
    }

    [Fact]
    public void GetBatchPrintJobs_SingleCarverAndEntry_ReturnsOneJob()
    {
        (int CarverId, int EntryNumber)[] jobs =
            MainWindow.GetBatchPrintJobs(10, 10, 1, 1).ToArray();

        Assert.Equal([(10, 1)], jobs);
    }
}
