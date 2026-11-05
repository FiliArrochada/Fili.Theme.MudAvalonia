// Keeps docs/testing.md's numbers true: each test project's total in the table at the top, the
// unit suite's "N tests in M classes", and each unit test class's row. A theory counts once per
// data row, as the runner counts it.
//
// Usage, from anywhere:
//
//     dotnet run tools/test-counts.cs                 rewrites the numbers that moved
//     dotnet run tools/test-counts.cs -- --check      changes nothing; fails if one moved
//     dotnet run tools/test-counts.cs -- --no-build   lists the tests as last built
//     ... --configuration Release                     the configuration to list (CI's)
//
// A class with no row, or a row whose class is gone, is reported and fails --check either way:
// a new class gets its row written by hand, saying what it checks and why.

using System.Diagnostics;
using System.Text.RegularExpressions;

var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var check = false;
var noBuild = false;
string? configuration = null;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--check": check = true; break;
        case "--no-build": noBuild = true; break;
        case "--configuration" when i + 1 < args.Length: configuration = args[++i]; break;
        default: return Fail("usage: dotnet run tools/test-counts.cs -- [--check] [--no-build] [--configuration <name>]");
    }
}

string[] projects = ["Fili.Theme.MudAvalonia.UnitTests", "Fili.Theme.MudAvalonia.NoDataGridTests", "Fili.Theme.MudAvalonia.PixelTests"];
var tests = new Dictionary<string, List<string>>();
foreach (var project in projects)
{
    var arguments = new List<string> { "test", "--project", Path.Combine(root, "tst", project), "--list-tests" };
    if (noBuild)
    {
        arguments.Add("--no-build");
    }

    if (configuration is not null)
    {
        arguments.AddRange(["--configuration", configuration]);
    }

    var (code, output) = Run(root, "dotnet", arguments);
    var listed = output.Split('\n')
        .Select(line => line.TrimEnd('\r'))
        .Where(line => line.StartsWith("  " + project + ".", StringComparison.Ordinal))
        .Select(line => line.Trim())
        .ToList();
    if (code != 0 || listed.Count == 0)
    {
        Console.Error.WriteLine(output);
        return Fail($"listing {project}'s tests failed.");
    }

    tests[project] = listed;
}

// The class is the name before the method; a theory's arguments may hold dots of their own.
static string ClassOf(string test)
{
    var name = test.Split('(')[0];
    var parts = name.Split('.');
    return parts[^2];
}

var unit = tests["Fili.Theme.MudAvalonia.UnitTests"];
var perClass = unit.GroupBy(ClassOf).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

var path = Path.Combine(root, "docs", "testing.md");
var original = File.ReadAllText(path);
var crlf = original.Contains("\r\n");
var text = original.Replace("\r\n", "\n");
var moved = new List<string>();

// The table at the top: "| `tst/<project>` | 1643 tests ...".
foreach (var project in projects)
{
    text = Regex.Replace(text, $@"(\| `tst/{Regex.Escape(project)}` \| )(\d+)( tests)", m =>
    {
        var count = tests[project].Count.ToString();
        if (m.Groups[2].Value != count)
        {
            moved.Add($"{project}: {m.Groups[2].Value} -> {count} tests");
        }

        return m.Groups[1].Value + count + m.Groups[3].Value;
    });
}

text = Regex.Replace(text, @"^(\d+) tests in (\d+) classes", m =>
{
    var (count, classes) = (unit.Count.ToString(), perClass.Count.ToString());
    if (m.Groups[1].Value != count || m.Groups[2].Value != classes)
    {
        moved.Add($"the unit suite: {m.Groups[1].Value} tests in {m.Groups[2].Value} classes -> {count} in {classes}");
    }

    return $"{count} tests in {classes} classes";
}, RegexOptions.Multiline);

// Each unit class's row: "| `ClassName` | 7 | ...".
var rows = new HashSet<string>(StringComparer.Ordinal);
text = Regex.Replace(text, @"^\| `(\w+)` \| (\d+) \|", m =>
{
    var name = m.Groups[1].Value;
    rows.Add(name);
    if (!perClass.TryGetValue(name, out var count))
    {
        return m.Value;
    }

    if (m.Groups[2].Value != count.ToString())
    {
        moved.Add($"{name}: {m.Groups[2].Value} -> {count}");
    }

    return $"| `{name}` | {count} |";
}, RegexOptions.Multiline);

var unlisted = perClass.Keys.Except(rows).Order(StringComparer.Ordinal).ToList();
var gone = rows.Where(name => name.EndsWith("Tests", StringComparison.Ordinal) && !perClass.ContainsKey(name)).Order(StringComparer.Ordinal).ToList();

foreach (var line in moved)
{
    Console.WriteLine((check ? "stale: " : "moved: ") + line);
}

foreach (var name in unlisted)
{
    Console.WriteLine($"no row: {name} ({perClass[name]} tests) - write it under the failure it catches");
}

foreach (var name in gone)
{
    Console.WriteLine($"no class: {name} has a row in docs/testing.md and no tests");
}

if (check)
{
    var stale = moved.Count + unlisted.Count + gone.Count;
    Console.WriteLine(stale == 0 ? "docs/testing.md's numbers are current." : "docs/testing.md is stale: run dotnet run tools/test-counts.cs.");
    return stale == 0 ? 0 : 1;
}

if (moved.Count > 0)
{
    File.WriteAllText(path, crlf ? text.Replace("\n", "\r\n") : text);
}

Console.WriteLine(moved.Count == 0 ? "docs/testing.md's numbers are current." : $"docs/testing.md: {moved.Count} number(s) rewritten.");
return unlisted.Count + gone.Count == 0 ? 0 : 1;

static (int Code, string Output) Run(string directory, string file, IEnumerable<string> arguments)
{
    var start = new ProcessStartInfo(file) { WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var argument in arguments)
    {
        start.ArgumentList.Add(argument);
    }

    using var process = Process.Start(start)!;
    var error = process.StandardError.ReadToEndAsync();
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, output + error.Result);
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
