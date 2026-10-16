using System.Text.RegularExpressions;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The README is also the package's page on nuget.org. Its relative links are rewritten to the
/// commit at pack time (the <c>PackReadmeWithAbsoluteLinks</c> target), and the pack fails on one
/// that leads nowhere; this is the same check on every build, so a renamed doc fails here rather
/// than at release.
/// </summary>
public class PackageReadmeTests
{
    private static readonly Regex RelativeLink = new(@"\]\((?!https?:|#|mailto:)([^)\s]+)\)", RegexOptions.Compiled);

    [Fact]
    public void EveryRelativeLinkNamesAFile()
    {
        var root = Path.GetFullPath(Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", ".."));
        var missing = RelativeLink.Matches(File.ReadAllText(Path.Combine(root, "README.md")))
            .Select(match => match.Groups[1].Value)
            .Where(link =>
            {
                var path = Path.Combine(root, link.Split('#')[0]);
                return !File.Exists(path) && !Directory.Exists(path);
            })
            .ToArray();

        Assert.True(missing.Length == 0, "The README links to files that do not exist:\n  " + string.Join("\n  ", missing));
    }
}
