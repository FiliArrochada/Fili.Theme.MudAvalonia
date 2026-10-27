using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

public partial class DocHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DocHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SummaryProperty =
        AvaloniaProperty.Register<DocHeader, string?>(nameof(Summary));

    /// <summary>
    /// The MudBlazor component this page mirrors, without the Mud: "Button". For a feature or
    /// customization page, its title as written: "Elevation", "MudThemeProvider".
    /// </summary>
    public static readonly StyledProperty<string?> MudBlazorProperty =
        AvaloniaProperty.Register<DocHeader, string?>(nameof(MudBlazor));

    /// <summary>Its page on mudblazor.com: "components/button", "features/elevation".</summary>
    public static readonly StyledProperty<string?> MudBlazorPathProperty =
        AvaloniaProperty.Register<DocHeader, string?>(nameof(MudBlazorPath));

    public DocHeader() => AvaloniaXamlLoader.Load(this);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Summary
    {
        get => GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    public string? MudBlazor
    {
        get => GetValue(MudBlazorProperty);
        set => SetValue(MudBlazorProperty, value);
    }

    public string? MudBlazorPath
    {
        get => GetValue(MudBlazorPathProperty);
        set => SetValue(MudBlazorPathProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != TitleProperty && change.Property != SummaryProperty
            && change.Property != MudBlazorProperty && change.Property != MudBlazorPathProperty)
        {
            return;
        }

        this.FindControl<TextBlock>("TitleText")!.Text = Title;
        var summary = this.FindControl<TextBlock>("SummaryText")!;
        summary.Text = Summary;
        summary.IsVisible = !string.IsNullOrEmpty(Summary);

        var link = this.FindControl<HyperlinkButton>("MudBlazorLink")!;
        link.IsVisible = !string.IsNullOrEmpty(MudBlazorPath);
        if (link.IsVisible)
        {
            link.NavigateUri = new Uri($"https://mudblazor.com/{MudBlazorPath}");
            var name = MudBlazorPath!.StartsWith("components/", StringComparison.Ordinal) ? $"Mud{MudBlazor}" : MudBlazor;
            this.FindControl<TextBlock>("MudBlazorText")!.Text = $"{name} on mudblazor.com";
        }
    }
}
