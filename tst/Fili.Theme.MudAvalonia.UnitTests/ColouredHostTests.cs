using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A user control inherits its host's text, as any other control does. The theme set the body
/// font, size and text-primary on every UserControl, and a style setter beats an inherited value:
/// a user control in a filled button, an app bar or a filled alert drew its text in grey on the
/// colour. Now only the roots - a window, a popup's - take them.
/// </summary>
public class ColouredHostTests
{
    public static TheoryData<string> Hosts => ["button", "appbar", "alert"];

    /// <summary>The host, with a plain TextBlock and a user control's TextBlock side by side in it.</summary>
    private static (TextBlock Plain, TextBlock InUserControl) Show(string host)
    {
        var plain = new TextBlock { Text = "Plain" };
        var inner = new TextBlock { Text = "Inside" };
        var content = new StackPanel { Children = { plain, new UserControl { Content = inner } } };
        Control root = host switch
        {
            "button" => new Button { Classes = { "filled", "primary" }, Content = content },
            "appbar" => new Border { Classes = { "appbar" }, Child = content },
            "alert" => new Border { Classes = { "alert", "filled", "error" }, Child = content },
            _ => throw new ArgumentOutOfRangeException(nameof(host)),
        };
        new Window { Content = root }.Show();
        Dispatcher.UIThread.RunJobs();
        return (plain, inner);
    }

    private static Color Colour(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    [Theory]
    [MemberData(nameof(Hosts))]
    public Task AUserControlKeepsItsHostsText(string host) => UiThread.RunAsync(() =>
    {
        var (plain, inner) = Show(host);

        Assert.Equal(Colour(plain.Foreground), Colour(inner.Foreground));
        Assert.Equal(plain.FontSize, inner.FontSize);
    });

    /// <summary>The roots still set the body text: a bare window's text is text-primary in the theme's font.</summary>
    [Fact]
    public Task AWindowSetsTheBodyText() => UiThread.RunAsync(() =>
    {
        var text = new TextBlock { Text = "Body" };
        new Window { Content = new UserControl { Content = text } }.Show();
        Dispatcher.UIThread.RunJobs();

        var variant = Avalonia.Application.Current!.ActualThemeVariant;
        Assert.True(Avalonia.Application.Current.TryFindResource("FiliTextPrimaryColor", variant, out var primary));
        Assert.Equal((Color)primary!, Colour(text.Foreground));
        Assert.Equal(14, text.FontSize);
        Assert.True(Avalonia.Application.Current!.TryFindResource("FiliFontFamily", out var family));
        Assert.Equal(family, text.FontFamily);
    });
}
