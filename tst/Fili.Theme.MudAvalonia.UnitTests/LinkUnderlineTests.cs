using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudLink's Underline: Hover by default (underlined under the pointer, on keyboard focus and
/// pressed), Always as `underline`, None as `no-underline`. The underline is set on the link and
/// inherited, so it reaches a string's text and a TextBlock the app puts in the content alike.
/// </summary>
public class LinkUnderlineTests
{
    private static TextBlock Text(HyperlinkButton link) =>
        link.GetVisualDescendants().OfType<TextBlock>().First();

    private static bool Underlined(TextBlock text) => text.TextDecorations is { Count: > 0 };

    public static TheoryData<bool> Contents() => new() { false, true };

    [Theory]
    [MemberData(nameof(Contents))]
    public Task UnderlineIsAlwaysUnderlined(bool ownText) => UiThread.RunAsync(() =>
    {
        var link = Matrix.Show(new HyperlinkButton { Classes = { "underline" }, Content = ownText ? new TextBlock { Text = "Terms" } : "Terms" });

        Assert.True(Underlined(Text(link)));
    });

    [Theory]
    [MemberData(nameof(Contents))]
    public Task HoverUnderlinesOnlyUnderThePointer(bool ownText) => UiThread.RunAsync(() =>
    {
        var link = Matrix.Show(new HyperlinkButton { Content = ownText ? new TextBlock { Text = "Terms" } : "Terms" });
        Assert.False(Underlined(Text(link)));

        Matrix.Hover(link);
        Assert.True(Underlined(Text(link)));
    });

    [Fact]
    public Task NoUnderlineStaysPlainUnderThePointer() => UiThread.RunAsync(() =>
    {
        var link = Matrix.Show(new HyperlinkButton { Classes = { "no-underline" }, Content = "Terms" });

        Matrix.Hover(link);
        Assert.False(Underlined(Text(link)));
    });
}
