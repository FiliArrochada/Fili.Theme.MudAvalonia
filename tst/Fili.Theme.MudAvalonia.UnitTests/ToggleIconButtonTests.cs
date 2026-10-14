using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudToggleIconButton, `icon` on a ToggleButton: the icon button's look, with ToggledColor as the
/// colour class while checked and Color as `unchecked-{color}` while not, and ToggledIcon as the
/// second of two PathIcons.
/// </summary>
public class ToggleIconButtonTests
{
    private static ToggleButton Toggle(params string[] classes)
    {
        var toggle = new ToggleButton { Content = new PathIcon() };
        toggle.Classes.Add("icon");
        toggle.Classes.AddRange(classes);
        return Matrix.Show(toggle);
    }

    private static void Check(ToggleButton toggle, bool value)
    {
        toggle.IsChecked = value;
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public Task ItIsARoundIconButton() => UiThread.RunAsync(() =>
    {
        var toggle = Toggle();

        Assert.Equal(new Avalonia.Thickness(12), toggle.Padding);
        Assert.Equal(new Avalonia.CornerRadius(999), toggle.CornerRadius);
        Assert.Equal(24, Assert.IsType<PathIcon>(toggle.Content).Width);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(toggle.Foreground));
    });

    /// <summary>Toggled ? (ToggledColor ?? Color) : Color, with Color.Default as action-default.</summary>
    [Fact]
    public Task TheColourIsToggledColorWhileCheckedAndColorWhileNot() => UiThread.RunAsync(() =>
    {
        var toggle = Toggle("primary", "unchecked-error");

        Assert.Equal(Matrix.Token("FiliErrorColor"), Matrix.Colour(toggle.Foreground));
        Check(toggle, true);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(toggle.Foreground));

        var plain = Toggle("primary");
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(plain.Foreground));
        Check(plain, true);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(plain.Foreground));
    });

    [Fact]
    public Task AFilledOneFillsWithTheToggledColour() => UiThread.RunAsync(() =>
    {
        var toggle = Toggle("filled", "success");
        Check(toggle, true);

        Assert.Equal(Matrix.Token("FiliSuccessColor"), Matrix.Colour(toggle.Background));
        Assert.Equal(Matrix.Token("FiliSuccessContrastTextColor"), Matrix.Colour(toggle.Foreground));
        Assert.Equal(new Avalonia.Thickness(5), toggle.Padding);
    });

    [Fact]
    public Task AnOutlinedOneDrawsItsLineInTheColour() => UiThread.RunAsync(() =>
    {
        var toggle = Toggle("outlined", "unchecked-secondary");

        Assert.Equal(Matrix.Token("FiliSecondaryColor"), Matrix.Colour(toggle.BorderBrush));
        Assert.Equal(new Avalonia.Thickness(1), toggle.BorderThickness);
    });

    /// <summary>ToggledIcon: of two PathIcons in a Panel, the second shows only while checked.</summary>
    [Fact]
    public Task TheSecondIconIsTheToggledIcon() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon();
        var toggled = new PathIcon();
        var toggle = new ToggleButton { Classes = { "icon" }, Content = new Panel { Children = { icon, toggled } } };
        Matrix.Show(toggle);

        Assert.True(icon.IsVisible);
        Assert.False(toggled.IsVisible);

        Check(toggle, true);
        Assert.False(icon.IsVisible);
        Assert.True(toggled.IsVisible);
    });

    [Fact]
    public Task ALoneIconStaysWhileChecked() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon();
        var toggle = new ToggleButton { Classes = { "icon" }, Content = new Panel { Children = { icon } } };
        Matrix.Show(toggle);
        Check(toggle, true);

        Assert.True(icon.IsVisible);
    });

    /// <summary>A ToggleButton without `icon` keeps MudToggleItem's look.</summary>
    [Fact]
    public Task WithoutIconItIsAToggleItem() => UiThread.RunAsync(() =>
    {
        var toggle = Matrix.Show(new ToggleButton { Content = "Bold" });

        Assert.Equal(new Avalonia.Thickness(6), toggle.Padding);
    });
}
