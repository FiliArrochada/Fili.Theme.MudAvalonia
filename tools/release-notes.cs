// Writes a version's release notes from its page in docs/versions/, for the GitHub release.
//
// The page is the notes: what changed and, where existing markup changes look, how to upgrade. A
// release body cannot follow a relative link, so each one is rewritten to the file at the tag; the
// page's own heading is dropped, as the release carries the version in its title, and wrapped
// lines are joined, as a release body breaks the line at every newline. A missing page, or a link
// to a file that does not exist, fails: a release is never created without its notes.
//
// Usage, from anywhere:
//
//     dotnet run tools/release-notes.cs -- 0.27.0 v0.27.0 release-notes.md

using System.Text.RegularExpressions;

const string Repository = "https://github.com/FiliArrochada/Fili.Theme.MudAvalonia";

if (args.Length != 3)
{
    return Fail("usage: dotnet run tools/release-notes.cs -- <version> <tag> <destination>");
}

var (version, tag, destination) = (args[0], args[1], args[2]);
var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var page = Path.Combine(root, "docs", "versions", version + ".md");
if (!File.Exists(page))
{
    return Fail($"docs/versions/{version}.md does not exist: rename unreleased.md to it at the bump.");
}

var lines = File.ReadAllLines(page);
if (lines.Length == 0 || lines[0].Trim() != "# " + version)
{
    return Fail($"docs/versions/{version}.md does not open with '# {version}'.");
}

var missing = new List<string>();
var body = Regex.Replace(
    string.Join("\n", Unwrap(lines.Skip(1))).Trim() + "\n",
    @"(!?)\[([^\]]*)\]\((?!https?:|#|mailto:)([^)\s]+)\)",
    match =>
    {
        var (image, text, target) = (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        var hash = target.IndexOf('#');
        var (path, anchor) = hash < 0 ? (target, "") : (target[..hash], target[hash..]);
        var file = Path.GetRelativePath(root, Path.GetFullPath(Path.Combine(root, "docs", "versions", path))).Replace('\\', '/');
        if (!File.Exists(Path.Combine(root, file)) && !Directory.Exists(Path.Combine(root, file)))
        {
            missing.Add(target);
        }

        return $"{image}[{text}]({Repository}/{(image.Length > 0 ? "raw" : "blob")}/{tag}/{file}{anchor})";
    });

if (missing.Count > 0)
{
    return Fail($"docs/versions/{version}.md links to files that do not exist: {string.Join(", ", missing)}");
}

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
File.WriteAllText(destination, body);
return 0;

// Joins a paragraph's or list item's wrapped lines; headings, tables, quotes, list items and fenced
// code start their own line.
static IEnumerable<string> Unwrap(IEnumerable<string> lines)
{
    var block = new Regex(@"^(#|\||>|[-*] |\d+\. )");
    var unjoinable = new Regex(@"^\s*(#|\||```)");
    var output = new List<string>();
    var fenced = false;
    foreach (var line in lines)
    {
        if (line.TrimStart().StartsWith("```"))
        {
            fenced = !fenced;
            output.Add(line);
        }
        else if (fenced || line.Trim().Length == 0 || block.IsMatch(line.TrimStart()) || output.Count == 0)
        {
            output.Add(line);
        }
        else if (output[^1].Trim().Length > 0 && !unjoinable.IsMatch(output[^1]))
        {
            output[^1] += " " + line.Trim();
        }
        else
        {
            output.Add(line);
        }
    }

    return output;
}

static int Fail(string message)
{
    Console.Error.WriteLine("::error::" + message);
    return 1;
}
