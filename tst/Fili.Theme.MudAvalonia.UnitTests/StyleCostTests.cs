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
/// can arrive later, so a style ending in <c>Border.surface</c> sets up a watcher on every Border
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

    private static IEnumerable<string> TemplateFiles()
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        return Directory.GetFiles(Path.Combine(theme, "Themes", "Controls"), "*.axaml")
            .Append(Path.Combine(theme, "Themes", "Optional", "DataGrid", "DataGrid.axaml"));
    }
}
