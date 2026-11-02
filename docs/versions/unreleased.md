# Unreleased

Work since 0.29.0, kept here as it lands; at the bump this file is renamed to its version, and this
line goes.

[All versions](README.md)

The keyboard focus ring keeps 3:1 on a coloured app bar as well as on the page, and the docs say
where `FiliButtonTextTransform` and the ripple keys can be set: the application, a container or one
control.

## What changed

- **The keyboard focus ring keeps its contrast on coloured surfaces.** A text-colour stroke alone
  could fall below 3:1 against a coloured app bar; a white stroke now fills the gap inside it, so
  at least one remains visible. The test checks the ring colours against page, surface and app-bar grounds
  in every variant.
- **The docs give the scope of `FiliButtonTextTransform`, `FiliRippleOpacity` and
  `FiliRippleOpacitySecondary`**: the application, a container's subtree or one control, as the
  tests already held them to. Tabs stay upper-case whatever the key says.

## Upgrading from 0.29.0 (unreleased)

- **The focus ring is two strokes**: text-primary where the single ring was, 2px out, and white
  filling the gap between it and the control. On a light page the white does not show, so the
  ring looks as it did in 0.29.0; on a dark page or a coloured app bar the white stroke shows.
  Nothing to change; a pixel baseline of a keyboard-focused control on a dark ground moves.
