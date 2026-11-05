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

    /// <summary>
    /// Avalonia is the one dependency, from the version the base is forked from
    /// (<c>AvaloniaVersion</c> in Directory.Packages.props) up to but not including 13, which would
    /// be this package's next major version; the DataGrid is not passed on.
    /// </summary>
    [Fact]
    public void ThePackageDependsOnAvalonia12Only()
    {
        var (_, nuspec) = Pack();
        var dependencies = System.Text.RegularExpressions.Regex.Matches(nuspec, "<dependency id=\"([^\"]+)\" version=\"([^\"]+)\"")
            .Select(match => match.Groups[1].Value + " " + match.Groups[2].Value)
            .ToArray();

        var forked = System.Xml.Linq.XDocument.Load(Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", "..", "Directory.Packages.props"))
            .Descendants("AvaloniaVersion").Single().Value;
        Assert.Equal([$"Avalonia [{forked}, 13.0.0)"], dependencies);
    }

    /// <summary>
    /// The assembly's XML documentation is packed beside it, so the public type's documentation -
    /// where to set the variant, how to name it in XAML - shows in an app's editor.
    /// </summary>
    [Fact]
    public void ThePackageCarriesTheApiDocumentation()
    {
        var (entries, _) = Pack();

        var documentation = Assert.Single(entries, entry => entry.StartsWith("lib/") && entry.EndsWith("/Fili.Theme.MudAvalonia.xml"));
        Assert.Contains(entries, entry => entry == documentation.Replace(".xml", ".dll"));
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

    /// <summary>
    /// nuget.org's release notes are the version's page: its summary and a link to the rest. A
    /// build between releases says it is one and links unreleased.md rather than claim a version.
    /// </summary>
    [Fact]
    public void ThePackageCarriesItsVersionsReleaseNotes()
    {
        var (_, nuspec) = Pack();
        var version = System.Text.RegularExpressions.Regex.Match(nuspec, "<version>([^<]+)</version>").Groups[1].Value;
        var notes = System.Text.RegularExpressions.Regex.Match(nuspec, "<releaseNotes>([^<]+)</releaseNotes>").Groups[1].Value;
        var page = Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", "..", "docs", "versions", version + ".md");

        Assert.Contains(File.Exists(page) ? $"/docs/versions/{version}.md" : "/docs/versions/unreleased.md", notes);
        Assert.DoesNotContain("](", notes);
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
