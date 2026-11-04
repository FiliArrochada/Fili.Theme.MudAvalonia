# Unreleased

Work since 1.0.0-rc.1, kept here as it lands; at the bump this file is renamed to its version, and
this line goes.

[All versions](README.md)

## What changed

- **Avalonia 12.1.5 is the minimum.** The forked templates are Avalonia Simple's at 12.1.5, and
  the package depends on `Avalonia` 12.1.5 or later, below 13. Of the fork, only CommandBar
  changed. `Avalonia.Controls.DataGrid` is still themed from 12.1.0.
- **A command bar's overflow scrolls.** A long overflow menu scrolls up and down, as a menu does,
  rather than running off the screen: Simple's does from 12.1.5, and the theme's own did not.
- **A date picker opened with the pointer shows no keyboard ring.** From 12.1.5 a picker's
  popover takes the focus when it opens, so its arrow keys work; the ring round the day they are
  on is drawn once a key is pressed there or in the picker that opened it, not on a click, which
  had drawn it over today's ring.

## Upgrading from 1.0.0-rc.1

Raise every `Avalonia*` package to 12.1.5 or later, together: an app pinned to an older 12.1
patch fails restore with NU1605, a package downgrade.
