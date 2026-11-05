// Times a style-cost change: the benchmark (tst/Fili.Theme.MudAvalonia.Benchmarks) on a clean
// export of a ref and on the working tree, alternated, so a machine that slows down part way
// slows both. Prints the mean of the rounds as the table docs/testing.md carries, with each
// side's allocations and the largest error any one run reported.
//
// Usage, from anywhere, on an idle machine:
//
//     dotnet run tools/bench.cs                                  every benchmark, against HEAD, 2 rounds
//     dotnet run tools/bench.cs -- --filter "*BuildScreen*"      one benchmark
//     dotnet run tools/bench.cs -- --against v1.0.0-rc.2 --rounds 3
//
// A round runs each side once; the whole suite takes about ten minutes a side.

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;

var root = Path.GetFullPath(Path.Combine((string)AppContext.GetData("EntryPointFileDirectoryPath")!, ".."));
var filter = "*";
var against = "HEAD";
var rounds = 2;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--filter" when i + 1 < args.Length: filter = args[++i]; break;
        case "--against" when i + 1 < args.Length: against = args[++i]; break;
        case "--rounds" when i + 1 < args.Length && int.TryParse(args[i + 1], out var n) && n > 0: rounds = n; i++; break;
        default: return Fail("usage: dotnet run tools/bench.cs -- [--filter <pattern>] [--against <ref>] [--rounds <n>]");
    }
}

// The ref, exported clean: no build output, no working-tree change.
var export = Path.Combine(Path.GetTempPath(), "fili-bench-" + Guid.NewGuid().ToString("N")[..8]);
var zip = export + ".zip";
if (Run(root, "git", "archive", "--format=zip", "-o", zip, against).Code != 0)
{
    return Fail($"git archive {against} failed: is it a commit or tag?");
}

ZipFile.ExtractToDirectory(zip, export);
File.Delete(zip);

var results = new Dictionary<(string Side, string Method, string Theme), List<Row>>();
try
{
    for (var round = 1; round <= rounds; round++)
    {
        foreach (var (side, directory) in new[] { (against, export), ("working tree", root) })
        {
            Console.Error.WriteLine($"round {round}/{rounds}: {side}");
            var run = Run(directory, "dotnet", "run", "-c", "Release", "--project",
                Path.Combine(directory, "tst", "Fili.Theme.MudAvalonia.Benchmarks"), "--", "--filter", filter);
            var rows = Parse(run.Output).ToList();
            if (run.Code != 0 || rows.Count == 0)
            {
                Console.Error.WriteLine(run.Output);
                return Fail($"the benchmark on {side} failed or reported nothing.");
            }

            foreach (var row in rows)
            {
                var key = (side, row.Method, row.Theme);
                results.TryAdd(key, []);
                results[key].Add(row);
            }
        }
    }
}
finally
{
    Directory.Delete(export, recursive: true);
}

Console.WriteLine($"| Method | Theme | {against} | Working tree | Allocated, {against} | Allocated, working tree |");
Console.WriteLine("|---|---|---:|---:|---:|---:|");
var error = 0.0;
foreach (var (method, theme) in results.Keys.Where(k => k.Side == against).Select(k => (k.Method, k.Theme)).Distinct())
{
    var before = results[(against, method, theme)];
    var after = results.GetValueOrDefault(("working tree", method, theme)) ?? [];
    error = Math.Max(error, before.Concat(after).Max(r => r.ErrorMs));
    Console.WriteLine($"| {method} | {theme} | {Ms(before)} | {Ms(after)} | {Mb(before)} | {Mb(after)} |");
}

Console.WriteLine();
Console.WriteLine($"The mean of {rounds} alternated rounds; the largest error any one run reported was {error.ToString("N0", CultureInfo.InvariantCulture)} ms.");
return 0;

static string Ms(List<Row> rows) => rows.Count == 0 ? "-" : rows.Average(r => r.MeanMs).ToString("N0", CultureInfo.InvariantCulture) + " ms";

static string Mb(List<Row> rows) => rows.Count == 0 ? "-" : rows.Average(r => r.AllocatedMb).ToString("0.0#", CultureInfo.InvariantCulture) + " MB";

// BenchmarkDotNet's summary table: a header naming the columns, then one row per case.
static IEnumerable<Row> Parse(string output)
{
    string[]? header = null;
    foreach (var line in output.Split('\n').Select(l => l.Trim()).Where(l => l.StartsWith('|')))
    {
        var cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
        if (cells.Contains("Method") && cells.Contains("Mean"))
        {
            header = cells;
            continue;
        }

        if (header is null || cells.All(c => c.All(ch => ch is '-' or ':')))
        {
            continue;
        }

        string Cell(string name) => Array.IndexOf(header, name) is var i and >= 0 && i < cells.Length ? cells[i] : "";
        yield return new Row(Cell("Method"), Cell("Theme"), Time(Cell("Mean")), Time(Cell("Error")), Size(Cell("Allocated")));
    }
}

static double Time(string cell)
{
    var m = Regex.Match(cell, @"([\d,.]+)\s*(ns|us|μs|ms|s)\b");
    if (!m.Success)
    {
        return 0;
    }

    var value = double.Parse(m.Groups[1].Value.Replace(",", ""), CultureInfo.InvariantCulture);
    return m.Groups[2].Value switch { "ns" => value / 1e6, "us" or "μs" => value / 1e3, "s" => value * 1e3, _ => value };
}

static double Size(string cell)
{
    var m = Regex.Match(cell, @"([\d,.]+)\s*(B|KB|MB|GB)\b");
    if (!m.Success)
    {
        return 0;
    }

    var value = double.Parse(m.Groups[1].Value.Replace(",", ""), CultureInfo.InvariantCulture);
    return m.Groups[2].Value switch { "B" => value / 1048576, "KB" => value / 1024, "GB" => value * 1024, _ => value };
}

static (int Code, string Output) Run(string directory, string file, params string[] arguments)
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

record Row(string Method, string Theme, double MeanMs, double ErrorMs, double AllocatedMb);
