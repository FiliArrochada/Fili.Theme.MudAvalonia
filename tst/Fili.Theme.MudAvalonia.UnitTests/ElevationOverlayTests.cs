using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// <c>material-elevation-overlay</c>: Material's dark-theme overlay, which MudBlazor does not
/// offer. Inside a container with the class, a raised surface in Dark is lightened by white at its
/// level's opacity, drawn as an inset shadow over its background; Light and high contrast are
/// unchanged, and only surfaces in the surface colour take it.
/// </summary>
public class ElevationOverlayTests
{
    private static readonly ThemeVariant HighContrast = FiliThemeVariants.HighContrast;

    private static BoxShadows Token(string key, ThemeVariant variant)
    {
        Assert.True(Application.Current!.TryFindResource(key, variant, out var value), $"{key} did not resolve.");
        return Assert.IsType<BoxShadows>(value);
    }

    /// <summary>Material's overlay opacity at a level: 4.5 ln(level + 1) + 2 percent, as an alpha byte.</summary>
    private static byte Alpha(int level) => (byte)Math.Round((4.5 * Math.Log(level + 1) + 2) / 100 * 255);

    private static BoxShadow? Inset(BoxShadows shadows)
    {
        for (var i = 0; i < shadows.Count; i++)
        {
            if (shadows[i].IsInset)
            {
                return shadows[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Each overlay token is its level's shadow, plus in Dark one inset layer of white at Material's
    /// opacity: 5% at 1, 9% at 4, 12% at 8, 16% at 24. None at 0, and none at 25, MudBlazor's softer
    /// shadow rather than a higher surface. Outside Dark the token is the shadow alone.
    /// </summary>
    [Fact]
    public Task EachLevelIsItsShadowAndMaterialsOverlay() => UiThread.RunAsync(() =>
    {
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark, HighContrast })
        {
            for (var level = 0; level <= 25; level++)
            {
                var shadow = Token($"FiliElevation{level}", variant);
                var overlay = Token($"FiliElevationOverlay{level}", variant);
                var expectsOverlay = variant == ThemeVariant.Dark && level is >= 1 and <= 24;
                var expected = expectsOverlay
                    ? $"{shadow}, inset 0 0 0 10000 #{Alpha(level):X2}FFFFFF"
                    : shadow.ToString();

                Assert.Equal(BoxShadows.Parse(expected).ToString(), overlay.ToString());
            }
        }

        Assert.Equal(0x0D, Alpha(1));
        Assert.Equal(0x18, Alpha(4));
        Assert.Equal(0x1E, Alpha(8));
        Assert.Equal(0x2A, Alpha(24));
    });

    private static Window Host(ThemeVariant variant, Control content, bool overlay = true)
    {
        var host = new StackPanel { Children = { content } };
        if (overlay)
        {
            host.Classes.Add("material-elevation-overlay");
        }

        var window = new Window { RequestedThemeVariant = variant, Content = host, Width = 600, Height = 400 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static string Shadow(Border border) => border.BoxShadow.ToString();

    private static string Expected(int level, ThemeVariant variant) => Token($"FiliElevationOverlay{level}", variant).ToString();

    [Theory]
    [InlineData("surface elevation4", 4)]
    [InlineData("surface elevation24", 24)]
    [InlineData("card", 1)]
    [InlineData("card elevation8", 8)]
    public Task APaperOrCardTakesItsLevelsOverlay(string classes, int level) => UiThread.RunAsync(() =>
    {
        var paper = new Border { Width = 100, Height = 40 };
        paper.Classes.AddRange(classes.Split(' '));
        Host(ThemeVariant.Dark, paper);

        Assert.Equal(Expected(level, ThemeVariant.Dark), Shadow(paper));
        Assert.NotNull(Inset(paper.BoxShadow));
    });

    /// <summary>Nothing changes without the class, in Light, or in high contrast, which rings rather than shades.</summary>
    [Fact]
    public Task OnlyDarkWithTheClassIsLightened() => UiThread.RunAsync(() =>
    {
        foreach (var (variant, overlay) in new[] { (ThemeVariant.Dark, false), (ThemeVariant.Light, true), (HighContrast, true) })
        {
            var paper = new Border { Width = 100, Height = 40, Classes = { "surface", "elevation4" } };
            Host(variant, paper, overlay);

            Assert.Null(Inset(paper.BoxShadow));
            Assert.Equal(Token("FiliElevation4", variant).ToString(), Shadow(paper));
        }
    });

    /// <summary>
    /// A paper with no elevation, an outlined card, and an elevation class on a Border that is
    /// not a surface - a coloured box - take none: Material lightens surfaces, not colours.
    /// </summary>
    [Theory]
    [InlineData("surface")]
    [InlineData("card outlined")]
    [InlineData("elevation4")]
    [InlineData("appbar primary")]
    public Task WhatIsNotARaisedSurfaceIsLeftAlone(string classes) => UiThread.RunAsync(() =>
    {
        var border = new Border { Width = 100, Height = 40 };
        border.Classes.AddRange(classes.Split(' '));
        Host(ThemeVariant.Dark, border);

        Assert.Null(Inset(border.BoxShadow));
    });

    [Fact]
    public Task AFilledButtonKeepsItsColour() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save", Classes = { "filled", "primary" } };
        Host(ThemeVariant.Dark, button);

        Assert.DoesNotContain(button.GetVisualDescendants().OfType<Border>(), b => Inset(b.BoxShadow) is not null);
    });

    /// <summary>An expansion panel is a surface at elevation 1, or at its class's level.</summary>
    [Theory]
    [InlineData("", 1)]
    [InlineData("elevation6", 6)]
    [InlineData("elevation0", 0)]
    public Task AnExpansionPanelTakesItsLevelsOverlay(string classes, int level) => UiThread.RunAsync(() =>
    {
        var panel = new Expander { Header = "Panel", Content = "Body", Width = 300 };
        if (classes.Length > 0)
        {
            panel.Classes.Add(classes);
        }

        Host(ThemeVariant.Dark, panel);

        Assert.Equal(Expected(level, ThemeVariant.Dark), Shadow(Matrix.Part<Border>(panel, "PART_Root")));
    });

    public sealed record Row(string Name);

    /// <summary>A grid is a surface at elevation 1 or its class's level; an outlined grid has no shadow and no overlay.</summary>
    [Theory]
    [InlineData("", 1)]
    [InlineData("elevation4", 4)]
    [InlineData("outlined", 0)]
    public Task ADataGridTakesItsLevelsOverlay(string classes, int level) => UiThread.RunAsync(() =>
    {
        var grid = new DataGrid
        {
            Width = 300,
            Height = 120,
            ItemsSource = new AvaloniaList<Row> { new("One") },
            Columns = { new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Row.Name)) } },
        };
        if (classes.Length > 0)
        {
            grid.Classes.Add(classes);
        }

        Host(ThemeVariant.Dark, grid);

        Assert.Equal(Expected(level, ThemeVariant.Dark), Shadow(Matrix.Part<Border>(grid, "DataGridBorder")));
    });

    /// <summary>
    /// The popups are elevation-8 sheets in the surface colour, and they take the overlay although
    /// each roots a tree of its own: a select's list, an autocomplete's, a submenu, a context
    /// menu, and a flyout's presenter, plain or a menu.
    /// </summary>
    [Fact]
    public Task EveryPopupSheetTakesTheLevel8Overlay() => UiThread.RunAsync(() =>
    {
        var expected = Expected(8, ThemeVariant.Dark);

        var combo = new ComboBox { ItemsSource = new[] { "One", "Two" }, Width = 200 };
        var auto = new AutoCompleteBox { ItemsSource = new[] { "One", "Two" }, Width = 200 };
        var submenu = new MenuItem { Header = "File", Items = { new MenuItem { Header = "Open" } } };
        var context = new ContextMenu { Items = { new MenuItem { Header = "Copy" } } };
        var flyoutText = new TextBlock { Text = "Flyout" };
        var flyout = new Flyout { Content = flyoutText };
        var rename = new MenuItem { Header = "Rename" };
        var menuFlyout = new MenuFlyout { Items = { rename } };
        var flyoutTarget = new Button { Content = "Flyout", Flyout = flyout };
        var menuTarget = new Button { Content = "Menu", Flyout = menuFlyout };
        var contextTarget = new Border { Width = 50, Height = 20, ContextMenu = context };

        var panel = new StackPanel { Children = { combo, auto, new Menu { Items = { submenu } }, contextTarget, flyoutTarget, menuTarget } };
        Host(ThemeVariant.Dark, panel);

        combo.IsDropDownOpen = true;
        auto.Text = "O";
        auto.IsDropDownOpen = true;
        submenu.IsSubMenuOpen = true;
        context.Open(contextTarget);
        flyout.ShowAt(flyoutTarget);
        menuFlyout.ShowAt(menuTarget);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(expected, Shadow(Sheet(combo)));
        Assert.Equal(expected, Shadow(Sheet(auto)));
        Assert.Equal(expected, Shadow(Sheet(submenu)));
        Assert.Equal(expected, Shadow(Matrix.Part<Border>(context, "PART_Border")));
        Assert.Equal(expected, Shadow(Presenter<FlyoutPresenter>(flyoutText)));
        Assert.Equal(expected, Shadow(Presenter<MenuFlyoutPresenter>(rename)));

        // A popup's sheet is its child, which lives in the popup's own tree, not under the control.
        static Border Sheet(Control owner) =>
            Assert.IsType<Border>(Matrix.Part<Avalonia.Controls.Primitives.Popup>(owner, "PART_Popup").Child);

        static Border Presenter<T>(Control inside) where T : Control =>
            Matrix.Part<Border>(inside.GetVisualAncestors().OfType<T>().First(), "PART_Border");
    });
}
