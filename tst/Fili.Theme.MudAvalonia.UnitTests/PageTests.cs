using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;
using Path = Avalonia.Controls.Shapes.Path;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Avalonia's page types and Carousel in MudBlazor's layout components: a NavigationPage's bar is
/// MudAppBar, a TabbedPage is MudTabs, a Carousel and a CarouselPage are MudCarousel.
/// </summary>
public class PageTests
{
    private static NavigationPage Navigation(params ContentPage[] pages)
    {
        var nav = new NavigationPage { Width = 420, Height = 300, PageTransition = null };
        Matrix.Show(nav);
        foreach (var page in pages)
        {
            var push = nav.PushAsync(page);
            for (var i = 0; i < 50 && !push.IsCompleted; i++)
            {
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }

            Assert.True(push.IsCompleted);
        }

        Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        return nav;
    }

    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    /// <summary>A ContentPage is text-primary on the app's background.</summary>
    [Fact]
    public Task AContentPageIsOnTheBackground() => UiThread.RunAsync(() =>
    {
        var page = Matrix.Show(new ContentPage { Width = 300, Height = 200 });

        Assert.Equal(Matrix.Token("FiliBackgroundColor"), Matrix.Colour(page.Background));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(page.Foreground));
    });

    /// <summary>
    /// A NavigationPage's bar is MudAppBar as `appbar` draws it: the appbar colours, 24px gutters,
    /// 64px, elevation 4; the page's Header is h6.
    /// </summary>
    [Fact]
    public Task TheBarIsAnAppBar() => UiThread.RunAsync(() =>
    {
        var nav = Navigation(new ContentPage { Header = "Inbox" });
        var bar = Part<Border>(nav, "PART_NavigationBar");

        Assert.Equal(Matrix.Token("FiliAppbarBackgroundColor"), Matrix.Colour(bar.Background));
        Assert.Equal(Matrix.Token("FiliAppbarTextColor"), Matrix.Colour(TextElement.GetForeground(bar)));
        Assert.Equal(new Thickness(24, 0), Part<Grid>(nav, "PART_BarContent").Margin);
        Assert.Equal(64, bar.Bounds.Height);
        Assert.True(Application.Current!.TryGetResource("FiliElevation4", Application.Current.ActualThemeVariant, out var elevation4));
        Assert.Equal(elevation4, bar.BoxShadow);
        Assert.Equal(20, Part<Avalonia.Controls.Presenters.ContentPresenter>(nav, "PART_Header").FontSize);
    });

    /// <summary>HasShadow="False" is Elevation 0.</summary>
    [Fact]
    public Task NoShadowIsElevationZero() => UiThread.RunAsync(() =>
    {
        var nav = Navigation(new ContentPage { Header = "Inbox" });
        nav.HasShadow = false;
        Dispatcher.UIThread.RunJobs();

        Assert.True(Application.Current!.TryGetResource("FiliElevation0", Application.Current.ActualThemeVariant, out var elevation0));
        Assert.Equal(elevation0, Part<Border>(nav, "PART_NavigationBar").BoxShadow);
    });

    /// <summary>The back button is an edge-start MudIconButton with ArrowBack, in the bar's text colour.</summary>
    [Fact]
    public Task TheBackButtonIsArrowBack() => UiThread.RunAsync(() =>
    {
        var nav = Navigation(new ContentPage { Header = "Inbox" }, new ContentPage { Header = "Message" });
        var back = Part<Button>(nav, "PART_BackButton");

        Assert.True(back.IsVisible);
        Assert.Contains("icon", back.Classes);
        Assert.Equal(new Thickness(-12, 0, 12, 0), back.Margin);
        Assert.Same(Application.Current!.FindResource("FiliArrowBackGeometry"), Part<Path>(back, "PART_BackButtonDefaultIcon").Data);
        Assert.Equal(Matrix.Token("FiliAppbarTextColor"), Matrix.Colour(back.Foreground));
    });

    /// <summary>A page's TopCommandBar sits on the bar, its commands in the bar's text colour.</summary>
    [Fact]
    public Task ThePagesCommandsTakeTheBarsColour() => UiThread.RunAsync(() =>
    {
        var bar = new CommandBar();
        var add = new CommandBarButton { Label = "Add", Icon = new PathIcon { Data = (Geometry)Application.Current!.FindResource("FiliEventGeometry")! } };
        bar.PrimaryCommands.Add(add);
        var page = new ContentPage { Header = "Inbox" };
        NavigationPage.SetTopCommandBar(page, bar);
        Navigation(page);

        Assert.Equal(Matrix.Token("FiliAppbarTextColor"), Matrix.Colour(add.Foreground));
        Assert.Equal(Matrix.Token("FiliAppbarTextColor"), Matrix.Colour(Part<Button>(bar, "PART_OverflowButton").Foreground));
    });

    /// <summary>A TabbedPage is MudTabs: the theme's tabs, the selected one in primary.</summary>
    [Fact]
    public Task ATabbedPageIsMudTabs() => UiThread.RunAsync(() =>
    {
        var tabs = new TabbedPage { Width = 600, Height = 300 };
        tabs.Pages = new List<Page> { new ContentPage { Header = "Home" }, new ContentPage { Header = "Recent" } };
        Matrix.Show(tabs);
        var items = tabs.GetVisualDescendants().OfType<TabItem>().ToList();

        Assert.Equal(48, items[0].Bounds.Height);
        Assert.Equal(Matrix.Token("FiliPrimaryColor"), Matrix.Colour(items.Single(i => i.IsSelected).Foreground));
        Assert.Equal(Matrix.Token("FiliTextPrimaryColor"), Matrix.Colour(items.Single(i => !i.IsSelected).Foreground));
    });

    /// <summary>
    /// A Carousel is MudCarousel: NavigateBefore and NavigateNext icon buttons at the sides and the
    /// bullets along the bottom, at .75; the buttons move it, and a bullet follows the selection.
    /// </summary>
    [Fact]
    public Task ACarouselHasArrowsAndBullets() => UiThread.RunAsync(() =>
    {
        var carousel = Matrix.Show(new Carousel { Width = 400, Height = 240, ItemsSource = new[] { "One", "Two", "Three" } });
        var previous = Part<Button>(carousel, "PART_PreviousButton");
        var next = Part<Button>(carousel, "PART_NextButton");
        var bullets = Part<PipsPager>(carousel, "PART_Bullets");

        Assert.Contains("icon", previous.Classes);
        Assert.Equal(0.75, next.Opacity);
        Assert.Same(Application.Current!.FindResource("FiliNavigateBeforeGeometry"), previous.GetVisualDescendants().OfType<Path>().Single().Data);
        Assert.Same(Application.Current!.FindResource("FiliChevronRightGeometry"), next.GetVisualDescendants().OfType<Path>().Single().Data);
        Assert.Equal(3, bullets.NumberOfPages);

        next.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, carousel.SelectedIndex);
        Assert.Equal(1, bullets.SelectedPageIndex);

        bullets.SelectedPageIndex = 2;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, carousel.SelectedIndex);
    });

    /// <summary>
    /// The bullets are Color.Inherit, as MudCarousel's are: every bullet in the carousel's text
    /// colour, the current one told apart by its glyph alone, so it shows on a coloured slide.
    /// </summary>
    [Fact]
    public Task TheBulletsAreTheCarouselsTextColour() => UiThread.RunAsync(() =>
    {
        var carousel = Matrix.Show(new Carousel { Width = 400, Height = 240, Foreground = Brushes.OrangeRed, ItemsSource = new[] { "One", "Two", "Three" } });
        var pips = Part<PipsPager>(carousel, "PART_Bullets").GetVisualDescendants().OfType<Path>().Where(p => p.Name == "Pip").ToList();

        Assert.Equal(3, pips.Count);
        Assert.All(pips, pip => Assert.Equal(Colors.OrangeRed, Matrix.Colour(pip.Fill)));
        Assert.Same(Application.Current!.FindResource("FiliRadioCheckedGeometry"), pips[0].Data);
        Assert.Same(Application.Current!.FindResource("FiliRadioUncheckedGeometry"), pips[1].Data);
    });

    /// <summary>ShowArrows="false" is `no-arrows`, ShowBullets="false" `no-bullets`.</summary>
    [Fact]
    public Task TheArrowsAndBulletsTurnOff() => UiThread.RunAsync(() =>
    {
        var carousel = new Carousel { Width = 400, Height = 240, ItemsSource = new[] { "One", "Two" } };
        carousel.Classes.AddRange(["no-arrows", "no-bullets"]);
        Matrix.Show(carousel);

        Assert.False(Part<Button>(carousel, "PART_PreviousButton").IsVisible);
        Assert.False(Part<Button>(carousel, "PART_NextButton").IsVisible);
        Assert.False(Part<PipsPager>(carousel, "PART_Bullets").IsVisible);
    });

    /// <summary>
    /// ArrowsPosition defaults to Center and BulletsPosition to Bottom; `arrows-top`,
    /// `arrows-bottom`, `bullets-top` and `bullets-center` are the others, as `align-self-*`.
    /// </summary>
    [Fact]
    public Task TheArrowsAndBulletsMove() => UiThread.RunAsync(() =>
    {
        Carousel Show(params string[] classes)
        {
            var carousel = new Carousel { Width = 400, Height = 240, ItemsSource = new[] { "One", "Two" } };
            carousel.Classes.AddRange(classes);
            return Matrix.Show(carousel);
        }

        void Arrows(Carousel carousel, VerticalAlignment expected)
        {
            Assert.Equal(expected, Part<Button>(carousel, "PART_PreviousButton").VerticalAlignment);
            Assert.Equal(expected, Part<Button>(carousel, "PART_NextButton").VerticalAlignment);
        }

        VerticalAlignment Bullets(Carousel carousel) => Part<PipsPager>(carousel, "PART_Bullets").VerticalAlignment;

        var plain = Show();
        Arrows(plain, VerticalAlignment.Center);
        Assert.Equal(VerticalAlignment.Bottom, Bullets(plain));

        Arrows(Show("arrows-top"), VerticalAlignment.Top);
        Arrows(Show("arrows-bottom"), VerticalAlignment.Bottom);
        Assert.Equal(VerticalAlignment.Top, Bullets(Show("bullets-top")));
        Assert.Equal(VerticalAlignment.Center, Bullets(Show("bullets-center")));
    });

    /// <summary>
    /// The arrows and the bullets are one row in MudCarousel.razor, every bullet a MudIconButton as
    /// tall as an arrow: on the same edge, the pips' centres are the arrows' centres.
    /// </summary>
    [Theory]
    [InlineData("arrows-top", "bullets-top")]
    [InlineData("arrows-bottom", "")]
    [InlineData("", "bullets-center")]
    public Task TheArrowsAndBulletsShareALine(string arrows, string bullets) => UiThread.RunAsync(() =>
    {
        var carousel = new Carousel { Width = 400, Height = 240, ItemsSource = new[] { "One", "Two" } };
        carousel.Classes.AddRange(new[] { arrows, bullets }.Where(c => c.Length > 0));
        Matrix.Show(carousel);

        double CentreY(Visual part) => part.TranslatePoint(new Point(0, part.Bounds.Height / 2), carousel)!.Value.Y;

        var arrow = CentreY(Part<Button>(carousel, "PART_PreviousButton"));
        var pips = Part<PipsPager>(carousel, "PART_Bullets").GetVisualDescendants().OfType<Path>().Where(p => p.Name == "Pip").ToList();

        Assert.NotEmpty(pips);
        Assert.All(pips, pip => Assert.Equal(arrow, CentreY(pip), 3));
    });

    /// <summary>A CarouselPage is a Carousel of pages, with the same bullets.</summary>
    [Fact]
    public Task ACarouselPageHasBullets() => UiThread.RunAsync(() =>
    {
        var pages = new CarouselPage { Width = 400, Height = 240 };
        pages.Pages = new List<Page> { new ContentPage { Header = "One" }, new ContentPage { Header = "Two" } };
        Matrix.Show(pages);

        Assert.Equal(2, Part<PipsPager>(pages, "PART_Bullets").NumberOfPages);
    });
}
