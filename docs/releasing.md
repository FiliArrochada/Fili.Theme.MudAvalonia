# Releasing


`.github/workflows/release.yml` publishes the package to nuget.org, and only when a version tag is
pushed:

```powershell
# 1. bump <Version> in Directory.Build.props, rename docs/versions/unreleased.md to <Version>.md,
#    run `dotnet restore Fili.Theme.MudAvalonia.sln --force-evaluate` (the lock files record the
#    version of each project reference, and CI's locked restore fails on a stale one), commit, push
# 2. tag that exact version and push the tag
git tag v0.1.0
git push origin v0.1.0
```

The workflow runs the unit and pixel suites on Windows first, because a tag can point at a commit
`build.yml` never saw pass. It then checks that the tag matches `<Version>` exactly and fails if it
does not, writes the release notes from `docs/versions/<Version>.md` and stops if that page is
missing, packs the `.nupkg` and its `.snupkg` symbols, pushes both, and creates a GitHub release
with the packages attached and the page as its notes. A version with a suffix
(`0.2.0-preview.1`) is marked as a prerelease.

The notes are the page with its heading dropped, its wrapped lines joined and its relative links
pointed at the tag (`tools/release-notes.cs`); `build.yml` writes them on every change, so a
missing page or a link that leads nowhere fails there first. A page opens with two or three
sentences of summary, then a `## What changed` list with one bold headline per change, grouped
under `###` subheadings when a version covers several areas, then its Upgrading section; a page
without the list fails, as paragraphs read as a wall of text on a release.

Work since the last release is written into `docs/versions/unreleased.md`, and its Upgrading
section, as it lands. At the bump that page is renamed to `<Version>.md`: its "Work since" line
goes, its heading and its Upgrading heading lose the unreleased marker, it gets its row in
`docs/versions/README.md`, and the index's bold moves from the previous release to the new one.
A fresh `unreleased.md` follows, with its "Work since" line, the `[All versions]` link and the
words **Nothing yet.**: the docs site deploys from `master`, and while that sentence is missing
its version label reads "v<Version> + unreleased", so the site never names a released version for
work no package has yet.

The same page is the package's **release notes on nuget.org**: the pack takes its summary and a
link to the page at the commit (`PackReleaseNotesFromVersionPage` in the theme's csproj). A pack
between releases, which has no page of its own, says so and links `unreleased.md`.

The pack runs **package validation** against `PackageValidationBaselineVersion` in the theme's
csproj, the last version on nuget.org, and fails on a public member that is gone or changed;
`PackageContentTests` packs on every build, so a break shows there before any tag. Once
a release is on nuget.org, raise the baseline to it in the next commit; until then it stays on the
one before.

Package validation sees only the C# API; the theme's real interface is its keys and classes, and
those are frozen in `tst/Fili.Theme.MudAvalonia.UnitTests/published-baseline.txt`
(`PublishedBaselineTests`): every published key with the type it resolves to, and every class. A
change that adds a key or a class appends it there with `FILI_REGENERATE=1` (the test's summary has
the command), so the release ships with it already frozen. The file only grows; nothing at release
edits it, and only a major version removes an entry, by hand.

Publishing uses **nuget.org trusted publishing**: the workflow swaps GitHub's OIDC token for an API
key that expires within the hour, so no key is stored anywhere. Three one-time steps:

1. On nuget.org, under *Trusted Publishing*, add a policy for owner `FiliArrochada`, repository
   `Fili.Theme.MudAvalonia`, workflow `release.yml`.
2. In this repository's *Settings → Secrets and variables → Actions → Variables*, set
   `NUGET_USER` to the nuget.org username that owns that policy. It is a name, not a secret; the
   workflow fails early with a clear message if it is missing.
3. In *Settings → Environments*, create `nuget` with the maintainer as a required reviewer and a
   deployment tag rule of `v*`. The publish job runs in that environment, so after the tests pass
   it waits for an approval: a pushed tag alone never reaches nuget.org. Until the environment is
   set up, GitHub creates it on the first run with no rules and the job runs unattended. The
   nuget.org policy can then name the environment too, so no other job can publish.

Every action in both workflows is **pinned to a commit**, with its release in a comment
(`actions/checkout@3d3c42e… # v7.0.1`): a tag can be moved to other code, a commit cannot. To
update one, take the commit of the release you want (`git ls-remote --tags` on the action's
repository) and change the hash and the comment together.

Packages built on a runner set `ContinuousIntegrationBuild`, so the PDBs carry `/_/` paths
instead of the runner's directory layout, and Source Link points each file at the tagged commit.
The XAML is the exception: Avalonia's XAML compiler writes its sequence points after the C#
compiler, with the real path and no path map (12.1.4's `CompileAvaloniaXamlTask` takes none), so
its documents keep the runner's path. `MapXamlSourcesInSourceLink` adds that unmapped root to the
same Source Link map, so a debugger stepping into a theme's XAML still fetches the file at the
commit.

Restores on a runner are **locked** (`Directory.Build.props`): each project restores exactly what
its `packages.lock.json` records, hashes included, or fails. Changing a package version means
restoring locally, which rewrites the lock files, and committing them with the change. The
browser gallery has none, as the WebAssembly SDK adds packages at the installed SDK's patch
version. `global.json` keeps `rollForward: latestFeature`: CI installs the SDK it names, and a
contributor on a later feature band of 10 still builds; the `wasm-tools` workload is installed
unversioned, so it is the set of whatever SDK is installed, never one for another band.

The build treats **warnings as errors**, the trimmer's included. The one exception is IL2104 in
the browser gallery, the trimmer's summary for `Avalonia.Controls.DataGrid`'s own assembly, which
this repository cannot change.

A version cannot be replaced on nuget.org, only unlisted. Re-running a release that failed after
the push is safe (`--skip-duplicate`), but fixing a bad package means a new version.

## Supported Avalonia versions

The package declares **`Avalonia` from `AvaloniaVersion` up to but not including 13.0.0**
(`Directory.Packages.props`): `AvaloniaVersion` is the tag `Themes/Base/` is forked from, and
the two move together; 13 would be this package's next major version
([Compatibility](compatibility.md)), so the range stops below it. Measured on 2026-10-13 by building a clean export of the
repository against each version and running all three suites (unit, pixel, no-DataGrid):

| Avalonia | Result |
|---|---|
| 12.0.0 - 12.0.5 | Does not compile: the forked templates use 12.1 API (`ScrollBar.PageUp`, `DatePicker.VerticalContentAlignment`) |
| 12.1.0, 12.1.1 | Does not compile: the forked date and time picker templates use a property those versions lack |
| 12.1.2 | Builds; every test passes |
| 12.1.3 | Builds; every test passes |
| **12.1.4** | Builds; every test passes - the declared minimum |

12.1.2 is therefore the measured floor, two patches under the declared one. The declared one stays,
because a fork promised on an older patch than its tag would be a promise no build checks.

`Avalonia.Controls.DataGrid` is optional and gated at runtime by `MinimumVersion` and
`BelowVersion` (13.0.0, the next major one) in `FiliTheme.axaml`; `MinimumVersion` is equal to
its version in `Directory.Packages.props` (`OptionalBoundaryTests` pins the two together, and
`BelowVersion` to the next major). Both are **12.1.0**, the first 12.1 release: 12.1.0, 12.1.1 and 12.1.2
each build and pass every test, and every 12.1 release needs Avalonia 12.1.0 or later. 12.0.x
needs Avalonia 12.0, which the theme cannot compile against. An app with a DataGrid outside the
gate gets a warning in Avalonia's log and no grid template from this theme: Avalonia has no default
theme, so the grid shows nothing unless the app includes a DataGrid theme itself
(`OptionalStyleInclude`). CI builds and
tests on 12.1.0, the floor; the newest is checked by the measurement above.

To re-measure after an upgrade, export `HEAD` (`git archive`), set every `Avalonia*` version in
the copy's `Directory.Packages.props`, and build and test it there; never in the working tree.
