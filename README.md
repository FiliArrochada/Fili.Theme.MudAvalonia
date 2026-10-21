# Fili - MudAvalonia Theme

[![NuGet](https://img.shields.io/nuget/v/Fili.MudAvalonia.Theme?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Fili.MudAvalonia.Theme)
[![Downloads](https://img.shields.io/nuget/dt/Fili.MudAvalonia.Theme?logo=nuget&label=downloads)](https://www.nuget.org/packages/Fili.MudAvalonia.Theme)
[![Build](https://github.com/FiliArrochada/Fili.Theme.MudAvalonia/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/FiliArrochada/Fili.Theme.MudAvalonia/actions/workflows/build.yml)
[![Gallery](https://github.com/FiliArrochada/Fili.Theme.MudAvalonia/actions/workflows/pages.yml/badge.svg?branch=master)](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/)
[![Avalonia](https://img.shields.io/badge/Avalonia-12.1-8B44AC)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

A design-token theme for Avalonia 12: palette, elevation, typography and geometry, in light, dark
and high contrast, with a gallery app to look at it in.

**[See the gallery live](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/)** — the same app,
compiled to WebAssembly and served by GitHub Pages.

**Where to read what**, in the order most people need it:

| If you want to | Read |
|---|---|
| install it and see the classes | this page |
| write a MudBlazor screen in AXAML | [Razor and AXAML](docs/razor-vs-axaml.md): the same UI written both ways |
| find the class or key for one parameter | [MudBlazor parity](docs/mudblazor-parity.md): every parameter, with ✅, ≈ or ❌ |
| know how a control behaves, and why | [Controls](docs/controls.md) |
| know what is not there | [Known gaps](docs/known-gaps.md) |
| upgrade | [Versions](docs/versions.md): what changed, and what to change; [Compatibility](docs/compatibility.md): what a version promises not to change |
| work on the theme itself | [Traps](docs/traps.md), [The base](docs/the-base.md), [Testing](docs/testing.md), [Releasing](docs/releasing.md) |

The token values are MudBlazor's defaults, transcribed from its source rather than eyeballed. The
aim is the *look* — this is not a component library: no new control types, no services, and one
public type. That type is `FiliThemeVariants`, whose only member is the high-contrast
`ThemeVariant`; it has to be public, because XAML can reach a custom variant only through
`x:Static`. A few `internal` converters exist where XAML cannot express a MudBlazor rule; see
[The converters](docs/controls.md#the-converters).

```
src/Fili.Theme.MudAvalonia                  the theme (the NuGet package, id Fili.MudAvalonia.Theme)
src/Fili.Theme.MudAvalonia.Gallery          gallery UI, shared across heads
src/Fili.Theme.MudAvalonia.Gallery.Desktop  desktop head
src/Fili.Theme.MudAvalonia.Gallery.Browser  browser head (WebAssembly), deployed to GitHub Pages
tst/Fili.Theme.MudAvalonia.UnitTests        headless resource-resolution tests
tst/Fili.Theme.MudAvalonia.PixelTests       gallery frames diffed against committed PNGs
tst/Fili.Theme.MudAvalonia.NoDataGridTests  the theme in an app without the DataGrid package
```

## Install

The NuGet package keeps its original id, **`Fili.MudAvalonia.Theme`**: a nuget.org id is
permanent, and this one already has downloads. Everything else - the assembly, the namespaces,
the `avares://` paths and the repository - is `Fili.Theme.MudAvalonia`.

```bash
dotnet add package Fili.MudAvalonia.Theme
```

or, in a project file:

```xml
<PackageReference Include="Fili.MudAvalonia.Theme" Version="0.12.0" />
```

Under Central Package Management the version goes in `Directory.Packages.props` instead, as
`<PackageVersion Include="Fili.MudAvalonia.Theme" Version="0.12.0" />`, and the reference stays
version-less.

| | |
|---|---|
| Package | [`Fili.MudAvalonia.Theme`](https://www.nuget.org/packages/Fili.MudAvalonia.Theme) on nuget.org, with symbols (`.snupkg`) |
| Target framework | `net10.0` |
| Depends on | `Avalonia` 12.1.3 or later, and nothing else - no `Avalonia.Themes.*` package. `Avalonia.Controls.DataGrid` is optional: built against 12.1.0, themed when the app references it. [Which versions were tested](docs/releasing.md#supported-avalonia-versions) |
| Fonts | Roboto (Light, Regular, Medium) embedded, under the SIL Open Font License |
| Licence | MIT |

What changed in each version, and what to change when upgrading, is in
[Versions](docs/versions.md).

Then add the two includes under [Using it](#using-it).

## Using it

**Standalone: no substrate theme.** Two includes, and order matters - base first, Fili second,
because later styles win for overlapping setters:

```xml
<Application.Styles>
  <StyleInclude Source="avares://Fili.Theme.MudAvalonia/Themes/Base/FiliBaseTheme.axaml" />
  <StyleInclude Source="avares://Fili.Theme.MudAvalonia/FiliTheme.axaml" />
</Application.Styles>
```

No `FluentTheme`, no `SimpleTheme`, no `Avalonia.Themes.*` package reference at all. The base is
this package's own fork of Avalonia Simple templates - see [The base](docs/the-base.md).

Then use the tokens by key, and the type ramp by class:

```xml
<Border Classes="paper elevation4" Padding="16">
  <StackPanel Spacing="4">
    <TextBlock Classes="h6" Text="Section" />
    <TextBlock Classes="caption" Text="Supporting line" />
    <TextBlock Classes="body2 error" Text="Could not reach the store" />
  </StackPanel>
</Border>

<Border Background="{DynamicResource FiliPrimaryBrush}" />
```

A colour class on a `TextBlock` is `MudText`'s `Color`: the palette colour, so `secondary` is the
secondary colour. Grey supporting text is `text-secondary`, which MudBlazor reaches through its
`mud-text-secondary` utility class rather than a `Color`; here it is
`Foreground="{DynamicResource FiliTextSecondaryBrush}"`.

Always `DynamicResource`, never `StaticResource` — see **The one rule** below.

### Classes are variants, not opt-in

Every control this theme templates is keyed to its **type**, so it arrives themed with no markup
change at all. A class only ever names a MudBlazor **variant** — `Button.primary`,
`TextBox.filled`, `ProgressBar.large` — exactly as `Color.Primary` and `Variant.Filled` do there.
The exceptions are named for where they come from, so they cannot be taken for MudBlazor's:
`material-fixed-tabs` on a tab bar, Material's fixed tabs, and `material-elevation-overlay` on a
container, Material's dark-theme elevation overlay. MudBlazor offers neither.

This was not always true. The package used to key every `ControlTheme` to a class, including a
literal `fili` marker meaning "please theme this one", because it layered over `FluentTheme` and
overriding a default `ControlTheme` would reach into controls composed out of that type. Once the
base became a fork of Simple that this repo owns, the hazard went with it: a `TextBox` inside a
`NumericUpDown` *should* look like this theme's text field. **The `fili` marker classes are gone**
— if you have `Classes="fili"` anywhere, delete it; it does nothing.

**The class names are MudBlazor's vocabulary, and they collide on purpose.** `primary`,
`secondary`, `outlined`, `text` and `filled` are `Color.Primary`, `Color.Secondary`,
`Variant.Outlined`, `Variant.Text` and `Variant.Filled` — API familiarity is the point of this
package, so they are not namespaced to `mud-primary` or hidden behind an attached property.

**All 152 of them, which is the list to grep an app against before adopting:**

| | |
|---|---|
| Type ramp (`TextBlock`) | `h1`–`h6`, `subtitle1`, `subtitle2`, `body1`, `body2`, `button`, `caption`, `overline` |
| Surfaces (`Border`) | `paper`, `appbar`, `square`, `elevation0` to `elevation25` (all 26 of MudBlazor's levels) |
| Colour (`Color`) | `primary`, `secondary`, `tertiary`, `info`, `success`, `warning`, `error`, `dark`, `inherit` |
| Shape (`Variant`) | `text`, `filled`, `outlined`, `rounded`, `flat` |
| Size (`Size`) | `small`, `medium`, `large`, `dense` |
| Placement | `inset`, `middle`, `vertical`, `light`, `underline`, `no-underline`, `border`, `centered`, `hide-indicator` |
| Component | `icon`, `chip`, `fab`, `extended`, `alert`, `avatar`, `badge`, `card`, `skeleton`, `circle`, `rectangle`, `counter`, `circular`, `nav`, `toolbar`, `drawer-header`, `normal`, `expansion-panels` |
| Card parts | `card-header`, `card-header-avatar`, `card-header-content`, `card-header-actions`, `card-media`, `card-content`, `card-actions` |
| Badge (`Origin`, `Dot`, `Bordered`, `Overlap`) | `top-left`, `top-center`, `center-left`, `center-center`, `center-right`, `bottom-left`, `bottom-center`, `bottom-right`, `dot`, `bordered`, `overlap` |
| Behaviour | `arrow`, `dense-items`, `helper-on-focus`, `hide-close`, `hide-icon`, `hover`, `multi-expansion`, `multi-selection`, `no-animation`, `no-gutters`, `no-outline`, `required`, `shrink-label`, `value-label`, `striped`, `wave` |
| Label placement (`LabelPlacement`) | `label-start`, `label-top`, `label-bottom` |
| Icons in content | `start-icon`, `end-icon` |
| Material, not MudBlazor | `material-fixed-tabs`, `material-elevation-overlay` |
| Tab indicator colour (`SliderColor`) | `indicator-primary`, `indicator-secondary`, `indicator-tertiary`, `indicator-info`, `indicator-success`, `indicator-warning`, `indicator-error`, `indicator-dark` |
| Unchecked colour (`UncheckedColor`) | `unchecked-primary`, `unchecked-secondary`, `unchecked-tertiary`, `unchecked-info`, `unchecked-success`, `unchecked-warning`, `unchecked-error`, `unchecked-dark` |
| Switch thumb icon colour (`ThumbIconColor`) | `thumb-primary`, `thumb-secondary`, `thumb-tertiary`, `thumb-info`, `thumb-success`, `thumb-warning`, `thumb-error`, `thumb-dark` |

`ClassVocabularyTests` asserts that list is exactly what the theme uses, in both directions — an
undeclared class is a collision nobody signed off on, and a declared one no selector uses is a
lie to adopters. It walks the styles *and* the control themes, because the variant names live in
a resource dictionary rather than in the styles collection: walking only `Application.Styles`
finds barely half of them.

**A MudBlazor option that is one property of its control is that property, not a class.**
`Square` on a paper is `CornerRadius="0"`, a data grid's `Bordered` is
`GridLinesVisibility="All"`, a toolbar's `Gutters="false"` is `Padding="0"`: the Avalonia property
already says it, and a class saying it again would only be one more word to collide with. A class
is kept where it does more - reaches a template part or the children, carries a token (`rounded`
follows `FiliCornerRadius`), sets several properties, gives a value that follows another class
(`inset` moves to the top of a `vertical` divider), or is one of the Size and Variant words that
must work alike on every control - and `ClassVocabularyTests` fails a class that is one fixed
value on its own control. The whole list is in
[MudBlazor parity](docs/mudblazor-parity.md#properties-not-classes).

Note how ordinary those words are — `small`, `flat`, `middle`, `vertical`, `error`. That is the
hazard, and it is why the list is a published interface rather than an implementation detail.

The cost is real and adopters should expect it: **an app that already uses those words gets its
controls retemplated the moment it includes this theme.** One early adopter collided on
`primary` in 20 places on its first day. Nothing broke visibly — its `/template/` selectors still matched
by name — but hover and press were being driven by the app's flat fill *and* the theme's state
layer at once. The reconciliation is to let the theme own shape and interaction and keep the app's
colour, which is two setters and a deletion. Grep for `Classes="` before adopting.

**An unclassed `<Button>` is the text button**, because `MudButton.Variant` defaults to
`Variant.Text`. The classes opt *up* from flat rather than opting in to being themed.

**A button's label is uppercase**, as MudBlazor's `Typography.Button.TextTransform` makes every
MudButton, MudFab label and tab: `Content="Save"` reads SAVE, on a button, a split or drop-down
button, a toggle item, an extended FAB and a tab. A chip, a link and a checkbox keep their case,
as in MudBlazor. Only a string is transformed; content that is already a control is yours. To keep
labels as written, everywhere or in one region, set the switch MudBlazor's `TextTransform = "none"`
is:

```xml
<x:String x:Key="FiliButtonTextTransform">none</x:String>
```

**A press ripples**, as in MudBlazor: a circle grows from the pointer in the label's colour, on
buttons, chips, FABs, toggles, tabs and list, select, menu and tree rows, and in the halo of a
checkbox, radio or switch. `Ripple="false"` is `FiliRippleOpacity` and `FiliRippleOpacitySecondary`
set to `0` in a control's resources, or the application's.

The raised variants step elevation 2 → 4 → 8 across rest, hover and press, and drop to 0 when
disabled. That step is the thing Fluent cannot express at all, because `Button` has no `BoxShadow`
property — only the `Border` inside a template does. `flat` is `DropShadow="false"`: a filled
button that stays at elevation 0 in every state.

**`icon` is MudIconButton, `chip` is MudChip** - classes on a `Button`, not new controls:

```xml
<Button Classes="icon"><PathIcon Data="{StaticResource MenuIcon}" /></Button>      grey
<Button Classes="icon primary small"><PathIcon Data="…" /></Button>
<Button Classes="chip" Content="Default" />                                        grey, filled
<ToggleButton Classes="chip primary" Content="Selected" IsChecked="True" />
```

An unclassed icon button is `action-default` grey rather than text-primary, because
`.mud-icon-button` comes after `.mud-button` in MudBlazor's bundle; a colour class beats it. The
`PathIcon` is 24px (18 small, 36 large) and the package ships no icon set - the gallery's glyphs
are its own. A chip is MudChip's `Variant.Filled` by default; on a `ToggleButton`, `IsChecked` is
`MudChip.GetVariant`'s swap, so a selected filled chip draws as a text chip and a selected text
chip as a filled one. Chip selection rules (`MudChipSet`) stay with the app.

**`alert` and `skeleton` are on a `Border`.** `<Border Classes="alert warning">` is a text-variant
MudAlert: the colour at 6% behind text in the colour's *darken* shade, which is what `_alert.scss`
does and is easy to mistake for the colour itself; a `PathIcon` inside takes the colour.
`outlined`, `filled` and `dense` are MudAlert's own. `<Border Classes="skeleton" Width="200" />`
pulses on MudSkeleton's timing, and `circle` / `rectangle` are its other two types;
`no-animation` is `Animation.False` and `wave` is `Animation.Wave`. The pixel suite masks an
animated skeleton for the same reason it masks an indeterminate progress bar, and compares a still
one like everything else.

**`fab`, `avatar`, `badge` and `card` are MudFab, MudAvatar, MudBadge and MudCard.** A FAB is a
`Button`, filled, grey and 56px by default; an avatar and a card are a `Border`; a badge is a
`HeaderedContentControl` whose `Content` is what it decorates and whose `Header` is the badge.
The card's parts are classes on whatever lays it out. [Controls](docs/controls.md#fabs-avatars-badges-and-cards)
has the details.

**A `DataGrid` is MudDataGrid with nothing to opt into.** The package does not depend on
`Avalonia.Controls.DataGrid`; it is built against it, and when the app references it (12.1.0 or
later) the theme finds it and themes its controls by type, so a grid on screen is surface at
elevation 1 with MudDataGrid's header, rows and sort arrow. `dense`, `striped` and `hover` are
its `Dense`, `Striped` and `Hover`, and `GridLinesVisibility="All"` its `Bordered`; `outlined` and
the elevation classes are the paper's, and `CornerRadius="0"` its `Square`. [Controls](docs/controls.md#data-grid) has the rest.

### One rule for adopters: no blanket metric styles

The type ramp sets `LineHeight` and `LetterSpacing` on its own classes — `body2`, `h4`, `caption`
— and **not** on a blanket `TextBlock` selector. That is a scar, not a style preference.

It used to set body2's `LineHeight` on every `TextBlock`. Inside this package that is harmless,
because the gallery labels everything with a ramp class that supplies its own. In an adopting app
it is not: an app whose headings set `FontSize` and nothing else had its 28px titles rendered in
a 20px line box with its descenders sliced off — on every page, silently, until somebody looked at
a screenshot.

The rule that falls out, and the reason this section exists at all:

> A blanket style is only safe when **everything it reaches also gets the matching value.**

Inside this package that can be arranged. An adopter's own type scale never will, because it does
not know the rule exists. So unclassed text keeps the font's natural line height, which cannot
clip, and `UnclassedTextKeepsItsNaturalLineHeight` keeps it that way.

### Every control

[Controls](docs/controls.md) is the usage guide, one section per control: buttons, split and
drop-down buttons, text fields, selection controls, slider and tabs, progress, dividers, panels
and links, FABs, avatars, badges and cards, the data grid, the drawer, snackbars, right to left, high contrast,
keyboard focus, the converters,
lists and selects, scrolling and menus, and overlays.

[The base](docs/the-base.md) explains the forked Avalonia Simple templates under every control
that has no hand-written theme, and why the package forks rather than layering.

## The one rule

**A missing or misspelt resource key is silent in Avalonia.** The lookup resolves to nothing and
whatever was there before simply stays, so a typo becomes a colour that quietly never changed
rather than an error. Likewise a `StaticResource` where a `DynamicResource` belonged: it freezes
the light value and that control stops following the theme at runtime, with no warning.

`tst/Fili.Theme.MudAvalonia.UnitTests/ResourceResolutionTests.cs` is what turns both into failures.
Every token is asserted to resolve under **both** theme variants. Add a token, add it there.

## Tokens

| Group | File | Notes |
|---|---|---|
| Palette | `Themes/Palette.axaml` | Light, dark and high contrast, as `ThemeDictionaries`. Colours and brushes. |
| Elevation | `Themes/Elevation.axaml` | Levels 0–25 (25 is MudBlazor's softer last entry), three stacked shadow layers each — one hard ring each in high contrast. |
| Typography | `Themes/Typography.axaml` | Roboto, embedded; base size **14px**, not 16. |
| Controls | `Themes/ControlThemes.axaml` | Aggregator; one file per control under `Themes/Controls/`. Keyed by type. |
| Icons | `Themes/Icons.axaml` | The seventeen Material glyphs the templates draw, and the switch's empty thumb icon. Each is a key an app can set in a control's `Resources` to swap that control's glyph: MudBlazor's icon parameters. Not an icon set. |
| Geometry | `Themes/Geometry.axaml` | 4px radius, 4px spacing scale, appbar and drawer sizes, and `FiliInputMinWidth` — the floor that keeps an unstretched empty field from measuring to nothing. |

Three things worth knowing before changing any of them:

- **The primary colour differs by variant.** `#594AE2` is the *light* primary; dark uses
  `#776BE7`. An app pinned to one variant that seeds from the other gets the brand colour wrong
  everywhere. `PrimaryDiffersBetweenVariants` pins this.
- **Base type is 14px.** That smaller baseline is much of why the look reads dense and tidy, and
  it is the first thing to check when a ported screen feels wrong.
- **Elevation does not vary between light and dark**, matching MudBlazor, which uses one shadow
  array for both — the two dictionaries hold the same ladder character for character, and
  `LightAndDarkShareTheSameLadder` keeps them that way. High contrast is the exception: there
  every raised level is a hard 1px ring instead. The ladder lives *inside* the theme dictionaries
  for that reason alone, and the reason is worth knowing on its own: **a dictionary's own entries
  are found before its `ThemeDictionaries`**, so a token declared outside them cannot be
  overridden by a variant. The outer value wins everywhere, silently.

## Fonts

Roboto is **embedded** (`src/Fili.Theme.MudAvalonia/Assets/Fonts`), in three static instances —
Light 300, Regular 400, Medium 500 — which are the weights this theme uses.

Two things to know before changing them:

- **Static instances, not the variable `Roboto[wdth,wght].ttf`.** Avalonia resolves a weight by
  picking a matching *face*, not by setting a variable axis, so a single variable file would
  render Light and Medium as Regular.
- **Their name tables are legacy.** `Roboto-Light` reports its family as `Roboto Light` and
  `Roboto-Medium` as `Roboto Medium`, each with subfamily `Regular`, rather than one `Roboto`
  family carrying three weights. Avalonia's embedded font collection groups them correctly
  anyway; `EveryUsedWeightHasItsOwnFace` is what keeps that true, and is why it asserts the
  family name *starts with* Roboto rather than equals it.

**Roboto covers Latin, Greek and Cyrillic, and nothing else.** On desktop that never shows: a
character Roboto lacks — Arabic, Hebrew, CJK — falls back to a font the operating system supplies.
**A browser build has no system fonts at all.** It can only draw with the faces it ships, so the
same text renders as nothing, silently. An app targeting the browser has to embed a face for every
script it displays and register it with `FontManagerOptions.FontFallbacks`. The theme does not do
this for you — which scripts an app needs is the app's decision, and each one is a few hundred
kilobytes. The gallery is the worked example: it ships static Noto Sans Arabic for the
right-to-left section, in `src/Fili.Theme.MudAvalonia.Gallery/Assets/Fonts`, and `GalleryFonts`
registers it for every head. Registering it on desktop too is deliberate even though Windows has
Arabic fonts of its own: an OS fallback is whatever that machine has installed, so text drawn with
it is not the same on Linux, on macOS, or on another Windows image — including the CI runner the
pixel baselines are compared on.

## Gallery

```powershell
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Desktop
```

Six tabs: palette swatches with computed WCAG contrast ratios, the elevation ladder, the type
ramp, a control-state matrix, a realistic sample screen, and **custom styles**: worked examples
rather than parity. One is an Avalonia control MudBlazor has no counterpart for, where the theme
styles only the part that matters: `DrawerPage`, with MudBlazor's drawer button and mini drawer.
The other is a pattern built from the theme's pieces: a page loader, MudOverlay with a spinning
MudProgressCircular. Each is a pixel frame, so the examples are tested like everything else, and
copying one into an app starts from markup that is known to render. The variant selector in the app bar
switches between light, dark and high contrast at runtime — which is the fastest way to find a
token that was wired statically, or one that high contrast never got its own value for.

`--capture <dir>` renders every view in every variant to PNG without a display, for reviewing
without running the app. They are the same twelve frames the pixel suite compares.

There used to be a substrate selector beside the variant one, flipping the base under the forked
controls between this package's Simple fork and Avalonia's Fluent. It mattered while the package
layered over Fluent; once the base was a fork it only showed a look this theme does not ship (and
drew Fluent's dark theme under the "High contrast" label), so it was removed along with the
gallery's Fluent reference. `StandaloneReadinessTests` is what says which controls are hand-written
and which are forked.

The sample screen matters more than the control matrix: a wall of buttons looks fine under any
theme, and only a real layout exposes flat hierarchy and wrong spacing.

### In the browser

The same gallery runs in a browser, from `src/Fili.Theme.MudAvalonia.Gallery.Browser`, and
`.github/workflows/pages.yml` publishes it to
[GitHub Pages](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/) on every push to `master`.
Both heads share one `MainView`; the desktop's `MainWindow` only hosts it.

```powershell
dotnet workload install wasm-tools     # once: Avalonia.Browser links Skia and HarfBuzz natively
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Browser
```

Building the solution needs the workload too, because the browser head is part of it.

Every URL the page loads is relative, so the published output works unchanged under the Pages
sub-path and at the root of a local server. The first time, Pages has to be switched to
**Settings → Pages → Source: GitHub Actions**; until then the deploy job fails.

The theme is loaded only by the two `StyleInclude`s in App.axaml, which compile to code. Keep it
that way: a `StyleInclude` constructed in C# loads its source by reflection at runtime, which the
browser build's trimming can strip, and the page would then render with no base theme at all.

## Testing and releasing

[Testing](docs/testing.md) walks through every test class, the pixel baselines and the CI
workflows, and what each one guards. [Releasing](docs/releasing.md) covers the tag-driven
publish to nuget.org and its one-time setup. [Versions](docs/versions.md) is the release history.

## Known gaps

47 of the 95 templated control types are hand-written; the other 48 wear forked Simple
templates repaletted onto these tokens, each classified with a reason. There is no date or time
picker restyle. [Known gaps](docs/known-gaps.md)
has the full list and the reason for each.

## Where the light palette's colours come from

MudBlazor's light palette does not spell most of its colours out: it names swatch constants, and
a port that does not read them back gets the light palette subtly wrong. They are `Colors.cs` in
MudBlazor:
`Pink.Accent2` = `#FF4081`, `Blue.Default` = `#2196F3`, `Green.Accent4` = `#00C853`,
`Orange.Default` = `#FF9800`, `Red.Default` = `#F44336`, `Gray.Darken3` = `#424242`.

## Licence and attribution

This package is MIT licensed — see [`LICENSE`](LICENSE). The licence covers the code, the XAML
and the gallery; the third-party material below keeps its own terms.

The token *values* come from [MudBlazor](https://github.com/MudBlazor/MudBlazor) (MIT). Colours,
sizes and shadow definitions are data, not code, and no MudBlazor code is used or derived here.

**This project is not affiliated with, endorsed by, or connected to MudBlazor.** The name is
descriptive — an Avalonia theme in MudBlazor's visual idiom — and the disclaimer matters more
now that the package name carries it, not less. Keep this section.

Roboto is © 2011 The Roboto Project Authors, under the **SIL Open Font License 1.1**; the licence
travels with the fonts in `src/Fili.Theme.MudAvalonia/Assets/Fonts/OFL.txt` and is packed into the
NuGet package. OFL requires that the licence stays with the font files and that they are not sold
on their own — neither constrains this use, but the file must not be removed.
