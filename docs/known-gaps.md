# Known gaps

What this theme does not do, and why. Per-parameter differences are in
[MudBlazor parity](mudblazor-parity.md).


**46 of the 95 templated control types are hand-written**; the other 49 wear forked Simple
templates repaletted onto these tokens. Nothing is invisible and nothing external is required —
the remaining gap is Material *shape*, not colour.

**The control list is closed, and "forty-nine still forked" is not a backlog.** Every one of
those forty-nine is classified with a reason in `StandaloneReadinessTests.Unthemed`, and the test asserts
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
- **MudExpansionPanels as a group.** Each `Expander` is one panel, with Elevation, Square, Dense
  and Gutters as classes. What MudBlazor does between the panels of a group - the `Outlined` rule
  under each (on by default), and the 16px an expanded one moves away from its neighbours - has
  nothing to hang on: Avalonia has no expansion-panel group, and its selectors cannot see the
  panel before an expanded one.
- **MudSlider's `TickMarkLabels` and MudProgressLinear's `Buffer`.** Both are data the Avalonia
  control has no property for - an array of strings, a second value - and the only way to pass
  it would be a public attached property, which this package does not add: its public API is
  one type. The tick dots and the bar itself are themed.

And the things that are not controls:

- **No ripple.** Avalonia has no primitive for it; faking it means an animated, clipped ellipse
  driven from pointer position. Everything interactive carries the static half — a tinted state
  layer on hover, press and keyboard focus.
- **No uppercase button text.** Avalonia has no text-transform; the tracking and weight are
  applied, the casing is not.
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
- **No spacing utility classes.** The 4px scale exists as values; `pa-4`-style generated classes
  do not.
- **High contrast does not reach the ring colour.** Every raised surface is outlined there rather
  than shadowed, but the outline is a `BoxShadows` string and those cannot carry a
  `DynamicResource`, so `#E8E8EE` is fixed. An app whose high-contrast palette uses a different
  line colour gets that colour on its borders and this one on its rings.
- **Elevation reads faintly in dark mode.** Material's shadows are black at low alpha, which is
  nearly invisible on a dark ground; MudBlazor has the same problem and this theme reproduces it
  rather than inventing a lighter shadow. Use `FiliSurfaceBrush` against `FiliBackgroundGrayBrush`
  to separate surfaces in dark, not elevation alone.
