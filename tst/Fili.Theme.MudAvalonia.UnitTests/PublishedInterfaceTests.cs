using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The interface an app compiles against, frozen: the resource keys in <c>docs/published-keys.md</c>,
/// and the one public type. <c>ClassVocabularyTests</c> is the third part, the classes.
///
/// <para>
/// A key is the other half of this package's interface, beside the class list: an app writes
/// <c>{DynamicResource FiliPrimaryBrush}</c> or sets <c>FiliCheckBoxCheckedGeometry</c> in a
/// control's <c>Resources</c>. Renaming one breaks that app silently - a missing key resolves to
/// nothing - and every other test here passes, because the theme's own references move with it.
/// So the published set is a committed file, and the theme's source must match it exactly: a key
/// that disappears is a breaking change, and a new one is added to the file on purpose.
/// </para>
///
/// <para>
/// Published means every <c>Fili*</c> key in the five token dictionaries and every
/// <c>Fili*Geometry</c> glyph key, wherever it is declared. The keyed control themes
/// (<c>FiliFilledButton</c>...) and the forked base's keys are implementation, and are not listed.
/// </para>
/// </summary>
public class PublishedInterfaceTests
{
    private static readonly string[] TokenFiles =
        ["Palette.axaml", "Elevation.axaml", "Geometry.axaml", "Icons.axaml", "Typography.axaml"];

    private static readonly Regex Key = new("x:Key=\"(Fili[^\"{]*)\"", RegexOptions.Compiled);

    private static string ListPath() =>
        Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", "..", "docs", "published-keys.md");

    private static readonly Regex ListedKey = new(@"^- `(Fili\w+)`$", RegexOptions.Compiled);

    /// <summary>The page's list items, one key each; the prose around them is not read.</summary>
    private static SortedSet<string> Listed() =>
        new(File.ReadLines(ListPath())
            .Select(line => ListedKey.Match(line.Trim()))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value),
            StringComparer.Ordinal);

    private static SortedSet<string> Declared()
    {
        var themes = Path.Combine(GeneratedThemeTests.ThemeDirectory(), "Themes");
        var keys = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var file in TokenFiles)
        {
            foreach (Match match in Key.Matches(File.ReadAllText(Path.Combine(themes, file))))
            {
                keys.Add(match.Groups[1].Value);
            }
        }

        foreach (var file in Directory.EnumerateFiles(themes, "*.axaml", SearchOption.AllDirectories))
        {
            foreach (Match match in Key.Matches(File.ReadAllText(file)))
            {
                if (match.Groups[1].Value.EndsWith("Geometry", StringComparison.Ordinal))
                {
                    keys.Add(match.Groups[1].Value);
                }
            }
        }

        return keys;
    }

    [Fact]
    public void NoPublishedKeyIsRemovedOrRenamed()
    {
        var gone = Listed().Except(Declared()).ToArray();

        Assert.True(
            gone.Length == 0,
            "These published keys are no longer declared by the theme. Renaming or removing one breaks "
                + "every app that names it, silently, so it waits for 2.0; keep the old key as an alias "
                + "until then:\n  " + string.Join("\n  ", gone));
    }

    [Fact]
    public void EveryNewKeyIsPublishedOnPurpose()
    {
        var added = Declared().Except(Listed()).ToArray();

        Assert.True(
            added.Length == 0,
            "These keys are declared by the theme but not in docs/published-keys.md. Once released, a key "
                + "is an interface apps can name: list it there in this change, under its section:\n  "
                + string.Join("\n  ", added));
    }

    /// <summary>A listed key is a promise only if it resolves, in every variant an app can pick.</summary>
    [Fact]
    public Task EveryPublishedKeyResolvesInEveryVariant() => UiThread.RunAsync(() =>
    {
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark, FiliThemeVariants.HighContrast })
        {
            var missing = Listed().Where(key => !Application.Current!.TryFindResource(key, variant, out _)).ToArray();

            Assert.True(missing.Length == 0, $"Unresolved under {variant}:\n  " + string.Join("\n  ", missing));
        }
    });

    /// <summary>
    /// The public C# API is one type, public only because a custom theme variant is unreachable
    /// from XAML except through <c>x:Static</c>. Anything else public is a design change.
    /// </summary>
    [Fact]
    public void ThePublicApiIsOneType()
    {
        // The XAML compiler emits its loader as public types in its own namespace; they are Avalonia's,
        // not this package's, and no app names them.
        var exported = typeof(FiliThemeVariants).Assembly.GetExportedTypes()
            .Where(t => t.Namespace != "CompiledAvaloniaXaml")
            .Select(t => t.FullName ?? t.Name)
            .ToArray();

        Assert.Equal(["Fili.Theme.MudAvalonia.FiliThemeVariants"], exported);
        Assert.Equal(
            ["HighContrast"],
            typeof(FiliThemeVariants).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(m => m.Name).Where(n => !n.StartsWith("get_", StringComparison.Ordinal)).ToArray());
    }
}
