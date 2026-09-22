using Xunit;

namespace ShowcaseLabel.Tests;

public class ApplicationVersionTests
{
    [Fact]
    public void DisplayValue_MatchesTheApplicationAssemblyVersion()
    {
        string expected = typeof(MainWindow).Assembly.GetName().Version!.ToString(3);

        Assert.Equal(expected, ApplicationVersion.DisplayValue);
    }
}
