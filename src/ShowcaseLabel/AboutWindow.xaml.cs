using System.Windows;

namespace ShowcaseLabel;

public partial class AboutWindow : Window
{
    public string VersionText => $"Version {ApplicationVersion.DisplayValue}";

    public AboutWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
