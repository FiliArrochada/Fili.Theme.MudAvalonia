# Security policy

`Fili.MudAvalonia.Theme` is a theme: XAML styles, resources and a handful of internal helpers,
with `Avalonia` as its one dependency. It opens no network connections and reads no files beyond
its own embedded resources, but it runs inside every app that uses it, so a problem in it is worth
reporting.

## Supported versions

Fixes go into the newest 1.x release, as a new patch or minor version. Within a major version the
package keeps its published interface ([Compatibility](docs/compatibility.md)), so moving to the
newest 1.x needs no change to an app: update before reporting. A version before 1.0 gets no fix of
its own, and a prerelease (`1.0.0-rc.1`) is followed by the next one or by the release rather than
fixed; [Versions](docs/versions/README.md) says what each one changed.

## Reporting a vulnerability

**Do not open a public issue.** Report it privately through GitHub: the repository's *Security*
tab, *Report a vulnerability*. Say what an attacker can do, which version you saw it in, and the
smallest app or XAML that shows it.

You will get a reply within two weeks. A confirmed problem is fixed in a new release, as a version
on nuget.org can never be replaced, and the affected versions are unlisted there. The version that
fixes it says so in its release notes once it is out. Credit is given unless you ask for none.

A problem in Avalonia itself belongs to the [Avalonia project](https://github.com/AvaloniaUI/Avalonia/security).
