# Known gaps

What this theme does not do, and why. Per-parameter differences are in
[MudBlazor parity](mudblazor-parity.md).


**47 of the 95 templated control types are hand-written**; the other 48 wear forked Simple
templates repaletted onto these tokens. Nothing is invisible and nothing external is required —
the remaining gap is Material *shape*, not colour.

**The control list is closed, and "forty-eight still forked" is not a backlog.** Every one of
those forty-eight is classified with a reason in `StandaloneReadinessTests.Unthemed`, and the test asserts
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

- **`Carousel`, `CarouselPage` and `GroupBox` are deliberately left forked**, which closes the
  list rather than leaving it open. `.mud-carousel` is `display: flex; position: relative;
  overflow: hidden` plus transition keyframes — its visible chrome is icon buttons and bullets,
  which are themed elsewhere, and the forked template is already a clipped `ScrollViewer` with no
  decoration. `CarouselPage` is one of Avalonia's shell page types, like `NavigationPage` and
  `TabbedPage`, and was never in scope. The one exception is `DrawerPage`'s drawer: its pane
  toggles, which would otherwise draw a white icon on nothing, and its compact rail, which is a
  mini drawer as a `SplitView`'s is. Its top bar and title keep the fork's look. `GroupBox` has no MudBlazor counterpart at all: the
  nearest things are `MudCard` and `MudPaper`, which are different shapes, so theming it would be
  invention rather than transcription.
- **The date and time pickers** (`Calendar` and its four helpers, `CalendarDatePicker`,
  `DatePicker`, `TimePicker`) are out of 1.0, by decision. They wear the forked Simple templates
  in this theme's palette, so they are legible in every variant, but not MudBlazor's shape.
  These are not restyling jobs: `MudDatePicker` and `MudTimePicker` are bespoke components, so
  matching them means a rewrite per template with no shortcut. Should that ever happen, it is a
  visible change and lands with an upgrading note.
- **`TableView`**, Avalonia's newer table. MudDataGrid's look is on the `DataGrid` package's
  control instead, the one apps use.
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
- **Uppercase reaches a string label only.** Avalonia has no text-transform, so a button's or
  tab's string is drawn in `FiliButtonTextTransform`'s case on its way into the template. Content
  that is already a control - a `TextBlock`, a panel with an icon and a label - keeps the case it
  was written in, where CSS would reach every piece of text inside, and so does the `button` type
  class on a `TextBlock`.
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
  type ramp - are classes here as well. `FiliSpacing1` … `FiliSpacing8` are numbers, so they
  fill a `Spacing`; a `Padding` or `Margin` is a `Thickness`, written out.
- **Elevation reads faintly in dark mode.** Material's shadows are black at low alpha, which is
  nearly invisible on a dark ground; MudBlazor has the same problem and this theme reproduces it
  by default. `material-elevation-overlay` on a window or container opts into Material's own
  answer, which MudBlazor does not offer: each raised surface is lightened by its level, so height
  reads as lightness. It reaches surfaces in the surface colour - papers, cards, panels, grids,
  menus, selects and flyouts - not tab bars, whose shadow sits outside the bar, nor app bars,
  buttons or badges, which are coloured. A shadow an app repaints needs its `FiliElevationOverlay`
  level repainted too.
