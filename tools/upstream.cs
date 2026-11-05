// Follows the two upstreams the theme is made from: Avalonia's Simple theme, forked into
// Themes/Base/, and MudBlazor, whose values the theme transcribes.
//
//     check            the newest Avalonia, Avalonia.Controls.DataGrid and MudBlazor against the
//                      versions the theme is on
//     avalonia <tag>   re-forks Simple at <tag> (FORK.md's fetch and its two edits), writes the
//                      changed templates into Themes/Base/, moves AvaloniaVersion, FORK.md's tag
//                      and the README's "Depends on" row to it, and lists what changed: templates,
//                      controls added or gone, and keys a template now reads that nothing defines
//     mudblazor <tag> [from]
//                      writes nothing: lists what MudBlazor <tag> changed since [from], or the
//                      version docs/compatibility.md names, in what the theme transcribes - Themes/Models
//                      and Colors (token values), Styles (component metrics), each component's
//                      [Parameter]s (parity rows), and the icons the theme draws
//
// Usage, from anywhere:
//
//     dotnet run tools/upstream.cs -- check
//     dotnet run tools/upstream.cs -- avalonia 12.1.5
//     dotnet run tools/upstream.cs -- mudblazor 9.12.0
//
// The rest of an upgrade is by hand: `git diff` the fork, add a line to Accents.axaml for a new
// key, a template for a new control, restore and run every suite (docs/releasing.md).

using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var theme = Path.Combine(root, "src", "Fili.Theme.MudAvalonia");
var fork = Path.Combine(theme, "Themes", "Base");
using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("Fili.Theme.MudAvalonia-upstream");

return args switch
{
    ["check"] => await Check(),
    ["avalonia", var tag] => await Avalonia(tag),
    ["mudblazor", var tag] => await MudBlazor(tag.TrimStart('v'), null),
    ["mudblazor", var tag, var from] => await MudBlazor(tag.TrimStart('v'), from.TrimStart('v')),
    _ => Fail("usage: dotnet run tools/upstream.cs -- check | avalonia <tag> | mudblazor <tag> [from]"),
};

// ---------------------------------------------------------------------------------------------
// check
// ---------------------------------------------------------------------------------------------

async Task<int> Check()
{
    var props = File.ReadAllText(Path.Combine(root, "Directory.Packages.props"));
    var avalonia = Regex.Match(props, "<AvaloniaVersion>([^<]+)</AvaloniaVersion>").Groups[1].Value;
    var dataGrid = Regex.Match(props, "Include=\"Avalonia.Controls.DataGrid\" Version=\"([^\"]+)\"").Groups[1].Value;
    var mud = TrackedMudBlazor();

    Console.WriteLine($"Avalonia                      {avalonia,-10} newest {await NewestOnNuGet("avalonia")}");
    Console.WriteLine($"Avalonia.Controls.DataGrid    {dataGrid,-10} newest {await NewestOnNuGet("avalonia.controls.datagrid")}  (held at the floor on purpose)");
    Console.WriteLine($"MudBlazor                     {mud,-10} newest {await NewestOnNuGet("mudblazor")}");
    return 0;
}

async Task<string> NewestOnNuGet(string id)
{
    using var json = JsonDocument.Parse(await http.GetStringAsync($"https://api.nuget.org/v3-flatcontainer/{id}/index.json"));
    return json.RootElement.GetProperty("versions").EnumerateArray()
        .Select(v => v.GetString()!)
        .Where(v => !v.Contains('-'))
        .Last();
}

string TrackedMudBlazor()
{
    var compatibility = File.ReadAllText(Path.Combine(root, "docs", "compatibility.md"));
    var match = Regex.Match(compatibility, @"transcribes MudBlazor (\d+\.\d+(?:\.\d+)?)");
    return match.Success ? match.Groups[1].Value : throw new InvalidOperationException("docs/compatibility.md names no MudBlazor version.");
}

// ---------------------------------------------------------------------------------------------
// avalonia <tag>
// ---------------------------------------------------------------------------------------------

async Task<int> Avalonia(string tag)
{
    var raw = $"https://raw.githubusercontent.com/AvaloniaUI/Avalonia/{tag}/src";
    using var listing = JsonDocument.Parse(await http.GetStringAsync(
        $"https://api.github.com/repos/AvaloniaUI/Avalonia/contents/src/Avalonia.Themes.Simple/Controls?ref={tag}"));
    var names = listing.RootElement.EnumerateArray()
        .Select(entry => entry.GetProperty("name").GetString()!)
        .Where(name => name.EndsWith(".xaml", StringComparison.Ordinal))
        .ToList();

    // FORK.md's two edits: .xaml to .axaml, and Simple's own paths rebased onto the package.
    static string Rebased(string xaml) => Regex.Replace(xaml,
        @"avares://Avalonia\.Themes\.Simple/Controls/(\w+)\.xaml",
        "avares://Fili.Theme.MudAvalonia/Themes/Base/Controls/$1.axaml");

    var upstream = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var name in names)
    {
        upstream["Controls/" + Path.ChangeExtension(name, ".axaml")] = Rebased(await http.GetStringAsync($"{raw}/Avalonia.Themes.Simple/Controls/{name}"));
    }

    upstream["InvariantResources.axaml"] = await http.GetStringAsync($"{raw}/Avalonia.Themes.Fluent/Strings/InvariantResources.xaml");

    var ours = Directory.EnumerateFiles(Path.Combine(fork, "Controls"), "*.axaml")
        .Select(path => "Controls/" + Path.GetFileName(path))
        .Append("InvariantResources.axaml")
        .ToHashSet(StringComparer.Ordinal);
    var before = ours.ToDictionary(file => file, file => File.ReadAllText(Path.Combine(fork, file)), StringComparer.Ordinal);

    var changed = new List<string>();
    foreach (var (file, text) in upstream)
    {
        var path = Path.Combine(fork, file);
        if (before.TryGetValue(file, out var old) && Normal(old) == Normal(text))
        {
            continue;
        }

        // Kept in the line ending the fork's copy has, so the diff is the change alone.
        var crlf = old?.Contains("\r\n") ?? false;
        File.WriteAllText(path, crlf ? Normal(text).Replace("\n", "\r\n") : Normal(text));
        if (old is not null)
        {
            changed.Add(file);
        }
    }

    var added = upstream.Keys.Except(ours).Order(StringComparer.Ordinal).ToList();
    var gone = ours.Except(upstream.Keys).Order(StringComparer.Ordinal).ToList();

    // A key a template now reads that it did not before, and that nothing in the theme or the
    // new fork defines, resolves to nothing: it needs a line in Accents.axaml.
    static IEnumerable<string> Reads(IEnumerable<string> texts) => texts
        .SelectMany(text => Regex.Matches(text, @"\{(?:DynamicResource|StaticResource)\s+([\w.]+)\}").Select(m => m.Groups[1].Value));
    var defined = Directory.EnumerateFiles(Path.Combine(theme, "Themes"), "*.axaml", SearchOption.AllDirectories)
        .Select(File.ReadAllText)
        .Concat(upstream.Values)
        .SelectMany(text => Regex.Matches(text, "x:Key=\"([^\"]+)\"").Select(m => m.Groups[1].Value))
        .ToHashSet(StringComparer.Ordinal);
    var newKeys = Reads(upstream.Values).Except(Reads(before.Values)).Distinct().Order(StringComparer.Ordinal).ToList();
    var undefined = newKeys.Where(key => !defined.Contains(key)).ToList();

    var previous = SetVersion(tag);

    Console.WriteLine($"Simple at {tag}, against the fork at {previous}:");
    Section("templates changed (take the upstream fix; git diff shows it)", changed);
    Section("controls added (written; give each a place in SimpleControls.axaml and StandaloneReadinessTests)", added);
    Section("controls gone upstream (still in the fork; remove them and their includes)", gone);
    Section("keys the templates now read", newKeys);
    Section("of those, defined nowhere (each needs a line in Accents.axaml)", undefined);
    Console.WriteLine();
    Console.WriteLine($"AvaloniaVersion, FORK.md and the README's \"Depends on\" row now say {tag}.");
    Console.WriteLine("Next: git diff, then `dotnet restore Fili.Theme.MudAvalonia.slnx --force-evaluate` and every suite.");
    return 0;
}

string SetVersion(string tag)
{
    var props = Path.Combine(root, "Directory.Packages.props");
    var previous = Regex.Match(File.ReadAllText(props), "<AvaloniaVersion>([^<]+)</AvaloniaVersion>").Groups[1].Value;
    Replace(props, $"<AvaloniaVersion>{previous}</AvaloniaVersion>", $"<AvaloniaVersion>{tag}</AvaloniaVersion>");
    Replace(Path.Combine(fork, "FORK.md"), $"tag **{previous}**", $"tag **{tag}**");
    Replace(Path.Combine(fork, "FORK.md"), $"/blob/{previous}/licence.md", $"/blob/{tag}/licence.md");
    Replace(Path.Combine(root, "README.md"), $"`Avalonia` **{previous} or later, below 13**", $"`Avalonia` **{tag} or later, below 13**");
    return previous;
}

void Replace(string path, string from, string to)
{
    var text = File.ReadAllText(path);
    if (from == to)
    {
        return;
    }

    if (!text.Contains(from, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"{Path.GetRelativePath(root, path)} has no \"{from}\" to move.");
    }

    File.WriteAllText(path, text.Replace(from, to, StringComparison.Ordinal));
}

// ---------------------------------------------------------------------------------------------
// mudblazor <tag>
// ---------------------------------------------------------------------------------------------

async Task<int> MudBlazor(string tag, string? since)
{
    var tracked = TrackedMudBlazor();
    var from = since ?? (tracked.Count(c => c == '.') == 1 ? tracked + ".0" : tracked);
    var before = await MudBlazorSource(from);
    var after = await MudBlazorSource(tag);

    Console.WriteLine($"MudBlazor {from} to {tag}, in what the theme transcribes:");

    Diffs("token values (Themes/Models, Colors): a changed value moves a token", file => file.StartsWith("Themes/", StringComparison.Ordinal) || file.StartsWith("Colors/", StringComparison.Ordinal));
    Diffs("component metrics (Styles): a changed rule moves a control's look", file => file.StartsWith("Styles/", StringComparison.Ordinal) && file.EndsWith(".scss", StringComparison.Ordinal));

    Console.WriteLine();
    Console.WriteLine("== parameters (each is a parity row on its component's page)");
    var any = false;
    foreach (var file in after.Keys.Union(before.Keys).Where(f => f.StartsWith("Components/", StringComparison.Ordinal) && f.EndsWith(".razor.cs", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
    {
        var old = Parameters(before.GetValueOrDefault(file));
        var @new = Parameters(after.GetValueOrDefault(file));
        var lines = @new.Keys.Except(old.Keys).Select(name => $"+ {name}{Default(@new[name])}")
            .Concat(old.Keys.Except(@new.Keys).Select(name => $"- {name}"))
            .Concat(@new.Keys.Intersect(old.Keys).Where(name => old[name] != @new[name]).Select(name => $"~ {name}: {old[name] ?? "(none)"} -> {@new[name] ?? "(none)"}"))
            .ToList();
        if (lines.Count > 0)
        {
            any = true;
            Console.WriteLine($"  {Path.GetFileName(file).Replace(".razor.cs", "")}");
            lines.ForEach(line => Console.WriteLine("    " + line));
        }
    }

    if (!any)
    {
        Console.WriteLine("  (none)");
    }

    // The theme's glyphs, each under an `Icons.Material.Filled.X` comment, against the new path.
    Console.WriteLine();
    Console.WriteLine("== icons the theme draws (Themes/Icons.axaml)");
    var icons = File.ReadAllText(Path.Combine(theme, "Themes", "Icons.axaml"));
    var filled = after["Icons/Material/Filled.cs"];
    var drift = new List<string>();
    foreach (Match glyph in Regex.Matches(icons, @"<!--\s*Icons\.Material\.Filled\.(\w+)\s*-->\s*<StreamGeometry[^>]*>([^<]+)</StreamGeometry>"))
    {
        var name = glyph.Groups[1].Value;
        var upstreamPath = Regex.Match(filled, $"const string {name} = \"(.*?)\";");
        // The viewbox spacer is a path with fill="none" (M0 0h24v24H0z, or ...H0V0z), dropped as
        // Icons.axaml drops it.
        var paths = Regex.Matches(upstreamPath.Groups[1].Value.Replace("\\\"", "\""), "<path d=\"([^\"]+)\"(\\s+fill=\"none\")?")
            .Where(m => !m.Groups[2].Success)
            .Select(m => m.Groups[1].Value)
            .ToList();
        if (!upstreamPath.Success)
        {
            drift.Add($"{name}: gone from Filled.cs");
        }
        else if (string.Join(" ", paths) != glyph.Groups[2].Value.Trim())
        {
            drift.Add($"{name}: the path differs from {tag}'s");
        }
    }

    Console.WriteLine(drift.Count == 0 ? "  every glyph matches" : string.Join("\n", drift.Select(d => "  " + d)));
    Console.WriteLine();
    Console.WriteLine($"Following it: change docs/compatibility.md's \"transcribes MudBlazor {tracked}\" with the work.");
    return 0;

    void Diffs(string title, Func<string, bool> include)
    {
        Console.WriteLine();
        Console.WriteLine($"== {title}");
        var files = after.Keys.Union(before.Keys).Where(include).Order(StringComparer.Ordinal).ToList();
        var shown = 0;
        foreach (var file in files)
        {
            var old = before.GetValueOrDefault(file) ?? "";
            var @new = after.GetValueOrDefault(file) ?? "";
            if (old == @new)
            {
                continue;
            }

            shown++;
            Console.WriteLine($"  {file}{(old.Length == 0 ? " (new)" : @new.Length == 0 ? " (gone)" : "")}");
            foreach (var line in LineDiff(old.Split('\n'), @new.Split('\n')))
            {
                Console.WriteLine("    " + line);
            }
        }

        if (shown == 0)
        {
            Console.WriteLine("  (none)");
        }
    }

    static string Default(string? value) => value is null ? "" : $" = {value}";
}

// src/MudBlazor of a tag, from its source archive, as relative path -> text.
async Task<Dictionary<string, string>> MudBlazorSource(string tag)
{
    var files = new Dictionary<string, string>(StringComparer.Ordinal);
    await using var archive = await http.GetStreamAsync($"https://codeload.github.com/MudBlazor/MudBlazor/tar.gz/refs/tags/v{tag}");
    await using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    await using var tar = new TarReader(gzip);
    while (await tar.GetNextEntryAsync() is { } entry)
    {
        var marker = entry.Name.IndexOf("/src/MudBlazor/", StringComparison.Ordinal);
        if (entry.EntryType != TarEntryType.RegularFile || entry.DataStream is null || marker < 0)
        {
            continue;
        }

        var name = entry.Name[(marker + "/src/MudBlazor/".Length)..];
        if (name.StartsWith("Themes/", StringComparison.Ordinal) || name.StartsWith("Colors/", StringComparison.Ordinal)
            || name.StartsWith("Styles/", StringComparison.Ordinal) || name.StartsWith("Components/", StringComparison.Ordinal)
            || name == "Icons/Material/Filled.cs")
        {
            using var reader = new StreamReader(entry.DataStream);
            files[name] = Normal(await reader.ReadToEndAsync());
        }
    }

    return files.Count > 0 ? files : throw new InvalidOperationException($"MudBlazor v{tag} has no src/MudBlazor: is the tag right?");
}

// Each [Parameter] property's name and its default, or null when it has none written.
static Dictionary<string, string?> Parameters(string? source)
{
    var found = new Dictionary<string, string?>(StringComparer.Ordinal);
    if (source is null)
    {
        return found;
    }

    foreach (Match m in Regex.Matches(source, @"\[Parameter\b[^\]]*\](?:\s*\[[^\]]*\])*\s*public\s+(?:virtual\s+|override\s+|new\s+)*.+?\s+(\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+);)?"))
    {
        found[m.Groups[1].Value] = m.Groups[2].Success ? m.Groups[2].Value.Trim() : null;
    }

    return found;
}

// ---------------------------------------------------------------------------------------------
// helpers
// ---------------------------------------------------------------------------------------------

static string Normal(string text) => text.Replace("\r\n", "\n");

// The lines only in one side, by longest common subsequence, after the common start and end.
static IEnumerable<string> LineDiff(string[] a, string[] b)
{
    var start = 0;
    while (start < a.Length && start < b.Length && a[start] == b[start])
    {
        start++;
    }

    var end = 0;
    while (end < a.Length - start && end < b.Length - start && a[^(end + 1)] == b[^(end + 1)])
    {
        end++;
    }

    var x = a[start..^end];
    var y = b[start..^end];
    var lcs = new int[x.Length + 1, y.Length + 1];
    for (var i = x.Length - 1; i >= 0; i--)
    {
        for (var j = y.Length - 1; j >= 0; j--)
        {
            lcs[i, j] = x[i] == y[j] ? lcs[i + 1, j + 1] + 1 : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
        }
    }

    for (int i = 0, j = 0; i < x.Length || j < y.Length;)
    {
        if (i < x.Length && j < y.Length && x[i] == y[j])
        {
            i++;
            j++;
        }
        else if (j < y.Length && (i == x.Length || lcs[i, j + 1] >= lcs[i + 1, j]))
        {
            yield return "+ " + y[j++].Trim();
        }
        else
        {
            yield return "- " + x[i++].Trim();
        }
    }
}

static void Section(string title, List<string> items)
{
    Console.WriteLine();
    Console.WriteLine($"== {title}");
    Console.WriteLine(items.Count == 0 ? "  (none)" : string.Join("\n", items.Select(item => "  " + item)));
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
