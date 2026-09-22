namespace ShowcaseLabel;

internal static class ApplicationVersion
{
    internal static string DisplayValue =>
        typeof(ApplicationVersion).Assembly.GetName().Version?.ToString(3) ?? "Unknown";
}
