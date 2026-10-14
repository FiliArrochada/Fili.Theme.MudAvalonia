using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The packages this theme builds against but does not depend on, loaded by FiliTheme.axaml's
/// <c>OptionalStyleInclude</c>s.
///
/// <para>
/// The rule they rest on: a package's types are named only in its folder under
/// <c>Themes/Optional/</c>. A DataGrid selector anywhere else compiles, passes every test in this
/// assembly (which has the package) and takes the whole theme down at startup in every app that
/// does not. NoDataGridTests catches that as a missing assembly; the scan here names the file and
/// line.
/// </para>
/// </summary>
public class OptionalBoundaryTests
{
    private sealed record Include(string Requires, Version MinimumVersion, string Folder);

    /// <summary>Every OptionalStyleInclude in FiliTheme.axaml, read from the source.</summary>
    private static List<Include> Includes()
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        var document = XDocument.Load(Path.Combine(theme, "FiliTheme.axaml"));
        var includes = document.Descendants()
            .Where(element => element.Name.LocalName == "OptionalStyleInclude")
            .Select(element =>
            {
                var source = new Uri((string)element.Attribute("Source")!);
                var folder = Path.GetDirectoryName(source.AbsolutePath.TrimStart('/'))!;
                return new Include(
                    (string)element.Attribute("Requires")!,
                    Version.Parse((string)element.Attribute("MinimumVersion")!),
                    Path.Combine(theme, folder));
            })
            .ToList();

        Assert.NotEmpty(includes);
        return includes;
    }

    [Fact]
    public void EveryOptionalPackageHasItsOwnFolderUnderOptional()
    {
        var optional = Path.Combine(GeneratedThemeTests.ThemeDirectory(), "Themes", "Optional");
        foreach (var include in Includes())
        {
            Assert.Equal(optional, Path.GetDirectoryName(include.Folder));
            Assert.True(Directory.Exists(include.Folder), include.Folder);
        }
    }

    /// <summary>
    /// MinimumVersion is the version the theme is built against, so the two move together: a
    /// package bump that forgets the include would let an app on the old version load templates
    /// naming parts it does not have.
    /// </summary>
    [Fact]
    public void EveryMinimumVersionIsTheVersionBuiltAgainst()
    {
        var props = XDocument.Load(Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", "..", "Directory.Packages.props"));
        foreach (var include in Includes())
        {
            var built = props.Descendants("PackageVersion")
                .Single(element => (string?)element.Attribute("Include") == include.Requires);
            Assert.Equal(Version.Parse((string)built.Attribute("Version")!), include.MinimumVersion);
        }
    }

    /// <summary>
    /// Scans every file in the theme project outside a package's folder for that package's type
    /// names, where XAML and C# name a type: an element, a selector, a TargetType or DataType, an
    /// x:Type, a property owner, or C# outside comments and strings.
    /// </summary>
    [Fact]
    public void OnlyAPackagesOwnFolderNamesItsTypes()
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        var files = Directory.EnumerateFiles(theme, "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".axaml") || file.EndsWith(".cs"))
            .Where(file => !IsUnder(file, Path.Combine(theme, "bin")) && !IsUnder(file, Path.Combine(theme, "obj")))
            .ToList();

        var crossings = new List<string>();
        foreach (var include in Includes())
        {
            var names = TypeNames(include.Requires);
            var inside = 0;
            foreach (var file in files)
            {
                var found = TypeReferences(file).Where(reference => names.Contains(reference.Name)).ToList();
                if (IsUnder(file, include.Folder))
                {
                    inside += found.Count;
                    continue;
                }

                crossings.AddRange(found.Select(reference =>
                    $"{Path.GetRelativePath(theme, file)}:{reference.Line} names {reference.Name} ({include.Requires})"));
            }

            // Proves the scan sees what it looks for: the package's own folder names plenty.
            Assert.True(inside > 0, $"The scan found no {include.Requires} type in {include.Folder}.");
        }

        Assert.True(
            crossings.Count == 0,
            "Only a package's folder under Themes/Optional may name its types; an app without the package "
            + "would fail to load the theme:\n" + string.Join("\n", crossings));
    }

    [Fact]
    public Task AnAbsentPackageLoadsNothing() => UiThread.RunAsync(() =>
    {
        var include = NewInclude("No.Such.Package", null);
        Assert.Empty(include);
    });

    [Fact]
    public Task AnOlderPackageLoadsNothingAndSaysWhy() => UiThread.RunAsync(() =>
    {
        var sink = new CapturingSink();
        var previous = Logger.Sink;
        Logger.Sink = sink;
        try
        {
            var include = NewInclude("Avalonia.Controls.DataGrid", new Version(99, 0));
            Assert.Empty(include);
        }
        finally
        {
            Logger.Sink = previous;
        }

        var warning = Assert.Single(sink.Warnings);
        Assert.Contains("Avalonia.Controls.DataGrid", warning);
        Assert.Contains("99.0", warning);
    });

    [Fact]
    public Task ThePackageHereIsLoaded() => UiThread.RunAsync(() =>
    {
        var include = Assert.Single(
            Flatten(global::Avalonia.Application.Current!.Styles),
            style => style.GetType().Name == "OptionalStyleInclude");
        Assert.NotEmpty((Styles)include);
    });

    /// <summary>
    /// Loading by URI finds the compiled XAML by reflection, through the type the XAML compiler
    /// generates, so a trimmed app dropped it: the browser gallery threw "No precompiled XAML
    /// found" for DataGridStyles.axaml at startup, and so would any app published trimmed or AOT
    /// with the DataGrid. Untrimmed, as every test here runs, it always loads, so the guard is the
    /// DynamicDependency that keeps that type, and that the type it names is really there.
    /// </summary>
    [Fact]
    public void TheIncludeKeepsTheCompiledXamlThroughTrimming()
    {
        var theme = typeof(FiliThemeVariants).Assembly;
        var include = theme.GetType("Fili.Theme.MudAvalonia.OptionalStyleInclude", throwOnError: true)!;
        var endInit = include.GetMethod("System.ComponentModel.ISupportInitialize.EndInit", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var keep = Assert.Single(endInit.GetCustomAttributes<DynamicDependencyAttribute>());
        Assert.Equal(theme.GetName().Name, keep.AssemblyName);
        Assert.NotNull(theme.GetType(keep.TypeName!));
        Assert.True(keep.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.PublicMethods) && keep.MemberTypes.HasFlag(DynamicallyAccessedMemberTypes.NonPublicMethods));
    }

    /// <summary>The internal include, built and initialised as compiled XAML does.</summary>
    private static Styles NewInclude(string requires, Version? minimum)
    {
        var type = typeof(FiliThemeVariants).Assembly.GetType("Fili.Theme.MudAvalonia.OptionalStyleInclude", throwOnError: true)!;
        var include = (Styles)Activator.CreateInstance(type, [null])!;
        type.GetProperty("Requires")!.SetValue(include, requires);
        type.GetProperty("MinimumVersion")!.SetValue(include, minimum);
        type.GetProperty("Source")!.SetValue(
            include,
            new Uri("avares://Fili.Theme.MudAvalonia/Themes/Optional/DataGrid/DataGridStyles.axaml"));
        ((ISupportInitialize)include).BeginInit();
        ((ISupportInitialize)include).EndInit();
        return include;
    }

    private static IEnumerable<IStyle> Flatten(IEnumerable<IStyle> styles)
    {
        foreach (var style in styles)
        {
            yield return style;
            IEnumerable<IStyle> children = style switch
            {
                Styles nested => nested,
                global::Avalonia.Markup.Xaml.Styling.StyleInclude { Loaded: { } loaded } => [loaded],
                _ => [],
            };
            foreach (var child in Flatten(children))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// The package's public type names that are its own: a name Avalonia's core assemblies also
    /// use is not evidence of anything.
    /// </summary>
    private static HashSet<string> TypeNames(string assemblyName)
    {
        var core = new[] { typeof(Button).Assembly, typeof(global::Avalonia.StyledElement).Assembly }
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Select(type => StripArity(type.Name))
            .ToHashSet();
        return Assembly.Load(new AssemblyName(assemblyName)).GetExportedTypes()
            .Select(type => StripArity(type.Name))
            .Where(name => !core.Contains(name))
            .ToHashSet();
    }

    private static string StripArity(string name) => name.Split('`')[0];

    private sealed record Reference(string Name, int Line);

    private static IEnumerable<Reference> TypeReferences(string file)
    {
        var text = File.ReadAllText(file);
        return file.EndsWith(".axaml") ? XamlReferences(text) : CSharpReferences(text);
    }

    private static readonly Regex XamlComment = new("<!--.*?-->", RegexOptions.Singleline);
    private static readonly Regex Element = new(@"</?\s*(?:\w+:)?([A-Za-z_]\w*)");
    private static readonly Regex TypeAttribute = new(@"\b(?:Selector|TargetType|x:DataType|DataType|Property)\s*=\s*""([^""]*)""");
    private static readonly Regex XType = new(@"\{x:Type\s+(?:\w+:)?([A-Za-z_]\w*)");

    // In a selector, a bare identifier is a type; one after # . : or ^ is a name, class or pseudo-class.
    private static readonly Regex SelectorType = new(@"(?<![#.:^\w-])(?:\w+\|)?([A-Za-z_]\w*)");

    private static IEnumerable<Reference> XamlReferences(string text)
    {
        text = Blank(text, XamlComment);
        foreach (Match match in Element.Matches(text))
        {
            yield return new Reference(match.Groups[1].Value, LineOf(text, match.Index));
        }

        foreach (Match match in XType.Matches(text))
        {
            yield return new Reference(match.Groups[1].Value, LineOf(text, match.Index));
        }

        foreach (Match match in TypeAttribute.Matches(text))
        {
            foreach (Match type in SelectorType.Matches(match.Groups[1].Value))
            {
                yield return new Reference(type.Groups[1].Value, LineOf(text, match.Index));
            }
        }
    }

    private static readonly Regex CSharpNoise = new(@"//[^\n]*|/\*.*?\*/|""(?:\\.|[^""\\])*""", RegexOptions.Singleline);
    private static readonly Regex Identifier = new(@"\b[A-Za-z_]\w*\b");

    private static IEnumerable<Reference> CSharpReferences(string text)
    {
        text = Blank(text, CSharpNoise);
        foreach (Match match in Identifier.Matches(text))
        {
            yield return new Reference(match.Value, LineOf(text, match.Index));
        }
    }

    /// <summary>Spaces out every match but keeps its newlines, so line numbers still hold.</summary>
    private static string Blank(string text, Regex pattern) =>
        pattern.Replace(text, match => Regex.Replace(match.Value, @"[^\n]", " "));

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

    private static bool IsUnder(string file, string folder) =>
        Path.GetFullPath(file).StartsWith(Path.GetFullPath(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private sealed class CapturingSink : ILogSink
    {
        public List<string> Warnings { get; } = [];

        public bool IsEnabled(LogEventLevel level, string area) => level >= LogEventLevel.Warning;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate) =>
            Log(level, area, source, messageTemplate, []);

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            if (level == LogEventLevel.Warning && source?.GetType().Name == "OptionalStyleInclude")
            {
                Warnings.Add(string.Join(" ", [messageTemplate, .. propertyValues.Select(value => value?.ToString())]));
            }
        }
    }
}
