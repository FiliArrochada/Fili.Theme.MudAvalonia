using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// A link to another docs page, by its slug in <see cref="DocCatalog"/>: the theme's link, which
/// opens the page in the shell rather than a URL.
/// </summary>
public sealed class DocLink : HyperlinkButton
{
    public static readonly StyledProperty<string?> SlugProperty =
        AvaloniaProperty.Register<DocLink, string?>(nameof(Slug));

    public string? Slug
    {
        get => GetValue(SlugProperty);
        set => SetValue(SlugProperty, value);
    }

    // Drawn as the HyperlinkButton it is, by the theme's own HyperlinkButton theme.
    protected override Type StyleKeyOverride => typeof(HyperlinkButton);

    protected override void OnClick()
    {
        base.OnClick();
        if (Slug is { } slug)
        {
            this.FindAncestorOfType<MainView>()?.Navigate(slug);
        }
    }
}
