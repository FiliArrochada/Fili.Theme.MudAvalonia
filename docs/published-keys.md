# Published resource keys

Every resource key an app may name: in a `{DynamicResource}`, or set in the application's
`Resources` to override the theme. Within a major version none is removed or renamed; see
[Compatibility](compatibility.md).

**A colour is overridden per control by its `*Brush` key, never its `*Color`.** Each brush resolves
its colour from the application's resources, so a `*Color` key takes effect only in the
application's `Resources` (per variant, in its `ThemeDictionaries`), where it changes every control,
in every window and `ThemeVariantScope` of that variant.
In a control's `Resources` it does nothing; `FiliPrimaryBrush` there recolours that control and
what is inside it.

Always reach a key through `DynamicResource`. A `StaticResource` freezes the value of the variant
that was active when it loaded, and a misspelt key resolves to nothing, silently.

`PublishedInterfaceTests` compares this page with the theme's source in both directions and
resolves every key in each variant: a new key is listed here in the change that adds it.

## Palette

Every colour as a `*Color` and its `*Brush`, in light, dark and high contrast. Values and provenance: [Tokens](../README.md#tokens).

Two kinds are roles rather than colours. `Fili{Colour}AlertTextColor` is a text or outlined
alert's text on its tint: the colour's darken shade in light and dark, as MudBlazor's, and its
lighten shade in high contrast, where darken on the tint fell below 4.5:1. `FiliChipHoverColor` is
a default chip's ground on hover, focus and press: action-disabled in light and dark, and an
opaque grey under white text in high contrast.

One exception: `FiliSkeletonWaveColor` is the middle stop of the wave's two gradients,
`FiliSkeletonWaveStartBrush` and `FiliSkeletonWaveEndBrush`, and has no solid brush, as the theme
paints nothing in it alone.

- `FiliActionDefaultBrush`
- `FiliActionDefaultColor`
- `FiliActionDefaultHoverBrush`
- `FiliActionDefaultHoverColor`
- `FiliActionDisabledBackgroundBrush`
- `FiliActionDisabledBackgroundColor`
- `FiliActionDisabledBrush`
- `FiliActionDisabledColor`
- `FiliAppbarBackgroundBrush`
- `FiliAppbarBackgroundColor`
- `FiliAppbarTextBrush`
- `FiliAppbarTextColor`
- `FiliAvatarBareTextBrush`
- `FiliAvatarBareTextColor`
- `FiliAvatarTextBrush`
- `FiliAvatarTextColor`
- `FiliBackgroundBrush`
- `FiliBackgroundColor`
- `FiliBackgroundGrayBrush`
- `FiliBackgroundGrayColor`
- `FiliBadgeTextBrush`
- `FiliBadgeTextColor`
- `FiliBlackBrush`
- `FiliBlackColor`
- `FiliChipHoverBrush`
- `FiliChipHoverColor`
- `FiliDarkAlertTextBrush`
- `FiliDarkAlertTextColor`
- `FiliDarkBrush`
- `FiliDarkColor`
- `FiliDarkContrastTextBrush`
- `FiliDarkContrastTextColor`
- `FiliDarkDarkenBrush`
- `FiliDarkDarkenColor`
- `FiliDarkHoverBrush`
- `FiliDarkHoverColor`
- `FiliDarkLightenBrush`
- `FiliDarkLightenColor`
- `FiliDividerBrush`
- `FiliDividerColor`
- `FiliDividerLightBrush`
- `FiliDividerLightColor`
- `FiliDrawerBackgroundBrush`
- `FiliDrawerBackgroundColor`
- `FiliDrawerBorderBrush`
- `FiliDrawerBorderColor`
- `FiliDrawerIconBrush`
- `FiliDrawerIconColor`
- `FiliDrawerTextBrush`
- `FiliDrawerTextColor`
- `FiliErrorAlertTextBrush`
- `FiliErrorAlertTextColor`
- `FiliErrorBrush`
- `FiliErrorColor`
- `FiliErrorContrastTextBrush`
- `FiliErrorContrastTextColor`
- `FiliErrorDarkenBrush`
- `FiliErrorDarkenColor`
- `FiliErrorHoverBrush`
- `FiliErrorHoverColor`
- `FiliErrorLightenBrush`
- `FiliErrorLightenColor`
- `FiliGrayDarkBrush`
- `FiliGrayDarkColor`
- `FiliGrayDarkerBrush`
- `FiliGrayDarkerColor`
- `FiliGrayDefaultBrush`
- `FiliGrayDefaultColor`
- `FiliGrayLightBrush`
- `FiliGrayLightColor`
- `FiliGrayLighterBrush`
- `FiliGrayLighterColor`
- `FiliInfoAlertTextBrush`
- `FiliInfoAlertTextColor`
- `FiliInfoBrush`
- `FiliInfoColor`
- `FiliInfoContrastTextBrush`
- `FiliInfoContrastTextColor`
- `FiliInfoDarkenBrush`
- `FiliInfoDarkenColor`
- `FiliInfoHoverBrush`
- `FiliInfoHoverColor`
- `FiliInfoLightenBrush`
- `FiliInfoLightenColor`
- `FiliInputFilledBrush`
- `FiliInputFilledColor`
- `FiliLinesDefaultBrush`
- `FiliLinesDefaultColor`
- `FiliLinesInputsBrush`
- `FiliLinesInputsColor`
- `FiliOverlayDarkBrush`
- `FiliOverlayDarkColor`
- `FiliOverlayHoverBrush`
- `FiliOverlayHoverColor`
- `FiliOverlayLightBrush`
- `FiliOverlayLightColor`
- `FiliOverlayPressedBrush`
- `FiliOverlayPressedColor`
- `FiliPrimaryAlertTextBrush`
- `FiliPrimaryAlertTextColor`
- `FiliPrimaryBrush`
- `FiliPrimaryColor`
- `FiliPrimaryContrastTextBrush`
- `FiliPrimaryContrastTextColor`
- `FiliPrimaryDarkenBrush`
- `FiliPrimaryDarkenColor`
- `FiliPrimaryHoverBrush`
- `FiliPrimaryHoverColor`
- `FiliPrimaryLightenBrush`
- `FiliPrimaryLightenColor`
- `FiliScrollbarThumbBrush`
- `FiliScrollbarThumbColor`
- `FiliScrollbarThumbHoverBrush`
- `FiliScrollbarThumbHoverColor`
- `FiliSecondaryAlertTextBrush`
- `FiliSecondaryAlertTextColor`
- `FiliSecondaryBrush`
- `FiliSecondaryColor`
- `FiliSecondaryContrastTextBrush`
- `FiliSecondaryContrastTextColor`
- `FiliSecondaryDarkenBrush`
- `FiliSecondaryDarkenColor`
- `FiliSecondaryHoverBrush`
- `FiliSecondaryHoverColor`
- `FiliSecondaryLightenBrush`
- `FiliSecondaryLightenColor`
- `FiliSkeletonBrush`
- `FiliSkeletonColor`
- `FiliSkeletonWaveColor`
- `FiliSkeletonWaveEndBrush`
- `FiliSkeletonWaveStartBrush`
- `FiliSuccessAlertTextBrush`
- `FiliSuccessAlertTextColor`
- `FiliSuccessBrush`
- `FiliSuccessColor`
- `FiliSuccessContrastTextBrush`
- `FiliSuccessContrastTextColor`
- `FiliSuccessDarkenBrush`
- `FiliSuccessDarkenColor`
- `FiliSuccessHoverBrush`
- `FiliSuccessHoverColor`
- `FiliSuccessLightenBrush`
- `FiliSuccessLightenColor`
- `FiliSurfaceBrush`
- `FiliSurfaceColor`
- `FiliSwitchThumbBrush`
- `FiliSwitchThumbColor`
- `FiliTableHoverBrush`
- `FiliTableHoverColor`
- `FiliTableLinesBrush`
- `FiliTableLinesColor`
- `FiliTableStripedBrush`
- `FiliTableStripedColor`
- `FiliTertiaryAlertTextBrush`
- `FiliTertiaryAlertTextColor`
- `FiliTertiaryBrush`
- `FiliTertiaryColor`
- `FiliTertiaryContrastTextBrush`
- `FiliTertiaryContrastTextColor`
- `FiliTertiaryDarkenBrush`
- `FiliTertiaryDarkenColor`
- `FiliTertiaryHoverBrush`
- `FiliTertiaryHoverColor`
- `FiliTertiaryLightenBrush`
- `FiliTertiaryLightenColor`
- `FiliTextDisabledBrush`
- `FiliTextDisabledColor`
- `FiliTextPrimaryBrush`
- `FiliTextPrimaryColor`
- `FiliTextSecondaryBrush`
- `FiliTextSecondaryColor`
- `FiliTooltipBackgroundBrush`
- `FiliTooltipBackgroundColor`
- `FiliWarningAlertTextBrush`
- `FiliWarningAlertTextColor`
- `FiliWarningBrush`
- `FiliWarningColor`
- `FiliWarningContrastTextBrush`
- `FiliWarningContrastTextColor`
- `FiliWarningDarkenBrush`
- `FiliWarningDarkenColor`
- `FiliWarningHoverBrush`
- `FiliWarningHoverColor`
- `FiliWarningLightenBrush`
- `FiliWarningLightenColor`
- `FiliWhiteBrush`
- `FiliWhiteColor`

The ripple's opacity, a `Double` the same in every variant: `FiliRippleOpacity` (0.1) on most
controls, `FiliRippleOpacitySecondary` (0.2) on filled buttons, filled chips and coloured FABs.
Its colour is the control's text. Set both to `0` in the application's `Resources` to turn off
all theme ripples. Set either key in a container's `Resources` to override the matching ripple
hosts below it, or in a ripple host's own `Resources` to override just that control. Filled
buttons, filled chips and coloured FABs use the secondary key; most other ripple hosts use the
primary key.

- `FiliRippleOpacity`
- `FiliRippleOpacitySecondary`

The slider's rail opacity, a `Double`: the colour at `0.30` in light and dark, MudBlazor's, and
`0.5` in high contrast, where a 30% rail fell below 3:1 on black.

- `FiliSliderRailOpacity`

Reduced motion, a `Boolean` the same in every variant, `False` by default. `True` in an app's
resources stops the ripple, the popup fade, the skeleton's pulse and wave, the snackbar's slide, an
expansion panel's and an overlay's fade, the tab indicator's slide and a striped bar's bands, and
every transition the theme declares: a drawer's slide, a switch thumb's travel, a field's floating
label and its focus underline, a chevron's turn, an expansion panel's margin, a determinate
circular progress's sweep, a data grid's sort arrow, a slider's value label and the hover and
selection fades. An indeterminate progress indicator keeps moving, and an app's own transitions
are left alone. MudBlazor has no such switch.

- `FiliReducedMotion`

The focus ring, a `Boolean`: `True` in every variant (since 0.29.0; it was `False` in light and
dark before). It draws two adjacent 2px strokes round each control focused by the keyboard, never
one clicked or touched: `FiliWhiteColor` against the control and `FiliTextPrimaryColor` outside it, 2px out, where the
single ring was. The two colours ensure
at least one stroke contrasts with the surrounding light, dark or coloured surface. `False` in an
app's resources leaves keyboard focus to MudBlazor's state-layer tint alone, in every variant. A
control's own `FocusAdorner` still wins, and one set to `{x:Null}`, in a style or on the control,
opts that control out of the ring; a control that leaves it unset takes the ring. MudBlazor has none.

- `FiliFocusRing`

The ring's look, the same in every variant (since 0.31.0): `FiliFocusRingBrush`, the outer
stroke, built from `FiliTextPrimaryColor`; `FiliFocusRingInnerBrush`, the stroke against the
control, built from `FiliWhiteColor`; and `FiliFocusRingThickness`, a `Double`, the outer stroke's
width, `2`. A thicker outer stroke moves the ring out by as much, so the inner one keeps its 2px and
the corners stay parallel to the control's. The two brushes are looked up from the control the ring
frames, so they follow its variant and may be set in the application's, a window's or a container's
`Resources`; the thickness is read by each window's adorner layer, where the ring is drawn, so it
is set in the application's or a window's. A brand-coloured ring is `FiliFocusRingBrush` set to the primary with
`FiliFocusRingInnerBrush` `Transparent`; check its 3:1 against every surface it is drawn on, as
the defaults' pairing is what guarantees it.

- `FiliFocusRingBrush`
- `FiliFocusRingInnerBrush`
- `FiliFocusRingThickness`

## Elevation

MudBlazor's 26 shadow levels as `BoxShadows`, 0 to 25. In high contrast each is a 1px ring instead, in
`FiliLinesDefaultColor`.

- `FiliElevation0`
- `FiliElevation1`
- `FiliElevation2`
- `FiliElevation3`
- `FiliElevation4`
- `FiliElevation5`
- `FiliElevation6`
- `FiliElevation7`
- `FiliElevation8`
- `FiliElevation9`
- `FiliElevation10`
- `FiliElevation11`
- `FiliElevation12`
- `FiliElevation13`
- `FiliElevation14`
- `FiliElevation15`
- `FiliElevation16`
- `FiliElevation17`
- `FiliElevation18`
- `FiliElevation19`
- `FiliElevation20`
- `FiliElevation21`
- `FiliElevation22`
- `FiliElevation23`
- `FiliElevation24`
- `FiliElevation25`

`material-elevation-overlay` draws its levels from a second ladder: each level's shadow, plus in Dark Material's white overlay as an inset layer. Outside Dark they equal the shadows above, and an app that repaints a shadow repaints its overlay level too.

- `FiliElevationOverlay0`
- `FiliElevationOverlay1`
- `FiliElevationOverlay2`
- `FiliElevationOverlay3`
- `FiliElevationOverlay4`
- `FiliElevationOverlay5`
- `FiliElevationOverlay6`
- `FiliElevationOverlay7`
- `FiliElevationOverlay8`
- `FiliElevationOverlay9`
- `FiliElevationOverlay10`
- `FiliElevationOverlay11`
- `FiliElevationOverlay12`
- `FiliElevationOverlay13`
- `FiliElevationOverlay14`
- `FiliElevationOverlay15`
- `FiliElevationOverlay16`
- `FiliElevationOverlay17`
- `FiliElevationOverlay18`
- `FiliElevationOverlay19`
- `FiliElevationOverlay20`
- `FiliElevationOverlay21`
- `FiliElevationOverlay22`
- `FiliElevationOverlay23`
- `FiliElevationOverlay24`
- `FiliElevationOverlay25`

## Geometry

Corner radii, border thickness, the 4px spacing scale and the layout sizes. The spacing scale is
MudBlazor's, 0 to 20 steps of 4px, each twice: `FiliSpacing4` is 16 as a number, for a panel's
`Spacing`, and `FiliSpacing4Thickness` is 16 on every side, for a `Padding` or `Margin`
(`pa-4`, `ma-4`).

- `FiliAppbarHeight`
- `FiliBorderThickness`
- `FiliCornerRadiusNone`
- `FiliCornerRadiusPill`
- `FiliCornerRadius`
- `FiliDrawerMiniWidth`
- `FiliDrawerWidth`
- `FiliInputMinWidth`
- `FiliSpacing0`
- `FiliSpacing1`
- `FiliSpacing2`
- `FiliSpacing3`
- `FiliSpacing4`
- `FiliSpacing5`
- `FiliSpacing6`
- `FiliSpacing7`
- `FiliSpacing8`
- `FiliSpacing9`
- `FiliSpacing10`
- `FiliSpacing11`
- `FiliSpacing12`
- `FiliSpacing13`
- `FiliSpacing14`
- `FiliSpacing15`
- `FiliSpacing16`
- `FiliSpacing17`
- `FiliSpacing18`
- `FiliSpacing19`
- `FiliSpacing20`
- `FiliSpacing0Thickness`
- `FiliSpacing1Thickness`
- `FiliSpacing2Thickness`
- `FiliSpacing3Thickness`
- `FiliSpacing4Thickness`
- `FiliSpacing5Thickness`
- `FiliSpacing6Thickness`
- `FiliSpacing7Thickness`
- `FiliSpacing8Thickness`
- `FiliSpacing9Thickness`
- `FiliSpacing10Thickness`
- `FiliSpacing11Thickness`
- `FiliSpacing12Thickness`
- `FiliSpacing13Thickness`
- `FiliSpacing14Thickness`
- `FiliSpacing15Thickness`
- `FiliSpacing16Thickness`
- `FiliSpacing17Thickness`
- `FiliSpacing18Thickness`
- `FiliSpacing19Thickness`
- `FiliSpacing20Thickness`

## Typography

The font family and MudBlazor's type ramp: a size, line height and letter spacing per style, and
the button label's case, a string in CSS's words (`uppercase` by default, `none` to keep labels as
written).

The baseline is MudBlazor's `Typography.Default`, the same values as body2. `FiliFontSize` is
every window's and popup's size, inherited by all unclassed text: set it in the application's
`Resources` to scale the app's body text. `FiliLineHeight` and `FiliLetterSpacing` are that
baseline's line height and spacing, for an app's own styles to read; the theme applies neither to
unclassed text, which keeps the font's natural line height so an app's own large text is not
clipped. `FiliButtonTextTransform` is dynamic: set it in the application's `Resources` to affect
all consumers, in a container's `Resources` to affect consumers in that subtree, or in a button's
`Resources` to change only that button's label. It accepts CSS text-transform values, including
`none`; tabs remain uppercase because their style hard-codes that case. The type ramp's classes
and the controls read their own style's keys (`FiliBody2*` and so on), so they do not follow
`FiliFontSize`.

- `FiliBody1FontSize`
- `FiliBody1LetterSpacing`
- `FiliBody1LineHeight`
- `FiliBody2FontSize`
- `FiliBody2LetterSpacing`
- `FiliBody2LineHeight`
- `FiliButtonFontSize`
- `FiliButtonLetterSpacing`
- `FiliButtonLineHeight`
- `FiliButtonTextTransform`
- `FiliCaptionFontSize`
- `FiliCaptionLetterSpacing`
- `FiliCaptionLineHeight`
- `FiliFontFamily`
- `FiliFontSize`
- `FiliH1FontSize`
- `FiliH1LetterSpacing`
- `FiliH1LineHeight`
- `FiliH2FontSize`
- `FiliH2LetterSpacing`
- `FiliH2LineHeight`
- `FiliH3FontSize`
- `FiliH3LetterSpacing`
- `FiliH3LineHeight`
- `FiliH4FontSize`
- `FiliH4LetterSpacing`
- `FiliH4LineHeight`
- `FiliH5FontSize`
- `FiliH5LetterSpacing`
- `FiliH5LineHeight`
- `FiliH6FontSize`
- `FiliH6LetterSpacing`
- `FiliH6LineHeight`
- `FiliLetterSpacing`
- `FiliLineHeight`
- `FiliOverlineFontSize`
- `FiliOverlineLetterSpacing`
- `FiliOverlineLineHeight`
- `FiliSubtitle1FontSize`
- `FiliSubtitle1LetterSpacing`
- `FiliSubtitle1LineHeight`
- `FiliSubtitle2FontSize`
- `FiliSubtitle2LetterSpacing`
- `FiliSubtitle2LineHeight`

## Strings

The words the theme's own templates say: the automation names of its icon-only buttons, which a
screen reader reads where a sighted user sees only a glyph. English, the same in every variant; an
app translates one by setting the key in its application's `Resources`, or a control's for that
control (`Themes/Strings.axaml`).

- `FiliStringAccept`
- `FiliStringClear`
- `FiliStringClose`
- `FiliStringDecrease`
- `FiliStringDismiss`
- `FiliStringIncrease`
- `FiliStringNext`
- `FiliStringPrevious`

## Glyphs

Every icon the theme draws. Set one in a control's `Resources`, or a parent's, to swap that control's glyph; see the docs site's [Icons](https://filiarrochada.github.io/Fili.Theme.MudAvalonia/#icons) page.

- `FiliAccessTimeGeometry`
- `FiliAlertSuccessGeometry`
- `FiliArrowBackGeometry`
- `FiliArrowDropDownGeometry`
- `FiliArrowDropUpGeometry`
- `FiliArrowRightGeometry`
- `FiliArrowUpwardGeometry`
- `FiliCheckBoxCheckedGeometry`
- `FiliCheckBoxIndeterminateGeometry`
- `FiliCheckBoxUncheckedGeometry`
- `FiliCheckGeometry`
- `FiliChevronRightGeometry`
- `FiliCloseGeometry`
- `FiliErrorOutlineGeometry`
- `FiliEventGeometry`
- `FiliEventNoteGeometry`
- `FiliExpandMoreGeometry`
- `FiliInfoOutlinedGeometry`
- `FiliKeyboardArrowDownGeometry`
- `FiliKeyboardArrowUpGeometry`
- `FiliMoreVertGeometry`
- `FiliNavigateBeforeGeometry`
- `FiliRadioCheckedGeometry`
- `FiliRadioUncheckedGeometry`
- `FiliReportProblemGeometry`
- `FiliSwitchThumbIconGeometry`
