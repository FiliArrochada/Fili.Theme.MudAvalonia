# Unreleased

Work since 0.27.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

The package requires Avalonia 12.1.4 ([Upgrading](#upgrading-from-0270-unreleased)).

## What changed

### The interface and releases

- **Requires Avalonia 12.1.4**, where it required 12.1.3: the Avalonia dependency is
  `[12.1.4, 13.0.0)`, and the forked templates are taken at tag 12.1.4. Avalonia changed none of
  Simple's templates between the two, so the theme looks and behaves as it did; the tests and the
  gallery run on 12.1.4.

## Upgrading from 0.27.0 (unreleased)

- **An app on Avalonia 12.1.3 moves to 12.1.4**, every `Avalonia*` package together. On 12.1.3,
  NuGet's NU1605 downgrade error stops the restore.
