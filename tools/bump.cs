// The release bump, and the package validation baseline after it (docs/releasing.md).
//
//     <version>   docs/versions/unreleased.md becomes <version>.md: its heading is the version,
//                 its "Work since" line goes, its Upgrading heading gains "to <version>". The
//                 index gets the version's row, in bold, and the previous release loses its
//                 bold; a fresh unreleased.md says "Nothing yet."; <Version> in
//                 Directory.Build.props is the version. Then tools/release-notes.cs checks the
//                 page as the release would.
//     baseline    once nuget.org lists the version in Directory.Build.props, moves
//                 PackageValidationBaselineVersion in the theme's csproj to it, and clears the
//                 local HTTP cache, which otherwise hides a version listed minutes ago from the
//                 restore. Before then it says so and changes nothing.
//
// Usage, from anywhere:
//
//     dotnet run tools/bump.cs -- 1.0.0
//     dotnet run tools/bump.cs -- baseline
//
// It writes the working tree only; the commit, the push and the tag stay the maintainer's.

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var versions = Path.Combine(root, "docs", "versions");
var props = Path.Combine(root, "Directory.Build.props");
var csproj = Path.Combine(root, "src", "Fili.Theme.MudAvalonia", "Fili.Theme.MudAvalonia.csproj");

return args switch
{
    ["baseline"] => await Baseline(),
    [var version] when Regex.IsMatch(version, @"^\d+\.\d+\.\d+(-[0-9A-Za-z.]+)?$") => Bump(version),
    _ => Fail("usage: dotnet run tools/bump.cs -- <version> | baseline"),
};

int Bump(string version)
{
    var previous = Regex.Match(Read(props), "<Version>([^<]+)</Version>").Groups[1].Value;
    var page = Path.Combine(versions, version + ".md");
    if (File.Exists(page))
    {
        return Fail($"docs/versions/{version}.md already exists.");
    }

    var unreleased = Path.Combine(versions, "unreleased.md");
    var text = Read(unreleased);
    if (!text.Contains("\n## What changed\n", StringComparison.Ordinal))
    {
        return Fail("unreleased.md has no '## What changed' list: nothing has landed since " + previous + ".");
    }

    // The heading, the "Work since" paragraph gone, and the Upgrading heading's target.
    text = Regex.Replace(text, @"\A# Unreleased\n", $"# {version}\n");
    text = Regex.Replace(text, @"\nWork since [^\n]*(?:\n[^\n]+)*\n", "");
    text = Regex.Replace(text, @"^## Upgrading from (\S+)\s*$", $"## Upgrading from $1 to {version}", RegexOptions.Multiline);
    var upgrading = Regex.Match(text, @"^## (Upgrading from \S+ to \S+)$", RegexOptions.Multiline);
    if (!upgrading.Success)
    {
        return Fail("unreleased.md has no '## Upgrading from <version>' section.");
    }

    if (Regex.IsMatch(text, @"\[All versions\]\(README\.md\)\n\n## What changed"))
    {
        Console.WriteLine("note: the page has no summary above 'What changed'; the pages open with two or three sentences.");
    }

    Write(page, text, LineEnding(unreleased));
    Write(unreleased, $"""
        # Unreleased

        Work since {version}, kept here as it lands; at the bump this file is renamed to its version, and
        this line goes.

        [All versions](README.md)

        Nothing yet.

        """, LineEnding(unreleased));

    // The index: the new row in bold above the previous release, which loses its bold.
    var index = Path.Combine(versions, "README.md");
    var rows = Read(index);
    var bold = $"| **[{previous}]({previous}.md)** |";
    if (!rows.Contains(bold, StringComparison.Ordinal))
    {
        return Fail($"docs/versions/README.md has no bold row for {previous}.");
    }

    var from = upgrading.Groups[1].Value.Split(' ')[2];
    var row = $"| **[{version}]({version}.md)** | [Upgrading from {from}]({version}.md#{Anchor(upgrading.Groups[1].Value)}) |\n";
    rows = rows.Replace(bold, row + $"| [{previous}]({previous}.md) |");
    Write(index, rows, LineEnding(index));

    Write(props, Read(props).Replace($"<Version>{previous}</Version>", $"<Version>{version}</Version>"), LineEnding(props));

    Console.WriteLine($"{previous} -> {version}: docs/versions/{version}.md, its index row, a fresh unreleased.md and <Version>.");
    var notes = Path.Combine(Path.GetTempPath(), $"release-notes-{version}.md");
    var check = Run("dotnet", "run", Path.Combine(root, "tools", "release-notes.cs"), "--", version, "v" + version, notes);
    Console.WriteLine(check == 0 ? $"The release notes check passes ({notes})." : "The release notes check FAILED: fix the page before committing.");
    return check;
}

async Task<int> Baseline()
{
    var version = Regex.Match(Read(props), "<Version>([^<]+)</Version>").Groups[1].Value;
    var project = Read(csproj);
    var baseline = Regex.Match(project, "<PackageValidationBaselineVersion>([^<]+)</PackageValidationBaselineVersion>").Groups[1].Value;
    var id = Regex.Match(project, "<PackageId>([^<]+)</PackageId>").Groups[1].Value;
    if (baseline == version)
    {
        Console.WriteLine($"The baseline is {version} already.");
        return 0;
    }

    using var http = new HttpClient();
    using var json = JsonDocument.Parse(await http.GetStringAsync($"https://api.nuget.org/v3-flatcontainer/{id.ToLowerInvariant()}/index.json"));
    var listed = json.RootElement.GetProperty("versions").EnumerateArray().Any(v => v.GetString() == version.ToLowerInvariant());
    if (!listed)
    {
        Console.WriteLine($"nuget.org does not list {id} {version} yet: the baseline stays {baseline}. Run this again once the release is approved and listed.");
        return 2;
    }

    Write(csproj, project.Replace(
        $"<PackageValidationBaselineVersion>{baseline}</PackageValidationBaselineVersion>",
        $"<PackageValidationBaselineVersion>{version}</PackageValidationBaselineVersion>"), LineEnding(csproj));
    Run("dotnet", "nuget", "locals", "http-cache", "--clear");
    Console.WriteLine($"The baseline is {version} (was {baseline}). Check it with the PackageContentTests before committing.");
    return 0;
}

// GitHub's heading anchor: lower case, punctuation but hyphens dropped, spaces to hyphens.
static string Anchor(string heading) =>
    Regex.Replace(heading.ToLowerInvariant(), @"[^a-z0-9 \-]", "").Replace(' ', '-');

static string Read(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

static string LineEnding(string path) => File.ReadAllText(path).Contains("\r\n") ? "\r\n" : "\n";

static void Write(string path, string text, string newline) =>
    File.WriteAllText(path, text.Replace("\r\n", "\n").Replace("\n", newline));

static int Run(string file, params string[] arguments)
{
    var start = new ProcessStartInfo(file) { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var argument in arguments)
    {
        start.ArgumentList.Add(argument);
    }

    using var process = Process.Start(start)!;
    var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        Console.Error.Write(output);
    }

    return process.ExitCode;
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
