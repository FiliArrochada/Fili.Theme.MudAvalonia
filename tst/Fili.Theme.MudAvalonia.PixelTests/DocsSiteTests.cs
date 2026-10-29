using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.Gallery;
using Fili.Theme.MudAvalonia.Gallery.Docs;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The docs site the gallery is: every page opens, every example's code is the file it renders,
/// and every link to a page here or on mudblazor.com goes somewhere.
/// </summary>
public class DocsSiteTests
{
    public static TheoryData<string> Slugs()
    {
        var data = new TheoryData<string>();
        foreach (var page in DocCatalog.Pages)
        {
            data.Add(page.Slug);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public Task EveryPageOpens(string slug) => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();

        view.Navigate(slug);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(slug, view.Current);
        window.Close();
    });

    /// <summary>
    /// No sentence has a run of bare whitespace in it: the line break between two Runs in XAML is
    /// a space of its own, on top of the spaces the Runs carry, so code was followed by a gap
    /// before its punctuation. A page writes its Runs on one line.
    /// </summary>
    [Theory]
    [MemberData(nameof(Slugs))]
    public Task NoSentenceHasAStraySpace(string slug) => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();
        view.Navigate(slug);
        Dispatcher.UIThread.RunJobs();

        foreach (var text in window.GetVisualDescendants().OfType<TextBlock>().Where(t => t.Inlines is { Count: > 1 }))
        {
            Assert.DoesNotContain(text.Inlines!, i => i is Avalonia.Controls.Documents.Run { Text: { } t } && string.IsNullOrWhiteSpace(t));
        }

        window.Close();
    });

    /// <summary>Each example card loaded its file, and shows that file's content as its code, and its C# file's where it names one.</summary>
    [Theory]
    [MemberData(nameof(Slugs))]
    public Task EveryExampleIsItsOwnCode(string slug) => UiThread.RunAsync(() =>
    {
        var view = new MainView();
        var window = new Window { Content = view, Width = 1440, Height = 900 };
        window.Show();
        view.Navigate(slug);
        Dispatcher.UIThread.RunJobs();

        foreach (var card in window.GetVisualDescendants().OfType<ExampleCard>())
        {
            Assert.NotNull(card.Content);
            Assert.False(string.IsNullOrWhiteSpace(card.Code), $"{card.Source} shows no code.");
            Assert.Equal(ExampleCard.Inner(ExampleCard.Read(card.Source!)), card.Code);
            Assert.DoesNotContain("<UserControl", card.Code);
            if (card.CodeBehind is { } file)
            {
                Assert.Equal(ExampleCard.ReadCustom(file).Replace("\r\n", "\n").Trim(), card.CSharp);
            }
        }

        window.Close();
    });

    /// <summary>A component's page here is in the catalogue, and its MudBlazor page is a docs route.</summary>
    [Fact]
    public void EveryComponentLinksSomewhere()
    {
        foreach (var entry in ComponentIndex.All)
        {
            Assert.Matches("^(components|features|customization)/[a-z-]+$", entry.MudBlazorPath);
            if (entry.Slug is { } slug)
            {
                Assert.NotNull(DocCatalog.Find(slug));
            }
        }

        Assert.Equal(ComponentIndex.All.Count, ComponentIndex.All.Select(e => e.Name).Distinct().Count());
    }

    /// <summary>
    /// A component's page links Avalonia's page for each control it is drawn on, and every one of
    /// those is a control guide or an API page on docs.avaloniaui.net.
    /// </summary>
    [Fact]
    public Task EveryComponentLinksAvalonias() => UiThread.RunAsync(() =>
    {
        foreach (var page in DocCatalog.Pages)
        {
            if (page.Create().GetLogicalDescendants().OfType<DocHeader>().FirstOrDefault() is { } header
                && header.MudBlazorPath?.StartsWith("components/", StringComparison.Ordinal) == true)
            {
                Assert.True(AvaloniaDocs.Names(header.AvaloniaControls).Count > 0, $"{page.Slug} links no Avalonia page.");
                Assert.All(AvaloniaDocs.Names(header.AvaloniaControls), name => Assert.Contains(name, AvaloniaDocs.Paths.Keys));
            }
        }

        Assert.All(AvaloniaDocs.Paths.Values, path => Assert.Matches("^(controls|api/avalonia)/[a-z/-]+$", path));
    });

    /// <summary>Each slug names one page.</summary>
    [Fact]
    public void SlugsAreUnique() =>
        Assert.Equal(DocCatalog.Pages.Count, DocCatalog.Pages.Select(p => p.Slug).Distinct().Count());

    /// <summary>
    /// An example is loaded by its path, which finds its compiled XAML by reflection, through the
    /// type the XAML compiler generates; the trimmed browser build dropped that type and every
    /// example on the site threw "No precompiled XAML found". Untrimmed, as here, it always loads,
    /// so the guard is the DynamicDependency that keeps the type, and that the type is there.
    /// </summary>
    [Fact]
    public void ExamplesKeepTheirCompiledXamlThroughTrimming()
    {
        var gallery = typeof(ExampleCard).Assembly;
        var load = typeof(ExampleCard).GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var keep = Assert.Single(load.GetCustomAttributes<DynamicDependencyAttribute>());
        Assert.Equal(gallery.GetName().Name, keep.AssemblyName);
        Assert.NotNull(gallery.GetType(keep.TypeName!));
        Assert.True(keep.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.PublicMethods) && keep.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.NonPublicMethods));
    }

    /// <summary>The address names a page: the site opens on it rather than on the overview.</summary>
    [Fact]
    public Task TheSiteOpensOnThePageItsAddressNames() => WithAddress("tooltip", (address, view) =>
    {
        Assert.Equal("tooltip", view.Current);
        Assert.Equal(("tooltip", "Tooltip"), address.Shown[^1]);
    });

    /// <summary>An address naming no page opens the overview, and says so.</summary>
    [Fact]
    public Task AnUnknownAddressOpensTheOverview() => WithAddress("no-such-page", (address, view) =>
    {
        Assert.Equal("overview", view.Current);
        Assert.Equal("overview", address.Shown[^1].Slug);
    });

    /// <summary>Each page shown is written into the address, with its title.</summary>
    [Fact]
    public Task APageShownIsWrittenToTheAddress() => WithAddress(null, (address, view) =>
    {
        view.Navigate("badge");

        Assert.Equal(("badge", "Badge"), address.Shown[^1]);
    });

    /// <summary>Back and Forward change the address from outside: the page follows it.</summary>
    [Fact]
    public Task ThePageFollowsTheAddressBackAndForward() => WithAddress(null, (address, view) =>
    {
        view.Navigate("badge");
        address.Go("avatar");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("avatar", view.Current);

        address.Go(string.Empty);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("overview", view.Current);
    });

    private static Task WithAddress(string? slug, Action<FakeAddress, MainView> test) => UiThread.RunAsync(() =>
    {
        var address = new FakeAddress { Slug = slug };
        DocAddress.Current = address;
        try
        {
            var view = new MainView();
            var window = new Window { Content = view, Width = 1440, Height = 900 };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            test(address, view);
            window.Close();
        }
        finally
        {
            DocAddress.Current = null;
        }
    });

    private sealed class FakeAddress : IDocAddress
    {
        public string? Slug { get; set; }

        public List<(string Slug, string Title)> Shown { get; } = [];

        public event Action<string>? Changed;

        public void Show(string slug, string title)
        {
            Slug = slug;
            Shown.Add((slug, title));
        }

        public void Go(string slug)
        {
            Slug = slug;
            Changed?.Invoke(slug);
        }
    }
}
