using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The baseline keys do what <c>published-keys.md</c> says. <c>FiliFontSize</c> was published as
/// the app-wide baseline while the roots read <c>FiliBody2FontSize</c>, so an app that set it saw
/// nothing change. Now it is every window's and popup's size; <c>FiliLineHeight</c> and
/// <c>FiliLetterSpacing</c> are for an app's own styles and stay off unclassed text, which keeps
/// the font's natural line.
/// </summary>
public class BaselineTypographyTests
{
    /// <summary>Runs <paramref name="body"/> with <paramref name="key"/> set in the application's resources.</summary>
    private static void WithAppResource(string key, object value, Action body)
    {
        var resources = Application.Current!.Resources;
        resources[key] = value;
        try
        {
            body();
        }
        finally
        {
            resources.Remove(key);
        }
    }

    [Fact]
    public Task AnAppsFontSizeReachesAWindow() => UiThread.RunAsync(() =>
        WithAppResource("FiliFontSize", 30d, () =>
        {
            var text = new TextBlock { Text = "Body" };
            var window = new Window { Content = new UserControl { Content = text } };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(30, text.FontSize);
            window.Close();
        }));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AnAppsFontSizeReachesAPopup(bool overlay) => UiThread.RunAsync(() =>
        WithAppResource("FiliFontSize", 30d, () =>
        {
            var text = new TextBlock { Text = "In a popup" };
            var target = new Button { Content = "Anchor" };
            var popup = new Popup { Child = text, PlacementTarget = target, ShouldUseOverlayLayer = overlay };
            var window = new Window { Content = new StackPanel { Children = { target, popup } } };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(30, text.FontSize);
            popup.IsOpen = false;
            window.Close();
        }));

    /// <summary>The default is body2's, so a theme that does not set the key renders as before.</summary>
    [Fact]
    public Task TheBaselineSizeIsBody2s() => UiThread.RunAsync(() =>
    {
        var app = Application.Current!;
        Assert.True(app.TryFindResource("FiliFontSize", out var baseline));
        Assert.True(app.TryFindResource("FiliBody2FontSize", out var body2));
        Assert.Equal(body2, baseline);
        Assert.True(app.TryFindResource("FiliLineHeight", out var line));
        Assert.True(app.TryFindResource("FiliBody2LineHeight", out var body2Line));
        Assert.Equal(body2Line, line);
        Assert.True(app.TryFindResource("FiliLetterSpacing", out var spacing));
        Assert.True(app.TryFindResource("FiliBody2LetterSpacing", out var body2Spacing));
        Assert.Equal(body2Spacing, spacing);
    });

    /// <summary>
    /// Line height and spacing are not inherited from the roots: an app's own 24px text under an
    /// inherited 20px line would clip. The keys are there for an app's own styles to read.
    /// </summary>
    [Theory]
    [InlineData("FiliLineHeight")]
    [InlineData("FiliLetterSpacing")]
    public Task UnclassedTextKeepsTheFontsOwnLine(string key) => UiThread.RunAsync(() =>
        WithAppResource(key, 40d, () =>
        {
            var text = new TextBlock { Text = "Body" };
            var window = new Window { Content = text };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.True(double.IsNaN(text.LineHeight));
            Assert.Equal(0, text.LetterSpacing);
            window.Close();
        }));

    /// <summary>An app's style that reads the baseline keys gets the app's values.</summary>
    [Fact]
    public Task AnAppsStyleCanReadTheBaselineLine() => UiThread.RunAsync(() =>
        WithAppResource("FiliLineHeight", 40d, () =>
            WithAppResource("FiliLetterSpacing", 2d, () =>
            {
                var text = new TextBlock { Text = "Body" };
                text[!TextBlock.LineHeightProperty] = text.GetResourceObservable("FiliLineHeight").ToBinding();
                text[!TextBlock.LetterSpacingProperty] = text.GetResourceObservable("FiliLetterSpacing").ToBinding();
                var window = new Window { Content = text };
                window.Show();
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(40, text.LineHeight);
                Assert.Equal(2, text.LetterSpacing);
                window.Close();
            })));
}
