# Published resource keys

Every resource key an app may name: in a `{DynamicResource}`, or set in a control's `Resources` to
override the theme for that control. Within a major version none is removed or renamed; see
[Compatibility](compatibility.md).

Always reach a key through `DynamicResource`. A `StaticResource` freezes the value of the variant
that was active when it loaded, and a misspelt key resolves to nothing, silently.

`PublishedInterfaceTests` compares this page with the theme's source in both directions and
resolves every key in each variant: a new key is listed here in the change that adds it.

## Palette

Every colour as a `*Color` and its `*Brush`, in light, dark and high contrast. Values and provenance: [Tokens](../README.md#tokens).

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
Its colour is the control's text. Set both to `0` to turn the ripple off.

- `FiliRippleOpacity`
- `FiliRippleOpacitySecondary`

Reduced motion, a `Boolean` the same in every variant, `False` by default. `True` in an app's
resources stops everything the theme moves or fades on its own: the ripple, the popup fade, the
skeleton's pulse and wave, the snackbar's slide, an expansion panel's and an overlay's fade, and the
tab indicator's slide. A progress indicator keeps moving. MudBlazor has no such switch.

- `FiliReducedMotion`

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
