# Releasing


`.github/workflows/release.yml` publishes the package to nuget.org, and only when a version tag is
pushed:

```powershell
# 1. bump <Version> in Directory.Build.props, rename docs/versions/unreleased.md to <Version>.md,
#    commit, push
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
missing page or a link that leads nowhere fails there first.

The pack runs **package validation** against `PackageValidationBaselineVersion` in the theme's
csproj, the last version on nuget.org, and fails on a public member that is gone or changed;
`PackageContentTests` packs on every build, so a break shows there before any tag. Once
a release is on nuget.org, raise the baseline to it in the next commit; until then it stays on the
one before.

Publishing uses **nuget.org trusted publishing**: the workflow swaps GitHub's OIDC token for an API
key that expires within the hour, so no key is stored anywhere. Two one-time steps:

1. On nuget.org, under *Trusted Publishing*, add a policy for owner `FiliArrochada`, repository
   `Fili.Theme.MudAvalonia`, workflow `release.yml`.
2. In this repository's *Settings → Secrets and variables → Actions → Variables*, set
   `NUGET_USER` to the nuget.org username that owns that policy. It is a name, not a secret; the
   workflow fails early with a clear message if it is missing.

Packages built on a runner set `ContinuousIntegrationBuild`, so the PDBs carry `/_/` paths
instead of the runner's directory layout, and Source Link points each file at the tagged commit.

A version cannot be replaced on nuget.org, only unlisted. Re-running a release that failed after
the push is safe (`--skip-duplicate`), but fixing a bad package means a new version.

## Supported Avalonia versions

The package declares **`Avalonia` `[12.1.3, 13.0.0)`**: 12.1.3 is the tag `Themes/Base/` is
forked from, and the two move together; 13 would be this package's next major version
([Compatibility](compatibility.md)), so the range stops below it. Measured on 2026-10-13 by building a clean export of the
repository against each version and running all three suites (unit, pixel, no-DataGrid):

| Avalonia | Result |
|---|---|
| 12.0.0 - 12.0.5 | Does not compile: the forked templates use 12.1 API (`ScrollBar.PageUp`, `DatePicker.VerticalContentAlignment`) |
| 12.1.0, 12.1.1 | Does not compile: the forked date and time picker templates use a property those versions lack |
| 12.1.2 | Builds; every test passes |
| **12.1.3** | Builds; every test passes - the declared minimum |
| 12.1.4 | Builds; every test passes |

12.1.2 is therefore the measured floor, one patch under the declared one. The declared one stays,
because a fork taken from 12.1.3 and promised on 12.1.2 would be a promise no build checks.

`Avalonia.Controls.DataGrid` is optional and gated at runtime by `MinimumVersion` and
`BelowVersion` (13.0.0, the next major one) in `FiliTheme.axaml`; `MinimumVersion` is equal to
its version in `Directory.Packages.props` (`OptionalBoundaryTests` pins the two together, and
`BelowVersion` to the next major). Both are **12.1.0**, the first 12.1 release: 12.1.0, 12.1.1 and 12.1.2
each build and pass every test, and every 12.1 release needs Avalonia 12.1.0 or later. 12.0.x
needs Avalonia 12.0, which the theme cannot compile against. An app with a DataGrid under the
gate keeps its grid on the package's own theme, with a warning in Avalonia's log. CI builds and
tests on 12.1.0, the floor; the newest is checked by the measurement above.

To re-measure after an upgrade, export `HEAD` (`git archive`), set every `Avalonia*` version in
the copy's `Directory.Packages.props`, and build and test it there; never in the working tree.
