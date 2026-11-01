using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Threading;
using BenchmarkDotNet.Attributes;
using Fili.Theme.MudAvalonia.UnitTests;

namespace Fili.Theme.MudAvalonia.Benchmarks;

/// <summary>
/// What the theme costs an app, timed: building the 1,600-control <see cref="BenchmarkScreen"/>
/// and switching the theme variant under it, with the whole theme and with the forked base alone,
/// which is the floor any style the theme adds is measured against (docs/traps.md, "A style costs
/// every control of the type its selector ends in").
///
/// <para>
/// A style-cost change is kept only on what this shows. The unit tests' style-instance ceiling
/// (<c>StyleCostTests</c>) catches a selector that multiplies, but fewer instances has built
/// slower before; run this before and after, on the same machine, and put both tables in the
/// change.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class ThemeBenchmarks
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.GetOrStartForAssembly(typeof(ThemeBenchmarks).Assembly);

    private Window? _shown;

    /// <summary><c>Fili</c>: both includes, as an app has them. <c>Base</c>: the forked base only.</summary>
    [Params("Fili", "Base")]
    public string Theme { get; set; } = "Fili";

    [GlobalSetup(Targets = [nameof(BuildScreen)])]
    public void LoadTheme() => Run(() => Load(Theme));

    [GlobalSetup(Targets = [nameof(SwitchVariant)])]
    public void ShowScreen() => Run(() =>
    {
        Load(Theme);
        _shown = Show();
    });

    [GlobalCleanup(Targets = [nameof(SwitchVariant)])]
    public void CloseScreen() => Run(() =>
    {
        _shown?.Close();
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
    });

    /// <summary>The screen built, laid out and closed: what opening a page costs.</summary>
    [Benchmark]
    public void BuildScreen() => Run(() => Show().Close());

    /// <summary>
    /// Light to dark, or back, with the screen on show: every brush the screen uses re-resolves.
    /// Alternating, so each invocation does the same work.
    /// </summary>
    [Benchmark]
    public void SwitchVariant() => Run(() =>
    {
        var app = Application.Current!;
        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        Dispatcher.UIThread.RunJobs();
    });

    private static Window Show()
    {
        var window = new Window { Content = BenchmarkScreen.Build(), Width = 1200, Height = 900 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Load(string theme)
    {
        // Load the theme assembly before naming it in an avares:// URI (docs/traps.md).
        _ = typeof(FiliThemeVariants).Assembly;
        var baseUri = new Uri("avares://Fili.Theme.MudAvalonia.Benchmarks");
        var styles = Application.Current!.Styles;
        styles.Clear();
        styles.Add(new StyleInclude(baseUri) { Source = new Uri("avares://Fili.Theme.MudAvalonia/Themes/Base/FiliBaseTheme.axaml") });
        if (theme == "Fili")
        {
            styles.Add(new StyleInclude(baseUri) { Source = new Uri("avares://Fili.Theme.MudAvalonia/FiliTheme.axaml") });
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static void Run(Action action) =>
        Session.Dispatch(action, CancellationToken.None).GetAwaiter().GetResult();
}
