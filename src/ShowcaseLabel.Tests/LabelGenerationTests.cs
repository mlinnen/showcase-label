using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace ShowcaseLabel.Tests;

public class LabelGenerationTests
{
    [Theory]
    [InlineData(
        0,
        "SIZE 101.6 mm,152.4 mm",
        "QRCODE 575,493,M,8,A,0,M2,S7,\"https://charlottewoodcarvers.com/r?event=2027T&carver_id=123&entry=4\"",
        "TEXT 5,585,\"3\",0,2,2,\"C123-4\"")]
    [InlineData(
        1,
        "SIZE 66.7 mm,25.4 mm",
        "QRCODE 440,58,M,3,A,0,M2,S7,\"https://charlottewoodcarvers.com/r?event=2027T&carver_id=123&entry=4\"",
        "TEXT 5,77,\"3\",0,2,2,\"C123-4\"")]
    public void BuildTsplLabel_PlacesTextBeforeQrAndPreservesSelectedLabelSize(
        int labelSizeIndex,
        string expectedSize,
        string expectedQrCode,
        string expectedText)
    {
        string tspl = RunOnSta(() =>
        {
            var window = new MainWindow();
            window.EventComboBox.SelectedItem = "2027T";
            window.LabelSizeComboBox.SelectedIndex = labelSizeIndex;

            object labelSize = typeof(MainWindow)
                .GetProperty("SelectedLabelSize", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(window)!;
            byte[] bytes = (byte[])typeof(MainWindow)
                .GetMethod("BuildTsplLabel", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(window, ["123", 4, labelSize])!;

            return Encoding.ASCII.GetString(bytes);
        });

        Assert.Contains(expectedSize, tspl);
        Assert.Contains("GAP 3 mm,0", tspl);
        Assert.Contains(expectedQrCode, tspl);
        Assert.Contains(expectedText, tspl);
        Assert.True(
            tspl.IndexOf(expectedText, StringComparison.Ordinal) <
            tspl.IndexOf(expectedQrCode, StringComparison.Ordinal));
        Assert.Contains("PRINT 1,1", tspl);
    }

    private static T RunOnSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? exception = null;
        using var completed = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        completed.Wait();
        thread.Join();

        Assert.Null(exception);
        return result!;
    }
}
