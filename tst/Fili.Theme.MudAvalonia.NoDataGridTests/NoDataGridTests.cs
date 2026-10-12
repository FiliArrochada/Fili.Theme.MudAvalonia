using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Fili.Theme.MudAvalonia.NoDataGridTests;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]

namespace Fili.Theme.MudAvalonia.NoDataGridTests;

/// <summary>
/// The theme in an app that does not have Avalonia.Controls.DataGrid.
///
/// <para>
/// The package builds against the DataGrid without depending on it, so the XAML that names its
/// types loads only when it is there (FiliTheme.axaml's <c>OptionalStyleInclude</c>). Get that wrong - a DataGrid
/// selector left in FiliTheme.axaml, an include the XAML compiler resolves eagerly - and every
/// app without a grid fails at startup looking for an assembly it never asked for. The main suite
/// cannot see that, because it has the package; this assembly does not.
/// </para>
/// </summary>
public class NoDataGridTests
{
    [Fact]
    public Task ThisAppReallyHasNoDataGrid() => UiThread.RunAsync(() =>
    {
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Avalonia.Controls.DataGrid.dll")));
        Assert.DoesNotContain(
            AppDomain.CurrentDomain.GetAssemblies(),
            assembly => assembly.GetName().Name == "Avalonia.Controls.DataGrid");
    });

    [Fact]
    public Task TheThemeLoadsAndThemesControls() => UiThread.RunAsync(() =>
    {
        // Loaded directly too, so a failure names the assembly it went looking for. The app's own
        // include would only leave every resource missing.
        Assert.IsType<Styles>(AvaloniaXamlLoader.Load(new Uri("avares://Fili.Theme.MudAvalonia/FiliTheme.axaml")));

        var button = new Button { Content = "Text" };
        var window = new Window { Content = button };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // The control themes merged, and the button wears one: a template, laid out.
        Assert.True(Application.Current!.TryFindResource("FiliFilledButton", out _));
        Assert.NotEmpty(button.GetVisualChildren());
        Assert.True(button.Bounds.Height > 0);
        window.Close();
    });

    [Fact]
    public Task NoDataGridThemeIsLoaded() => UiThread.RunAsync(() =>
    {
        Assert.False(Application.Current!.TryFindResource("FiliDataGrid", out _));
        Assert.False(Application.Current!.TryFindResource("FiliArrowUpwardGeometry", out _));
    });
}

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

/// <summary>Runs test bodies on the shared headless UI thread, as the main suite's does.</summary>
public static class UiThread
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.GetOrStartForAssembly(typeof(TestApp).Assembly);

    public static Task RunAsync(Action action) => Session.Dispatch(action, CancellationToken.None);
}

/// <summary>The two includes a consuming app writes, in the same order.</summary>
public class TestApp : Application
{
    public override void Initialize()
    {
        var baseUri = new Uri("avares://Fili.Theme.MudAvalonia.NoDataGridTests");

        // Load the theme assembly before naming it in an avares:// URI; see the main suite's TestApp.
        _ = typeof(FiliThemeVariants).Assembly;

        Styles.Add(new StyleInclude(baseUri)
        {
            Source = new Uri("avares://Fili.Theme.MudAvalonia/Themes/Base/FiliBaseTheme.axaml"),
        });

        Styles.Add(new StyleInclude(baseUri)
        {
            Source = new Uri("avares://Fili.Theme.MudAvalonia/FiliTheme.axaml"),
        });
    }
}
