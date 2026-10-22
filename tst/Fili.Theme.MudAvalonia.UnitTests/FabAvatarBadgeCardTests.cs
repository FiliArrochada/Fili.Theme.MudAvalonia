using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudFab as the <c>fab</c> class on a Button, from _fab.scss and MudFab.razor.cs: filled, Color.Default
/// and Size.Large by default, lifting 6 -> 8 -> 12.
/// </summary>
public class FabTests
{
    private static Button Show(params string[] classes)
    {
        var button = new Button { Content = new PathIcon { Data = Geometry.Parse("M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z") } };
        button.Classes.Add("fab");
        button.Classes.AddRange(classes);
        return Matrix.Show(button);
    }

    internal static BoxShadows Shadow(int level)
    {
        Assert.True(Application.Current!.TryFindResource($"FiliElevation{level}", ThemeVariant.Light, out var value));
        return Assert.IsType<BoxShadows>(value);
    }

    private static void Press(Control target)
    {
        var window = (Window)TopLevel.GetTopLevel(target)!;
        var centre = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseDown(centre, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public Task NoClassIsALargeGreyFilledFabAtElevation6() => UiThread.RunAsync(() =>
    {
        var fab = Show();
        var root = Matrix.Part<Border>(fab, "PART_Root");

        Assert.Equal(56, fab.Bounds.Width);
        Assert.Equal(56, fab.Bounds.Height);
        Assert.Equal(new CornerRadius(28), fab.CornerRadius);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(root.Background));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(fab.Foreground));
        Assert.Equal(Shadow(6), root.BoxShadow);
    });

    [Fact]
    public Task HoverLiftsTo8AndPressTo12() => UiThread.RunAsync(() =>
    {
        var fab = Show();
        var root = Matrix.Part<Border>(fab, "PART_Root");

        Matrix.Hover(fab);
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Matrix.Colour(root.Background));
        Assert.Equal(Shadow(8), root.BoxShadow);

        Press(fab);
        Assert.Equal(Shadow(12), root.BoxShadow);
    });

    [Theory]
    [InlineData("medium", 48)]
    [InlineData("small", 40)]
    public Task SizesAreRoundAtEverySize(string size, double side) => UiThread.RunAsync(() =>
    {
        var fab = Show(size);

        Assert.Equal(side, fab.Bounds.Width);
        Assert.Equal(side, fab.Bounds.Height);
        Assert.Equal(new CornerRadius(side / 2), fab.CornerRadius);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourFillsItAndDarkensOnHover(string colour) => UiThread.RunAsync(() =>
    {
        var fab = Show(colour.ToLowerInvariant());
        var root = Matrix.Part<Border>(fab, "PART_Root");

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(root.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(fab.Foreground));

        Matrix.Hover(fab);
        Assert.Equal(Matrix.Token($"Fili{colour}DarkenColor"), Matrix.Colour(root.Background));
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task OutlinedAndTextAreTheColourWithNoShadow(string colour) => UiThread.RunAsync(() =>
    {
        var cls = colour.ToLowerInvariant();

        var outlined = Show("outlined", cls);
        var line = Matrix.Part<Border>(outlined, "PART_Root");
        Assert.Equal(Colors.Transparent, Matrix.Colour(line.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(line.BorderBrush));
        Assert.Equal(new Thickness(1), line.BorderThickness);
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(outlined.Foreground));
        Assert.Equal(0, line.BoxShadow.Count);
        Matrix.Hover(outlined);
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(line.Background));
        Assert.Equal(0, line.BoxShadow.Count);

        var text = Show("text", cls);
        var ground = Matrix.Part<Border>(text, "PART_Root");
        Assert.Equal(Colors.Transparent, Matrix.Colour(ground.Background));
        Assert.Equal(new Thickness(0), ground.BorderThickness);
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(text.Foreground));
        Matrix.Hover(text);
        Assert.Equal(Matrix.Token($"Fili{colour}HoverColor"), Matrix.Colour(ground.Background));
        Assert.Equal(0, ground.BoxShadow.Count);
    });

    [Fact]
    public Task OutlinedAndTextWithNoColourAreTextPrimary() => UiThread.RunAsync(() =>
    {
        var outlined = Show("outlined");
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(Matrix.Part<Border>(outlined, "PART_Root").BorderBrush));
        Matrix.Hover(outlined);
        Assert.Equal(Matrix.Token("FiliActionDefaultHoverColor"), Matrix.Colour(Matrix.Part<Border>(outlined, "PART_Root").Background));

        var text = Show("text");
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(text.Foreground));
    });

    /// <summary>`.mud-fab-extended`: a pill as wide as its label.</summary>
    [Theory]
    [InlineData(null, 48, 16, 24)]
    [InlineData("medium", 40, 16, 20)]
    [InlineData("small", 34, 12, 17)]
    public Task ExtendedIsAPill(string? size, double height, double padding, double radius) => UiThread.RunAsync(() =>
    {
        var fab = Show(size is null ? ["extended"] : ["extended", size]);
        fab.Content = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { new PathIcon { Classes = { "start-icon" } }, new TextBlock { Text = "Create a new item" } },
        };
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(height, fab.Bounds.Height);
        Assert.Equal(new Thickness(padding, 0), fab.Padding);
        Assert.Equal(new CornerRadius(radius), fab.CornerRadius);
        Assert.True(fab.Bounds.Width > 100, "An extended FAB is as wide as its label.");
    });

    [Theory]
    [InlineData(null, 8)]
    [InlineData("small", 4)]
    public Task TheLabelsGapSitsBetweenIconAndText(string? size, double gap) => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon { Classes = { "start-icon" } };
        var fab = Show(size is null ? ["extended"] : ["extended", size]);
        fab.Content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Children = { icon, new TextBlock { Text = "Go" } } };
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(24, icon.Bounds.Width);
        Assert.Equal(new Thickness(0, 0, gap, 0), icon.Margin);
    });

    [Theory]
    [InlineData("small")]
    [InlineData("medium")]
    [InlineData("large")]
    public Task TheIconIs24AtEverySize(string size) => UiThread.RunAsync(() =>
    {
        var fab = Show(size);

        Assert.Equal(24, Assert.IsType<PathIcon>(fab.Content).Bounds.Width);
    });

    [Fact]
    public Task FlatHasNoShadowInAnyState() => UiThread.RunAsync(() =>
    {
        var fab = Show("primary", "flat");
        var root = Matrix.Part<Border>(fab, "PART_Root");

        Assert.Equal(0, root.BoxShadow.Count);
        Matrix.Hover(fab);
        Assert.Equal(0, root.BoxShadow.Count);
    });

    [Fact]
    public Task DisabledIsGroundedAndGrey() => UiThread.RunAsync(() =>
    {
        var filled = Show("primary");
        filled.IsEnabled = false;
        var outlined = Show("outlined", "primary");
        outlined.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        var root = Matrix.Part<Border>(filled, "PART_Root");
        Assert.Equal(0, root.BoxShadow.Count);
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Matrix.Colour(root.Background));
        Assert.Equal(Matrix.Token("FiliActionDisabledColor"), Matrix.Colour(Matrix.Part<ContentPresenter>(filled, "PART_ContentPresenter").Foreground));

        var line = Matrix.Part<Border>(outlined, "PART_Root");
        Assert.Equal(Colors.Transparent, Matrix.Colour(line.Background));
        Assert.Equal(Matrix.Token("FiliActionDisabledBackgroundColor"), Matrix.Colour(line.BorderBrush));
    });
}

/// <summary>MudAvatar as the <c>avatar</c> class on a Border, from _avatar.scss and MudAvatar.razor.cs.</summary>
public class AvatarTests
{
    private static Border Show(Control? child, params string[] classes)
    {
        var avatar = new Border { Child = child };
        avatar.Classes.Add("avatar");
        avatar.Classes.AddRange(classes);
        return Matrix.Show(avatar);
    }

    [Fact]
    public Task NoClassIsAFilledFortyPixelCircle() => UiThread.RunAsync(() =>
    {
        var initials = new TextBlock { Text = "ON" };
        var avatar = Show(initials);

        Assert.Equal(40, avatar.Bounds.Width);
        Assert.Equal(40, avatar.Bounds.Height);
        Assert.Equal(new CornerRadius(20), avatar.CornerRadius);
        Assert.Equal(Matrix.Token("FiliLinesInputsColor"), Matrix.Colour(avatar.Background));
        Assert.Equal(Matrix.Token("FiliAvatarTextColor"), Matrix.Colour(initials.Foreground));
        Assert.Equal(20, initials.FontSize);

        // Centred, as `.mud-avatar` is.
        var middle = initials.TranslatePoint(new Point(initials.Bounds.Width / 2, initials.Bounds.Height / 2), avatar)!.Value;
        Assert.Equal(20, middle.X, 0.5);
        Assert.Equal(20, middle.Y, 0.5);
    });

    [Theory]
    [InlineData("small", 24, 12)]
    [InlineData("large", 56, 24)]
    public Task Sizes(string size, double side, double fontSize) => UiThread.RunAsync(() =>
    {
        var initials = new TextBlock { Text = "A" };
        var avatar = Show(initials, size);

        Assert.Equal(side, avatar.Bounds.Width);
        Assert.Equal(new CornerRadius(side / 2), avatar.CornerRadius);
        Assert.Equal(fontSize, initials.FontSize);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task FilledAndOutlinedFollowTheColour(string colour) => UiThread.RunAsync(() =>
    {
        var cls = colour.ToLowerInvariant();

        var filledText = new TextBlock { Text = "A" };
        var filled = Show(filledText, cls);
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(filled.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(filledText.Foreground));

        var outlinedText = new TextBlock { Text = "A" };
        var outlined = Show(outlinedText, "outlined", cls);
        Assert.Equal(Colors.Transparent, Matrix.Colour(outlined.Background));
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(outlined.BorderBrush));
        Assert.Equal(new Thickness(1), outlined.BorderThickness);
        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(outlinedText.Foreground));
    });

    [Fact]
    public Task OutlinedWithNoColourIsTextPrimary() => UiThread.RunAsync(() =>
    {
        var avatar = Show(new TextBlock { Text = "A" }, "outlined");

        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(avatar.BorderBrush));
    });

    /// <summary>Variant.Text has no rule in _avatar.scss: the bare `.mud-avatar`, whatever the colour.</summary>
    [Fact]
    public Task TextIsWhiteOnGrayLightWhateverTheColour() => UiThread.RunAsync(() =>
    {
        var initials = new TextBlock { Text = "A" };
        var avatar = Show(initials, "text", "primary");

        Assert.Equal(Matrix.Token("FiliGrayLightColor"), Matrix.Colour(avatar.Background));
        Assert.Equal(Matrix.Token("FiliAvatarBareTextColor"), Matrix.Colour(initials.Foreground));
    });

    [Fact]
    public Task RoundedAndSquare() => UiThread.RunAsync(() =>
    {
        Assert.Equal(new CornerRadius(4), Show(null, "rounded").CornerRadius);
        Assert.Equal(new CornerRadius(0), Show(null, "square", "large").CornerRadius);
    });

    /// <summary>The avatar never clips itself, or an elevation class's shadow would go with it.</summary>
    [Fact]
    public Task ElevationIsTheBordersOwnAndNotClipped() => UiThread.RunAsync(() =>
    {
        var avatar = Show(null, "elevation4");

        Assert.Equal(FabTests.Shadow(4), avatar.BoxShadow);
        Assert.False(avatar.ClipToBounds);
    });

    [Theory]
    [InlineData(new string[0], 40, 20)]
    [InlineData(new[] { "small" }, 24, 12)]
    [InlineData(new[] { "large", "rounded" }, 56, 4)]
    [InlineData(new[] { "outlined" }, 38, 19)]
    [InlineData(new[] { "outlined", "rounded", "small" }, 22, 3)]
    public Task AnImageCoversTheShapeAndIsClippedToIt(string[] classes, double side, double radius) => UiThread.RunAsync(() =>
    {
        var image = new Image { Source = Photo() };
        Show(image, classes);

        Assert.Equal(Stretch.UniformToFill, image.Stretch);
        Assert.Equal(side, image.Bounds.Width);
        var clip = Assert.IsType<RectangleGeometry>(image.Clip);
        Assert.Equal(new Rect(0, 0, side, side), clip.Rect);
        Assert.Equal(radius, clip.RadiusX);
        Assert.Equal(radius, clip.RadiusY);
    });

    /// <summary>A landscape photo, so covering the square has to crop it.</summary>
    internal static WriteableBitmap Photo() => new(new PixelSize(30, 20), new Vector(96, 96));

    [Fact]
    public Task AnImageInASquareAvatarIsNotClipped() => UiThread.RunAsync(() =>
    {
        var image = new Image { Source = Photo() };
        Show(image, "square");

        Assert.Null(image.Clip);
    });

    [Fact]
    public Task AnIconIsMudIconsMedium() => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon();
        Show(icon, "large");

        Assert.Equal(24, icon.Bounds.Width);
    });
}

/// <summary>
/// MudBadge as the <c>badge</c> class on a HeaderedContentControl, from _badge.scss and
/// MudBadge.razor.cs: Content is what it decorates, Header the badge.
/// </summary>
public class BadgeTests
{
    private static HeaderedContentControl Show(object? header, params string[] classes)
    {
        var badge = new HeaderedContentControl
        {
            Header = header,
            Content = new Border { Width = 24, Height = 24, Background = Brushes.Gray },
        };
        badge.Classes.Add("badge");
        badge.Classes.AddRange(classes);
        return Matrix.Show(badge);
    }

    private static Border Pill(HeaderedContentControl badge) => Matrix.Part<Border>(badge, "PART_Badge");

    private static Point Where(HeaderedContentControl badge) => Pill(badge).TranslatePoint(default, badge)!.Value;

    [Fact]
    public Task NoClassIsAGreyPillOnTheTopRightCorner() => UiThread.RunAsync(() =>
    {
        var badge = Show("5");
        var pill = Pill(badge);

        Assert.Equal(20, pill.Bounds.Height);
        Assert.Equal(20, pill.Bounds.Width);
        Assert.Equal(new CornerRadius(10), pill.CornerRadius);
        Assert.Equal(Matrix.Token("FiliGrayLightColor"), Matrix.Colour(pill.Background));
        Assert.Equal(Matrix.Token("FiliBadgeTextColor"), Matrix.Colour(Matrix.Part<ContentPresenter>(badge, "PART_HeaderPresenter").Foreground));

        // `inset: auto auto calc(100% - 4px) calc(100% - 4px)`: 4px into the corner each way.
        Assert.Equal(new Point(24 - 4, 4 - 20), Where(badge));
    });

    /// <summary>The root shrinks to what it decorates, like the inline-block `.mud-badge-root`.</summary>
    [Fact]
    public Task TheRootIsTheSizeOfItsContent() => UiThread.RunAsync(() =>
    {
        var badge = new HeaderedContentControl { Header = "5", Content = new Border { Width = 24, Height = 24 }, Classes = { "badge" } };
        Matrix.Show(new Grid { Width = 300, Height = 100, Children = { badge } });

        Assert.Equal(new Size(24, 24), badge.Bounds.Size);
        Assert.Equal(new Point(24 - 4, 4 - 20), Where(badge));
    });

    [Theory]
    [InlineData("top-left", -16, -16)]
    [InlineData("top-center", 2, -16)]
    [InlineData("center-left", -16, 2)]
    [InlineData("center-center", 2, 2)]
    [InlineData("center-right", 20, 2)]
    [InlineData("bottom-left", -16, 20)]
    [InlineData("bottom-center", 2, 20)]
    [InlineData("bottom-right", 20, 20)]
    public Task OriginPlacesItCalc100MinusFourFromTheOppositeEdge(string origin, double x, double y) => UiThread.RunAsync(() =>
    {
        Assert.Equal(new Point(x, y), Where(Show("5", origin)));
    });

    [Theory]
    [InlineData(null, 12, -8)]
    [InlineData("top-left", -8, -8)]
    [InlineData("center-left", -8, 2)]
    [InlineData("bottom-center", 2, 12)]
    [InlineData("bottom-right", 12, 12)]
    public Task OverlapSitsTwelvePixelsIn(string? origin, double x, double y) => UiThread.RunAsync(() =>
    {
        var badge = origin is null ? Show("5", "overlap") : Show("5", "overlap", origin);

        Assert.Equal(new Point(x, y), Where(badge));
    });

    /// <summary>A badge wider than what it decorates still centres on it.</summary>
    [Fact]
    public Task AWideBadgeCentresOnNarrowContent() => UiThread.RunAsync(() =>
    {
        var badge = Show("1234567", "top-center");
        var pill = Pill(badge);

        Assert.True(pill.Bounds.Width > 24);
        Assert.Equal(12, Where(badge).X + pill.Bounds.Width / 2, 0.5);
    });

    [Theory]
    [MemberData(nameof(Matrix.Colours), MemberType = typeof(Matrix))]
    public Task AColourIsTheBadgesAlone(string colour) => UiThread.RunAsync(() =>
    {
        var icon = new PathIcon();
        var badge = new HeaderedContentControl { Header = 3, Content = icon, Classes = { "badge", colour.ToLowerInvariant() } };
        Matrix.Show(badge);

        Assert.Equal(Matrix.Token($"Fili{colour}Color"), Matrix.Colour(Pill(badge).Background));
        Assert.Equal(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(Matrix.Part<ContentPresenter>(badge, "PART_HeaderPresenter").Foreground));

        // `.mud-theme-{color}` is on the badge's span, not on MudBadge's root.
        Assert.NotEqual(Matrix.Token($"Fili{colour}ContrastTextColor"), Matrix.Colour(icon.Foreground));
    });

    [Fact]
    public Task TheTextIsTwelvePixelsOnATwelvePixelLine() => UiThread.RunAsync(() =>
    {
        var badge = Show("5");
        var text = Matrix.Part<ContentPresenter>(badge, "PART_HeaderPresenter").GetVisualDescendants().OfType<TextBlock>().Single();

        Assert.Equal(12, text.FontSize);
        Assert.Equal(12, text.Bounds.Height);
        Assert.Equal(0, text.LetterSpacing);
    });

    [Theory]
    [InlineData(99, "99")]
    [InlineData(100, "99+")]
    [InlineData(4000, "99+")]
    public Task AnIntOverMaxReads99Plus(int count, string shown) => UiThread.RunAsync(() =>
    {
        var badge = Show(count);

        Assert.Equal(shown, Matrix.Part<ContentPresenter>(badge, "PART_HeaderPresenter").Content?.ToString());
    });

    [Fact]
    public Task ANullHeaderIsVisibleFalse() => UiThread.RunAsync(() =>
    {
        var badge = Show(null);

        Assert.False(Matrix.Part<Panel>(badge, "PART_BadgeAnchor").IsVisible);

        badge.Header = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.True(Matrix.Part<Panel>(badge, "PART_BadgeAnchor").IsVisible);
    });

    [Fact]
    public Task ADotIsANinePixelCircleWithNothingInIt() => UiThread.RunAsync(() =>
    {
        var badge = Show(7, "dot", "error");
        var pill = Pill(badge);

        Assert.Equal(new Size(9, 9), pill.Bounds.Size);
        Assert.Equal(new CornerRadius(4.5), pill.CornerRadius);
        Assert.False(Matrix.Part<Viewbox>(badge, "PART_HeaderBox").IsVisible);
        Assert.Equal(new Point(24 - 4, 4 - 9), Where(badge));
    });

    [Fact]
    public Task BorderedIsATwoPixelSurfaceRing() => UiThread.RunAsync(() =>
    {
        var pill = Pill(Show("5", "bordered", "primary"));

        Assert.Equal(new Thickness(2), pill.BorderThickness);
        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(pill.BorderBrush));
        Assert.Equal(20, pill.Bounds.Height);
    });

    [Fact]
    public Task AnIconIsTwelvePixelsInATwentyPixelCircle() => UiThread.RunAsync(() =>
    {
        var badge = Show(new PathIcon { Data = Geometry.Parse("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20z") }, "icon", "primary");
        var pill = Pill(badge);
        var box = Matrix.Part<Viewbox>(badge, "PART_HeaderBox");

        Assert.Equal(new Size(20, 20), pill.Bounds.Size);
        Assert.Equal(new Size(12, 12), box.Bounds.Size);
        Assert.Equal(new Point(4, 4), box.TranslatePoint(default, pill)!.Value);
    });

    [Fact]
    public Task ElevationIsOnTheBadge() => UiThread.RunAsync(() =>
    {
        var badge = Show("5", "elevation4");

        Assert.Equal(FabTests.Shadow(4), Pill(badge).BoxShadow);
    });

    /// <summary>
    /// The anchor reaches 10000px past the content and must never catch the pointer: only the pill
    /// is `pointer-events: auto`.
    /// </summary>
    [Fact]
    public Task OnlyThePillIsHitTestable() => UiThread.RunAsync(() =>
    {
        var badge = new HeaderedContentControl { Header = "5", Content = new Border { Width = 24, Height = 24 }, Classes = { "badge" } };
        Matrix.Show(new Border { Padding = new Thickness(60), Child = badge });
        var window = (Window)TopLevel.GetTopLevel(badge)!;
        var pill = Pill(badge);
        var anchor = Matrix.Part<Panel>(badge, "PART_BadgeAnchor");

        Assert.Null(anchor.Background);

        Assert.False(badge.ClipToBounds);

        // The pill, drawn past the control's edge, takes the pointer.
        Matrix.Hover(pill);

        // Past it, inside the anchor's reach, nothing of the badge does.
        window.MouseMove(pill.TranslatePoint(new Point(40, -20), window)!.Value, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(anchor.IsPointerOver);
        Assert.False(badge.IsPointerOver);
    });
}

/// <summary>
/// The text on MudBlazor's light-grey grounds has tokens of its own only so High Contrast can make
/// it black. In light and dark it must stay exactly what _avatar.scss and _badge.scss say.
/// </summary>
public class GreyGroundTextTests
{
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public Task InLightAndDarkItIsMudBlazorsOwn(string variant) => UiThread.RunAsync(() =>
    {
        var theme = variant == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        Assert.Equal(Colour("FiliTextPrimaryColor", theme), Colour("FiliAvatarTextColor", theme));
        Assert.Equal(Colour("FiliTextPrimaryColor", theme), Colour("FiliBadgeTextColor", theme));
        Assert.Equal(Colour("FiliWhiteColor", theme), Colour("FiliAvatarBareTextColor", theme));
    });

    [Fact]
    public Task InHighContrastItIsBlackOnTheNearWhiteGrounds() => UiThread.RunAsync(() =>
    {
        var theme = FiliThemeVariants.HighContrast;

        Assert.Equal(Colors.Black, Colour("FiliAvatarTextColor", theme));
        Assert.Equal(Colors.Black, Colour("FiliBadgeTextColor", theme));
        Assert.Equal(Colors.Black, Colour("FiliAvatarBareTextColor", theme));
    });

    private static Color Colour(string key, ThemeVariant theme)
    {
        Assert.True(Application.Current!.TryFindResource(key, theme, out var value), $"{key} did not resolve.");
        return Assert.IsType<Color>(value);
    }
}

/// <summary>MudPaper as the <c>paper</c> class on a Border, from _paper.scss and MudPaper.razor.cs.</summary>
public class PaperTests
{
    /// <summary>MudPaper's Elevation defaults to 1: a bare paper casts it, in text-primary.</summary>
    [Fact]
    public Task APaperIsAtElevation1() => UiThread.RunAsync(() =>
    {
        var paper = Matrix.Show(new Border { Classes = { "paper" }, Width = 200, Height = 100 });

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(paper.Background));
        Assert.Equal(new CornerRadius(4), paper.CornerRadius);
        Assert.Equal(FabTests.Shadow(1), paper.BoxShadow);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(paper.GetValue(TextElement.ForegroundProperty)));
    });

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public Task AnElevationClassReplacesIt(int level) => UiThread.RunAsync(() =>
    {
        var paper = Matrix.Show(new Border { Classes = { "paper", $"elevation{level}" } });

        Assert.Equal(FabTests.Shadow(level), paper.BoxShadow);
    });

    /// <summary>MudPaper only adds `mud-elevation-{n}` when it is not outlined.</summary>
    [Fact]
    public Task OutlinedIsALineAndNoShadowWhateverTheElevation() => UiThread.RunAsync(() =>
    {
        var paper = Matrix.Show(new Border { Classes = { "paper", "outlined", "elevation8" } });

        Assert.Equal(0, paper.BoxShadow.Count);
        Assert.Equal(new Thickness(1), paper.BorderThickness);
    });
}

/// <summary>MudCard as the <c>card</c> class on a Border, with its parts as classes, from _card.scss.</summary>
public class CardTests
{
    [Fact]
    public Task ACardIsPaperAtElevation1() => UiThread.RunAsync(() =>
    {
        var card = Matrix.Show(new Border { Classes = { "card" }, Width = 200, Height = 100 });

        Assert.Equal(Matrix.Token("FiliSurfaceColor"), Matrix.Colour(card.Background));
        Assert.Equal(new CornerRadius(4), card.CornerRadius);
        Assert.Equal(FabTests.Shadow(1), card.BoxShadow);
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(card.GetValue(TextElement.ForegroundProperty)));
    });

    [Fact]
    public Task AnElevationClassReplacesIt() => UiThread.RunAsync(() =>
    {
        var card = Matrix.Show(new Border { Classes = { "card", "elevation8" } });

        Assert.Equal(FabTests.Shadow(8), card.BoxShadow);
    });

    /// <summary>MudPaper only adds `mud-elevation-{n}` when it is not outlined.</summary>
    [Fact]
    public Task OutlinedIsALineAndNoShadowWhateverTheElevation() => UiThread.RunAsync(() =>
    {
        var card = Matrix.Show(new Border { Classes = { "card", "outlined", "elevation8" } });

        Assert.Equal(0, card.BoxShadow.Count);
        Assert.Equal(Matrix.Token("FiliLinesDefaultColor"), Matrix.Colour(card.BorderBrush));
        Assert.Equal(new Thickness(1), card.BorderThickness);
    });

    [Fact]
    public Task SquareHasNoRadiusOnACard() => UiThread.RunAsync(() =>
    {
        Assert.Equal(new CornerRadius(0), Matrix.Show(new Border { Classes = { "card", "square" } }).CornerRadius);
    });

    [Fact]
    public Task TheHeaderLaysOutAvatarContentAndActions() => UiThread.RunAsync(() =>
    {
        var avatar = new Border { Classes = { "avatar", "card-header-avatar" } };
        var actions = new Button { Classes = { "icon", "card-header-actions" }, Content = new PathIcon() };
        var content = new TextBlock { Classes = { "card-header-content", "body1" }, Text = "Shrimp and Chorizo Paella" };
        var header = new DockPanel { Classes = { "card-header" }, Children = { avatar, actions, content } };
        var card = Matrix.Show(new Border { Classes = { "card" }, Width = 345, Child = new StackPanel { Children = { header } } });

        Assert.Equal(new Point(16, 16), avatar.TranslatePoint(default, card)!.Value);
        Assert.Equal(16 + 40 + 16, content.TranslatePoint(default, card)!.Value.X);
        Assert.Equal(20, content.TranslatePoint(new Point(0, content.Bounds.Height / 2), avatar)!.Value.Y, 0.5);

        // `margin-top: -8px; margin-right: -8px`, and `align-self: flex-start`.
        var corner = actions.TranslatePoint(new Point(actions.Bounds.Width, 0), card)!.Value;
        Assert.Equal(new Point(345 - 16 + 8, 16 - 8), corner);
    });

    [Fact]
    public Task MediaIs300HighAndTakesTheCardsTopCornersOnlyWhenFirst() => UiThread.RunAsync(() =>
    {
        var image = new Image();
        var first = new Border { Classes = { "card-media" }, Child = image };
        var second = new Border { Classes = { "card-media" } };
        Matrix.Show(new Border { Classes = { "card" }, Width = 300, Child = new StackPanel { Children = { first, second } } });

        Assert.Equal(300, first.Bounds.Height);
        Assert.True(first.ClipToBounds);
        Assert.Equal(Stretch.UniformToFill, image.Stretch);
        Assert.Equal(new CornerRadius(4, 4, 0, 0), first.CornerRadius);
        Assert.Equal(new CornerRadius(0), second.CornerRadius);
    });

    [Fact]
    public Task MediaInASquareCardIsSquare() => UiThread.RunAsync(() =>
    {
        var media = new Border { Classes = { "card-media" } };
        Matrix.Show(new Border { Classes = { "card", "square" }, Width = 300, Child = new StackPanel { Children = { media } } });

        Assert.Equal(new CornerRadius(0), media.CornerRadius);
    });

    [Fact]
    public Task ContentIs16RoundAndActionsARow8Round() => UiThread.RunAsync(() =>
    {
        var content = new TextBlock { Classes = { "card-content" }, Text = "Body" };
        var first = new Button { Content = "Share" };
        var second = new Button { Content = "Learn more" };
        var actions = new StackPanel { Classes = { "card-actions" }, Children = { first, second } };
        var card = Matrix.Show(new Border { Classes = { "card" }, Width = 300, Child = new StackPanel { Children = { content, actions } } });

        Assert.Equal(new Thickness(16), content.Margin);
        Assert.Equal(new Thickness(8), actions.Margin);
        Assert.Equal(Avalonia.Layout.Orientation.Horizontal, actions.Orientation);
        Assert.Equal(8, first.TranslatePoint(default, card)!.Value.X);
        Assert.Equal(first.TranslatePoint(default, card)!.Value.Y, second.TranslatePoint(default, card)!.Value.Y);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Center, second.VerticalAlignment);
    });
}
