# Releasing


`.github/workflows/release.yml` publishes the package to nuget.org, and only when a version tag is
pushed:

```powershell
# 1. bump <Version> in Directory.Build.props, commit, push
# 2. tag that exact version and push the tag
git tag v0.1.0
git push origin v0.1.0
```

The workflow runs the unit and pixel suites on Windows first, because a tag can point at a commit
`build.yml` never saw pass. It then checks that the tag matches `<Version>` exactly and fails if it
does not, packs the `.nupkg` and its `.snupkg` symbols, pushes both, and creates a GitHub release
with the packages attached and generated notes. A version with a suffix (`0.2.0-preview.1`) is
marked as a prerelease.

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
