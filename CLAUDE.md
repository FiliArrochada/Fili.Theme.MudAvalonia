# CLAUDE.md

Guidance for Claude Code working in this repository. The workspace `CLAUDE.md` one level up
applies too; this file wins where they differ. It holds the rules; the stories behind them, and
many more traps than are listed here, live in [`docs/traps.md`](docs/traps.md): read it before any
non-trivial change.

## What this is

**The project is `Fili.Theme.MudAvalonia`; its NuGet id is `Fili.MudAvalonia.Theme`.** A
nuget.org id is permanent and that one has downloads: never "fix" `<PackageId>` or the nuget.org
links to match the project name.

A **design-token theme for Avalonia 12**, not a component library: resource dictionaries, style
classes, `ControlTheme`s where a Material look cannot be faked without one, and a gallery that is
the docs site (`src/Fili.Theme.MudAvalonia.Gallery/Docs/Pages/`, listed in `Docs/DocCatalog.cs`).
`README.md` is the install and class list; `docs/` is for contributors.

- **Never name another app in this repository**, commit messages included. A bug an app found is
  "found by an adopting app".
- **The docs are part of the change, not a follow-up.** A class, token or glyph key added, renamed
  or dropped updates, in the same commit, its `<docs:ParityRow>` on its component's page and the
  page's text and examples; a token, the Theming or Typography page's table; a glyph, the Icons
  page's. A component newly drawn or given a page updates `Docs/ComponentIndex.cs`. A gap's reason
  goes in its row's `Note`. A page's header names its Avalonia controls in `AvaloniaControls`; a
  new one goes into `Docs/AvaloniaDocs.cs` with a path from docs.avaloniaui.net's sitemap, never
  a guessed one.
- **Classes, resource keys and `FiliThemeVariants` are the published interface**
  (`docs/compatibility.md`). A new token or glyph key goes into `docs/published-keys.md` in the
  same change (`PublishedInterfaceTests`), and a new key or class is appended to the frozen
  `published-baseline.txt` with `FILI_REGENERATE=1` (`PublishedBaselineTests`); none is renamed or
  removed before 2.0, only aliased, and no entry leaves the baseline.
- **The README is the nuget.org page too.** Keep its links relative; the pack rewrites them and
  fails on one that leads nowhere (`PackageReadmeTests`).
- **The public API is ONE TYPE:** `FiliThemeVariants`. Anything else public is a design change
  and belongs in a separate package.
- **`internal` helpers only where XAML genuinely cannot express something.** Each is listed, with
  why, in [`docs/the-base.md`](docs/the-base.md#the-internal-helpers); a new one goes there too.
- **Retemplating an Avalonia control is in scope; inventing one is not.** A MudBlazor component
  that is a look on an existing control is a class (`Button.fab`, `Border.card`) or a template
  (MudDatePicker on `CalendarDatePicker`). A control Avalonia lacks belongs in a separate package.
- **One package, one dependency: `Avalonia`.** `Avalonia.Controls.DataGrid` is built against and
  not passed on; **its types are named only under `Themes/Optional/DataGrid/`**, which an
  `OptionalStyleInclude` loads when the app has a version in range. A DataGrid selector anywhere
  else takes the theme down in every app without the package (`OptionalBoundaryTests`,
  `NoDataGridTests`). Another optional package joins the same way.

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

- **Palette, shadows, type, layout:** MudBlazor `src/MudBlazor/Themes/Models/`, with `Colors.*`
  resolved through `src/MudBlazor/Colors/Colors.cs`.
- **Control metrics:** `src/MudBlazor/Styles/components/*.scss` (the drawer's is under
  `Styles/layout/`) AND the component's `.razor.cs` for parameter defaults, which are as
  load-bearing and easier to get wrong (`MudProgressLinear.Size` is `Small`).
- **Icon glyphs:** `src/MudBlazor/Icons/Material/Filled.cs`, never hand-drawn. Drop the
  `M0 0h24v24H0z` spacer; keep 24x24 bounds with `Stretch="None"`.
- **Conversions:** CSS `rgba()` alpha folds into `#AARRGGBB`; `line-height` ratios and `em`
  letter-spacing become absolute pixels (`1.43` on 14px is `LineHeight="20"`).

## Traps

The ones a change most often trips; [`docs/traps.md`](docs/traps.md) has these in full, the test
that pins each, and the control-specific ones.

**Styles, priority and selectors**

- A colour class is only a colour: `primary` alone is a primary TEXT button, `filled primary` the
  filled one. Most MudBlazor components default to `Color.Default`, which is NOT primary.
- Control themes are keyed by TYPE; a class only ever names a variant, and an option that is one
  property of its own control is that property, not a class (`ClassVocabularyTests`). When a
  control gets a ControlTheme, delete it from the blanket styles in `FiliTheme.axaml`.
- **A Style always outranks a ControlTheme setter** - the most likely way to break this package.
- A Style overrides a value set inline in a template ONLY IF its selector has an activator; a
  `{Binding}` in a template binds at LocalValue and beats every style.
- **A top-level style is matched against every control of the type it ends in**, template parts
  included. Never end a selector in a common type or `:is(Control)`: read an inherited property,
  or set it from the host in code (`IconSlot`, `ChildAlignment`). Every Border in a hand-written
  template is a `PartBorder` (`StyleCostTests`).
- Keep a style-cost change only on a timed benchmark: fewer styles once built 55% slower.
- A style's `Theme` setter uses `DynamicResource`; a `StaticResource` builds the theme at load in
  every app.
- A colour class sets `Accent`'s values in ONE style; never a style per colour per part per state.
- A selector may cross only ONE `/template/` boundary, and a ControlTheme may not hold a child or
  descendant selector: both throw, and take other themes down with them.
- A helper's `SetValue` below LocalValue returns an entry: keep it and dispose it to let go.
- Every `TemplatedControl` clips to its bounds: a template that draws past its edge sets
  `ClipToBounds="False"`. Never clip a `Border` that has a shadow.
- RTL needs nothing from a template - do not "fix" hardcoded `Left`/`Right`.

**Tokens, resources and variants**

- `Button.axaml` and the marked regions elsewhere are GENERATED by `ThemeColourGenerator`; change
  the generator and regenerate, never one colour's block by hand.
- `DynamicResource` is not type-checked: a `*Color` token bound to a `Background` renders nothing.
  Every colour token has a paired `*Brush`.
- A dictionary's own entries are found BEFORE its `ThemeDictionaries`, so a per-variant value
  needs every variant to declare it. High contrast inherits from Dark silently
  (`EveryPaletteTokenHasItsOwnValue`).
- `x:Key="HighContrast"` is not a theme variant; reach it through `x:Static FiliThemeVariants`.
- Every brush is declared in each variant's dictionary, written once in Light's and copied by
  `ThemeColourGenerator`, so a window or `ThemeVariantScope` paints in its own variant; a lookup
  naming no variant, or the application's own, finds `FallbackBrushes.axaml` through
  `ApplicationVariantBrushes`, so a switch recolours brushes instead of swapping them; it must
  stay out of `Palette.axaml`'s merged set (a plain entry there shadows every variant dictionary's).

**Templates and controls**

- A theme glyph is a `Path` at 24 with `Stretch="None"`, and always a `DynamicResource`; a
  `PathIcon` stretches a glyph's own bounds to its box.
- A part a control rarely shows is built by a style, not held hidden in its template; what is
  built so is not reached by `/template/` and has no `TemplatedParent`.
- A forked template's part that needs another variant gets it from a `/template/` style, never by
  editing the fork.
- Avalonia's transform parser has no `%` unit and fails at runtime; use `FactorConverter`.
- A property animated between gradients must REST on a gradient too, or the animation throws.
- A template binding through a value that can be null (`Text.Length`) logs an error in every app;
  bind the value and convert (`BindingErrorTests`, `GalleryAuditTests`).
- A control inside a template is a Tab stop unless it sets `Focusable="False"`; a control is one
  stop at most (`TabStopTests`).
- A popup's host roots its own tree and inherits nothing from the window.
- A template part works only under the exact `[TemplatePart]` name its control looks for.

**Writing tests**

- Every test body runs inside `UiThread.RunAsync`; one that does not poisons the headless session.
- The headless mouse is shared by every window: release every press. Headless timers run on the
  next `RunJobs`, not when due.
- Re-focusing a focused control does not revisit `:focus-visible`; use two controls.
- Transitioned properties cannot be asserted synchronously, and a key-frame animation stalls when
  ticked frame by frame: catch a value part way, and read the end from the `Animation`.
- Load the theme assembly before naming it in an `avares://` URI built in code: the name resolves
  by prefix to a LOADED assembly, and the test assembly matches first.

**The browser build and pixel frames**

- Anything loaded by URI at runtime is invisible to trimming: keep its compiled XAML with a
  `DynamicDependency`, or the browser gallery and trimmed apps fail. Desktop tests cannot see it.
- `UseHeadlessDrawing` defaults to a no-op renderer; `GalleryFrames.Configure` turns it off.
  Capture with `RenderTargetBitmap`, never `CaptureRenderedFrame`.

## The base is forked, not depended on

`Themes/Base/` holds Avalonia Simple's templates, forked verbatim at the tag `FORK.md` names and
repaletted by `Themes/Base/Accents.axaml`. `AvaloniaVersion` in `Directory.Packages.props` is that
tag, so the two move together.

- **Do not restyle a forked template.** Change a look through `Accents.axaml` or a hand-written
  theme in `Themes/Controls/`, in that order. `FORK.md` has the upgrade procedure.
- **Order is load-bearing:** `FiliBaseTheme` first, then `FiliTheme`.
- **No hand-written theme for a control MudBlazor has no counterpart for.**

## Commands

`global.json` opts `dotnet test` into Microsoft.Testing.Platform: filter with `--filter-class`.

```powershell
dotnet workload install wasm-tools   # once - the solution includes the browser gallery
dotnet build Fili.Theme.MudAvalonia.slnx
dotnet test  Fili.Theme.MudAvalonia.slnx
dotnet run --project src/Fili.Theme.MudAvalonia.Gallery.Desktop

# Accept new pixel baselines, after looking at the diff image the failure printed:
$env:FILI_PIXEL_BASELINES = "accept"; dotnet test tst/Fili.Theme.MudAvalonia.PixelTests

# Rewrite the generated regions after editing ThemeColourGenerator:
$env:FILI_REGENERATE = "1"; dotnet test tst/Fili.Theme.MudAvalonia.UnitTests --filter-class "*GeneratedThemeTests"

# Time a style-cost change against HEAD, on an idle machine (prints testing.md's table):
dotnet run tools/bench.cs

# The release bump, and the validation baseline once the release is on nuget.org:
dotnet run tools/bump.cs -- <version>
dotnet run tools/bump.cs -- baseline

# docs/testing.md's test counts, after adding tests (CI checks them):
dotnet run tools/test-counts.cs

# The upstreams: what is new, re-forking Avalonia, what a MudBlazor release changed:
dotnet run tools/upstream.cs -- check | avalonia <tag> | mudblazor <tag>
```

Warnings fail the build. There are no lock files: a package version change is
`Directory.Packages.props` alone. A selector change that moves the benchmark screen's style instances moves
`StyleCostTests.StyleInstanceCeiling` with the benchmark's tables beside it.

**Releasing is a tag, and the tag is the human's to push.** Bump `<Version>` with its page in
`docs/versions/` (renamed from `unreleased.md`, where work is written as it lands) with
`tools/bump.cs`; never tag.
[`docs/releasing.md`](docs/releasing.md) has the procedure, the page's required shape and the
package validation baseline to raise after a release.

**`docs/testing.md` describes every test class and workflow** and carries the test totals: a
change that adds tests updates it (`tools/test-counts.cs` rewrites the numbers; CI fails on stale ones).
