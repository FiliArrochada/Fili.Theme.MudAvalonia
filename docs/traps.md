# Traps

What has bitten this theme, why, and the rule each one left behind. `CLAUDE.md` carries the rules
in one line apiece; this page is the full account, kept because the reasons are what stop the same
mistake coming back in a different costume. Every entry names the test that pins it where there is
one.

## Styles, priority and selectors

- **A colour class is only a colour, exactly as `Color` is in MudBlazor - on every control.**
  `primary` alone is a primary TEXT button; `filled primary` is the filled one. `SplitButton` takes
  the same classes (MudButtonGroup's Variant x Color x Size), and on a `TextBlock` a colour class
  is MudText's `Color` - so `secondary` is the PINK secondary colour, and grey supporting text is
  `Foreground="{DynamicResource FiliTextSecondaryBrush}"`. `text`/`outlined`/`filled` switch the
  ControlTheme (FiliTheme.axaml); colour and size are nested `^.primary` / `^.small` styles inside
  each theme.
- **Most MudBlazor components default to `Color.Default`, and Color.Default is NOT primary.**
  MudButton, MudMenu, MudButtonGroup, MudCheckBox, MudRadio and MudSwitch all default to it, and
  it means text-primary text or the grey `action-default` icon colour - a checked checkbox with no
  class is grey. Only MudSlider and MudLink default to primary. Primary
  everywhere is MATERIAL's default and this theme carried it until each control was read against
  its `.razor.cs`; check the component's parameter default before assuming one.
- **Control themes are keyed by TYPE. A class only ever names a variant.** Every theme is declared
  under a `Fili*` name for the inventory, then aliased at the bottom of its file:

  ```xml
  <ControlTheme x:Key="{x:Type TextBox}" TargetType="TextBox" BasedOn="{StaticResource FiliStandardTextBox}" />
  ```

  This was the opposite rule while the package layered over Fluent, because overriding a default
  `ControlTheme` reaches into every control composed out of that type — a `TextBox` inside
  `NumericUpDown`, a `Button` inside `ButtonSpinner`. Once the base became a fork this repo owns,
  that stopped being a hazard and became the point: those embedded controls *should* look like
  this theme. The `fili` marker classes were deleted with it. **Do not reintroduce a class that
  means "please theme me".**

  **Class names are MudBlazor's vocabulary** (`primary` = Color.Primary, `outlined`/`filled`/`text`
  = Variant.*, `small`/`medium`/`large` = Size.*), deliberately NOT namespaced — API familiarity is
  the point. So they collide with any app already using those words, and the app's own setters win
  for the properties it declares while every property it does *not* declare leaks through from the
  theme. Grep an adopter for `Classes="` first.
- **When a control gets a ControlTheme, delete it from the blanket styles in `FiliTheme.axaml`.**
  A `Style` outranks a `ControlTheme` setter, so a leftover blanket `CornerRadius` silently
  overrides the template's. The remaining blanket selector is the pickers, which are still forked.
- **A Style always outranks a ControlTheme setter.** This has bitten three times, in different
  costumes, and it is the single most likely way to break this package:
  - A blanket `Selector="TextBlock"` setting `Foreground` also matched the `TextBlock` a
    `ContentPresenter` makes for button content, repainting white-on-primary text body-grey.
    Fixed by putting inheritable defaults (`FontFamily`, `FontSize`, `Foreground`) on
    `Window, UserControl` and leaving only `LineHeight` and `LetterSpacing` on `TextBlock`.
  - A blanket input style setting `CornerRadius` flattened the filled field's top-only
    `4,4,0,0`. Fixed with `:not(.filled):not(.outlined)` on that selector.
  - `Border.appbar TextBlock` painted app-bar text white - and, because a descendant selector
    walks the LOGICAL tree, also every item of a ComboBox on the bar, which renders in a popup on
    a white surface. White on white. Fixed by setting `TextElement.Foreground` on the bar
    instead, so the colour is inherited and loses to any control's own.

  The rule that falls out: **any blanket style that sets a property a ControlTheme also sets needs
  a `:not()` guard, or it must move to the container as an inherited value.**
  `ButtonContentKeepsItsContrastForeground`, `FilledTextFieldKeepsTopOnlyRounding` and
  `AppBarTests` pin all three. A descendant selector reaching into popups is the specific thing
  to suspect whenever a container's style shows up somewhere it should not.
- **A Style overrides a value set inline in a template ONLY IF the selector has an activator.**
  The priority order is `Animation < LocalValue < StyleTrigger < Template < Style`, lower binding
  harder. A value written inside a `ControlTemplate` binds at **Template**, which OUTRANKS a
  plain `Style` — so `SplitView /template/ Rectangle#HCPaneBorder { Fill: ... }` does nothing at
  all, silently, and reads exactly like a selector that failed to match. Add any pseudo-class and
  the same setter binds at **StyleTrigger**, which wins. This is why the floating labels work:
  they set a resting `RenderTransform` inline in the template and every style that moves it is on
  a `:focus`. It is also why the high-contrast drawer edge is selected through the four
  `DisplayMode` pseudo-classes rather than on the bare type. Pinned by
  `TheDrawerGainsAnEdgeInHighContrastOnly`.

  **It also applies between a theme and the themes `BasedOn` it.** A variant theme's plain
  `^ /template/ X#PART_Y` style is not an activator either, so it cannot move anything the base
  template wrote inline. Filled and outlined text fields shipped with NONE of their 12px / 14px
  side padding for exactly this reason, while their `.dense` styles, which do carry an
  activator, applied fine. The rule: **a value a variant theme changes is set by a style in the base
  theme, never inline in the template.** `VariantsPadTheirContentFromTheSide` pins it.

  And a `{Binding}` in a template is stronger still: it binds at **LocalValue**, which beats
  every style, activator or not. So a part whose `IsVisible` a style must be able to switch off
  cannot also bind it - put the binding on a wrapping `Panel` instead, as the helper line does.
- **A selector may cross only ONE `/template/` boundary.** Two hops throw
  `InvalidOperationException: ControlTemplate styles cannot contain multiple template selectors`
  when the theme is first instantiated — it compiles fine, and because the throw happens while the
  merged dictionary is being built, it takes *other* themes down with it. That is worth knowing on
  its own: a single bad selector in `Slider.axaml` made unrelated themes fail to resolve and a
  Button style silently stop applying, which looked like three separate bugs. When a parent must
  reach a grandchild part, set a property on the child and have the child's template bind to it —
  `Slider` lights the `Thumb`'s halo through `BorderBrush` for exactly this reason.
- **A ControlTheme may not hold a child or descendant selector.** MudTreeView's `Hover` tints a
  row only when the TREE has it, so the first attempt put `^.hover TreeViewItem ...` in the
  tree's ControlTheme. It compiles, then throws `InvalidOperationException: ControlTheme style may
  not directly contain a child or descendent selector` the first time a tree is themed. A rule
  that reaches from a control to its items is a plain Style in `FiliTheme.axaml`, where it also
  outranks the item's own ControlTheme, so it says `:not(:selected)` rather than relying on order.
  `TreeViewTests` is the one that caught it.
- **A class cannot be passed down a template by a style.** A setter cannot add a class, and the
  options of a field inside an `AutoCompleteBox` or `NumericUpDown` are classes its own theme
  reads two templates down. `FieldClasses` mirrors them from the outer control onto the inner
  `TextBox`, and keeps mirroring, because an app flips `error` as validation runs.
- **A template's style loses to the theme of the control it targets.** The snackbar's close
  button is a `Button.icon` in the card's template, and `^ /template/ Button#PART_CloseButton`
  setting its `Foreground` did nothing: the button's own theme sets `Foreground` in its `^.icon`
  style, and that won. The button is `inherit` instead, and the card colours the `Panel` round it,
  which has no theme of its own. Reach into a template for a property its control's theme leaves
  alone, or colour the parent it inherits from. `TheCloseButtonClosesTheCard` caught it.
- **Every `TemplatedControl` clips to its bounds.** Avalonia 12 overrides `ClipToBounds` to
  true for the whole family, so anything a template draws past the control's edge is cut off,
  and cannot be clicked either. It bit three times. The outlined label of a `TextBox` and a
  `ComboBox` floats onto the stroke, 6px above the field as MudBlazor's `translate(14px, -6px)`
  puts it, and lost its top and its mask - since the first release, because nothing but an eye on
  the gallery could see it; both base themes set `ClipToBounds="False"`, and the select's
  `PART_ContentPresenter` clips itself instead, so a long item still stops at the chevron. Then
  every resting SHADOW: a filled button's elevation is drawn by the `Border` inside its template,
  past the button's edge, so filled buttons, split and drop-down buttons, the expander and the
  snackbar shipped with their elevation set, asserted by value and never drawn. The pixel
  baselines had captured them flat. And a badge, which hangs past the corner of what it decorates
  by design. `AFloatedOutlinedLabelIsNotClippedByItsField`, `ARaisedControlDoesNotClipItsOwnShadow`
  and `OnlyThePillIsHitTestable` pin them. Anything drawn outside a control's bounds needs the
  same setter - and a test that looks at the control, because a test of the part's value passes
  either way.
- **Clipping a `Border` clips its own shadow.** Avalonia pushes a visual's clip before it draws
  the visual itself, so `ClipToBounds` on a `Border` with a `BoxShadow` deletes the shadow. An
  avatar therefore never clips; the theme clips the `Image` inside it instead.
- **RTL needs nothing from a template, and one thing from a glyph.** Avalonia mirrors a whole
  subtree with a single transform where the flow direction CHANGES, so hardcoded `Left`/`Right`
  alignment, dock sides and grid columns flip on their own — do not "fix" them. What does need
  saying is the opposite: a glyph that must NOT mirror (a checkmark) opts out with
  `FlowDirection="LeftToRight"`, while a directional one (a disclosure arrow) is left alone.
  Note also that `HasMirrorTransform` is false *inside* the subtree, and that measuring the flip
  requires translating a point across the boundary, not to the mirrored panel.

## Tokens, resources and variants

- **The primary colour differs between variants.** `#594AE2` is the *light* primary; dark is
  `#776BE7`. Seeding from the wrong one renders the wrong brand colour everywhere in the only
  variant users see. Pinned by `PrimaryDiffersBetweenVariants`.
- **Base type is 14px, not 16.** First thing to check when a ported screen feels off.
- **Button.axaml, and marked regions in SplitButton.axaml (split and drop-down), Chip, CheckBox,
  RadioButton, ToggleSwitch, Slider, ProgressBar and FiliTheme.axaml (TextBlock, alert, app bar
  and tab bar colours), are GENERATED** by `ThemeColourGenerator` (unit-test project,
  `Generation/`) from one list of eight colours. Change the generator, never one colour's block by
  hand: `GeneratedThemeTests` fails on any difference, and rewrites the files when
  `FILI_REGENERATE=1` is set. Each colour needs five
  tokens - `Fili{C}Color`, `…ContrastTextColor`, `…HoverColor` (the colour at 6%, 35% in high
  contrast), `…DarkenColor` and `…LightenColor`. The last two are MudBlazor's derivation, ported
  as `MudColorPort`, and `PaletteDerivationTests` asserts every one in every variant against it, so
  changing a colour without its shades fails the build. The port must use .NET's own
  `Math.Round`, which scales by 100 in double arithmetic before rounding half-to-even (0.465 ->
  0.46, 0.575 -> 0.57); it is checked against the 14 values mudblazor.com publishes.
  `ButtonMatrixTests` and `SplitButtonMatrixTests` check every cell under a real headless
  `MouseMove`.
- **`DynamicResource` is not type-checked.** Binding a `Background` to a `*Color` token instead of
  a `*Brush` compiles cleanly and renders nothing at runtime. Every colour token therefore has a
  paired brush, and `ResourceResolutionTests` asserts the brush list resolves to `IBrush` — which
  is what catches the paired brush being forgotten. This bit once, on the switch tokens.
- **A dictionary's own entries are found BEFORE its `ThemeDictionaries`.** So a token declared
  once outside the variant dictionaries cannot be given a per-variant value by adding one - the
  outer value wins in every variant, silently. This is why the elevation ladder had to move
  *into* Light and Dark as two identical copies rather than gaining a third dictionary beside it.
  Pinned by `OwnEntriesWinOverThemeDictionaries`.
- **`x:Key="HighContrast"` is not a theme variant.** Avalonia's `ThemeVariant` type converter
  accepts the built-in variants only and throws `NotSupportedException` while the merged
  dictionary is being built - which takes unrelated themes down with it, like the two-hop selector
  above. A custom variant is reachable from XAML only through
  `x:Key="{x:Static theme:FiliThemeVariants.HighContrast}"`.
- **High contrast inherits from Dark, and the fallback is silent.** A token the high-contrast
  dictionary does not declare resolves to the dark one, so it neither fails nor goes missing - it
  comes back subtly wrong, usually as a translucent line over a black ground, which is to say no
  line. Resolution tests therefore prove nothing here: `EveryPaletteTokenHasItsOwnValue` asserts
  each token DIFFERS from dark, which is the only evidence the key was actually written.
- **Every brush in this package is one shared application-level object** whose `Color` is a
  `DynamicResource`. A per-WINDOW `RequestedThemeVariant` therefore does not work: the window gets
  the same brush instance and so the same colour. Set the variant on the `Application`.
- **Elevation does not vary between light and dark**, matching MudBlazor - but it DOES vary
  in high contrast, where every raised level becomes a hard 1px ring instead of a shadow. That is
  what gives the variant its separation without a single control template knowing it exists.

## Templates and controls

- **Avalonia's `PathIcon` stretches a glyph's own bounds to its box.** A Material glyph lives in
  a 24px viewBox with padding round it, which MudIcon draws as is; Avalonia's template scales the
  path's bounds to fill its `Width` and `Height`, so the same glyph came out larger, by an amount
  that depends on the glyph. The snackbar's close X drew at 18px where MudBlazor's is 10.5, and
  every app padded its glyphs back to 24 by hand. The theme's `PathIcon` (Controls/PathIcon.axaml)
  draws the path at its own coordinates on a 24px canvas that scales instead; `IconViewBoxTests`
  pins it.
- **A style cannot set a XAML list's `SelectionMode`.** A `SelectingItemsControl` writes
  `SelectionMode` as a LOCAL value the moment it creates its selection model, and one declared in
  XAML creates it at `EndInit`, before any style applies. `ListBox.multi-selection` setting
  `Multiple, Toggle` worked in every test, where the list was built in code, and did nothing in the
  gallery: the second of two selected rows replaced the first. So `multi-selection` is the look
  only and the app sets `SelectionMode`; `MultiSelectionTests` pins the local value, so the class
  can take the setting back if Avalonia ever stops doing this.
- **A glyph is a `DynamicResource`, or no app can change it.** A `StaticResource` resolves once,
  at load, from the theme; a `DynamicResource` resolves from the control upward, so a key set in
  a control's `Resources` swaps that control's glyph. That is how MudBlazor's icon parameters
  (`CheckedIcon`, `OpenIcon`, `ExpandIcon`, `CloseIcon`) are reached without a public property.
- **A forked template's own control can need a different variant, and a style cannot add a
  class.** `DrawerPage`'s pane toggles are plain `ToggleButton`s with `Background="Transparent"`
  in the template. Once a checked toggle became MudToggleItem's filled one, that local-to-the-
  template value beat the fill and left a white icon on nothing while the pane was open. The
  forked template stays byte-faithful, so a style in `FiliTheme.axaml` reaches the parts through
  `/template/` and sets `Theme` to the icon toggle's, plus the values its `icon` class would have
  added. Drawing the fill over the template's background instead would have filled the drawer
  button and overridden every app's own `Background` too. `DrawerPageTests` pins both sides.
  The same control taught a second lesson: its drawer's logical parent is the `DrawerPage`, not
  the `SplitView` inside its template, so a `SplitView … TreeView.nav` selector never reaches it.
  The mini drawer's rules name `DrawerPage[IsOpen=False][DisplayMode=CompactInline]` as well.
- **`Button` has no `BoxShadow`.** Only `Border` does, which is why the raised button is a
  `ControlTheme` with a `Border` in its template rather than a handful of setters.
- **`TextPresenter` has no `Foreground` AvaloniaProperty.** It reads the inherited
  `TextElement.Foreground` from its parent, so a setter targeting `PART_TextPresenter` fails to
  compile with `AVLN3000`. Set `Foreground` on the `TextBox` itself instead.
- **Avalonia's transform parser has no `%` unit.** `translateX(-35%)` throws
  `FormatException: Invalid unit: %` — **at runtime**, when the template is instantiated, because
  a transform string in a key frame is parsed lazily and compiles cleanly either way. MudBlazor
  sizes several things as a percentage of their container, so the conversion is a binding to a
  pixel dimension the control publishes, times a constant, through `FactorConverter`.
- **`:empty` on an `ItemsControl` means NO ITEMS, not "no selection".** `ComboBox:not(:empty)`
  reads like "has a selection" and is true for every populated select, which floated every label
  permanently. There is no pseudo-class for a selection; use `ObjectConverters.IsNull` on
  `SelectionBoxItem` and swap two elements.
- **A `DataValidationErrors.Errors` entry is usually an `Exception`**, so binding straight to it
  renders `"System.InvalidOperationException: the message"`. `ErrorMessageConverter` exists for
  exactly that.
- **A binding through a null logs an error in the app, not here.** The counter bound
  `$parent[TextBox].Text.Length`: a `FallbackValue` showed 0 for a null `Text`, but Avalonia logged
  a binding error first, for every `TextBox` an app had not filled yet - the counter is hidden, not
  unbound. Nothing in this repository failed; an adopting app that fails its view tests on a
  binding error carried an exception for it from 0.6 to 0.9.2. The counter now binds `Text`
  through `TextLengthConverter`, and `BindingErrorTests` shows the common controls with a sink
  that fails on any binding error.
- **A checkbox in a table cell is the compact one.** `_table.scss` gives `.mud-table-cell
  .mud-checkbox` a -4px margin and its icon button 4px of padding, so a selection column does not
  stretch the row; without it, the 48px circle made a 57px row 81. The circle's 48 is set inline
  in the template, so the cell's rule for the medium size needs an activator
  (`:not(.small):not(.large)`) to win, as every inline-template override does.
- **A control inside a template is a Tab stop unless it says otherwise.** The slider's two rail
  halves are `RepeatButton`s, focusable by default; the base's theme for them sets
  `Focusable="False"`, the hand-written one did not, and every slider was three Tab stops, two of
  them a 2px rail. Nothing looks wrong until someone uses the keyboard. `TabStopTests` counts the
  stops in the common controls: one at most, the control or the one part that stands for it.
- **The base draws a focus rectangle; this theme does not.** The forked `AdornerLayer` gives every
  focused control Simple's dashed box, and nothing had turned it off, so the state layer - the
  only affordance MudBlazor has, `docs/controls.md` says - had a dotted outline over it. A Style
  in FiliTheme.axaml sets `DefaultFocusAdorner` to null; an app's own `FocusAdorner` still draws.
- **A popup is the root of its own tree.** The theme set the font on `Window` and `UserControl`,
  and a popup's host is neither: it took the base's `ContentControlThemeFontFamily`, an alias
  `Accents.axaml` resolves once inside the theme, so always Roboto. An app that set its own
  `FiliFontFamily` got it everywhere but its menus, drop-downs and calendars. The hosts
  (`PopupRoot`, `OverlayPopupHost`, `EmbeddableControlRoot`) are styled beside the window.
- **A `MenuFlyout` binds its own `ItemContainerTheme` over its presenter's.** Its presenter's
  theme set the dropdown row as the container theme, and the flyout's binding, null unless the
  app sets one, beat it, so every flyout row was the base's: 24px, no state layer. A submenu's own
  rows had no container theme either. `MenuItem` is therefore themed by TYPE as the dropdown row;
  only a `Menu` names the strip item for its own items.
- **Access keys are the presenter's, not the control's.** The base sets `RecognizesAccessKey` on
  each content presenter and gives a menu header an `AccessText`; a hand-written template that
  does neither shows "_Save" with its underscore and has no Alt shortcut. One Style in
  FiliTheme.axaml sets it on every presenter the base sets it on - `Button.axaml` is generated -
  and the menu templates carry the `AccessText`. `AccessKeyTests` covers each.
- **A text field's label is placed relative to the field's own height.** Centred in the field and
  floated a fixed distance from there, it landed on the text of a multi-line field. A multi-line
  field's text starts at the top and its label centres in a band of the single-line height above
  it, the inside of the stroke for `outlined`; a single-line field is untouched, because a
  numeric field's spinner makes it 56px and its label is centred with its text. The first line
  starts where a single-line field's text sits (1px down, 4.5 filled, 3.5 outlined): flush with
  the top, a filled field's first line met its label, which a 6px tolerance in the test hid until
  the gallery showed it. The test now compares the gap with a single-line field's.
- **A start adornment floats the label, which a selector cannot see.** MudInput shrinks the label
  for `Adornment.Start`; `InnerLeftContent` is a property a selector cannot test for null, so
  `Adornments` keeps it as `:start-adornment`. A resting label had been drawn over the
  adornment.
- **A checkbox column's boxes are disabled in most rows.** `DataGridCheckBoxColumn` disables the
  box in every row but the current one, and in every row of a read-only grid; the theme drew a
  disabled box in text-disabled, so the column looked disabled. The grid's styles keep a cell's
  box action-default, as MudDataGrid's read-only checkbox is.
- **A control stretched to its row lays out its template in the extra height.** The switch is
  40px; beside a 48px checkbox its row stretched it, and its template's Auto rows sat at the top.
  Its grid now takes `VerticalContentAlignment`, Center, as MudSwitch's inline-flex centres.
- **A thin bar's text is clipped by the bar.** `ShowProgressText` on a 4px `ProgressBar` showed a
  4px slice of "40%", arranged in the bar and clipped with it (the `TemplatedControl` trap above).
  The bar no longer clips, its bars clip themselves, and the text sits in a panel with a negative
  margin, so it is whole and centred whatever the bar's thickness.
- **A template part works only under the exact name the control looks for.** The DataGrid's
  group row finds its row header as `PART_RowHeader`, to hide it with the grid's other row
  headers; the template named it `RowHeader`, as Avalonia's own DataGrid theme does, so it was
  never found, always showed, and its 1px edge pushed every group title right. Nothing fails or
  logs. `ANestedGroupHeaderIsIndented48PxPerLevel` measures the expander to the pixel, which is
  what caught it; check the `[TemplatePart]` names in the control's source when writing one.

- **A gradient key frame needs a gradient resting value.** Animating `Background` between two
  `LinearGradientBrush`es makes Avalonia pick its gradient animator, which casts the property's
  own value to a gradient when the animation starts - so over a `SolidColorBrush` it throws
  `InvalidCastException`, and in an app that is a crash on the first frame. Nothing fails until
  the animation actually runs: it compiles, the resources resolve, and a headless test whose clock
  never advances passes. The skeleton `wave` therefore rests on its own start gradient, whose band
  is wholly off the edge and so draws the plain skeleton.
- **The compositor clips a visual to a RECTANGLE.** In 12.1.3's source a composition visual's clip
  is its bounds when `ClipToBounds` is set, and only the immediate renderer - the one
  `RenderTargetBitmap` uses - reads a `Border`'s radius, so a pixel frame can look right where a
  window does not. The ripple sets a rounded `Clip` geometry on its layer instead
  (`Ripple.RoundedRect`).

## Writing tests

- **Re-focusing an already-focused control does not revisit `:focus-visible`.** A test that
  focuses one control with `NavigationMethod.Pointer` and then with `Tab` sees no tint and passes
  for the wrong reason. Use two controls, one per navigation method.
- **Every test body runs inside `UiThread.RunAsync`, with no exceptions.** Constructing almost any
  Avalonia object — a `FluentTheme` included — touches the compositor, and doing that from the
  xunit thread throws *"The calling thread cannot access this object"* and then **poisons the
  shared headless session**, so every later test in the assembly fails too. A plain `[Fact]` that
  forgets the wrapper does not fail alone; it fails the suite.
- **Transitioned properties cannot be asserted synchronously.** A value carrying a
  `TransformOperationsTransition` or `BrushTransition` is still interpolating when a test reads
  it, and compares equal to its resting value. Assert a non-transitioned property of the same
  state instead — `CheckedToggleSwitchTintsItsTrack` uses track opacity for exactly this reason.
  When the motion IS the point, as with the tabs' sliding indicator, read the starting value
  straight after the change, before `Dispatcher.UIThread.RunJobs()`: the animation moves on in
  real time from there, a few pixels per run, and no headless call advances it on demand
  (`ForceRenderTimerTick` does not). Read where it will end with `GetBaseValue`, and the timing
  from its `Transitions`.
- **A key-frame animation cannot be followed to its end frame by frame, either.** Sampled with a
  tick every few milliseconds, a fade stalls part way - at 0.2 on a plain `Border` in a plain
  `Window`, so the stall is the headless platform's, not this theme's - and a single tick after a
  long sleep can be the first the animation sees, which it takes as its start. Each looks exactly
  like a popup stuck part transparent, or invisible. Show that an animation runs by catching a
  value part way, and pin where it ends and how long it takes by reading the `Animation` from the
  styles, as `PopupFadeTests` does.
- **The headless mouse is one device for every window.** A press a test leaves down keeps its
  control captured, and the next test's press - in a new window - goes to that control instead.
  Release every press (`RippleTests` holds one in a `using`). And a `DispatcherTimer` runs on the
  next pass of jobs rather than when it is due, so anything timed - the ripple's .5s hold - is
  read from the code, not waited for.
- **`GetBaseValue` of a property nobody set is empty, and `GetValueOrDefault` makes that 0**, not
  the property's default: a panel whose `Opacity` was never set reads 0. Read the live value for
  a property that is not transitioning.
- **Roboto's static instances use legacy name tables.** `Roboto-Light` reports family
  `Roboto Light`, not weight 300 of `Roboto`. Avalonia groups them correctly; assert family names
  with `StartsWith`, not `Equal`. And do not swap in the variable `Roboto[wdth,wght].ttf` —
  Avalonia picks a face per weight rather than setting an axis, so Light and Medium would render
  as Regular.

- **An `avares://` name resolves by PREFIX to a loaded assembly** (since Avalonia 12.1.3, in
  `AssemblyDescriptorResolver`). Until the theme assembly is loaded, `avares://Fili.Theme.MudAvalonia/`
  matches `Fili.Theme.MudAvalonia.UnitTests.dll` instead, and a `StyleInclude` built in code throws
  *"No precompiled XAML found"* - which, from `TestApp.Initialize`, fails nearly every test in the
  assembly at once. `TestApp` touches `typeof(FiliThemeVariants).Assembly` first. A compiled
  include in an `.axaml` file is not affected, which is why the gallery and the pixel suite never
  noticed.

## The browser build and fonts

- **A browser build has no system fonts.** Text in any script Roboto and Inter lack — the
  gallery's Arabic, first — renders as nothing there while looking fine on desktop, where the OS
  falls back silently. The gallery embeds static Noto Sans Arabic in its own `Assets/Fonts` and
  `GalleryFonts.WithGalleryFonts()` registers it as a `FontManagerOptions.FontFallbacks` entry for
  the desktop head, the browser head AND `GalleryFrames.Configure` - so the pixel baselines do not
  depend on whichever Arabic face the machine has installed either. New non-Latin text in the
  gallery needs a face added there too; on desktop nothing fails without one, so the checks are
  the published page and a baseline that changes when it should not. Symbols count too: the
  embedded Roboto has no `→` (U+2192), so a caption that used one showed boxes on the published
  page only; it was reworded. Check a new symbol against Roboto's character map before using it.
- **Avalonia's browser host swallows every key.** It calls `preventDefault` on keydown for
  effectively all keys, so on the published gallery Ctrl+F, zoom, reload, print and the dev tools
  did nothing. `wwwroot/main.js` registers a capture listener on `window` before the runtime
  starts and stops browser-only shortcuts there, so Avalonia never sees them and the browser acts.
  Editing keys (Ctrl+A/C/V/X/Y/Z, arrows, Tab) are deliberately left to Avalonia. Verified in
  headless Chrome by wrapping `KeyboardEvent.prototype.preventDefault`; the find bar opens but
  finds nothing, because the gallery is drawn on a canvas.

**The gallery has two heads and one view.** `MainView` is the whole UI; `MainWindow` hosts it on
desktop and the browser head sets it as the single view. Put gallery UI in `MainView`, never in
the window, or the GitHub Pages build silently loses it. And never construct a `StyleInclude` in
gallery code: the two in App.axaml compile to code, while one built in C# loads by reflection,
which the browser build's trimming can strip - leaving the page with no base theme at all.

**The theme's own `OptionalStyleInclude` loads by URI too, so it keeps what it loads.** Loading a
URI finds the compiled XAML by reflection, through the type the XAML compiler generates
(`CompiledAvaloniaXaml.!XamlLoader`), and trimming removed it: from 0.7.0 to 0.9.0 the published
gallery threw "No precompiled XAML found" for `DataGridStyles.axaml` at startup and drew nothing
past the splash, as would any trimmed or AOT app with the DataGrid. Every test passed, because
none of them is trimmed. A `DynamicDependency` on `EndInit` keeps that type;
`OptionalBoundaryTests` pins the attribute, and a trimmed app without the DataGrid still publishes
with no warnings. Check the published page, not only the suites, after touching how the theme
loads.

The gallery is the development loop, not a deliverable added at the end. A theme has no surface of
its own, so tune against the gallery rather than against an app.

## Pixel frames

- **`UseHeadlessDrawing` defaults to TRUE, and it is a no-op renderer.** Every capture comes back
  blank, nothing throws, and a pixel suite built on it compares two blank images and passes
  forever. Skia plus `UseHeadlessDrawing = false` is the only configuration that produces a
  frame; `GalleryFrames.Configure` is the single place that says so.
- **An indeterminate `ProgressBar` is not reproducible** - nor a `striped` one, nor a pulsing
  skeleton, all masked the same way. Its band follows a wall-clock animation
  clock, so two runs of the SAME BUILD differ by about eighty pixels. The pixel suite masks that
  rectangle, located from the live tree rather than remembered, and compares everything else
  strictly. Do not answer a flaky frame with a wider tolerance: a budget big enough to absorb an
  animation is big enough to hide a redrawn glyph, everywhere in the frame.
- **A one-shot animation is not reproducible either, until it finishes.** The same wall clock
  drives the snackbar's 0.45s enter fade, and a frame captured at 99% of it fails strictly on
  one machine and passes on another. `GalleryFrames.Render` therefore waits out `SettleTime`
  before capturing. Raise that constant if a longer animation starts on load; do not mask a
  region whose colours the suite is meant to be watching.
- **A style costs every control of the type its selector ends in.** Avalonia cannot know a class
  will never come, so a style ending in `Border.paper` sets up a watcher on every Border in the
  window, and a descendant one watches each Border's ancestors too. A screen of 1,600 ordinary
  controls built in 7.3 seconds here against the forked base's 1.2: nineteen in twenty of its
  Borders are template parts, each paying for 127 Border class styles, and 27 descendant overlay
  styles (`.material-elevation-overlay Border.card`) cost more than all the rest. Template Borders
  are `PartBorder`s now, which those styles pass over by type, and the overlay sets an inherited
  property each surface reads on itself: 2.2 seconds. `StyleCostTests` keeps a plain Border out
  of the templates; measure a new top-level style that ends in a common type before keeping it.
- **A property selector on an inherited value watches every control too.** The 23 descendant
  styles that sized `start-icon` and `end-icon` per host (`Button.small :is(Control).start-icon`)
  were rewritten as three, `:is(Control).start-icon[(theme|IconSlot.StartsSized)=True]` and two
  more, reading what the host set: they cost 0.5 seconds on the benchmark screen, more than the 23
  had. The class check and the property watch land on every control either way. The icon now
  takes its host's values in code, as they arrive (`IconSlot`), and no style reaches it.
- **A `Border` with square corners and a one-sided `BorderThickness` draws no `BoxShadow`.** A
  closed `expansion-panels` group's middle panels - no radius, a rule under them - lost their
  shadow, a notch in the group's edge, and in high contrast their ring. Rounded corners or a
  uniform thickness hide it. The `Expander` keeps the shape and shadow on `PART_Root`, with no
  border, and draws the rule in `PART_Rule` over it; any template that puts a partial border and
  a shadow on one `Border` needs the same split.
- **A transition that moves layout changed the frame's height.** An expanded panel in an
  `expansion-panels` group eases out to its 16px margin over .3s from the moment it joins, and the
  window was sized to the page before that had finished: the light controls frame, rendered first
  in a cold process, came out 2 to 5px taller or shorter from run to run. `GalleryFrames.Render`
  measures the page again after `SettleTime` and resizes before capturing.
- **A frame taller than the cap was cut off at the cap, silently.** The frame grows to the page,
  up to a guard against a runaway measurement; the controls page outgrew the 12,000px guard and
  its new last section was missing from every frame, with nothing failing. The guard is 20,000px
  and a page past it now fails, naming the view.
- **A frame must not depend on the machine's locale.** The palette tab formats contrast ratios
  with the current culture, so baselines recorded on a pt-PT machine said `6,00:1` and the en-US
  CI runner rendered `6.00:1` - three palette frames failed in CI while every other frame passed,
  which is the signature to recognise. `GalleryFrames.Render` pins the invariant culture for the
  capture. Anything else that varies per machine (time zone, current date) needs the same
  treatment before it appears in a baselined view.

**There are three test projects.** `tst/{Name}.NoDataGridTests` is an app without the DataGrid
package, the one place a DataGrid type leaking out of `Themes/Optional/DataGrid/` shows at run
time; `OptionalBoundaryTests` in the unit suite catches the same leak by reading the source.
`tst/{Name}.UnitTests` is the fast one and renders nothing;
`tst/{Name}.PixelTests` renders nine gallery frames with Skia and diffs them against committed
PNGs. The split is deliberate - the pixel suite needs headless drawing turned OFF, which changes
text measurement, and the unit suite should not silently start measuring differently because the
pixel suite needed a different platform.

**CI runs all three, on two runners, and the pixel suite on only one of them.** Linux builds the
whole solution and runs the unit and no-DataGrid tests; Windows builds only the three test projects (everything but
the browser gallery, so it skips the `wasm-tools` install) and also runs the baselines, because
those PNGs were rendered on Windows with Skia. A failing frame uploads the rendered image and the
diff as the `pixel-diffs` artifact - which is why the diff directory is overridable through
`FILI_PIXEL_DIFF_DIR`, the default being a system temp path no artifact upload can reach.

**The hosted Windows runner rasterises identically to a developer machine.** Its failures so
far were the locale (see the trap above) and the capture itself (below). If a frame fails in CI
and nowhere else, compare the uploaded artifact with the committed baseline and look for
something machine-dependent first, then try to reproduce it under load - run the pixel suite
while every core is busy. If the runner ever genuinely rasterises differently, the fix is a
second committed set of baselines per environment, never a wider tolerance.

**A frame is captured with `RenderTargetBitmap`, never `CaptureRenderedFrame`.** The latter
returns the compositor's frame, which is the sum of every partial redraw since the window opened,
and how many there were depends on timing. The antialiased ends of a few pill shapes - a large
slider's rail and knob, a large switch's track - came out up to 36 levels off on about one run in
three on a loaded machine, failed a build and then the v0.2.0 release in CI, and passed on an
idle machine every time. It was first misread as CPU-dependent rasterisation and answered with a
4/255 tolerance, which was the wrong fix and has been reverted. The tell was that it always hit
the same few pixels: a rasteriser difference would touch every rounded edge in the frame.

## The fork in detail

Avalonia has no implicit default theme: a control with no `ControlTheme` in scope has no template
and renders nothing at all. `Themes/Base/` holds 79 templates forked verbatim from Avalonia Simple
at tag 12.1.3, rebased onto this package and repaletted by `Themes/Base/Accents.axaml`. There is
no `Avalonia.Themes.*` reference anywhere in the library.

**Do not restyle a forked template.** Keeping them byte-faithful is what makes an Avalonia upgrade
a re-download and a diff rather than a merge. To change how a control looks, either redefine what
it paints from in `Accents.axaml`, or write a hand-written ControlTheme in `Themes/Controls/`
which wins by include order. `FORK.md` has the procedure.

Two consequences to keep in mind when editing:

- **Order is load-bearing.** `FiliBaseTheme` first, then `FiliTheme`; later styles win. That is
  the only reason the hand-written control themes beat their forked counterparts.
- **The accent is now just a token.** Standalone, `FiliPrimaryColor` flows into the forked
  templates through `ThemeAccentBrush`/`ThemeAccentColor` in `Accents.axaml` — no theme-specific
  API. (Historically it had to go through `FluentTheme.Palettes`, and setting a `SystemAccentColor`
  resource instead failed silently.)

**Nothing in this repo references `Avalonia.Themes.Fluent` any more.** The gallery's substrate
selector, which flipped the forked controls to Fluent for comparison, was removed: it showed a look
the package does not ship and drew Fluent's dark theme under the High contrast label. Do not bring
Fluent back for a comparison - `StandaloneReadinessTests` is the record of what is hand-written.

**Coverage is pinned, not reported.** `StandaloneReadinessTests` asserts the exact set of
hand-written themes — **47 of the 95 templated types** — plus the count of both. The other 48 wear
forked templates, and `SimpleBridgeTests` asserts the ~96 contract keys those paint from still
resolve in both variants. Adding a theme means adding its `Fili*` key AND its target type to the
first test in the same change; that is what keeps the number honest and turns an Avalonia version
that adds control types into a failing build rather than a silent gap.

**There are THREE ways to change how a control looks, and reaching for the heaviest one by
default is the mistake.** In order of increasing cost: a `Style` in `FiliTheme.axaml` when the
shape is right and only values are wrong (`SplitView`/MudDrawer is the worked example — a Style
outranks a ControlTheme setter, which is the trap above used deliberately; its own theme only adds
the part its elevation needs); `Accents.axaml` when
the control paints from one of Simple's shared keys; a hand-written `ControlTheme` when the shape
itself is wrong.

**`Accents.axaml` cannot override a key a forked control file declares for itself.** It is merged
BEFORE the control dictionaries, and in a merged `ResourceDictionary` the later entry wins. So
`SplitViewOpenPaneThemeLength`, declared inside `Base/Controls/SplitView.axaml`, is out of its
reach — a Style setting the property directly is the way.

**Do not add a hand-written theme for a control MudBlazor has no counterpart for.** `PopupRoot`,
`AdornerLayer`, `TextSelectionHandle`, `ManagedFileChooser` and roughly 20 others exist because Avalonia needs them, not because a design system has an
opinion about them. They stay forked permanently, and that is the fork earning its keep rather
than a gap to close.

The two deliberate deletions worth not re-adding: **`ScrollViewer`** (structural — get a part name
wrong and it lays out perfectly and does not scroll) and **`Window`** (`VisualLayerManager` and
`PART_TransparencyFallback` are load-bearing and invisible until missing). Both were written while
layering and both went when the fork made them redundant.
