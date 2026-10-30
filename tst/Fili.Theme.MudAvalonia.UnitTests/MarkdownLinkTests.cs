using System.Text.RegularExpressions;
using Fili.Theme.MudAvalonia.UnitTests.Generation;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The repository's Markdown links to itself by heading: the compatibility page, which is the 1.0
/// promise, points at the README's sections, and each version page at its Upgrading section.
/// GitHub drops an anchor that names no heading without a word, landing at the top of the page, so
/// a renamed heading breaks every link to it silently. This reads every Markdown file, skipping
/// fenced code, and checks each link to a Markdown file, or to a heading on the same page, names a
/// heading GitHub gives that anchor to.
/// </summary>
public partial class MarkdownLinkTests
{
    [Fact]
    public void EveryAnchorNamesAHeading()
    {
        var root = Path.GetFullPath(Path.Combine(GeneratedThemeTests.ThemeDirectory(), "..", ".."));
        var anchors = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var broken = new List<string>();

        foreach (var file in MarkdownFiles(root))
        {
            foreach (Match link in Link().Matches(Prose(File.ReadAllText(file))))
            {
                var (target, anchor) = (link.Groups[1].Value, link.Groups[2].Value);
                var path = target.Length == 0 ? file : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, target));
                if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!anchors.TryGetValue(path, out var known))
                {
                    known = anchors[path] = File.Exists(path) ? Anchors(File.ReadAllText(path)) : [];
                }

                if (!known.Contains(Uri.UnescapeDataString(anchor)))
                {
                    broken.Add(Path.GetRelativePath(root, file).Replace('\\', '/') + ": " + link.Value);
                }
            }
        }

        Assert.True(broken.Count == 0, "These links name a heading that does not exist:\n  " + string.Join("\n  ", broken));
    }

    [Theory]
    [InlineData("Quick start", "quick-start")]
    [InlineData("Upgrading from 0.25.0 to 0.26.0", "upgrading-from-0250-to-0260")]
    [InlineData("`FiliThemeVariants` and *the* variant", "filithemevariants-and-the-variant")]
    [InlineData("Licence and attribution", "licence-and-attribution")]
    public void SlugsFollowGitHub(string heading, string slug) => Assert.Equal(slug, Slug(heading));

    private static IEnumerable<string> MarkdownFiles(string root) =>
        Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar)
                .Any(part => part is "bin" or "obj" or "node_modules" || part.StartsWith('.')));

    /// <summary>The page without its fenced code blocks, whose text is not a link.</summary>
    private static string Prose(string markdown) => Fence().Replace(markdown.Replace("\r\n", "\n"), "");

    /// <summary>GitHub's anchors for a page: one per heading, a repeat getting -1, -2, ...</summary>
    private static HashSet<string> Anchors(string markdown)
    {
        var anchors = new HashSet<string>();
        foreach (Match heading in Heading().Matches(Prose(markdown)))
        {
            var slug = Slug(heading.Groups[1].Value);
            var unique = slug;
            for (var n = 1; !anchors.Add(unique); n++)
            {
                unique = slug + "-" + n;
            }
        }

        foreach (Match id in HtmlId().Matches(markdown))
        {
            anchors.Add(id.Groups[1].Value);
        }

        return anchors;
    }

    /// <summary>
    /// GitHub's slug: a link's text for the link, lower case, every character but a letter, digit,
    /// underscore, hyphen or space dropped, and each space a hyphen.
    /// </summary>
    private static string Slug(string heading)
    {
        var text = InlineLink().Replace(heading.Trim(), "$1").ToLowerInvariant();
        return new string(text.Where(c => char.IsLetterOrDigit(c) || c is '_' or '-' or ' ').ToArray()).Replace(' ', '-');
    }

    [GeneratedRegex(@"\]\((?!https?:|mailto:)([^)\s#]*)#([^)\s]+)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"^(?:```|~~~).*?^(?:```|~~~)[ \t]*$", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex Fence();

    [GeneratedRegex(@"^#{1,6}[ \t]+(.+?)[ \t#]*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"<a\s+(?:id|name)=""([^""]+)""")]
    private static partial Regex HtmlId();

    [GeneratedRegex(@"!?\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex InlineLink();
}
