# Unreleased

Work since 0.30.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

The keyboard focus ring's colours and thickness are three new keys, so an app can give it its own
accent or a heavier ring on a television instead of drawing a ring of its own. The high contrast
page now says what the ring is there: 4px of white.

## What changed

- **`FiliFocusRingBrush`, `FiliFocusRingInnerBrush` and `FiliFocusRingThickness`.** The outer
  stroke's brush (text-primary), the inner stroke's (white) and the outer stroke's width (2). A
  thicker stroke moves the ring out by as much, keeping the inner 2px and corners parallel to the
  control's, and a change while a control has focus moves its ring at once. They are set in the
  application's or a window's resources, as the ring is drawn in the window's adorner layer
  ([Published keys](../published-keys.md)).
- **The high contrast page described the ring as 2px.** Both strokes are white in high contrast, so
  it is 4px of white.

## Upgrading from 0.30.0 (unreleased)

Nothing to change: the three keys default to the ring 0.30.0 drew. An app that set
`FiliFocusRing` to `False` to draw a ring of its own, for its colour or its thickness, can set
these instead and drop its `FocusAdorner` styles; one that leaves the inner stroke out by setting
it `Transparent` checks the outer stroke's 3:1 against each surface itself.
