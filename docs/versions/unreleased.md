# Unreleased

Work since 0.26.0, kept here as it lands; at the bump this file is renamed to its version.

[All versions](README.md)

The package carries the notices of the third-party material in it, in `THIRD-PARTY-NOTICES.md` at
its root: the templates forked from Avalonia's Simple theme (MIT), eight of which keep the
Silverlight Toolkit's Ms-PL header, the Material Icons glyphs (Apache-2.0), the values transcribed
from MudBlazor (MIT) and Roboto (OFL). Earlier versions packed Roboto's licence alone. An app that
redistributes the theme's assembly passes the file on with it. The package's licence expression
is `MIT AND Apache-2.0 AND MS-PL AND OFL-1.1`, where it was `MIT`; nothing in the package changed
licence, the expression now names what it always bundled. The README's licence section says the
same; it had said MIT covered all of the XAML.

The [compatibility page](../compatibility.md)'s links to the README's quick start and class list,
and the base page's to the gallery, lead to their sections again; the README's rewrite in 0.26.0
had renamed them.

Docs that promised more than the theme does now say what it does:

- `FiliThemeVariants.HighContrast`'s documentation no longer says a variant can be set on any
  control. Set it on the `Application`, as the README and the docs site say: a window's or a
  `ThemeVariantScope`'s variant changes only half of what is in it.
- A colour is overridden per control by its `*Brush` key; a `*Color` key in a control's `Resources`
  does nothing, and works in the application's only. The published keys page and the Theming page
  had said any key worked per control.
- `FiliReducedMotion` stops the ripple, fades, slides and pulses, but not yet a drawer's slide, a
  switch thumb, floating labels, chevrons, an expansion panel's margin or a circular progress's
  sweep. The README, the published keys page and the Reduced motion page had said it stopped
  everything.
- The README's class table is now checked against the theme's classes, as it said it was.

The interface 1.0 will promise is written down in [Compatibility](../compatibility.md): what a
resource key promises (its name, its type, and that a `*Color` key is set in the application's
resources while a `*Brush` or glyph key may be set on a control), that the variant is set on the
`Application`, that the `"HighContrast"` variant string is stable, that the pseudo-classes the
theme sets and every `Fili*` key not on the published list are implementation, and that a minor
version may correct accessibility and high contrast and widen what `FiliReducedMotion` covers.
The Toggle group page no longer suggests styling the check mark through its template part.

The Avalonia dependency is now `[12.1.3, 13.0.0)`: an app moving to Avalonia 13 needs the
theme's next major version. The DataGrid is themed from 12.1.0 up to but not including 13.0.0, and
a 13 grid is left alone with a warning in Avalonia's log, as an older one already was.

High contrast reads where it did not. Light and dark are unchanged.

- The `dark` colour is a grey, `#BDBDBD`, 11:1 on black, with black text on it. It was black, so
  `dark` text, links, outlines and avatars were invisible on the black ground. Its hover tint,
  darken and lighten shades follow it.
- A text or outlined alert's text is its colour's lighten shade, 5.6:1 or more on its tint; the
  darken shade was 3.4 to 4.4:1.
- A default chip hovered, focused or pressed has an opaque grey ground under its white text, 6.8:1;
  it was the light disabled grey, 1.5:1.
- A slider's rail is its colour at half, 3:1 or more on black; at 30% it was 1.9:1.

Three new kinds of key carry these, each published and declared in every variant:
`FiliPrimaryAlertTextColor` to `FiliDarkAlertTextColor` (and their brushes), the darken shade in
light and dark; `FiliChipHoverColor` (and its brush), action-disabled in light and dark; and
`FiliSliderRailOpacity`, 0.30 in light and dark. An app that overrides a colour in high contrast
sets its alert text too.

The release notes on GitHub are this page from now on, not a list of commits: a tag whose version
has no page here fails before anything is published. The pack validates the package against the
last published one and fails on a public API dropped, which every build's tests exercise.
