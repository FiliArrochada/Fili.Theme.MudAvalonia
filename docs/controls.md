# Controls

How each control this theme templates is used, which MudBlazor parameter each class stands for,
and where the values came from. Installing the theme and the full class list are in the
[README](../README.md); every parameter mapped row by row is in [MudBlazor parity](mudblazor-parity.md).

## Buttons

MudButton's whole Variant × Color × Size matrix, one class for each parameter:

```xml
<Button Content="Learn more" />                         text, default colour — MudButton's defaults
<Button Classes="primary" Content="Learn more" />       text, primary
<Button Classes="outlined error" Content="Delete" />    outlined, error
<Button Classes="filled primary" Content="Save" />      filled, primary, elevation 2/4/8
<Button Classes="filled success small" Content="OK" />  filled, success, small
<Button Classes="inherit" Content="Sign in" />          the surrounding text colour
```

- **Variant:** `text` (the default), `outlined`, `filled`. Each is a `ControlTheme`.
- **Colour:** `primary`, `secondary`, `tertiary`, `info`, `success`, `warning`, `error`, `dark`,
  `inherit`, or none for `Color.Default`.
- **Size:** `small`, `large`, or none for `Size.Medium`.

**A colour class is only a colour**, as `Color` is in MudBlazor: `primary` on its own is a primary
*text* button, and the fill comes from `filled`. With no colour, a button is `text-primary` with a
grey hover, which is MudBlazor's default and not Material's primary one.

Every value is from `_button.scss`. Text and outlined buttons tint with the colour at 6%
(`{color}-hover`) on hover, focus and press. Filled ones switch to `{color}-darken` and lift 2 → 4
→ 8. Outlined's line is the colour itself, because `BorderOpacity` is 1.0. The darker and lighter
shades are MudBlazor's own derivation, HSL lightness ∓ 0.075 with .NET's rounding, ported to C# in
`MudColorPort`; `PaletteDerivationTests` checks every shade token against it in all three variants,
and the port against the 14 values mudblazor.com publishes. `Button.axaml` is written by
`ThemeColourGenerator` from one list of colours, so no colour can differ from the others, and
`GeneratedThemeTests` fails if the file is edited by hand. `ButtonMatrixTests` checks every cell of
the matrix, at rest and on a real pointer hover. Both helpers live in the unit-test project, under
`Generation/`.

## Split and drop-down buttons

`SplitButton` is a two-segment `MudButtonGroup`; `DropDownButton` is a `MudMenu` whose activator
is a button.

```xml
<SplitButton Content="Save" />                         text group, text-primary
<SplitButton Classes="outlined primary" Content="Export" />
<SplitButton Classes="filled primary" Content="Deploy" />
```

**The split button takes Button's classes, meaning the same things**: `text` by default,
`outlined` and `filled`, a colour class that is only a colour, `small` and `large`. That is
MudButtonGroup's own Variant × Color × Size, and the same generator writes it
(`ThemeColourGenerator`, into a marked region of `SplitButton.axaml`). The line between the halves
follows the group's rule: text-primary or the colour for a text group, the colour's own border
for an outlined one, `divider` for a filled one with no colour, and the colour's lighten shade
between filled coloured segments.

`_buttongroup.scss` is almost entirely about joining segments into one shape: a non-last child
loses its right-hand corner radii, a non-first child loses its left-hand ones and pulls back by
`-1px` so two borders collapse into one, and a text group draws `border-left: 1px solid
rgba(text-primary, border-opacity)` between segments.

**`BorderOpacity` is 1.0 in `Palette.cs`**, so that divider is text-primary at full strength, not
a hairline. It looks like a mistake in a screenshot and is not one.

The `-1px` does not appear here: Avalonia's `SplitButton` already puts a dedicated separator
element between its halves, so there are no two borders to collapse. Same result, different
route — which is the usual shape of transcribing CSS onto a templated control.

## Text fields

```xml
<TextBox PlaceholderText="Email" />                              standard — the default
<TextBox Classes="filled"   PlaceholderText="Email" />
<TextBox Classes="outlined" PlaceholderText="Email" />
<TextBox Classes="outlined error" PlaceholderText="Email" Text="nope" />
```

**Standard is the base and the other two derive from it**, mirroring `_input.scss` where
`.mud-input` carries the box model — and matching `MudTextField.Variant`, which defaults to
`Variant.Text`. The three differ only in padding and how far the label travels:

| variant | content padding | label resting | label floated |
|---|---|---|---|
| standard | `6px 0 7px` | `translate(0, 24)` | `translate(0, 1.5) scale(.75)` |
| filled | `12px` horizontal | `translate(12, 20)` | `translate(12, 10) scale(.75)` |
| outlined | `14px` horizontal | `translate(14, 20)` | `translate(0, 1.5) scale(.75)` |

**`PlaceholderText` doubles as the floating label.** Avalonia's `TextBox` has no `Label` property,
and adding an attached one would be the first real API in a package whose point is not having any.
The cost: these variants have no separate placeholder — the label occupies that slot until it
floats. If both are ever needed, a `FiliTextField.Label` attached property is the follow-up, in a
package that depends on this one.

The label floats when the field is **focused or non-empty**, which is expressible as a selector
only because Avalonia gives `TextBox` an `:empty` pseudo-class.

**Two ways to be invalid, and both paint the same.** `Classes="error"` is the one you set by hand
(`MudTextField.Error`); the other fires on its own when a *binding* fails validation and Avalonia
sets `DataValidationErrors.HasErrors`. The template hosts a `DataValidationErrors`, so the message
renders **under the field** at 12px the way MudBlazor's helper text does — not as the forked
template's red circle with a tooltip off to the right. Before this was wired, a failing binding
recoloured nothing and showed nothing.

**Under the field is MudInputControl's helper line**, one row for three things:

```xml
<TextBox Classes="filled" PlaceholderText="Email" AutomationProperties.HelpText="We never share it" />
<TextBox Classes="outlined counter" PlaceholderText="Title" MaxLength="40" />     17 / 40
<TextBox Classes="dense" PlaceholderText="Email" />                              Margin.Dense
```

- **Helper text is `AutomationProperties.HelpText`**, Avalonia's own attached property, so it
  costs no API and a screen reader announces it, as MudBlazor wires `HelperText` into
  `aria-describedby`. An error takes its place, as MudBlazor's does.
- **`counter` is `MudTextField.Counter` over `MaxLength`**: `17 / 40` on the right, or the length
  alone when `MaxLength` is 0, which is `Counter="0"`. It turns red with the field.
- **`dense` is `Margin.Dense`**: 3px off a standard field, 8px off a filled one, 16px off an
  outlined one, from `.mud-input-root-margin-dense`, with the label resting and floating to match.

A `ComboBox` takes the same: `filled`, `outlined`, `dense`, `error`, and helper text. A select
used to have no error host at all, so a failing binding on one showed nothing.

**Filled and outlined fields had no side padding until 0.2.0**, though the themes declared 12px
and 14px. The template wrote the content margin inline, which binds at Template priority, and a
variant's plain `^ /template/` style cannot beat that; see [the trap](traps.md#styles-priority-and-selectors).
`VariantsPadTheirContentFromTheSide` now pins it.

The outlined variant masks the border behind the floated label with an opaque patch rather than
cutting a real notch in the stroke — a notch needs a generated `Geometry`, and the mask is
indistinguishable when the field sits on `FiliSurface`.

## Selection controls

```xml
<CheckBox     Content="Enable sync" />
<RadioButton  Content="Weekly" GroupName="cadence" />
<ToggleSwitch Content="Dark mode" />
```

With no class they are MudBlazor's `Color.Default`, which is **grey, not primary**: the icon
button a checkbox or radio glyph sits in is `action-default`, checked or not, and a switch keeps
`_switch.scss`'s literal `#fafafa` thumb on and off. A colour class is `Color` — on a checkbox or
radio it paints the glyph in every state, on a switch the thumb and track only when on — and
`small` / `large` are MudIcon's 20 and 36px and the switch's own spans:

```xml
<CheckBox Classes="primary" Content="Enable sync" />
<ToggleSwitch Classes="success large" Content="Online" />
```

**MudBlazor does not draw them — it renders Material icons.** `MudCheckBox` picks between
`Icons.Material.Filled.CheckBox`, `CheckBoxOutlineBlank` and `IndeterminateCheckBox`; `MudRadio`
between `RadioButtonChecked` and `RadioButtonUnchecked`. So a hand-drawn box with a stroked tick
is not a close approximation, it is a *different artefact* — it cannot match the glyph's corner
geometry or the indeterminate bar. `Themes/Icons.axaml` carries those glyphs as `StreamGeometry`,
transcribed from `Icons/Material/Filled.cs`, and the templates swap between them.

- **CheckBox / RadioButton** — the real glyphs at 24px, swapped per state. Note the radio: checked
  grows an inner disc inside the ring rather than filling it; filling it is the usual mistake and
  the result reads as a round checkbox.
- **ToggleSwitch** — a 20px thumb that is *larger* than its 14px track, overhanging it above and
  below, with elevation under the thumb. The track is `action-default` at 48%, lifting to 50% when
  on, and only a colour class changes its colour.

All three centre a circular 40px state layer on the control rather than tinting the whole row.

**One trap in transcribing a Material glyph.** Each one carries an `M0 0h24v24H0z` viewbox spacer,
which is a transparent bounding rect in SVG and a *painted square* in a filled `StreamGeometry`.
It is dropped; bounds are preserved by drawing at `Width`/`Height` 24 with `Stretch="None"`, since
the data is already in 0–24 space.

## Slider and tabs

```xml
<Slider Value="40" />
<TabControl> <TabItem Header="One" /> </TabControl>
```

- **Slider** — `MudSlider` defaults to `Size.Small`, `Color.Primary` and `Variant.Text`: a 2px
  rail and a 12px thumb, the whole rail the colour at 30%. `medium` and `large` are 4px / 20px and
  6px / 24px, and a colour class repaints it. The thumb does not grow: MudBlazor rings it, 1px of
  the colour at 24% on hover and 2px on focus and press.

  ```xml
  <Slider Classes="filled" Value="40" />                                   Variant.Filled
  <Slider TickPlacement="BottomRight" TickFrequency="10" Value="50" />     TickMarks
  <Slider Classes="value-label" Value="75" />                              ValueLabel
  ```

  `filled` draws the colour solid up to the thumb. Tick marks are Avalonia's own `TickPlacement`
  and `TickFrequency` (or `Ticks`), drawn as MudBlazor draws them: dots on the rail, its thickness
  and the colour. `value-label` shows the value in a chip of the colour above the thumb while it is
  held, in the app's number format. Structure is dictated by Avalonia, not Material: `Slider`
  requires a `Track` named `PART_Track` with two `RepeatButton`s, which here are the hit areas a
  click pages through and, for the decrease half, the `filled` part; the 30% rail and the dots sit
  behind them. Both orientations need their own `Template`; a horizontal one applied to a
  vertical slider renders sideways rather than degrading.
- **TabControl** — MudTabs' 48px bar with a 2px primary indicator. MudTabs' defaults decide the
  rest, and this theme used to differ on each: the bar is `surface` with **no** rule under it
  (`Border` defaults to false), tabs are text-primary rather than text-secondary, and
  `MinimumTabWidth` is 160px. `ItemContainerTheme` is what carries the header theme down to a bare
  `<TabItem>`; without it the control is themed and its headers are not, which looks like the
  theme half-applied.

```xml
<TabControl Classes="primary">…</TabControl>              MudTabs Color: the bar in the colour
<TabStrip Classes="border centered">…</TabStrip>          Border, Centered
<TabControl Classes="outlined rounded">…</TabControl>     Outlined, Rounded
```

A colour class is MudTabs' `Color`, the colour of the **bar** - the tabs and the indicator take
its contrast text, and the active tab's hover is `{color}-lighten`. A tab's theme cannot see its
parent's class, so those are `TabControl.primary > TabItem` child selectors in `FiliTheme.axaml`;
a descendant selector would also repaint a `TabControl` nested in the content.

The indicator does not slide between tabs — that needs to measure both headers and animate
between them, which means code-behind and a custom panel. A per-item indicator that fades is the
declarative 90%.

**`TabStrip` is the same thing without a content area.** Avalonia splits the header row and the
tab control into two types; MudBlazor does not, so both carry the same metrics. They are written
out twice rather than shared, because a `ControlTheme` cannot be `BasedOn` one whose `TargetType`
differs — if you change one, change the other, which is why the gallery shows them together.

**`PipsPager` is MudCarousel's bullets, not `MudPagination`.** Avalonia has no numbered pager, and
`MudCarousel.razor.cs` names the parts exactly: `CheckedIcon` is `RadioButtonChecked`,
`UncheckedIcon` is `RadioButtonUnchecked`, and the arrows are `NavigateBefore`/`NavigateNext`. So
the pips are **the same two glyphs the radio button uses** — selecting one swaps the glyph, a ring
gaining a disc, rather than a dot changing colour. The 0.75 opacity on the row is transcribed from
the inline style in that razor file. (`NavigateNext` turns out to be byte-identical to
`ChevronRight`, so `Icons.axaml` carries one path under the name it was first needed for.)

## Progress, dividers, panels and links

```xml
<ProgressBar Value="65" />                                4px, square — the MudBlazor defaults
<ProgressBar Classes="primary large rounded" Value="65" />
<ProgressBar Classes="circular" Value="65" />            MudProgressCircular, 40px
<ProgressBar Classes="circular primary small" IsIndeterminate="True" />
<Separator />                                             1px, no margin
<Separator Classes="inset" />                             72px indent, to clear an avatar column
<Expander Header="Details"> … </Expander>
<HyperlinkButton Content="Learn more" />
```

Five places where the MudBlazor default is not the one you would guess:

- **A bare `ProgressBar` is a 4px square hairline.** `MudProgressLinear.Size` defaults to
  `Size.Small` and `Rounded` to `false`; `medium` and `large` are 8px and 12px, `rounded` opts
  into the 4px radius. The track is always the bar's own colour at 20% — only `Color.Default`
  splits them (track `action-disabled`, bar `action-default`), which is why `Background` and
  `Foreground` are the two knobs.
- **`circular` is MudProgressCircular, and it is grey with no colour.** Its default is
  `text-secondary`, not primary, and there is no track. It is a theme the same `ProgressBar` takes,
  as `chip` is one a `Button` takes, so `Value`, `Minimum`, `Maximum` and `IsIndeterminate` all
  carry over: 40px (`small` 24, `large` 56), starting at twelve o'clock and filling clockwise.
  `rounded` rounds the ends and `ShowProgressText` puts the value in the middle, which is where
  MudBlazor puts its `ChildContent`.
- **A `Separator` has `margin: 0`.** The forked Simple template ships `Margin="29,1,0,1"`, a
  menu-shaped indent baked into every divider, which used to survive because the theme only
  recoloured it. `inset` (72px) and `middle` (16px) are MudBlazor's real variants.
- **An `Expander` header is 15px**, which is neither body1 (16) nor body2 (14) — MudBlazor sizes
  `.mud-expand-panel-header` on its own at `.9375rem`. The panel carries a surface and elevation 1
  because `MudExpansionPanels` is a `MudPaper`; `Classes="flat"` drops both when it already sits
  on a card.
- **A `HyperlinkButton` is not underlined at rest.** `MudLink.Underline` defaults to
  `Underline.Hover`. `Classes="underline"` is always-on, `no-underline` is never.

## The drawer, and a third way to theme a control

`SplitView` is MudDrawer, and it is the one control here treated with **styles rather than a
`ControlTheme`** — so it is not in the 34 above, and that is the point rather than an oversight.

Everything MudDrawer contributes is a *value*: `layout/_drawer.scss` (in `Styles/layout/`, not
`components/`, which is why it takes some finding) paints from the `drawer-background` and
`drawer-text` tokens, `LayoutProperties.cs` gives `DrawerWidthLeft = "240px"` and
`DrawerMiniWidthLeft = "56px"` — already `FiliDrawerWidth` and `FiliDrawerMiniWidth` — and
`MudDrawer.Elevation` defaults to 1. What the forked template carries is *structure*: pane
sliding, the four display modes, the light-dismiss layer. Get a part name wrong there and the
drawer lays out perfectly and never opens, which is the same reason `ScrollViewer` and `Window`
are left alone.

So there are three ways to change how a control looks here, and the third is new:

| | when |
|---|---|
| Hand-written `ControlTheme` | the shape is wrong — a floating label, a raised button |
| `Accents.axaml` | the control paints from one of Simple's ~96 shared keys |
| A `Style` in `FiliTheme.axaml` | the shape is right and only values are wrong |

The third works because **a `Style` outranks a `ControlTheme` setter** — normally the trap this
file warns about twice, relied on deliberately here. `DrawerTakesMudDrawerMetricsOverTheForkedTemplate`
pins it, because if that precedence ever reversed every drawer would quietly revert to Simple's
320px grey pane.

One caveat found while doing this: `Accents.axaml` **cannot** override a key a forked control
file declares for itself, such as `SplitViewOpenPaneThemeLength`. Accents is merged *before* the
control dictionaries, and in a merged `ResourceDictionary` the later entry wins.

Not carried over: the pane's elevation-1 shadow. The pane root is a `Panel`, and only a `Border`
can carry a `BoxShadow` — the same limitation that made `Button` need a template in the first
place. Adding it means owning the template.

## Snackbars

`WindowNotificationManager` and the `NotificationCard`s it builds are MudSnackbar: 6px/16px
padding, 288px minimum and 500px maximum width, 16px between stacked cards, and the manager 24px
from whichever edges it is anchored to.

Two things were read rather than chosen. The **elevation is 6** — `_snackbar.scss` spells out the
same three shadow layers `FiliElevation6` carries, character for character, so the level is
transcribed rather than picked. And the **severity colours come from `_alert.scss`**, because a
snackbar is a filled `MudAlert`: the palette colour as ground, its contrast text as foreground,
at **Medium** weight — the filled alert's 500 overrides the snackbar's own 400. Each severity has
its own contrast token (`FiliInfoContrastTextBrush` and so on). In light and dark all four are
MudBlazor's white; in high contrast they are black, because that variant's status colours are
pastels and white on them would be about 1.6:1.

**The enter and exit animations are copied from the forked template deliberately**, and one of
them is load-bearing: the key frame that sets `IsClosed` at 100% is what actually removes a card.
A theme that drops it renders beautifully and leaves notifications on screen forever.

## Right to left

Set `FlowDirection="RightToLeft"` and everything mirrors. No template in this package does
anything to make that happen, and that is the finding rather than an omission:

> **Avalonia mirrors an entire subtree with a single transform, applied where the flow direction
> CHANGES** — not per control.

So grid columns, dock sides and `Left`/`Right` alignment all flip on their own. The numeric
field's spin column moves to the left, the tree indents from the right, the split button's
rounded corners swap ends, and the inset divider indents from the right, none of which is coded
for anywhere. MudBlazor reaches the same place with logical CSS properties (`margin-inline-end`,
`padding-inline-start`), which is exactly why those have no counterpart here.

**The exception is a glyph that must not mirror.** A checkmark is not a directional symbol, so
Material's bidirectionality guidance leaves it alone — a reversed tick just looks broken. The
three checkbox glyphs therefore opt out with `FlowDirection="LeftToRight"`, which is the same
thing Avalonia's own Simple theme does to its check path and is how the case was spotted at all.

Arrows are deliberately **not** opted out. A tree's disclosure arrow *should* point left in RTL,
and the mirror gives that for free; pinning it left-to-right would leave it pointing away from
the content it opens. `RightToLeftTests` asserts both halves of that split.

Two things to know if you go looking:

- Measuring the mirror requires **crossing the boundary**. Translate a point to the mirrored
  panel and the coordinates are still left-to-right; translate it to the window and the flip
  appears. The first version of that test failed for exactly this reason.
- `HasMirrorTransform` is false on the controls *inside* an RTL subtree. It is true only on the
  element where the direction changed.
- **In a browser, RTL text needs a font for its script.** The mirroring works anywhere; the
  glyphs do not, because there are no system fonts to fall back on — see [Fonts](../README.md#fonts).

## High contrast

A third variant, and **not a MudBlazor one** — MudBlazor ships light and dark and nothing else, so
this one is derived rather than transcribed. Select it like any other variant:

```csharp
Application.Current.RequestedThemeVariant = FiliThemeVariants.HighContrast;
```

```xml
<Application xmlns:theme="using:Fili.Theme.MudAvalonia"
             RequestedThemeVariant="{x:Static theme:FiliThemeVariants.HighContrast}">
```

**`x:Key="HighContrast"` does not work**, and it does not fail quietly: Avalonia's `ThemeVariant`
type converter accepts the built-in variants only and throws `NotSupportedException` while the
merged dictionary is being built, which takes the rest of the theme down with it. A custom variant
is reachable from XAML only through `x:Static`, so `FiliThemeVariants.HighContrast` is the one
object every dictionary here — and every adopting app — has to name.

Three rules produced every value in it:

1. **Separation comes from lines, not fills.** `FiliSurfaceColor` sits one step off the page
   ground and is not doing the work; `#0D0D0D` against `#000000` is a difference nobody can see.
   What divides a card, a menu or a dialog from the page is its outline.
2. **Nothing is conveyed by alpha.** Light and dark lean on translucent blacks and whites — a
   divider is `#1FFFFFFF` — which composite against whatever is behind them. On a black ground a
   12% white line is not a faint line, it is no line. Alpha survives in six tokens, all of them
   overlays rather than content: the two state layers, the selection tint, the input fill and the
   two scrims.
3. **Disabled still has to be legible.** `#CFCFDA` on black is about 12:1, far too strong to read
   as disabled in a normal palette, which is the point. Disabled is signalled by being dimmer than
   enabled, not by being nearly gone.

**The shadow ladder becomes a hard 1px ring.** That is the whole reason this variant needed no
control template edits: every surface that already asked for a `FiliElevation*` gets its outline
for free. Two things follow — a surface painted with `FiliElevation0` has no outline here either,
and **the ring colour is fixed at `#E8E8EE`**, because a `BoxShadows` string cannot carry a
`DynamicResource`. An app that repaints `FiliLinesDefaultColor` for high contrast will find the
rings did not follow it.

### What an adopting app has to do

**Declare the variant, not just the tokens.** `FiliThemeVariants.HighContrast` inherits from
`ThemeVariant.Dark`, which is load-bearing — it is what lets the eighty-one forked control themes,
which declare Light and Dark only, work here untouched. It is also the trap: an app that overrides
palette tokens in its `Light` and `Dark` dictionaries and stops there does **not** get a
high-contrast version of its own colours. It gets its *dark* ones, silently, because that is what
the variant falls back to.

```xml
<ResourceDictionary.ThemeDictionaries>
  <ResourceDictionary x:Key="Light">   <!-- … --> </ResourceDictionary>
  <ResourceDictionary x:Key="Dark">    <!-- … --> </ResourceDictionary>
  <ResourceDictionary x:Key="{x:Static theme:FiliThemeVariants.HighContrast}">
    <!-- the same Fili* keys again, in this variant's colours -->
  </ResourceDictionary>
</ResourceDictionary.ThemeDictionaries>
```

An app that swaps whole palette dictionaries at runtime rather than using `RequestedThemeVariant`
needs nothing new: its high-contrast palette just has to carry the same
`Fili*` overrides as its other palettes.

## Keyboard focus

Every interactive control lights its state layer on `:focus-visible` — the same tint hovering
gives it. That is MudBlazor's own affordance rather than a choice made here: `_reset.scss` sets
`outline: none` and `outline: 0`, deliberately removing the browser's focus ring, and each
component maps `:focus-visible` to the hover colour:

```
_button.scss          &:focus-visible, &:active { background-color: action-default-hover }
_expansionpanel.scss  &:focus-visible           { background-color: action-default-hover }
_link.scss            &:focus-visible, &:active { text-decoration: underline }
_list.scss            &:focus:not(.mud-selected-item) { background-color: action-default-hover }
```

Three details worth keeping:

- **`:focus-visible`, not `:focus`.** Clicking a button should not leave it tinted once the
  pointer has gone; only keyboard navigation should mark it.
- **A selected list row is excluded**, per `_list.scss`. Arrow keys move focus and selection
  together, so tinting both would just be the selection tint twice.
- **Text fields keep `:focus`**, not `:focus-visible` — a field shows its accent rule however it
  was focused, because that rule says "this is where typing goes" rather than "this is where the
  keyboard is".

The affordance is a 4% tint, which is what MudBlazor ships and is on the subtle side. An app with
a stricter accessibility bar should add a ring of its own; the theme deliberately does not invent
one.

## Two converters

Besides `FiliThemeVariants`, the package's only code is two `internal` converters, each for
something XAML cannot express on its own:

- **`FactorConverter`** multiplies a bound pixel dimension by a constant. The indeterminate
  progress bar needs it: MudBlazor's keyframes animate `left`/`right` percentages, so each bar
  changes *width* as it travels (35% → 90%, and 200% → 1% for the second), and Avalonia's
  transform parser rejects a `%` unit outright — `FormatException: Invalid unit: %`, thrown at
  **runtime**, because a transform string in a key frame is parsed lazily and compiles either way.
- **`ErrorMessageConverter`** turns one `DataValidationErrors.Errors` entry into the sentence to
  show. An entry is usually an `Exception`, and binding straight to it renders
  `"System.InvalidOperationException: Must be a valid email address"`.

Compiled XAML in the same assembly can construct an `internal` type, so being internal costs
nothing and keeps the public surface at that one type. Reach for one only after establishing
that the XAML route does not exist.

## Lists, selects and the things built out of them

`MudSelect`, `MudAutocomplete` and `MudList` share their parts, and so do these:

```xml
<ComboBox PlaceholderText="Platform"> <ComboBoxItem>PC</ComboBoxItem> </ComboBox>
<ListBox Classes="surface"> <ListBoxItem>First</ListBoxItem> </ListBox>
<AutoCompleteBox PlaceholderText="Search" />
<NumericUpDown Value="42" PlaceholderText="Quantity" />
```

- **ComboBox** — the finding in `_select.scss` is a *negative* one: `.mud-select` has no box model
  at all, and every visual comes from `.mud-select-input`, which is a `MudInput`. So a select
  **is** a text field with a drop-down adornment, and it wears the standard field's underline,
  accent rule and floating label. Anything else would make selects and fields disagree in the
  same form.
- **A selected row is a tint, never a filled bar.** `MudList` marks it with
  `mud-selected-item mud-primary-text mud-primary-hover` — primary text over primary at 6% — so it
  stays as readable as the rows around it. `ListBox`, `ComboBox` and `ToggleButton` all use that
  one rule.
- **NumericUpDown** — a `MudInput` with a 24px spin column, which is not a guess:
  `_inputcontrol.scss` reserves exactly `padding-right: 24px` for it, with the comment *"This must
  be the same width of the spinners"*.
- **AutoCompleteBox** — both halves were already themed (a `TextBox` and a `ListBox`), so its
  theme exists only for the popup surface.
- **TreeView** — rows are 32px with 4px/8px padding, indented **17px per level**, and the arrow is
  `ChevronRight` turning **90°** rather than the expansion panel's `ExpandMore` turning 180°. A
  leaf keeps the 32px arrow column, for alignment, and hides the glyph. The indent is Avalonia's
  mechanism rather than MudBlazor's: a `TreeViewItem` multiplies its own `Level` by an indent
  resource through `TreeViewItemIndentConverter` instead of nesting margins, so the 17px is
  declared once and every level derives from it.

**The select's floating label needed two TextBlocks**, and the reason generalises. A select must
float its label when it holds a *value*, not only when focused, or the resting label sits on top
of the selected text — and Avalonia has no pseudo-class for "has a selection". `:empty` on an
`ItemsControl` means **no items**, which is why the first attempt floated every label permanently.
A `Style` selector is the only thing that can drive an animation, so the animated label exists
only while nothing is selected and a static copy takes over once something is. They share
position, size and colour, so the handover is invisible.

## Scrolling and menus

```xml
<ScrollViewer> … </ScrollViewer>
<Menu> <MenuItem Header="Library"> … </MenuItem> </Menu>
```

- **ScrollBar** — a thin buttonless rail with a rounded thumb that darkens on hover and press.
  Dropping the line buttons is safe (their lookups are null-safe, and wheel/drag/keyboard/touch
  are `ScrollViewer`'s job) but a `Track` named `PART_Track` is required or the thumb never moves.
- **ScrollViewer keeps its forked template**, deliberately. There was a hand-written one; it was
  deleted once the fork landed, because it did nothing the fork does not and it is the control
  where a mistake costs most: `PART_ContentPresenter` must be a `ScrollContentPresenter`
  specifically, alongside `PART_HorizontalScrollBar` and `PART_VerticalScrollBar`. Rename any of
  the three and you get a control that lays out perfectly and does not scroll. The themed
  `ScrollBar` is reached *through* it.
- **Menu** — **two** item themes, not one. A top-level strip item and a dropdown row look alike in
  markup and are different controls on screen: the strip item has no chevron, no icon column, and
  drops its popup downward. The first render of this menu showed `Library >  View >` across the
  top, because the dropdown template hides its chevron only for *leaf* items and a top-level item
  always has children. `Menu.ItemContainerTheme` is the strip item; that item's own
  `ItemContainerTheme` is the row.
- The reserved icon column means labels line up down a menu whether or not each item has an icon —
  the detail most hand-rolled menus miss.

## Overlays

`ToolTip`, `FlyoutPresenter` and `MenuFlyoutPresenter` are created by the framework rather than
placed in markup — `ToolTip.Tip="text"` builds the ToolTip internally, `<Flyout>` builds its
presenter — which is why they were keyed by type even back when everything else was keyed by
class. They now look like every other theme in the package.

The tooltip is deliberately **not theme-varying**: a Material tooltip is a dark chip in light and
dark alike, so it reads as an annotation floating above the UI rather than as another surface of
it. Its fill is transcribed rather than chosen — `_tooltip.scss` paints
`var(--mud-palette-gray-darker)`, which is `Palette.GrayDarker` = `Colors.Gray.Darken2` = `#616161`,
**solid**. An earlier version used 90% alpha, which is Material's spec and not MudBlazor's.

Flyout templates carry an 8px outer margin because `BoxShadow` draws *outside* the border and a
popup window is sized to its content — with no room to spill into, the shadow is clipped and the
flyout reads flat.

**`Window` is not themed**, and that is a deletion rather than an omission. There was a hand-written
`Window` theme, written while layering; it went when the fork landed, because two of its parts are
load-bearing and invisible until missing — `VisualLayerManager` (leave it out and every tooltip
and flyout in the app silently fails to appear) and `PART_TransparencyFallback` (without it, a
window asking for acrylic on a compositor that cannot provide it renders with no background at
all). The forked template already gets both right, and Material has no opinion about window
chrome.
