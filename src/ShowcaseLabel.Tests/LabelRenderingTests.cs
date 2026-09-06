using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Xunit;

namespace ShowcaseLabel.Tests;

public class LabelRenderingTests
{
    [Fact]
    public void BuildTsplLabel_UsesUnprefixedCarverAndEntryText()
    {
        string tspl = RunOnSta(() =>
        {
            MethodInfo buildTsplLabel = typeof(MainWindow).GetMethod(
                "BuildTsplLabel",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("BuildTsplLabel was not found.");

            ParameterInfo[] parameters = buildTsplLabel.GetParameters();
            Assert.Equal(3, parameters.Length);
            Assert.DoesNotContain(parameters, parameter =>
                parameter.Name?.Contains("division", StringComparison.OrdinalIgnoreCase) == true);

            FieldInfo labelSizesField = typeof(MainWindow).GetField(
                "LabelSizes",
                BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("LabelSizes was not found.");
            Array labelSizes = (Array)(labelSizesField.GetValue(null)
                ?? throw new InvalidOperationException("LabelSizes was not initialized."));

            var window = new MainWindow();
            try
            {
                byte[] label = (byte[])(buildTsplLabel.Invoke(
                    window,
                    ["123", 7, labelSizes.GetValue(0)])
                    ?? throw new InvalidOperationException("BuildTsplLabel returned no data."));

                return Encoding.ASCII.GetString(label);
            }
            finally
            {
                window.Close();
            }
        });

        string textCommand = Assert.Single(
            tspl.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            command => command.StartsWith("TEXT ", StringComparison.Ordinal));

        Assert.Matches("^TEXT .*,\"C123-7\"$", textCommand);
    }

    private static T RunOnSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? exception = null;
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
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
            ExceptionDispatchInfo.Capture(exception).Throw();

        return result!;
    }
}
