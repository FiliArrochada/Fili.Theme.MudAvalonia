// Checks a published browser gallery for the compiled XAML that trimming can strip.
//
// XAML loaded by its path at runtime is found through a type the XAML compiler generates,
// `CompiledAvaloniaXaml.!XamlLoader`, by reflection, so a trimmed build drops it unless a
// DynamicDependency keeps it. The desktop tests never trim, so only a published build shows the
// loss: every docs example, or the theme's DataGrid styles, failing with "No precompiled XAML found".
//
// Usage, from the repository root after `dotnet publish src/Fili.Theme.MudAvalonia.Gallery.Browser
// --configuration Release --output publish`:
//
//     dotnet run tools/check-gallery-publish.cs -- publish

using System.Text;
using System.Text.RegularExpressions;

var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var examples = Path.Combine(root, "src", "Fili.Theme.MudAvalonia.Gallery", "Docs", "Examples");
var framework = Path.Combine(args.Length > 0 ? args[0] : "publish", "wwwroot", "_framework");
var loader = Encoding.ASCII.GetBytes("!XamlLoader");
var failures = new List<string>();

var gallery = Assembly("Fili.Theme.MudAvalonia.Gallery");
var expected = Directory.EnumerateFiles(examples, "*.axaml", SearchOption.AllDirectories)
    .Select(path => Path.ChangeExtension(Path.GetRelativePath(examples, path), null).Replace('\\', '/'))
    .ToHashSet();
var kept = Regex.Matches(Strings(gallery), @"avares://Fili\.Theme\.MudAvalonia\.Gallery/Docs/Examples/([A-Za-z0-9/]+)\.axaml")
    .Select(match => match.Groups[1].Value)
    .ToHashSet();
if (gallery.AsSpan().IndexOf(loader) < 0)
{
    failures.Add("the gallery's compiled XAML loader was trimmed: ExampleCard's DynamicDependency is missing");
}

var missing = expected.Except(kept).Order(StringComparer.Ordinal).ToList();
if (missing.Count > 0)
{
    failures.Add($"{missing.Count} of {expected.Count} docs examples cannot be loaded, e.g. {string.Join(", ", missing.Take(5))}");
}

var theme = Assembly("Fili.Theme.MudAvalonia");
if (theme.AsSpan().IndexOf(loader) < 0 || !Strings(theme).Contains("Themes/Optional/DataGrid/DataGridStyles.axaml"))
{
    failures.Add("the theme's compiled XAML loader or its DataGrid styles were trimmed: OptionalStyleInclude's DynamicDependency is missing");
}

foreach (var failure in failures)
{
    Console.WriteLine("::error::" + failure);
}

if (failures.Count == 0)
{
    Console.WriteLine($"ok: {expected.Count} docs examples and the theme's DataGrid styles survive trimming");
}

return failures.Count == 0 ? 0 : 1;

// The published assembly: a webcil .wasm, or a .dll where webcil is off, its name maybe fingerprinted.
byte[] Assembly(string name)
{
    var pattern = new Regex("^" + Regex.Escape(name) + @"(\.[a-z0-9]+)?\.(wasm|dll)$");
    var found = Directory.EnumerateFiles(framework).Where(path => pattern.IsMatch(Path.GetFileName(path))).ToList();
    if (found.Count != 1)
    {
        Console.WriteLine($"::error::expected one published {name}, found [{string.Join(", ", found.Select(Path.GetFileName))}]");
        Environment.Exit(1);
    }

    return File.ReadAllBytes(found[0]);
}

// The assembly's UTF-16 string literals, read at both byte alignments.
static string Strings(byte[] data) => Encoding.Unicode.GetString(data) + Encoding.Unicode.GetString(data, 1, data.Length - 1);
