# CLAUDE.md

Guidance for Claude Code working in this repository. The workspace `CLAUDE.md` one level up
applies too; this file wins where they differ. It holds the rules; the stories behind them live in
[`docs/traps.md`](docs/traps.md), and that page is worth reading before any non-trivial change.

## What this is

**The project is `Fili.Theme.MudAvalonia`; its NuGet id is `Fili.MudAvalonia.Theme`.** It was
renamed after 0.2.0: everything carries the new name except the package id, because a nuget.org id
is permanent and that one has downloads. Never "fix" `<PackageId>` or the nuget.org links to match
the project name.

A **design-token theme for Avalonia 12**, not a component library: resource dictionaries, style
classes, `ControlTheme`s where a Material look cannot be faked without one, and a gallery. Read
`README.md` first: install, the published class list, the tokens and fonts. Per-control usage is
`docs/controls.md`, the fork's rationale `docs/the-base.md`, and the gaps `docs/known-gaps.md`.

- **The docs are part of the change, not a follow-up.** Adding, renaming or dropping a class or
  token updates its row in `docs/mudblazor-parity.md` in the same commit.
- **The public API is ONE TYPE:** `FiliThemeVariants`, the high-contrast `ThemeVariant`, public
  only because a custom variant is unreachable from XAML except through `x:Static`. Anything else
  public is a design change, and belongs in a separate package.
- **`internal` helpers only where XAML genuinely cannot express something** (compiled XAML can
  construct them, so nothing leaks): `FactorConverter`, `ErrorMessageConverter`,
  `TickPositionsConverter`, `BadgeContentConverter` and `StripedRowConverter` in `Converters/`;
  `OptionalStyleInclude`, a `StyleInclude` that loads only when the app has the package;
  `SlidingIndicator`, the tabs' slide; `FieldClasses`, the text field's classes passed to the
  TextBox inside an AutoCompleteBox or NumericUpDown; `MenuIconGutter`, a menu's shared icon
  column; `TabBarScroll`, the tab bar's scroll buttons; and `DataGridLastColumn` and
  `GroupIndentConverter`,
  which name DataGrid types and so live in `Themes/Optional/DataGrid/`.
- **Retemplating an Avalonia control is in scope; inventing one is not.** A MudBlazor component
  that is a *look* on an existing control is a class (`Button.icon`, `Button.chip`, `Button.fab`,
  `Border.alert`, `Border.avatar`, `Border.card`, `Border.skeleton`,
  `HeaderedContentControl.badge`). A date picker or dialog service belongs in a separate
  `Fili.Theme.MudAvalonia.*` package.
- **One package, one dependency: `Avalonia`.** `Avalonia.Controls.DataGrid` is built against
  (`PrivateAssets="all"`) and not passed on; in an app that references it, its controls are
  themed by type with nothing to opt into. **Its types are named only under
  `Themes/Optional/DataGrid/`**, which FiliTheme.axaml's `OptionalStyleInclude` loads by URI
  once the assembly is there at its `MinimumVersion` (an older one is skipped with a warning).
  A DataGrid selector anywhere else takes the theme down at startup in every app without the
  package; `OptionalBoundaryTests` names the line, `NoDataGridTests` is that app. Another
  control package joins the same way: a folder under `Themes/Optional/`, one include line,
  `MinimumVersion` equal to its version in `Directory.Packages.props`.

## The one rule

**A missing or misspelt resource key is silent.** Avalonia resolves it to nothing and leaves the
previous value in place; a `StaticResource` where a `DynamicResource` belonged freezes the light
value. So:

- Every reference to a token uses `{DynamicResource}`. No exceptions in this repo.
- **Every new token gets an entry in `ResourceResolutionTests`** in the same change (light and
  dark), and `HighContrastTests` covers the third variant and asserts each token differs from dark.

## Token provenance

**Transcribe, never eyeball.** Re-read the source rather than sampling a screenshot, and say in a
comment which file a new value came from.

- **Palette, shadows, type, layout:** MudBlazor `src/MudBlazor/Themes/Models/` (`Palette.cs`,
  `PaletteDark.cs`, `Shadow.cs`, `Typography.cs`, `LayoutProperties.cs`), with `Colors.*` resolved
  through `src/MudBlazor/Colors/Colors.cs`.
- **Control metrics:** `src/MudBlazor/Styles/components/*.scss` AND the component's `.razor.cs`
  for parameter defaults, which are as load-bearing and easier to get wrong
  (`MudProgressLinear.Size` is `Small`, `MudButton.Variant` is `Text`, `MudLink.Underline` is
  `Hover`).
- **Icon glyphs:** `src/MudBlazor/Icons/Material/Filled.cs`, never hand-drawn. Drop the
  `M0 0h24v24H0z` spacer (a painted square in a filled `StreamGeometry`); keep 24x24 bounds with
  `Stretch="None"`.
- **Conversions, applied consistently:** CSS `rgba()` alpha folds into `#AARRGGBB` (table in
  `Themes/Palette.axaml`); `line-height` ratios and `em` letter-spacing become absolute pixels
  (`1.43` on 14px is `LineHeight="20"`).

## Traps

One line each; [`docs/traps.md`](docs/traps.md) has the full account and the test that pins each.

**Styles, priority and selectors**

- A colour class is only a colour: `primary` alone is a primary TEXT button, `filled primary` the
  filled one; on a `TextBlock` it is MudText's `Color`.
- Most MudBlazor components default to `Color.Default`, which is NOT primary - a checked checkbox
  with no class is grey. Only MudSlider and MudLink default to primary.
- Control themes are keyed by TYPE (declared as `Fili*`, aliased at the file's foot); a class only
  ever names a variant.
- When a control gets a ControlTheme, delete it from the blanket styles in `FiliTheme.axaml`.
- **A Style always outranks a ControlTheme setter** - the most likely way to break this package.
- A Style overrides a value set inline in a template ONLY IF its selector has an activator; a
  `{Binding}` in a template binds at LocalValue and beats every style.
- A `/template/` style from a parent's theme loses to the targeted control's own theme for any
  property that theme sets (the snackbar's close button): colour the parent it inherits from.
- A selector may cross only ONE `/template/` boundary; two throw at load and take other themes
  down with them.
- A ControlTheme may not hold a child or descendant selector (`^.hover TreeViewItem`): it throws
  when the theme is applied. A rule that reaches from a control to its items is a Style in
  FiliTheme.axaml.
- Every `TemplatedControl` clips to its bounds: a template that draws past its edge (a shadow, a
  floated label, a badge) sets `ClipToBounds="False"`. And never clip a `Border` that has a shadow.
- An optional package's types live only in its `Themes/Optional/` folder; one anywhere else breaks
  every app without the package, and the main suite cannot see it.
- RTL needs nothing from a template - do not "fix" hardcoded `Left`/`Right`. A glyph that must not
  mirror opts out.

**Tokens, resources and variants**

- The primary differs between variants: `#594AE2` light, `#776BE7` dark.
- Base type is 14px, not 16.
- `Button.axaml` and the marked regions elsewhere are GENERATED by `ThemeColourGenerator`; change
  the generator and regenerate, never one colour's block by hand.
- `DynamicResource` is not type-checked: a `*Color` token bound to a `Background` renders nothing.
  Every colour token has a paired `*Brush`.
- A dictionary's own entries are found BEFORE its `ThemeDictionaries`, so a per-variant value
  needs every variant to declare it.
- `x:Key="HighContrast"` is not a theme variant; reach it through `x:Static FiliThemeVariants`.
- High contrast inherits from Dark silently; `EveryPaletteTokenHasItsOwnValue` is what catches a
  missing token.
- Every brush is one shared application-level object: set the variant on the `Application`, not a
  window.
- Elevation varies only in high contrast, where it becomes a 1px ring.

**Templates and controls**

- `Button` has no `BoxShadow`; only `Border` does.
- A `PathIcon` stretches a glyph's own bounds to its box; a theme glyph is a `Path` at 24 with
  `Stretch="None"`.
- A style cannot set a XAML-declared list's `SelectionMode`: it is pinned as a local value at
  `EndInit`. `multi-selection` is the look; the app sets the mode.
- Every glyph is a `DynamicResource`, so a key in a control's `Resources` swaps its icon; a
  `StaticResource` glyph takes that away.
- A forked template's part that needs another variant gets it from a `/template/` style setting
  `Theme` and that variant's class values (a style cannot add a class), never by editing the
  fork: `DrawerPage`'s pane toggles.
- `TextPresenter` has no `Foreground` property; set it on the `TextBox`.
- Avalonia's transform parser has no `%` unit, and fails at runtime; use `FactorConverter`.
- `:empty` on an `ItemsControl` means no items, not no selection.
- A property animated between gradients must REST on a gradient too, or the animation throws
  `InvalidCastException` when it starts.
- A `DataValidationErrors.Errors` entry is usually an `Exception`; show it through
  `ErrorMessageConverter`.
- A template part works only under the exact `[TemplatePart]` name its control looks for; any
  other name is silently ignored (the DataGrid's group row and `PART_RowHeader`).

**Writing tests**

- Every test body runs inside `UiThread.RunAsync`; one that does not poisons the headless session.
- Re-focusing a focused control does not revisit `:focus-visible`; use two controls.
- Transitioned properties cannot be asserted synchronously; assert a non-transitioned property of
  the same state, or read an animation's start before the dispatcher runs and its end with
  `GetBaseValue`.
- Load the theme assembly before naming it in an `avares://` URI built in code: since 12.1.3 the
  name resolves by prefix to a LOADED assembly, and the test assembly matches first.
- Roboto's static instances use legacy family names; assert with `StartsWith`. Do not swap in the
  variable font.

**The browser build and pixel frames**

- A browser build has no system fonts; the gallery embeds its own fallbacks.
- Avalonia's browser host swallows every key; `wwwroot/main.js` passes browser shortcuts through.
- `UseHeadlessDrawing` defaults to a no-op renderer; `GalleryFrames.Configure` turns it off.
- Indeterminate, striped and pulsing-skeleton regions are masked, located from the live tree.
- A one-shot animation is waited out (`SettleTime`), never masked.
- A frame must not depend on the locale; `GalleryFrames.Render` pins the invariant culture.
- Anything loaded by URI at runtime is invisible to trimming: `OptionalStyleInclude` keeps the
  compiled XAML with a `DynamicDependency`, or the browser gallery and trimmed apps fail at startup.
- Capture with `RenderTargetBitmap`, never `CaptureRenderedFrame`.

## The base is forked, not depended on

`Themes/Base/` holds 79 templates forked verbatim from Avalonia Simple at tag 12.1.3, repaletted by
`Themes/Base/Accents.axaml`. There is no `Avalonia.Themes.*` reference in the library.

- **Do not restyle a forked template.** Byte-faithful forks make an upgrade a re-download and a
  diff. Change a look through `Accents.axaml` or a hand-written theme in `Themes/Controls/`, in
  that order of preference. `FORK.md` has the procedure.
- **Order is load-bearing:** `FiliBaseTheme` first, then `FiliTheme`.
- **Coverage is pinned** by `StandaloneReadinessTests`.
- **No hand-written theme for a control MudBlazor has no counterpart for.**

[`docs/traps.md`](docs/traps.md#the-fork-in-detail) has the rest.

## Layout and conventions

Standard workspace shape (see the workspace CLAUDE.md). Repo-specific: the Avalonia version
(**12.1.3**) is the tag `Themes/Base/` is forked from, so the two move together (`FORK.md`).

`global.json` pins the SDK and opts `dotnet test` into Microsoft.Testing.Platform, which xunit v3
needs on SDK 10.

## Commands

```powershell
dotnet workload install wasm-tools   # once - the solution includes the browser gallery
dotnet build Fili.Theme.MudAvalonia.sln
dotnet test  Fili.Theme.MudAvalonia.sln
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Desktop
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Browser     # needs the wasm-tools workload

# Accept new pixel baselines, after looking at the diff image the failure printed:
$env:FILI_PIXEL_BASELINES = "accept"; dotnet test tst/Fili.Theme.MudAvalonia.PixelTests

# Button.axaml and the marked regions are generated - edit ThemeColourGenerator, then rewrite them:
$env:FILI_REGENERATE = "1"; dotnet test tst/Fili.Theme.MudAvalonia.UnitTests --filter-class "*GeneratedThemeTests"
```

**Releasing is a tag, and the tag is the human's to push.** `release.yml` runs on `v*` tags,
reruns both suites, refuses a tag that differs from `<Version>` in `Directory.Build.props`, then
publishes to nuget.org via trusted publishing and creates the GitHub release. A nuget.org version
can never be replaced: bump `<Version>` in a reviewed commit, add its row to `docs/versions.md`
(plus an "Upgrading" section when existing markup changes look), and leave the tag to the human.
Work since the last release is kept in an *(unreleased)* row and its Upgrading section as it
lands; at the bump only the marker goes.

**`docs/testing.md` describes every test class and workflow.** It and the comment in `build.yml`
carry the unit-test total, so a change that adds tests updates both.
