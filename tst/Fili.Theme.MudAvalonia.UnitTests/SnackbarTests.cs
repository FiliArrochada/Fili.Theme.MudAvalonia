using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudSnackbarElement's card: the severity icon, the message and the close button, and
/// SnackbarVariant, Severity.Normal, HideIcon and ShowCloseIcon as classes on the card.
/// </summary>
public class SnackbarTests
{
    private static NotificationCard Card(NotificationType type, params string[] classes)
    {
        var card = new NotificationCard { NotificationType = type, Content = "Saved" };
        card.Classes.AddRange(classes);
        Matrix.Show(card);
        Dispatcher.UIThread.RunJobs();
        return card;
    }

    private static Geometry Glyph(string key)
    {
        Application.Current!.TryFindResource(key, out var value);
        return Assert.IsAssignableFrom<Geometry>(value);
    }

    [Theory]
    [InlineData(NotificationType.Information, "FiliInfoOutlinedGeometry")]
    [InlineData(NotificationType.Success, "FiliAlertSuccessGeometry")]
    [InlineData(NotificationType.Warning, "FiliReportProblemGeometry")]
    [InlineData(NotificationType.Error, "FiliErrorOutlineGeometry")]
    public Task EverySeverityHasItsIcon(NotificationType type, string glyph) => UiThread.RunAsync(() =>
    {
        var icon = Matrix.Part<Avalonia.Controls.Shapes.Path>(Card(type), "PART_Icon");

        Assert.True(icon.IsVisible);
        Assert.Same(Glyph(glyph), icon.Data);
        Assert.Equal(24, icon.Width);
        Assert.Equal(new Thickness(0, 7, 12, 7), icon.Margin);
    });

    [Fact]
    public Task NormalIsFilledDarkWithTheEventNoteIcon() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Information, "normal");

        Assert.Equal(Matrix.Token("FiliDarkColor"), Matrix.Colour(card.Background));
        Assert.Equal(Matrix.Token("FiliDarkContrastTextColor"), Matrix.Colour(card.Foreground));
        Assert.Same(Glyph("FiliEventNoteGeometry"), Matrix.Part<Avalonia.Controls.Shapes.Path>(card, "PART_Icon").Data);
    });

    /// <summary>ShowCloseIcon defaults to true: a small icon button, in the card's own colour.</summary>
    [Fact]
    public Task TheCloseButtonClosesTheCard() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Success);
        var close = Matrix.Part<Button>(card, "PART_CloseButton");

        Assert.True(close.IsVisible);
        Assert.True(NotificationCard.GetCloseOnClick(close));
        Assert.Equal(new Thickness(3), close.Padding);
        Assert.Equal(20, Assert.IsType<Viewbox>(close.Content).Bounds.Width);
        Assert.Equal(Matrix.Token("FiliSuccessContrastTextColor"), Matrix.Colour(close.Foreground));
    });

    [Fact]
    public Task HideIconAndHideCloseTakeThemAway() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Warning, "hide-icon", "hide-close");

        Assert.False(Matrix.Part<Avalonia.Controls.Shapes.Path>(card, "PART_Icon").IsVisible);
        Assert.False(Matrix.Part<Button>(card, "PART_CloseButton").IsVisible);
    });

    /// <summary>
    /// SnackbarVariant.Text: `.mud-snackbar-surface` beats the alert's tint, so the surface, with
    /// the text in the colour's darken shade at 400 and an action-default close button.
    /// </summary>
    [Fact]
    public Task TextIsTheColourOnTheSurface() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Error, "text");

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(card.Background));
        Assert.Equal(Matrix.Token("FiliErrorDarkenColor"), Matrix.Colour(card.Foreground));
        Assert.Equal(FontWeight.Normal, card.FontWeight);
        Assert.Equal(new Thickness(0), card.BorderThickness);
        Assert.Equal(Matrix.Token("FiliActionDefaultColor"), Matrix.Colour(Matrix.Part<Button>(card, "PART_CloseButton").Foreground));
    });

    [Fact]
    public Task OutlinedAddsALineInTheColour() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Information, "outlined");

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(card.Background));
        Assert.Equal(Matrix.Token("FiliInfoColor"), Matrix.Colour(card.BorderBrush));
        Assert.Equal(new Thickness(1), card.BorderThickness);
    });

    /// <summary>
    /// Filled, and Regular: `.mud-snackbar`'s 400 and `.mud-alert-filled-*`'s 500 are one class
    /// each, and _snackbar comes after _alert in MudBlazor.scss, so 400 wins.
    /// </summary>
    [Fact]
    public Task FilledStaysTheDefault() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Information);

        Assert.Equal(Matrix.Token("FiliInfoColor"), Matrix.Colour(card.Background));
        Assert.Equal(FontWeight.Normal, card.FontWeight);
    });

    /// <summary>The card's elevation 6 shadow falls past its edge: nothing round the card clips it.</summary>
    [Fact]
    public Task TheCardsShadowIsNotClipped() => UiThread.RunAsync(() =>
    {
        var card = Card(NotificationType.Information);

        Assert.All(card.GetVisualDescendants().OfType<Control>().TakeWhile(c => c.Name != "PART_Card"),
            c => Assert.False(c.ClipToBounds, $"{c.GetType().Name}#{c.Name} clips the card's shadow."));
    });
}
