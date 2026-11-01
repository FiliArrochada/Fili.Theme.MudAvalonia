using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The published interface as it was released, frozen in <c>published-baseline.txt</c>: every
/// published resource key with the type it resolves to in each variant, and every style class.
///
/// <para>
/// <see cref="PublishedInterfaceTests"/> and <see cref="ClassVocabularyTests"/> hold the theme's
/// source to its documentation, in both directions, so a key or a class deleted from the source and
/// from the page in the same change passes them. This file is the third party that does not move
/// with them: whatever it lists must still resolve, with a type an app can still use the same way,
/// and every class must still be in the vocabulary. It only grows.
/// </para>
///
/// <para>
/// <b>When a key or a class is added</b>, <see cref="EverythingPublishedIsInTheBaseline"/> fails
/// until it is appended:
/// <code>$env:FILI_REGENERATE = "1"; dotnet test tst/Fili.Theme.MudAvalonia.UnitTests --filter-class "*PublishedBaselineTests"</code>
/// The rewrite only appends, so the result is a reviewable diff of additions. Nothing removes an
/// entry: a key that has to go stays as an alias of its replacement until the next major version,
/// and a major version is the one change that edits this file by hand.
/// </para>
/// </summary>
public class PublishedBaselineTests
{
    private const string RegenerateVariable = "FILI_REGENERATE";

    private static readonly (string Name, ThemeVariant Variant)[] Variants =
    [
        ("Light", ThemeVariant.Light),
        ("Dark", ThemeVariant.Dark),
        ("HighContrast", FiliThemeVariants.HighContrast),
    ];

    private static string BaselinePath() =>
        Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", "..", "tst", "Fili.Theme.MudAvalonia.UnitTests", "published-baseline.txt");

    private sealed record Baseline(SortedDictionary<string, string> Keys, SortedSet<string> Classes);

    /// <summary>
    /// One entry a line: <c>key</c>, the key and its type, the same in every variant or else
    /// <c>Light=…;Dark=…;HighContrast=…</c>; or <c>class</c> and the class. <c>#</c> starts a comment.
    /// </summary>
    private static Baseline Read()
    {
        var keys = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var classes = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var line in File.ReadLines(BaselinePath()))
        {
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var cells = line.Split('\t');
            switch (cells[0])
            {
                case "key" when cells.Length == 3:
                    keys.Add(cells[1], cells[2]);
                    break;
                case "class" when cells.Length == 2:
                    classes.Add(cells[1]);
                    break;
                default:
                    throw new InvalidDataException($"published-baseline.txt: cannot read \"{line}\".");
            }
        }

        return new Baseline(keys, classes);
    }

    /// <summary>
    /// The type a key promises, by name. A glyph promises a <c>Geometry</c>, whichever kind the
    /// parser builds; everything else promises the type it is.
    /// </summary>
    private static string ContractName(object? value) => value switch
    {
        null => "null",
        Geometry => nameof(Geometry),
        _ => value.GetType().Name,
    };

    /// <summary>The key's type in each variant, as one word when they agree.</summary>
    private static string TypeOf(string key)
    {
        var types = Variants
            .Select(v => (v.Name, Type: Application.Current!.TryFindResource(key, v.Variant, out var value) ? ContractName(value) : "missing"))
            .ToArray();

        return types.Select(t => t.Type).Distinct().Count() == 1
            ? types[0].Type
            : string.Join(';', types.Select(t => $"{t.Name}={t.Type}"));
    }

    /// <summary>A value still keeps a recorded type when that type is its own, a base or an interface.</summary>
    private static bool Keeps(object? value, string recorded)
    {
        if (value is null)
        {
            return recorded == "null";
        }

        for (var type = value.GetType(); type is not null; type = type.BaseType)
        {
            if (type.Name == recorded)
            {
                return true;
            }
        }

        return value.GetType().GetInterfaces().Any(i => i.Name == recorded);
    }

    private static string Recorded(string types, string variant) =>
        types.Contains('=')
            ? types.Split(';').Select(part => part.Split('=')).Single(pair => pair[0] == variant)[1]
            : types;

    [Fact]
    public Task NoBaselineKeyIsRemoved() => UiThread.RunAsync(() =>
    {
        var baseline = Read();
        var listed = PublishedInterfaceTests.Listed();
        var problems = new List<string>();

        foreach (var key in baseline.Keys.Keys)
        {
            if (!listed.Contains(key))
            {
                problems.Add($"{key}: no longer on docs/published-keys.md");
            }

            foreach (var (name, variant) in Variants)
            {
                if (!Application.Current!.TryFindResource(key, variant, out _))
                {
                    problems.Add($"{key}: does not resolve under {name}");
                }
            }
        }

        Assert.True(
            problems.Count == 0,
            "Keys in the released interface are gone. Removing or renaming a published key breaks every "
                + "app that names it, silently, so it waits for the next major version; keep it as an alias of "
                + "its replacement until then:\n  " + string.Join("\n  ", problems));
    });

    [Fact]
    public Task EveryBaselineKeyKeepsItsType() => UiThread.RunAsync(() =>
    {
        var problems = new List<string>();

        foreach (var (key, types) in Read().Keys)
        {
            foreach (var (name, variant) in Variants)
            {
                if (Application.Current!.TryFindResource(key, variant, out var value)
                    && !Keeps(value, Recorded(types, name)))
                {
                    problems.Add($"{key} under {name}: was {Recorded(types, name)}, is {ContractName(value)}");
                }
            }
        }

        Assert.True(
            problems.Count == 0,
            "Published keys changed type. An app that binds or casts one breaks, silently - a "
                + "DynamicResource is not type-checked:\n  " + string.Join("\n  ", problems));
    });

    [Fact]
    public void NoBaselineClassIsRemoved()
    {
        var gone = Read().Classes.Except(ClassVocabularyTests.Vocabulary, StringComparer.Ordinal).ToArray();

        Assert.True(
            gone.Length == 0,
            "Classes in the released interface are no longer in the theme's vocabulary. An app that sets "
                + "one loses its look, silently; it waits for the next major version:\n  " + string.Join("\n  ", gone));
    }

    /// <summary>
    /// The baseline keeps up with what is published, so a key added in a minor version is frozen
    /// too. <c>FILI_REGENERATE=1</c> appends what is missing and never removes or retypes an entry.
    /// </summary>
    [Fact]
    public Task EverythingPublishedIsInTheBaseline() => UiThread.RunAsync(() =>
    {
        var baseline = Read();
        var newKeys = PublishedInterfaceTests.Listed().Where(key => !baseline.Keys.ContainsKey(key)).ToArray();
        var newClasses = ClassVocabularyTests.Vocabulary.Distinct()
            .Where(c => !baseline.Classes.Contains(c))
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToArray();

        if (newKeys.Length + newClasses.Length == 0)
        {
            return;
        }

        if (Environment.GetEnvironmentVariable(RegenerateVariable) == "1")
        {
            File.AppendAllLines(
                BaselinePath(),
                newKeys.Select(key => $"key\t{key}\t{TypeOf(key)}").Concat(newClasses.Select(c => $"class\t{c}")));
            return;
        }

        Assert.Fail(
            "Published but not yet in tst/Fili.Theme.MudAvalonia.UnitTests/published-baseline.txt. Append them "
                + $"with {RegenerateVariable}=1 (see this class's summary):\n  "
                + string.Join("\n  ", newKeys.Concat(newClasses.Select(c => "class " + c))));
    });
}
