# Unreleased

Work since 0.33.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

## What changed

- **Switching variant is fast again.** Since 0.32.0 every variant has brushes of its own, so a
  window or `ThemeVariantScope` paints in its own variant; but a switch of the application's
  variant handed every control a new brush in every brush property, and took up to three times as
  long. A lookup for the application's own variant now finds one brush per key that follows it,
  so a switch recolours the brushes a control has, as before 0.32.0, while a window or scope with
  another variant still paints in that one. On the benchmark screen a switch takes 40% less
  time than in 0.33.0, and allocates a thirteenth of what it did.
- **The compatibility page states its promise in the present tense:** the package follows
  semantic versioning over its published interface.

## Upgrading from 0.33.0

Nothing has to change. A brush looked up in code for the application's variant is now the same
object before and after a switch, recoloured, where 0.32.0 and 0.33.0 returned a different one
per variant.
