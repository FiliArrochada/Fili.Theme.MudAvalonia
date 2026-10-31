"""Writes a version's release notes from its page in docs/versions/, for the GitHub release.

The page is the notes: what changed and, where existing markup changes look, how to upgrade. A
release body cannot follow a relative link, so each one is rewritten to the file at the tag, and
the page's own heading is dropped, as the release carries the version in its title, and wrapped
lines are joined, as a release body breaks the line at every newline. A missing
page, or a link to a file that does not exist, fails: a release is never created without its notes.

Usage, from the repository root:

    python3 tools/release-notes.py 0.27.0 v0.27.0 release-notes.md
"""
import pathlib
import posixpath
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
REPOSITORY = "https://github.com/FiliArrochada/Fili.Theme.MudAvalonia"
LINK = re.compile(r"(!?)\[([^\]]*)\]\((?!https?:|#|mailto:)([^)\s]+)\)")


BLOCK = re.compile(r"(#|\||>|[-*] |\d+\. )")


def unwrap(lines: list[str]) -> list[str]:
    """Joins a paragraph's or list item's wrapped lines: a release body breaks the line at each newline."""
    out: list[str] = []
    fenced = False
    for line in lines:
        if line.lstrip().startswith("```"):
            fenced = not fenced
            out.append(line)
        elif fenced or not line.strip() or BLOCK.match(line.lstrip()) or not out:
            out.append(line)
        elif out[-1].strip() and not re.match(r"\s*(#|\||```)", out[-1]):
            out[-1] += " " + line.strip()
        else:
            out.append(line)
    return out


def main(version: str, ref: str, destination: str) -> None:
    page = ROOT / "docs" / "versions" / f"{version}.md"
    if not page.is_file():
        sys.exit(f"docs/versions/{version}.md does not exist: rename unreleased.md to it at the bump.")

    lines = page.read_text(encoding="utf-8").splitlines()
    if not lines or lines[0].strip() != f"# {version}":
        sys.exit(f"docs/versions/{version}.md does not open with '# {version}'.")

    missing = []

    def absolute(match: re.Match) -> str:
        image, text, target = match.groups()
        path, _, anchor = target.partition("#")
        file = posixpath.normpath(posixpath.join("docs/versions", path))
        if not (ROOT / file).exists():
            missing.append(target)
        kind = "raw" if image else "blob"
        url = f"{REPOSITORY}/{kind}/{ref}/{file}" + (f"#{anchor}" if anchor else "")
        return f"{image}[{text}]({url})"

    body = LINK.sub(absolute, "\n".join(unwrap(lines[1:])).strip() + "\n")
    if missing:
        sys.exit(f"docs/versions/{version}.md links to files that do not exist: {', '.join(missing)}")

    out = pathlib.Path(destination)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(body, encoding="utf-8")


if __name__ == "__main__":
    if len(sys.argv) != 4:
        sys.exit(__doc__)
    main(*sys.argv[1:])
