# Unreleased

Work since 0.31.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

## What changed

- **The glyphs in a Panel in an icon button take the button's icon size.** Two glyphs swapped in a
  Panel, the toggled-icon idiom included, were left at 24px on a `small` or `large` icon button;
  they now take its 20, 36 or 32px, as a glyph directly in the button does. A glyph in a flyout the
  button opens keeps its own size.
- **How a control opts out of the focus ring is documented.** A `FocusAdorner` set to `{x:Null}`,
  in a style or on the control, leaves that control without the ring; one left unset takes it, a
  control the app templates itself included ([Published keys](../published-keys.md)).

## Upgrading from 0.31.0 (unreleased)

Nothing has to change. One thing may now be removed:

- **A `Width`/`Height` or a size class set on a glyph in a Panel in an icon button**, to size it as
  the button's icon, can go: the theme sizes it now. One set to a different size still wins.
