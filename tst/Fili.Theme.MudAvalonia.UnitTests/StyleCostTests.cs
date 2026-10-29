using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// What a style costs every app, kept down.
///
/// <para>
/// Avalonia matches a style against every control of the type its selector ends in, and a class
/// can arrive later, so a style ending in <c>Border.paper</c> sets up a watcher on every Border
/// in the window whether it carries the class or not; a descendant selector watches each one's
/// ancestors as well. A screen of 1,600 ordinary controls took 7.3 seconds to build here against
/// the forked base's 1.2: nineteen in twenty Borders are parts of a control's template, and each
/// paid for every Border class style, and 27 descendant overlay styles cost more than all the
/// others together. Template Borders are now <see cref="PartBorder"/>s, which those styles pass
/// over by type, and the overlay reads an inherited property; the same screen takes 2.2 seconds.
/// </para>
/// </summary>
public class StyleCostTests
{
    /// <summary>
    /// Every Border in a hand-written template is a PartBorder. A plain one is matched against
    /// every one of the app's Border class styles, in every instance of the control.
    /// </summary>
    [Fact]
    public Task EveryTemplateBorderIsAPartBorder() => UiThread.RunAsync(() =>
    {
        var plain = TemplateFiles()
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"<Border[\s>/.]")
                .Select(m => $"{Path.GetFileName(file)} at {m.Index}"))
            .ToList();

        Assert.True(plain.Count == 0, "A plain Border in a template, which every Border class style watches: " + string.Join(", ", plain));
    });

    /// <summary>
    /// A selector reaching into a hand-written template names its parts' own type. Naming
    /// <c>Border</c> there matches nothing, silently, since the template has none.
    /// </summary>
    [Fact]
    public Task NoSelectorLooksForAPlainBorderInAHandWrittenTemplate() => UiThread.RunAsync(() =>
    {
        var ours = StandaloneReadinessTests.ThemeKeys
            .Select(key =>
            {
                Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value);
                return ((ControlTheme)value!).TargetType!.Name;
            })
            .ToHashSet();

        var stale = new[] { "FiliTheme.axaml", Path.Combine("Themes", "Optional", "DataGrid", "DataGridStyles.axaml") }
            .SelectMany(file => Regex.Matches(File.ReadAllText(Path.Combine(GeneratedThemeTests.ThemeDirectory(), file)), "Selector=\"([^\"]*)\"")
                .SelectMany(m => m.Groups[1].Value.Split(','))
                .Where(part => Regex.Match(part, @"([A-Za-z]+)[^\s>]*\s*/template/\s*Border(?=[#\s.:\[]|$)") is { Success: true } hit
                    && ours.Contains(hit.Groups[1].Value))
                .Select(part => $"{Path.GetFileName(file)}: {part.Trim()}"))
            .ToList();

        Assert.True(stale.Count == 0, "Looks for a Border a hand-written template no longer has: " + string.Join("; ", stale));
    });

    /// <summary>
    /// No selector ends in a base every control derives from, <c>:is(Control)</c> or
    /// <c>:is(Panel)</c> and their kin: it is matched against every control in the window,
    /// template parts included, and a class or a parent after it makes each one an instance. Ten
    /// such styles, the card parts', the avatar's and the toolbar's children and
    /// <c>material-elevation-overlay</c>, were 40% of every style instance on a screen. A class
    /// names the controls it goes on; a host that aligns its children does it in code
    /// (<see cref="ChildAlignment"/>).
    /// </summary>
    [Fact]
    public Task NoSelectorEndsInEveryControl() => UiThread.RunAsync(() =>
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        var universal = Directory.GetFiles(theme, "*.axaml", SearchOption.AllDirectories)
            .Where(file => !file.Contains(Path.Combine("Themes", "Base")))
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), "Selector=\"([^\"]*)\"")
                .SelectMany(m => m.Groups[1].Value.Split(','))
                .Where(part => Regex.IsMatch(Regex.Split(part.Trim(), @"\s+|>|/template/").Last(e => e.Length > 0),
                    @"^:is\((Control|Panel|Layoutable|Visual|InputElement|Interactive|StyledElement|TemplatedControl)\)"))
                .Select(part => $"{Path.GetFileName(file)}: {part.Trim()}"))
            .ToList();

        Assert.True(universal.Count == 0, "Matched against every control: " + string.Join("; ", universal));
    });

    /// <summary>
    /// A top-level style that switches a control's theme names it with <c>DynamicResource</c>. A
    /// <c>StaticResource</c> is resolved as the styles load, which builds the control theme and
    /// every style nested in it, the chip's 395 among them, in every app whether it shows one or
    /// not: the 20 such setters were 80 of a 300ms cold load, and a control's first showing costs
    /// the same either way.
    /// </summary>
    [Fact]
    public Task NoStyleBuildsAControlThemeAtLoad() => UiThread.RunAsync(() =>
    {
        var file = Path.Combine(GeneratedThemeTests.ThemeDirectory(), "FiliTheme.axaml");
        var eager = Regex.Matches(File.ReadAllText(file), @"<Setter Property=""Theme"" Value=""\{StaticResource ([^}]*)\}""")
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.True(eager.Count == 0, "Built at load: " + string.Join(", ", eager));
    });

    private static IEnumerable<string> TemplateFiles()
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        return Directory.GetFiles(Path.Combine(theme, "Themes", "Controls"), "*.axaml")
            .Append(Path.Combine(theme, "Themes", "Optional", "DataGrid", "DataGrid.axaml"));
    }
}
