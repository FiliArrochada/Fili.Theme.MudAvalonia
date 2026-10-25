using Avalonia.Controls;
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

    /// <summary>Each example card loaded its file, and shows that file's content as its code.</summary>
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
        }

        window.Close();
    });

    /// <summary>A component's page here is in the catalogue, and its MudBlazor page is a route.</summary>
    [Fact]
    public void EveryComponentLinksSomewhere()
    {
        foreach (var entry in ComponentIndex.All)
        {
            Assert.StartsWith("components/", entry.MudBlazorPath);
            if (entry.Slug is { } slug)
            {
                Assert.NotNull(DocCatalog.Find(slug));
            }
        }

        Assert.Equal(ComponentIndex.All.Count, ComponentIndex.All.Select(e => e.Name).Distinct().Count());
    }

    /// <summary>Each slug names one page.</summary>
    [Fact]
    public void SlugsAreUnique() =>
        Assert.Equal(DocCatalog.Pages.Count, DocCatalog.Pages.Select(p => p.Slug).Distinct().Count());
}
