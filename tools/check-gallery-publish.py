"""Checks a published browser gallery for the compiled XAML that trimming can strip.

XAML loaded by its path at runtime is found through a type the XAML compiler generates,
`CompiledAvaloniaXaml.!XamlLoader`, by reflection, so a trimmed build drops it unless a
DynamicDependency keeps it. The desktop tests never trim, so only a published build shows the loss:
every docs example, or the theme's DataGrid styles, failing with "No precompiled XAML found".

Usage, from the repository root after `dotnet publish src/Fili.Theme.MudAvalonia.Gallery.Browser
--configuration Release --output publish`:

    python3 tools/check-gallery-publish.py publish
"""
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
EXAMPLES = ROOT / "src/Fili.Theme.MudAvalonia.Gallery/Docs/Examples"


def assembly(framework: pathlib.Path, name: str) -> bytes:
    """The published assembly `name`: a webcil .wasm, or a .dll where webcil is off."""
    found = [p for p in framework.iterdir()
             if p.suffix in (".wasm", ".dll") and re.fullmatch(re.escape(name) + r"(\.[a-z0-9]+)?", p.stem)]
    if len(found) != 1:
        sys.exit(f"expected one published {name}, found {[p.name for p in found]}")
    return found[0].read_bytes()


def strings(data: bytes) -> str:
    """The assembly's UTF-16 string literals, read at both byte alignments."""
    return data.decode("utf-16-le", "ignore") + data[1:].decode("utf-16-le", "ignore")


def main() -> int:
    framework = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else "publish") / "wwwroot/_framework"
    failures = []

    gallery = assembly(framework, "Fili.Theme.MudAvalonia.Gallery")
    expected = {p.relative_to(EXAMPLES).with_suffix("").as_posix() for p in EXAMPLES.rglob("*.axaml")}
    kept = set(re.findall(r"avares://Fili\.Theme\.MudAvalonia\.Gallery/Docs/Examples/([A-Za-z0-9/]+)\.axaml", strings(gallery)))
    if b"!XamlLoader" not in gallery:
        failures.append("the gallery's compiled XAML loader was trimmed: ExampleCard's DynamicDependency is missing")
    if missing := sorted(expected - kept):
        failures.append(f"{len(missing)} of {len(expected)} docs examples cannot be loaded, e.g. {missing[:5]}")

    theme = assembly(framework, "Fili.Theme.MudAvalonia")
    if b"!XamlLoader" not in theme or "Themes/Optional/DataGrid/DataGridStyles.axaml" not in strings(theme):
        failures.append("the theme's compiled XAML loader or its DataGrid styles were trimmed: OptionalStyleInclude's DynamicDependency is missing")

    for failure in failures:
        print(f"::error::{failure}")
    if not failures:
        print(f"ok: {len(expected)} docs examples and the theme's DataGrid styles survive trimming")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
