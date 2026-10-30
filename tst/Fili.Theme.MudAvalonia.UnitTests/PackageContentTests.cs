using System.Diagnostics;
using System.IO.Compression;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The package carries third-party material - Avalonia's templates (MIT, eight of them Ms-PL),
/// Material Icons' glyphs (Apache-2.0), MudBlazor's values (MIT) and Roboto (OFL) - whose licences
/// require their notices to travel with it. This packs the theme as built for this run, without
/// building it again, and reads the <c>.nupkg</c>: a notice dropped from the csproj fails here
/// rather than on nuget.org, where a published version can never be replaced.
/// </summary>
public class PackageContentTests
{
    [Fact]
    public void ThePackageCarriesEveryLicenceNotice()
    {
        var (entries, nuspec) = Pack();

        foreach (var required in new[] { "THIRD-PARTY-NOTICES.md", "OFL.txt", "README.md", "PackageIcon.png" })
        {
            Assert.True(entries.Contains(required), required + " is not at the root of the package. It holds: " + string.Join(", ", entries));
        }

        Assert.Contains("<license type=\"expression\">MIT AND Apache-2.0 AND MS-PL AND OFL-1.1</license>", nuspec);
    }

    [Fact]
    public void TheNoticesNameEveryMsPlTemplate()
    {
        var theme = GeneratedThemeTests.ThemeDirectory();
        var notices = File.ReadAllText(Path.Combine(theme, "..", "..", "THIRD-PARTY-NOTICES.md"));
        var unnamed = Directory.GetFiles(Path.Combine(theme, "Themes", "Base", "Controls"), "*.axaml")
            .Where(path => File.ReadAllText(path).Contains("Microsoft Public License (Ms-PL)"))
            .Select(path => "Themes/Base/Controls/" + Path.GetFileName(path))
            .Where(name => !notices.Contains("`" + name + "`"))
            .ToArray();

        Assert.True(unnamed.Length == 0, "THIRD-PARTY-NOTICES.md does not name these Ms-PL templates:\n  " + string.Join("\n  ", unnamed));
    }

    private static (HashSet<string> Entries, string Nuspec) Pack()
    {
        var project = Path.Combine(GeneratedThemeTests.ThemeDirectory(), "Fili.Theme.MudAvalonia.csproj");
        // This assembly runs from bin/<Configuration>/<TargetFramework>/, and the theme was built
        // in the same configuration.
        var configuration = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Parent!.Name;
        var output = Directory.CreateTempSubdirectory("fili-pack-");
        try
        {
            var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var argument in new[] { "pack", project, "--no-build", "-c", configuration, "-o", output.FullName, "-nologo" })
            {
                start.ArgumentList.Add(argument);
            }

            using var pack = Process.Start(start)!;
            var stdout = pack.StandardOutput.ReadToEndAsync();
            var stderr = pack.StandardError.ReadToEndAsync();
            pack.WaitForExit();
            Assert.True(pack.ExitCode == 0, "dotnet pack failed:\n" + stdout.Result + stderr.Result);

            var package = Directory.GetFiles(output.FullName, "*.nupkg").Single();
            using var zip = ZipFile.OpenRead(package);
            using var nuspec = new StreamReader(zip.Entries.Single(entry => entry.FullName.EndsWith(".nuspec")).Open());
            return (zip.Entries.Select(entry => entry.FullName).ToHashSet(), nuspec.ReadToEnd());
        }
        finally
        {
            output.Delete(recursive: true);
        }
    }
}
