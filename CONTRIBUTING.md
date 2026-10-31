# Contributing

Thanks for helping. This is a theme, not a component library: resource dictionaries, style
classes and control themes that give Avalonia 12's own controls MudBlazor's look. A control
Avalonia does not have belongs in a package of its own, not here.

## Build and test

You need the .NET SDK `global.json` names, and the `wasm-tools` workload for the browser gallery:

```powershell
dotnet workload install wasm-tools
dotnet build Fili.Theme.MudAvalonia.sln
dotnet test  Fili.Theme.MudAvalonia.sln
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Desktop
```

The gallery is the docs site, one page per component. [Testing](docs/testing.md) describes every
test class and what it exists to catch; the pixel tests compare frames rendered on Windows.

## The rules a change is checked against

Read [Traps](docs/traps.md) before a non-trivial change: most of what breaks a theme breaks
silently, and each trap there has a test.

- **A resource key is always a `{DynamicResource}`.** A missing or misspelt key resolves to
  nothing without an error, and a `StaticResource` freezes the light value. A new token gets an
  entry in `ResourceResolutionTests`, and in `HighContrastTests` for the third variant.
- **Transcribe, never eyeball.** A value comes from MudBlazor's source - its `Themes/Models`
  files, its `.scss` and the component's `.razor.cs` defaults - and a comment names the file.
- **The docs are part of the change.** A new or changed class, token or glyph key updates its
  component's page and its parity row in the same change, and a new key goes into
  [Published resource keys](docs/published-keys.md). [Compatibility](docs/compatibility.md) says
  what may change before 2.0; a key is never renamed or removed, only aliased.
- **Generated regions are generated.** `Button.axaml` and the marked regions elsewhere come from
  `ThemeColourGenerator`: change the generator and regenerate.
- **The forked base is not restyled.** `Themes/Base/` is Avalonia's Simple theme, forked
  verbatim so an upgrade is a diff ([FORK.md](src/Fili.Theme.MudAvalonia/Themes/Base/FORK.md));
  change a look in `Accents.axaml` or a hand-written theme instead.
- **A changed look needs its pixel baseline reviewed**, from the diff image the failing test
  writes, before it is accepted.
- **No other app is named** in code, comments, docs or commit messages: a bug an app found is
  "found by an adopting app".

## Pull requests

One topic per pull request, with its tests and docs. Note what changed in
[`docs/versions/unreleased.md`](docs/versions/unreleased.md), and in its Upgrading section when
existing markup looks different. Releases are tagged by the maintainer ([Releasing](docs/releasing.md)).

A security problem is reported privately, as [the security policy](SECURITY.md) says, never in an
issue or a pull request.
