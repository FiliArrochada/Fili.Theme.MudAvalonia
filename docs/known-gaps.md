# Known gaps

What this theme does not do, and why. Per-parameter differences are in
[MudBlazor parity](mudblazor-parity.md).


**47 of the 95 templated control types are hand-written**; the other 48 wear forked Simple
templates repaletted onto these tokens. Nothing is invisible and nothing external is required —
the remaining gap is Material *shape*, not colour.

**The control list is closed, and "twenty-eight still forked" is not a backlog.** Every one of
those twenty-eight is classified with a reason in `StandaloneReadinessTests.Unthemed`, and the test asserts
that inventory is exhaustive and says each type once — so a future Avalonia that adds a control
fails by name asking to be classified, rather than quietly making the coverage number mean less.
What is left is framework plumbing with no design opinion to express, a bespoke MudBlazor
component that would be a rewrite rather than a restyle, or a control MudBlazor does not have.

Two of those reasons were surprises worth recording:

- **`MaskedTextBox` and `ToggleSplitButton` already get the themed templates.** Their
  `StyleKeyOverride` points the theme lookup at `TextBox` and `SplitButton`, so they render
  identically — same desired size, same descendant count. A theme of their own would be a second
  copy of one that already applies.
- **`GridSplitter` is left alone on purpose.** MudBlazor's counterpart exists, and transcribing
  it faithfully would make the control *invisible*: `_splitpanel.scss` gives the divider a
  cursor, a 12px invisible hit area and a focus outline, and nothing else — the separation comes
  from the two panels having different backgrounds, which Avalonia does not guarantee. Simple's
  visible hairline is kept deliberately. This is the same failure this package has already
  shipped twice in other costumes, caught before rather than after.

Controls MudBlazor has a counterpart for and this theme does not, roughly by how often an app
hits them:

- **`GroupBox` is deliberately left forked.** It has no MudBlazor counterpart: the nearest
  things are `MudCard` and `MudPaper`, which are different shapes, so theming it would be
  invention rather than transcription.
- **`DrawerPage` keeps the fork's top bar and title.** Its drawer is themed - the pane toggles,
  which would otherwise draw a white icon on nothing, and the compact rail, a mini drawer as a
  `SplitView`'s is - by styles rather than a theme of its own.
- **The spinner `DatePicker`** wears the forked Simple template in this theme's palette, legible
  in every variant but not MudBlazor's shape. MudDatePicker is `CalendarDatePicker`; see
  [Date picker](controls.md#date-picker).
- **MudTimePicker's clock face is Avalonia's columns.** The field, the adornment, the toolbar and
  the popover are MudTimePicker's; under the toolbar MudBlazor draws a clock face, which is a
  control of its own rather than a template, so the hours and minutes turn in Avalonia's columns.
  The toolbar's hour and minute are not buttons switching the view, since both columns show at
  once, and AM and PM are not buttons either: the period column sets them.
- **Two parts of MudDatePicker are Avalonia's.** The day names are the `CalendarItem`'s shortest
  names (S M T), where MudBlazor writes the abbreviated ones (Sun Mon Tue); and the year view is the
  `CalendarItem`'s decade, twelve years in a grid, where MudBlazor scrolls a list of every year.
  Months sit four to a row, where MudBlazor's sit three. The `CalendarItem` builds those itself.
- **What MudDataGrid does beyond its look.** No column options menu, no filter row, no pager:
  each is a control with behaviour of its own, which belongs in a separate package rather than
  a theme. Columns resize when `CanUserResizeColumns` is set, but without MudBlazor's 2px primary
  line down the grid: the DataGrid keeps its resize state internal, and showing the line would
  mean re-implementing its pointer handling. A selected row is tinted although MudDataGrid draws
  none - see [Data grid](controls.md#data-grid).
- **MudSlider's `TickMarkLabels` and MudProgressLinear's `Buffer`.** Both are data the Avalonia
  control has no property for - an array of strings, a second value - and the only way to pass
  it would be a public attached property, which this package does not add: its public API is
  one type. The tick dots and the bar itself are themed.

And the things that are not controls:

- **The ripple answers a pointer only**, as MudBlazor's does: mudRipple.js listens for
  `pointerdown`, so a key press shows the state layer and no ripple, here as there. Its colour is
  the text's at the moment of the press; a ripple already growing keeps it.
- **Uppercase reaches plain text only.** Avalonia has no text-transform, so a button's string,
  and each `TextBlock` in its content, is drawn in `FiliButtonTextTransform`'s case. A `TextBlock`
  whose runs the app wrote keeps them as written, and text a nested control draws in its own
  popup is not reached, where CSS would reach every piece of text inside.
- **No separate placeholder on a text field** — `PlaceholderText` is the floating label; see
  [Text fields](controls.md#text-fields).
- **The `dark` colour barely shows as text or a line in dark mode, and not at all in high
  contrast**: `#27272F` on a `#32333D` page, and black on black. MudBlazor's dark theme behaves the
  same way. A filled dark button stays visible.
- **Colour is on every control MudBlazor gives a `Color` that has an Avalonia counterpart here**,
  the tab bar and the app bar included. See [MudBlazor parity](mudblazor-parity.md) for every remaining gap.
- **RTL works, with one deliberate exception** — see [Right to left](controls.md#right-to-left). The remaining gap is
  narrow: no control here has a *bidi-aware* behaviour beyond mirroring, so if one ever needs to
  keep a numeral or a code fragment left-to-right inside otherwise-RTL content, that is the app's
  `FlowDirection` to set, not the theme's.
- **MudBlazor's layout utility classes are properties here.** `pa-4`, `mx-2`, `d-flex` and the
  rest are CSS for what an Avalonia control sets itself: spacing is `Padding`, `Margin` and a
  panel's `Spacing`, at 4px a step (`pa-4` is `Padding="16"`, `gap-2` is `Spacing="8"`);
  `d-flex` and `flex-column` are a `StackPanel` and its `Orientation`; `justify-*` and `align-*`
  are `HorizontalAlignment` and `VerticalAlignment`; `rounded-*` is `CornerRadius`, `border-*`
  `BorderThickness`, `d-none` `IsVisible`, `cursor-pointer` `Cursor`, `overflow-hidden`
  `ClipToBounds` and `z-*` `ZIndex`. The utilities that carry a token - colour, elevation, the
  type ramp - are classes here as well. MudBlazor's spacing scale is a pair of keys per step,
  0 to 20: `FiliSpacing4` is 16 as a number, for a `Spacing`, and `FiliSpacing4Thickness` 16 on
  every side, for a `Padding` or `Margin`; a one-sided step is a `Thickness` written out.
- **Elevation reads faintly in dark mode.** Material's shadows are black at low alpha, which is
  nearly invisible on a dark ground; MudBlazor has the same problem and this theme reproduces it
  by default. `material-elevation-overlay` on a window or container opts into Material's own
  answer, which MudBlazor does not offer: each raised surface is lightened by its level, so height
  reads as lightness. It reaches surfaces in the surface colour - papers, cards, panels, grids,
  menus, selects and flyouts - not tab bars, whose shadow sits outside the bar, nor app bars,
  buttons or badges, which are coloured. A shadow an app repaints needs its `FiliElevationOverlay`
  level repainted too.
