using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

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

    /// <summary>The Avalonia controls it is drawn on, as AvaloniaDocs names them: "TabControl, TabStrip".</summary>
    public static readonly StyledProperty<string?> AvaloniaControlsProperty =
        AvaloniaProperty.Register<DocHeader, string?>(nameof(AvaloniaControls));

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

    public string? AvaloniaControls
    {
        get => GetValue(AvaloniaControlsProperty);
        set => SetValue(AvaloniaControlsProperty, value);
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
            && change.Property != MudBlazorProperty && change.Property != MudBlazorPathProperty
            && change.Property != AvaloniaControlsProperty)
        {
            return;
        }

        this.FindControl<TextBlock>("TitleText")!.Text = Title;
        var summary = this.FindControl<TextBlock>("SummaryText")!;
        summary.Text = Summary;
        summary.IsVisible = !string.IsNullOrEmpty(Summary);

        // The MudBlazor row reads as the Avalonia one: a label, then the page's link.
        var mudBlazor = this.FindControl<WrapPanel>("MudBlazorLinks")!;
        mudBlazor.IsVisible = !string.IsNullOrEmpty(MudBlazorPath);
        if (mudBlazor.IsVisible)
        {
            this.FindControl<HyperlinkButton>("MudBlazorLink")!.NavigateUri = new Uri($"https://mudblazor.com/{MudBlazorPath}");
            var name = MudBlazorPath!.StartsWith("components/", StringComparison.Ordinal) ? $"Mud{MudBlazor}" : MudBlazor;
            this.FindControl<TextBlock>("MudBlazorText")!.Text = name;
        }

        // One link per control, after the row's label; rebuilt whole, as the names may change.
        var avalonia = this.FindControl<WrapPanel>("AvaloniaLinks")!;
        while (avalonia.Children.Count > 1)
        {
            avalonia.Children.RemoveAt(1);
        }

        foreach (var control in AvaloniaDocs.Names(AvaloniaControls))
        {
            avalonia.Children.Add(new HyperlinkButton
            {
                NavigateUri = AvaloniaDocs.Uri(control),
                Margin = new Thickness(0, 0, 12, 0),
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Classes = { "body2" }, Text = control },
                        OpenInNewIcon(),
                    },
                },
            });
        }

        avalonia.IsVisible = avalonia.Children.Count > 1;
    }

    private static PathIcon OpenInNewIcon()
    {
        var icon = new PathIcon { Classes = { "small" } };
        icon[!PathIcon.DataProperty] = new DynamicResourceExtension("DocsOpenInNewIcon");
        return icon;
    }
}
