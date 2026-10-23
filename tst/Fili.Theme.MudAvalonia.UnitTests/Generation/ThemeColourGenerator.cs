using System.Text;

namespace Fili.Theme.MudAvalonia.UnitTests.Generation;

/// <summary>
/// Writes <c>Themes/Controls/Button.axaml</c>, and one marked region each in
/// <c>SplitButton.axaml</c> and <c>FiliTheme.axaml</c>, from ONE list of colours.
///
/// <para>
/// Three variants x eight colours x three states is a few hundred styles. Written by hand, one
/// colour's block can quietly differ from the others - and a typo in a resource key does not fail,
/// it resolves to nothing and leaves the default in place. Generated, every colour follows exactly
/// the same rule. <see cref="GeneratedThemeTests"/> fails when the files on disk differ from what
/// this would write, and rewrites them when <c>FILI_REGENERATE=1</c> is set.
/// </para>
/// </summary>
public static class ThemeColourGenerator
{
    /// <summary>How the generated files name their source.</summary>
    public const string Name = "ThemeColourGenerator";

    public const string SplitLabel = "split button variants, colours and sizes.";
    public const string DropDownLabel = "drop-down button variants, colours and sizes.";
    public const string TextLabel = "TextBlock colours.";
    public const string CheckBoxLabel = "checkbox colours and sizes.";
    public const string RadioLabel = "radio colours and sizes.";
    public const string SwitchLabel = "switch colours and sizes.";
    public const string SliderLabel = "slider colours and sizes.";
    public const string ProgressLabel = "progress bar colours.";
    public const string CircularLabel = "circular progress colours.";
    public const string ChipLabel = "chip variants, colours, selection and sizes.";
    public const string ToggleLabel = "toggle item colours, selection and sizes.";
    public const string LinkLabel = "link colours.";
    public const string ListLabel = "list selection colours.";
    public const string TreeLabel = "tree view selection colours.";
    public const string NavLabel = "nav menu colours.";
    public const string FabLabel = "fab variants, colours and sizes.";
    public const string BadgeLabel = "badge origins, colours and elevation.";
    public const string AvatarLabel = "avatar colours.";
    public const string DataGridLabel = "data grid elevation.";
    public const string PaperLabel = "paper elevation.";
    public const string TabBarLabel = "tab bar elevation.";
    public const string ExpanderLabel = "expander elevation.";
    public const string OverlayLabel = "material elevation overlay.";
    public const string AlertLabel = "alert colours.";
    public const string TabsLabel = "tab bar colours.";
    public const string AppBarLabel = "app bar colours.";
    public const string ToggleIconLabel = "toggle icon button variants, colours and sizes.";
    public const string IconLabel = "icon sizes and colours.";
    public const string DrawerLabel = "drawer colours.";
    public const string DrawerElevationLabel = "drawer elevation.";
    public const string SnackbarLabel = "snackbar variants and colours.";
    public const string TooltipLabel = "tooltip colours.";
    public const string ThumbIconLabel = "switch thumb icon colours.";
    public const string MultiSelectionLabel = "multi-selection checkbox colours.";

    private static readonly string[] Colours =
        ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    private static readonly string[] States = [":pointerover", ":focus-visible", ":pressed"];

    /// <summary>Every generated file, as (path relative to the theme project, full content).</summary>
    public static IEnumerable<(string RelativePath, string Content)> Generate(string themeDirectory)
    {
        // Normalised because the raw string literals carry this source file's own line endings.
        yield return (ButtonPath, Normalise(Header + TextTheme() + OutlinedTheme() + FilledTheme() + Footer));

        var regions = new (string Path, string Label, string Body)[]
        {
            (SplitButtonPath, SplitLabel, SplitRegion()),
            (SplitButtonPath, DropDownLabel, DropDownRegion()),
            (FiliThemePath, TextLabel, TextBlockRegion()),
            (Controls("CheckBox.axaml"), CheckBoxLabel, GlyphRegion(
                ["PART_Unchecked", "PART_Checked", "PART_Indeterminate"],
                ["PART_Unchecked", "PART_Indeterminate"], [":unchecked", ":indeterminate"])),
            (Controls("RadioButton.axaml"), RadioLabel, GlyphRegion(
                ["PART_Unchecked", "PART_Checked"],
                ["PART_Unchecked"], [":unchecked"])),
            (Controls("ToggleSwitch.axaml"), SwitchLabel, SwitchRegion()),
            (Controls("Slider.axaml"), SliderLabel, SliderRegion()),
            (Controls("ProgressBar.axaml"), ProgressLabel, ProgressRegion()),
            (Controls("ProgressCircular.axaml"), CircularLabel, CircularRegion()),
            (Controls("Chip.axaml"), ChipLabel, ChipRegion()),
            (Controls("ToggleButton.axaml"), ToggleLabel, ToggleRegion()),
            (Controls("HyperlinkButton.axaml"), LinkLabel, LinkRegion()),
            (FiliThemePath, ListLabel, SelectionRegion("ListBox", "ListBoxItem", "Border#PART_StateLayer", list: true)),
            (FiliThemePath, TreeLabel, SelectionRegion("TreeView", "TreeViewItem", "Border#SelectionBorder", list: false)),
            (FiliThemePath, NavLabel, NavRegion()),
            (Controls("Fab.axaml"), FabLabel, FabRegion()),
            (Controls("Badge.axaml"), BadgeLabel, BadgeRegion()),
            (Path.Combine("Themes", "Optional", "DataGrid", "DataGrid.axaml"), DataGridLabel, DataGridRegion()),
            (FiliThemePath, PaperLabel, PaperRegion()),
            (Controls("TabControl.axaml"), TabBarLabel, ElevationRegion("PART_TabBarHost")),
            (Controls("TabStrip.axaml"), TabBarLabel, ElevationRegion("PART_TabBarHost")),
            (Controls("Expander.axaml"), ExpanderLabel, ElevationRegion("PART_Root")),
            (FiliThemePath, OverlayLabel, OverlayRegion()),
            (Path.Combine("Themes", "Optional", "DataGrid", "DataGridStyles.axaml"), OverlayLabel, DataGridOverlayRegion()),
            (FiliThemePath, AlertLabel, AlertRegion()),
            (FiliThemePath, AvatarLabel, AvatarRegion()),
            (FiliThemePath, TabsLabel, TabsRegion()),
            (FiliThemePath, AppBarLabel, AppBarRegion()),
            (Controls("ToggleButton.axaml"), ToggleIconLabel, ToggleIconRegion()),
            (FiliThemePath, IconLabel, IconRegion()),
            (FiliThemePath, DrawerLabel, DrawerRegion()),
            (FiliThemePath, DrawerElevationLabel, DrawerElevationRegion()),
            (Controls("Notifications.axaml"), SnackbarLabel, SnackbarRegion()),
            (Controls("Overlays.axaml"), TooltipLabel, TooltipRegion()),
            (Controls("ToggleSwitch.axaml"), ThumbIconLabel, ThumbIconRegion()),
            (FiliThemePath, MultiSelectionLabel, MultiSelectionRegion()),
        };

        // A file can hold more than one region, so each file's regions are applied in turn.
        foreach (var file in regions.GroupBy(r => r.Path))
        {
            var text = File.ReadAllText(Path.Combine(themeDirectory, file.Key));

            foreach (var (_, label, body) in file)
            {
                text = Patched(text, label, Normalise(body));
            }

            yield return (file.Key, text);
        }
    }

    private static string Controls(string file) => Path.Combine("Themes", "Controls", file);

    public static readonly string ButtonPath = Controls("Button.axaml");
    public static readonly string SplitButtonPath = Controls("SplitButton.axaml");
    public static readonly string FiliThemePath = "FiliTheme.axaml";

    /// <summary>The marker a hand-written file carries above the region this class owns.</summary>
    public static string Marker(string label) => $"<!-- GENERATED by {Name}: {label} -->";

    /// <summary>Line endings normalised to LF, which is how the files are generated and compared.</summary>
    public static string Normalise(string text) => text.Replace("\r\n", "\n");

    private static string Patched(string text, string label, string body)
    {
        text = Normalise(text);

        var start = text.IndexOf(Marker(label), StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"No '{Marker(label)}' marker to generate into.");
        }

        start = text.IndexOf('\n', start) + 1;
        var end = text.IndexOf("<!-- END GENERATED -->", start, StringComparison.Ordinal);
        end = text.LastIndexOf('\n', end) + 1;

        return text[..start] + body + text[end..];
    }

    // ---------------------------------------------------------------------------------------
    // Building blocks.
    // ---------------------------------------------------------------------------------------

    private static string Res(string key) => "{DynamicResource " + key + "}";

    /// <summary>
    /// A Border in a hand-written template is a PartBorder, which the app's Border class styles
    /// pass over (PartBorder.cs); a selector reaching one through /template/ names that type.
    /// </summary>
    private static string Parts(string selector) => selector.Replace("/template/ Border", "/template/ theme|PartBorder");

    private static string Style(string selector, params (string Property, string Value)[] setters)
    {
        var sb = new StringBuilder();
        sb.Append("    <Style Selector=\"").Append(Parts(selector)).Append("\">\n");
        foreach (var (property, value) in setters)
        {
            sb.Append("      <Setter Property=\"").Append(property).Append("\" Value=\"").Append(value).Append("\" />\n");
        }

        return sb.Append("    </Style>\n").ToString();
    }

    /// <summary>_button.scss gives hover, :focus-visible and :active one value; :active is :pressed.</summary>
    private static string ButtonStates(string prefix, string part, string property, string value) =>
        string.Concat(States.Select(s => Style($"^{prefix}{s} /template/ Border#{part}", (property, value))));

    /// <summary>DropShadow="false" - `.mud-button-disable-elevation`: no shadow at rest or in any state.</summary>
    private static string Flat(string part, string[] states, string prefix = "") =>
        "\n    <!-- `flat` is DropShadow=\"false\": `.mud-button-disable-elevation` drops the shadow in every state. -->\n"
        + Style($"^{prefix}.flat /template/ Border#{part}", ("BoxShadow", Res("FiliElevation0")))
        + string.Concat(states.Select(st => Style($"^{prefix}.flat{st} /template/ Border#{part}", ("BoxShadow", Res("FiliElevation0")))));

    private static string Sizes(string small, string large) =>
        Style("^.small", ("Padding", small), ("FontSize", "13"))
        + Style("^.large", ("Padding", large), ("FontSize", "15"))
        + IconSlots("button");

    /// <summary>
    /// What a `start-icon` or `end-icon` in this host's content takes, through IconSlot's
    /// inherited properties: _button.scss's 20px (18 small, 22 large), pulled 4px out (2 small) and
    /// 8px from the label; a chip's `.mud-chip-icon` 20px (18 small, 24 large), 4px out and in (6
    /// large), its end icon the close button's 18px, 6px in; a FAB's 24px at any size, 8px from the
    /// label (4 small). A chip's `avatar` is _chip.scss's `.mud-avatar`: 24px, 8px out and in, 12px
    /// text; 18px, 4px and 10px small; 32px, 12px out and 8px in, 16px large. Set on the host, so
    /// only hosts are styled and the icon reads one value.
    /// </summary>
    private static string IconSlots(string kind)
    {
        static (string, string) Start(string size) => ("theme:IconSlot.StartSize", size);
        static (string, string) StartGap(string margin) => ("theme:IconSlot.StartMargin", margin);
        static (string, string) End(string size) => ("theme:IconSlot.EndSize", size);
        static (string, string) EndGap(string margin) => ("theme:IconSlot.EndMargin", margin);
        static (string, string) Avatar(string size) => ("theme:IconSlot.AvatarSize", size);
        static (string, string) AvatarGap(string margin) => ("theme:IconSlot.AvatarMargin", margin);
        static (string, string) AvatarText(string size) => ("theme:IconSlot.AvatarFontSize", size);

        return kind switch
        {
            "chip" => Style("^", Start("20"), StartGap("-4,0,4,0"), End("18"), EndGap("6,0,-4,0"), Avatar("24"), AvatarGap("-8,0,8,0"), AvatarText("12"))
                + Style("^.small", Start("18"), Avatar("18"), AvatarGap("-4,0,4,0"), AvatarText("10"))
                + Style("^.large", Start("24"), StartGap("-6,0,6,0"), Avatar("32"), AvatarGap("-12,0,8,0"), AvatarText("16")),
            "fab" => Style("^", Start("24"), StartGap("0,0,8,0"), End("24"), EndGap("8,0,0,0"))
                + Style("^.small", StartGap("0,0,4,0"), EndGap("4,0,0,0")),
            _ => Style("^", Start("20"), StartGap("-4,0,8,0"), End("20"), EndGap("8,0,-4,0"))
                + Style("^.small", Start("18"), StartGap("-2,0,8,0"), End("18"), EndGap("8,0,-2,0"))
                + Style("^.large", Start("22"), End("22")),
        };
    }

    /// <summary>
    /// The label's line box, in the Button themes only: a toggle icon button is an icon button
    /// whether or not it carries the class (DrawerPage's pane toggles cannot be given one), and
    /// has no label line.
    /// </summary>
    private const string LineBox = """
            <!-- The label's line box. `.mud-button` sets the button typography's line-height,
                 1.75 (Typography.cs), so the label is 1.75 of its font size tall whatever it
                 holds: 6 + 24.5 + 6 = 36.5px at 14px, 30.75 small, 42.25 large. The text alone
                 is 17px, and the button was 29. Nothing is drawn; it only holds the height. -->
            <Rectangle x:Name="PART_LineBox"
                       Margin="{TemplateBinding Padding}"
                       Height="{Binding FontSize, RelativeSource={RelativeSource TemplatedParent}, Converter={x:Static conv:FactorConverter.Instance}, ConverterParameter=1.75}"
                       IsHitTestVisible="False" />
""";

    private static string Template(bool shadow, bool lineBox)
    {
        var sh = shadow ? "\n                BoxShadow=\"{DynamicResource FiliElevation2}\"" : "";

        // `.mud-button-filled` raises the ripple to --mud-ripple-opacity-secondary.
        var ripple = shadow ? "FiliRippleOpacitySecondary" : "FiliRippleOpacity";
        return "    <!-- MudBlazor's ripple, drawn in PART_Ripple (Ripple.cs). -->\n"
            + "    <Setter Property=\"theme:Ripple.Opacity\" Value=\"{DynamicResource " + ripple + "}\" />\n"
            + """
    <Setter Property="Template">
      <ControlTemplate>
        <theme:PartBorder x:Name="PART_Root"
                Background="{TemplateBinding Background}"
                BorderBrush="{TemplateBinding BorderBrush}"
                BorderThickness="{TemplateBinding BorderThickness}"
"""
            + "\n                CornerRadius=\"{TemplateBinding CornerRadius}\"" + sh + ">\n"
            + """
          <Panel>
            <!-- State layer. Sits under the content, over the fill. -->
            <theme:PartBorder x:Name="PART_StateLayer"
                    CornerRadius="{TemplateBinding CornerRadius}"
                    Background="Transparent" />
            <theme:PartBorder x:Name="PART_Ripple"
                    theme:Ripple.IsLayer="True"
                    CornerRadius="{TemplateBinding CornerRadius}" />
""" + "\n" + (lineBox ? LineBox + "\n" : "") + """
            <ContentPresenter x:Name="PART_ContentPresenter"
                              theme:TextTransform.Case="{DynamicResource FiliButtonTextTransform}"
                              ContentTemplate="{TemplateBinding ContentTemplate}"
                              Foreground="{TemplateBinding Foreground}"
                              Padding="{TemplateBinding Padding}"
                              HorizontalContentAlignment="{TemplateBinding HorizontalContentAlignment}"
                              VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}">
              <ContentPresenter.Content>
                <MultiBinding Converter="{x:Static theme:TextTransform.Converter}">
                  <TemplateBinding Property="Content" />
                  <Binding Path="(theme:TextTransform.Case)" RelativeSource="{RelativeSource Self}" />
                </MultiBinding>
              </ContentPresenter.Content>
            </ContentPresenter>
          </Panel>
        </theme:PartBorder>
      </ControlTemplate>
    </Setter>
"""
            + "\n";
    }

    private const string Common = """
    <Setter Property="CornerRadius" Value="{DynamicResource FiliCornerRadius}" />
    <Setter Property="MinWidth" Value="64" />
    <Setter Property="FontFamily" Value="{DynamicResource FiliFontFamily}" />
    <Setter Property="FontSize" Value="{DynamicResource FiliButtonFontSize}" />
    <Setter Property="FontWeight" Value="Medium" />
    <Setter Property="LetterSpacing" Value="{DynamicResource FiliButtonLetterSpacing}" />
    <Setter Property="HorizontalContentAlignment" Value="Center" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="Cursor" Value="Hand" />

""";

    // ---------------------------------------------------------------------------------------
    // Button.axaml
    // ---------------------------------------------------------------------------------------

    private static string Header => """
<!--
  Button: MudButton. Transcribed from _button.scss and MudButton.razor.cs.

  Three kinds of class, combined the way MudButton's parameters are:

    Variant   text (the default - MudButton.Variant defaults to Variant.Text), outlined, filled
    Color     none (Color.Default), primary, secondary, tertiary, info, success, warning, error,
              dark, inherit
    Size      none (Size.Medium), small, large
    Shape     icon (MudIconButton), flat (DropShadow="false")

    <Button Content="Learn more" />                        text, default colour
    <Button Classes="primary" Content="Learn more" />      text, primary
    <Button Classes="outlined error" Content="Delete" />   outlined, error
    <Button Classes="filled primary" Content="Save" />     filled, primary
    <Button Classes="filled success small" Content="OK" /> filled, success, small
    <Button Classes="icon primary"><PathIcon /></Button>   a round primary icon button

  A colour class is ONLY a colour, as Color is in MudBlazor. `primary` on its own is a primary
  TEXT button; the fill comes from `filled`. Each variant follows one rule for every colour:

    text      colour as text; hover, focus and press tint with {color}-hover (the colour at 6%)
    outlined  the same, plus a 1px line in the colour (BorderOpacity is 1.0, so it IS the colour)
    filled    colour as fill, {color}-text on it; hover, focus and press switch the fill to
              {color}-darken, and elevation goes 2 -> 4 (hover, focus) -> 8 (press)

  With no colour, text and outlined use text-primary (and so does outlined's line), tinting with
  action-default-hover. A filled button with no colour is grey: action-default-hover as the
  fill, action-disabled-background on hover.

  :active and :focus-visible share the hover value in _button.scss, so press is not a stronger
  tint here either. MudBlazor's extra press feedback is the ripple, the part with no Avalonia
  primitive: an animated, clipped ellipse driven from pointer position.

""" + "\n" + GeneratedNote + """

  Order inside each theme is load-bearing: defaults, then colours, then sizes, then disabled.
  Of two styles matching the same element, the later one wins - so a colour beats the default
  tint, and disabled beats everything.
-->
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:theme="using:Fili.Theme.MudAvalonia"
                    xmlns:conv="using:Fili.Theme.MudAvalonia.Converters">


""";

    private static string GeneratedNote => """
  GENERATED by ThemeColourGenerator (tst/Fili.Theme.MudAvalonia.UnitTests/Generation) from one
  list of colours, so every colour follows exactly the same rules. Do not hand-edit one colour's
  block: change the generator, then rewrite this file with FILI_REGENERATE=1 (GeneratedThemeTests).

""";

    /// <summary>
    /// The selector prefixes one colour's rules hang on: `.primary` on a Button. A toggle icon
    /// button passes its own, so the same rules give MudToggleIconButton's two colours.
    /// </summary>
    private static string[] Prefixes(string colour, Func<string, string[]>? prefixes) =>
        prefixes?.Invoke(colour.ToLowerInvariant()) ?? [$".{colour.ToLowerInvariant()}"];

    private static string TextTheme(string key = "FiliTextButton", string target = "Button", Func<string, string[]>? prefixes = null)
    {
        var o = new StringBuilder();
        o.Append("""
  <!-- Text: `.mud-button-text`. No fill, no elevation, ever. -->
  <ControlTheme x:Key="FiliTextButton" TargetType="Button">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{DynamicResource FiliTextPrimaryBrush}" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="8,6" />

""".Replace("x:Key=\"FiliTextButton\" TargetType=\"Button\"", $"x:Key=\"{key}\" TargetType=\"{target}\""));
        o.Append(Common).Append(Template(false, target == "Button"));
        o.Append("""

    <!-- No colour: action-default-hover. Keyboard focus paints the same tint - that IS the
         affordance in MudBlazor, whose _reset.scss drops the browser focus ring. -->

""");
        o.Append(ButtonStates("", "PART_StateLayer", "Background", Res("FiliActionDefaultHoverBrush"))).Append('\n');
        o.Append("    <!-- MudIconButton, Variant.Text: round, 12px round a 24px icon, and action-default rather\n");
        o.Append("         than text-primary - `.mud-icon-button` comes after `.mud-button` in MudBlazor's\n");
        o.Append("         bundle, so its colour wins. Before the colours, so a colour class still beats it. -->\n");
        o.Append(Style("^.icon", ("CornerRadius", "999"), ("Padding", "12"), ("MinWidth", "0"), ("Foreground", Res("FiliActionDefaultBrush")))).Append('\n');
        if (target == "Button")
        {
            o.Append("    <!-- `.mud-icon-button` is its padding round the icon: no label, so no line box. -->\n");
            o.Append(Style("^.icon /template/ Rectangle#PART_LineBox", ("IsVisible", "False"))).Append('\n');
        }

        foreach (var c in Colours)
        {
            foreach (var p in Prefixes(c, prefixes))
            {
                o.Append(Style($"^{p}", ("Foreground", Res($"Fili{c}Brush"))));
                o.Append(ButtonStates(p, "PART_StateLayer", "Background", Res($"Fili{c}HoverBrush")));
            }
        }

        o.Append("""

    <!-- Color.Inherit: `color: inherit`. Bound to the parent's inherited text colour, because a
         ControlTheme setter cannot withdraw its own Foreground and fall back to inheriting. -->

""");
        o.Append(Style("^.inherit", ("Foreground", "{Binding $parent[Control].(TextElement.Foreground)}")));
        o.Append("\n    <!-- `.mud-button-text-size-*`: 4px 5px / 8px 11px at 0.8125rem / 0.9375rem. -->\n");
        o.Append(Sizes("5,4", "11,8"));
        o.Append("\n    <!-- `.mud-icon-button-size-small`: 3px; large keeps 12px and grows the icon instead. -->\n");
        o.Append(Style("^.icon.small", ("Padding", "3")));
        o.Append(Style("^.icon.large", ("Padding", "12"))).Append('\n');
        o.Append(Style("^:disabled /template/ ContentPresenter#PART_ContentPresenter", ("Foreground", Res("FiliActionDisabledBrush"))));
        o.Append(Style("^:disabled /template/ Border#PART_StateLayer", ("Background", "Transparent")));
        o.Append("  </ControlTheme>\n\n");
        return o.ToString();
    }

    private static string OutlinedTheme(string key = "FiliOutlinedButton", string target = "Button", Func<string, string[]>? prefixes = null, string basedOn = "FiliTextButton")
    {
        var o = new StringBuilder();
        o.Append("""
  <!--
    Outlined: `.mud-button-outlined`. The text button plus a 1px line, at 5px 15px so the line
    does not grow the button. The line is text-primary at BorderOpacity, which Palette.cs sets to
    1.0 - a hard line in the text colour, not the faint input outline.
  -->
  <ControlTheme x:Key="FiliOutlinedButton" TargetType="Button" BasedOn="{StaticResource FiliTextButton}">
    <Setter Property="BorderBrush" Value="{DynamicResource FiliTextPrimaryBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="15,5" />


""".Replace("x:Key=\"FiliOutlinedButton\" TargetType=\"Button\" BasedOn=\"{StaticResource FiliTextButton}\"",
            $"x:Key=\"{key}\" TargetType=\"{target}\" BasedOn=\"{{StaticResource {basedOn}}}\""));
        foreach (var c in Colours)
        {
            foreach (var p in Prefixes(c, prefixes))
            {
                o.Append(Style($"^{p}", ("BorderBrush", Res($"Fili{c}Brush"))));
            }
        }

        o.Append("\n    <!-- `.mud-button-outlined-inherit`: `border-color: currentColor`. -->\n");
        o.Append(Style("^.inherit", ("BorderBrush", "{Binding $self.Foreground}")));
        o.Append("\n    <!-- `.mud-button-outlined-size-*`: 3px 9px / 7px 21px. -->\n");
        o.Append(Sizes("9,3", "21,7"));
        o.Append("\n    <!-- `.mud-button-outlined.mud-icon-button`: the default radius again, 5px; 4px small or large. -->\n");
        o.Append(Style("^.icon", ("CornerRadius", Res("FiliCornerRadius")), ("Padding", "5")));
        o.Append(Style("^.icon.small", ("Padding", "4")));
        o.Append(Style("^.icon.large", ("Padding", "4"))).Append('\n');
        o.Append(Style("^:disabled /template/ Border#PART_Root", ("BorderBrush", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append("  </ControlTheme>\n\n");
        return o.ToString();
    }

    private static string FilledTheme(string key = "FiliFilledButton", string target = "Button", Func<string, string[]>? prefixes = null)
    {
        var o = new StringBuilder();
        o.Append("""
  <!--
    Filled: `.mud-button-filled`. The elevation change across rest -> hover -> press is the one
    thing a style on Button cannot do, because Button has no BoxShadow - only the Border inside
    its template does. The three shadows are the layers _button.scss spells out, which are
    FiliElevation2, 4 and 8 exactly.
  -->
  <ControlTheme x:Key="FiliFilledButton" TargetType="Button">
    <Setter Property="Background" Value="{DynamicResource FiliActionDefaultHoverBrush}" />
    <Setter Property="Foreground" Value="{DynamicResource FiliTextPrimaryBrush}" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="16,6" />
    <!-- Every TemplatedControl clips to its bounds in Avalonia 12, which would cut the shadow
         off at the edge it is drawn past. -->
    <Setter Property="ClipToBounds" Value="False" />

""".Replace("x:Key=\"FiliFilledButton\" TargetType=\"Button\"", $"x:Key=\"{key}\" TargetType=\"{target}\""));
        o.Append(Common).Append(Template(true, target == "Button")).Append('\n');
        o.Append(Style("^:pointerover /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^:focus-visible /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^:pressed /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation8"))));
        o.Append("\n    <!-- No colour: grey, going to action-disabled-background. -->\n");
        o.Append(ButtonStates("", "PART_Root", "Background", Res("FiliActionDisabledBackgroundBrush"))).Append('\n');
        o.Append("    <!-- `.mud-button-filled.mud-icon-button`: 5px inside a transparent 1px border. -->\n");
        o.Append(Style("^.icon", ("MinWidth", "0"), ("Padding", "5"), ("BorderThickness", "1"), ("BorderBrush", "Transparent"), ("Foreground", Res("FiliActionDefaultBrush")))).Append('\n');
        if (target == "Button")
        {
            o.Append("    <!-- `.mud-icon-button` is its padding round the icon: no label, so no line box. -->\n");
            o.Append(Style("^.icon /template/ Rectangle#PART_LineBox", ("IsVisible", "False"))).Append('\n');
        }

        foreach (var c in Colours)
        {
            foreach (var p in Prefixes(c, prefixes))
            {
                o.Append(Style($"^{p}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
                o.Append(ButtonStates(p, "PART_Root", "Background", Res($"Fili{c}DarkenBrush")));
            }
        }

        o.Append("""

    <!-- `.mud-button-color-inherit`: the text inherits and the grey fill stays, as in MudBlazor,
         which generates no filled-inherit class. -->

""");
        o.Append(Style("^.inherit", ("Foreground", "{Binding $parent[Control].(TextElement.Foreground)}")));
        o.Append("\n    <!-- `.mud-button-filled-size-*`: 4px 10px / 8px 22px. -->\n");
        o.Append(Sizes("10,4", "22,8"));
        o.Append(Style("^.icon.small", ("Padding", "4")));
        o.Append(Style("^.icon.large", ("Padding", "4")));
        o.Append(Flat("PART_Root", [":pointerover", ":focus-visible", ":pressed"]));
        o.Append("\n    <!-- Disabled drops to the ground: no shadow at all, not a dimmed one. -->\n");
        o.Append(Style("^:disabled /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation0")), ("Background", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^:disabled /template/ ContentPresenter#PART_ContentPresenter", ("Foreground", Res("FiliActionDisabledBrush"))));
        o.Append("  </ControlTheme>\n\n");
        return o.ToString();
    }

    private const string Footer = """
  <!--
    MudButton defaults to Variant.Text and Color.Default, so an unclassed Button is the text one in
    text-primary. `outlined` and `filled` switch the theme (FiliTheme.axaml); colour and size
    classes work inside whichever theme is active.
  -->
  <ControlTheme x:Key="{x:Type Button}" TargetType="Button" BasedOn="{StaticResource FiliTextButton}" />

</ResourceDictionary>

""";

    // ---------------------------------------------------------------------------------------
    // SplitButton.axaml region: MudButtonGroup, from _buttongroup.scss.
    //
    // Every rule sets one of three SplitButton properties - Foreground, Background (both halves'
    // fill) and BorderBrush (the line: separator, and outline when outlined) - or lights a half
    // through `/template/ Button`, because a selector may cross only one /template/ boundary.
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Hover and press on either half, keyboard focus on the group (lights the main half), and an
    /// open flyout (lights the chevron half). _buttongroup.scss gives them one value.
    /// </summary>
    private static string SplitStates(string prefix, params (string, string)[] setters) =>
        Style($"^{prefix} /template/ Button:pointerover", setters)
        + Style($"^{prefix} /template/ Button:pressed", setters)
        + Style($"^{prefix}:focus-visible /template/ Button#PART_PrimaryButton", setters)
        + Style($"^{prefix}[Tag=flyout-open] /template/ Button#PART_SecondaryButton", setters);

    private static string SplitRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Text, no colour: action-default-hover on the half under the pointer. -->\n");
        o.Append(SplitStates("", ("BorderBrush", Res("FiliActionDefaultHoverBrush"))));
        o.Append("\n    <!-- Text and outlined colours: the label, the line, and the hover tint. -->\n");

        foreach (var c in Colours)
        {
            o.Append(Style($"^.{c.ToLowerInvariant()}", ("Foreground", Res($"Fili{c}Brush")), ("BorderBrush", Res($"Fili{c}Brush"))));
            o.Append(SplitStates($".{c.ToLowerInvariant()}", ("BorderBrush", Res($"Fili{c}HoverBrush"))));
        }

        o.Append("\n    <!-- Color.Inherit: the surrounding text colour, for the label and the line. -->\n");
        o.Append(Style("^.inherit", ("Foreground", "{Binding $parent[Control].(TextElement.Foreground)}"), ("BorderBrush", "{Binding $self.Foreground}")));
        o.Append("\n    <!-- `.mud-button-group-text-size-*`. -->\n");
        o.Append(Sizes("5,4", "11,8"));
        o.Append("""

    <!-- Outlined: each segment's own 1px border in the line colour, 5px 15px. Avalonia draws one
         outline round both halves plus the separator between them, which is the same picture. -->

""");
        o.Append(Style("^.outlined", ("BorderThickness", "1"), ("Padding", "15,5")));
        o.Append(Style("^.outlined.small", ("Padding", "9,3"), ("FontSize", "13")));
        o.Append(Style("^.outlined.large", ("Padding", "21,7"), ("FontSize", "15")));
        o.Append("""

    <!-- Filled: action-default-hover as the fill, `divider` between the segments, elevation 2
         lifting to 4. A coloured fill divides with {color}-lighten and darkens on hover. -->

""");
        o.Append(Style("^.filled", ("Background", Res("FiliActionDefaultHoverBrush")), ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderBrush", Res("FiliDividerBrush")), ("Padding", "16,6"),
            ("theme:Ripple.Opacity", Res("FiliRippleOpacitySecondary"))));
        o.Append(Style("^.filled /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation2"))));
        o.Append(Style("^.filled:pointerover /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^.filled:focus-visible /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(SplitStates(".filled", ("Background", Res("FiliActionDisabledBackgroundBrush")), ("BorderBrush", "Transparent")));

        foreach (var c in Colours)
        {
            o.Append(Style($"^.filled.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush")), ("BorderBrush", Res($"Fili{c}LightenBrush"))));
            o.Append(SplitStates($".filled.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}DarkenBrush"))));
        }

        o.Append(Style("^.filled.small", ("Padding", "10,4"), ("FontSize", "13")));
        o.Append(Style("^.filled.large", ("Padding", "22,8"), ("FontSize", "15")));
        o.Append(Flat("PART_Root", [":pointerover", ":focus-visible"], ".filled"));
        o.Append("\n    <!-- Disabled last, so it beats every colour. -->\n");
        o.Append(Style("^:disabled", ("Foreground", Res("FiliActionDisabledBrush")), ("BorderBrush", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^:disabled /template/ Button", ("BorderBrush", "Transparent")));
        o.Append(Style("^.filled:disabled", ("Background", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^.filled:disabled /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation0"))));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // SplitButton.axaml, second region: DropDownButton is a MudMenu with a button activator, so
    // it takes MudButton's matrix. The fill and shadow live on RootBorder; the tint on the state
    // layer; the glyph is painted with Foreground and follows every colour on its own.
    // ---------------------------------------------------------------------------------------

    private static readonly string[] MenuStates = [":pointerover", ":focus-visible", ":pressed", ":flyout-open"];

    private static string MenuStatesOn(string prefix, string part, params (string, string)[] setters) =>
        string.Concat(MenuStates.Select(s => Style($"^{prefix}{s} /template/ Border#{part}", setters)));

    private static string DropDownRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Text, no colour: action-default-hover, which an open menu keeps lit. -->\n");
        o.Append(MenuStatesOn("", "PART_StateLayer", ("Background", Res("FiliActionDefaultHoverBrush"))));
        o.Append("\n    <!-- Colours: the label and glyph, and the colour's hover tint. -->\n");

        foreach (var c in Colours)
        {
            o.Append(Style($"^.{c.ToLowerInvariant()}", ("Foreground", Res($"Fili{c}Brush"))));
            o.Append(MenuStatesOn($".{c.ToLowerInvariant()}", "PART_StateLayer", ("Background", Res($"Fili{c}HoverBrush"))));
        }

        o.Append(Style("^.inherit", ("Foreground", "{Binding $parent[Control].(TextElement.Foreground)}")));
        o.Append(Sizes("5,4", "11,8"));

        o.Append("\n    <!-- Outlined: a 1px line in text-primary, or the colour. -->\n");
        o.Append(Style("^.outlined", ("BorderBrush", Res("FiliTextPrimaryBrush")), ("BorderThickness", "1"), ("Padding", "15,5")));
        foreach (var c in Colours)
        {
            o.Append(Style($"^.outlined.{c.ToLowerInvariant()}", ("BorderBrush", Res($"Fili{c}Brush"))));
        }

        o.Append(Style("^.outlined.inherit", ("BorderBrush", "{Binding $self.Foreground}")));
        o.Append(Style("^.outlined.small", ("Padding", "9,3"), ("FontSize", "13")));
        o.Append(Style("^.outlined.large", ("Padding", "21,7"), ("FontSize", "15")));

        o.Append("\n    <!-- Filled: the fill darkens instead of tinting, and the button lifts 2 -> 4 -> 8. -->\n");
        o.Append(Style("^.filled", ("Background", Res("FiliActionDefaultHoverBrush")), ("Foreground", Res("FiliTextPrimaryBrush")), ("Padding", "16,6"),
            ("theme:Ripple.Opacity", Res("FiliRippleOpacitySecondary"))));
        o.Append(Style("^.filled /template/ Border#RootBorder", ("BoxShadow", Res("FiliElevation2"))));
        o.Append(Style("^.filled:pointerover /template/ Border#RootBorder", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^.filled:focus-visible /template/ Border#RootBorder", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^.filled:flyout-open /template/ Border#RootBorder", ("BoxShadow", Res("FiliElevation4"))));
        o.Append(Style("^.filled:pressed /template/ Border#RootBorder", ("BoxShadow", Res("FiliElevation8"))));
        o.Append(MenuStatesOn(".filled", "PART_StateLayer", ("Background", "Transparent")));
        o.Append(MenuStatesOn(".filled", "RootBorder", ("Background", Res("FiliActionDisabledBackgroundBrush"))));

        foreach (var c in Colours)
        {
            o.Append(Style($"^.filled.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
            o.Append(MenuStatesOn($".filled.{c.ToLowerInvariant()}", "RootBorder", ("Background", Res($"Fili{c}DarkenBrush"))));
        }

        o.Append(Style("^.filled.small", ("Padding", "10,4"), ("FontSize", "13")));
        o.Append(Style("^.filled.large", ("Padding", "22,8"), ("FontSize", "15")));
        o.Append(Flat("RootBorder", [":pointerover", ":focus-visible", ":flyout-open", ":pressed"], ".filled"));

        o.Append("\n    <!-- Disabled last, so it beats every colour. -->\n");
        o.Append(Style("^:disabled", ("Foreground", Res("FiliActionDisabledBrush"))));
        o.Append(Style("^:disabled /template/ Border#PART_StateLayer", ("Background", "Transparent")));
        o.Append(Style("^.outlined:disabled", ("BorderBrush", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^.filled:disabled /template/ Border#RootBorder", ("Background", Res("FiliActionDisabledBackgroundBrush")), ("BoxShadow", Res("FiliElevation0"))));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // CheckBox.axaml and RadioButton.axaml: MudCheckBox and MudRadio sit their MudIcon in a
    // `.mud-icon-button`, whose colour is action-default - so Color.Default is a GREY glyph, on
    // and off - and whose halo is action-default-hover on hover, focus and press. A colour adds
    // `mud-{color}-text` to the icon in every state (UncheckedColor is null) and the colour's hover
    // tint to the halo. Size is MudIcon's: 20 / 24 / 36px, scaled with its halo.
    // ---------------------------------------------------------------------------------------

    private static string GlyphRegion(string[] glyphs, string[] uncheckedGlyphs, string[] uncheckedStates)
    {
        var o = new StringBuilder();
        o.Append("    <!-- No colour: the action-default halo. -->\n");
        o.Append(string.Concat(States.Select(s => Style($"^{s} /template/ Ellipse#PART_StateLayer", ("Fill", Res("FiliActionDefaultHoverBrush"))))));
        o.Append("\n    <!-- Colours: every glyph, checked or not, the halo, and the ripple (`.mud-{color}-text`'s\n");
        o.Append("         ripple colour). With no colour the ripple is text-primary, the control's own. -->\n");

        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(string.Concat(glyphs.Select(g => Style($"^.{cls} /template/ Path#{g}", ("Fill", Res($"Fili{c}Brush"))))));
            o.Append(string.Concat(States.Select(s => Style($"^.{cls}{s} /template/ Ellipse#PART_StateLayer", ("Fill", Res($"Fili{c}HoverBrush"))))));
            o.Append(Style($"^.{cls} /template/ Border#PART_Ripple", ("theme:Ripple.Brush", Res($"Fili{c}Brush"))));
        }

        // After the colours, so it wins where both are set: `primary unchecked-error` is
        // Color.Primary with UncheckedColor.Error, the unchecked glyph and its halo in error.
        o.Append("\n    <!-- UncheckedColor: `unchecked-{colour}`, the colour of the glyph and its halo while NOT\n");
        o.Append("         checked (MudCheckBox: false or null), in place of the colour class. -->\n");

        foreach (var c in Colours)
        {
            var cls = $"unchecked-{c.ToLowerInvariant()}";
            o.Append(string.Concat(uncheckedGlyphs.Select(g => Style($"^.{cls} /template/ Path#{g}", ("Fill", Res($"Fili{c}Brush"))))));
            o.Append(string.Concat(uncheckedStates.SelectMany(u => States.Select(s =>
                Style($"^.{cls}{u}{s} /template/ Ellipse#PART_StateLayer", ("Fill", Res($"Fili{c}HoverBrush")))))));
            o.Append(string.Concat(uncheckedStates.Select(u =>
                Style($"^.{cls}{u} /template/ Border#PART_Ripple", ("theme:Ripple.Brush", Res($"Fili{c}Brush"))))));
        }

        o.Append("\n    <!-- `mud-icon-size-small` / `-large`: 1.25rem and 2.25rem against 1.5rem. -->\n");
        o.Append(Style("^.small /template/ LayoutTransformControl#PART_Scale", ("LayoutTransform", "scale(0.8333333)")));
        o.Append(Style("^.large /template/ LayoutTransformControl#PART_Scale", ("LayoutTransform", "scale(1.5)")));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // ToggleSwitch.axaml: MudSwitch, from _switch.scss. Sizes are the `-small` / `-large` rules:
    // track = span minus padding, thumb 14 / 20 / 26px, the halo 20px wider than the thumb.
    // ---------------------------------------------------------------------------------------

    private static readonly (string Size, int TrackWidth, int TrackHeight, int Thumb)[] SwitchSizes =
        [("small", 30, 10, 14), ("large", 38, 18, 26)];

    /// <summary>The thumb's halo and its ripple layer, which move and size together.</summary>
    private static string Halo(string prefix) =>
        $"^{prefix} /template/ Ellipse#PART_ThumbStateLayer, ^{prefix} /template/ Border#PART_ThumbRipple";

    private static string SwitchRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- On: the thumb travels the track width minus its own, 34 - 20 = 14px, and the\n");
        o.Append("         track lifts from 48% to 50%. -->\n");
        o.Append(Style("^:checked /template/ Border#PART_Thumb", ("RenderTransform", "translateX(14px)")));
        o.Append(Style(Halo(":checked"), ("RenderTransform", "translateX(14px)")));
        o.Append(Style("^:checked /template/ Border#PART_Track", ("Opacity", "0.5")));
        o.Append("\n    <!-- `.mud-switch-base:hover`: action-default-hover; keyboard focus and press the same. -->\n");
        o.Append(string.Concat(States.Select(s => Style($"^{s} /template/ Ellipse#PART_ThumbStateLayer", ("Fill", Res("FiliActionDefaultHoverBrush"))))));

        foreach (var (size, trackWidth, trackHeight, thumb) in SwitchSizes)
        {
            var halo = thumb + 20;
            var travel = trackWidth - thumb;
            o.Append($"\n    <!-- `.mud-switch-span-{size}`: a {trackWidth}x{trackHeight} track and a {thumb}px thumb. -->\n");
            o.Append(Style($"^.{size}", ("MinHeight", $"{halo}")));
            o.Append(Style($"^.{size} /template/ Panel#PART_SwitchArea", ("Width", $"{trackWidth + 18}"), ("Height", $"{halo}")));
            o.Append(Style($"^.{size} /template/ Border#PART_Track", ("Width", $"{trackWidth}"), ("Height", $"{trackHeight}")));
            o.Append(Style($"^.{size} /template/ Panel#PART_SwitchKnob", ("Width", $"{trackWidth}"), ("Height", $"{thumb}")));
            o.Append(Style($"^.{size} /template/ Border#PART_Thumb", ("Width", $"{thumb}"), ("Height", $"{thumb}"), ("CornerRadius", $"{thumb / 2}")));
            o.Append(Style(Halo($".{size}"), ("Width", $"{halo}"), ("Height", $"{halo}")));
            o.Append(Style($"^.{size}:checked /template/ Border#PART_Thumb", ("RenderTransform", $"translateX({travel}px)")));
            o.Append(Style(Halo($".{size}:checked"), ("RenderTransform", $"translateX({travel}px)")));
        }

        o.Append("\n    <!-- Colours paint the thumb and the track only when ON, with the colour's hover tint and\n");
        o.Append("         ripple. -->\n");
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls}:checked /template/ Border#PART_Thumb", ("Background", Res($"Fili{c}Brush"))));
            o.Append(Style($"^.{cls}:checked /template/ Border#PART_Track", ("Background", Res($"Fili{c}Brush"))));
            o.Append(string.Concat(States.Select(s => Style($"^.{cls}:checked{s} /template/ Ellipse#PART_ThumbStateLayer", ("Fill", Res($"Fili{c}HoverBrush"))))));
            o.Append(Style($"^.{cls}:checked /template/ Border#PART_ThumbRipple", ("theme:Ripple.Brush", Res($"Fili{c}Brush"))));
        }

        // MudSwitch's UncheckedColor: `mud-{color}-text` on the thumb (its currentColor) and
        // `mud-{color}` on the track, while the switch is off.
        o.Append("\n    <!-- UncheckedColor: `unchecked-{colour}`, the thumb and the track while OFF, with the\n");
        o.Append("         colour's hover tint and ripple. -->\n");
        foreach (var c in Colours)
        {
            var cls = $"unchecked-{c.ToLowerInvariant()}";
            o.Append(Style($"^.{cls}:not(:checked) /template/ Border#PART_Thumb", ("Background", Res($"Fili{c}Brush"))));
            o.Append(Style($"^.{cls}:not(:checked) /template/ Border#PART_Track", ("Background", Res($"Fili{c}Brush"))));
            o.Append(string.Concat(States.Select(s => Style($"^.{cls}:not(:checked){s} /template/ Ellipse#PART_ThumbStateLayer", ("Fill", Res($"Fili{c}HoverBrush"))))));
            o.Append(Style($"^.{cls}:not(:checked) /template/ Border#PART_ThumbRipple", ("theme:Ripple.Brush", Res($"Fili{c}Brush"))));
        }

        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // Slider.axaml: MudSlider, from _slider.scss. `$slider-sizes: small 2, medium 4, large 6` is
    // the rail; the thumb is the rail times `$thumbscale` 6 / 5 / 4 = 12 / 20 / 24px; the ring is
    // 1px on hover and 2px on focus and press. Thumb Padding is (40 - thumb) / 2, and the ring's
    // margin is that minus the ring width.
    // ---------------------------------------------------------------------------------------

    private static readonly (string Size, int Rail, int Thumb)[] SliderSizes = [("medium", 4, 20), ("large", 6, 24)];

    private static string SliderStates(string prefix, int inset) =>
        Style($"^{prefix} /template/ Thumb#PART_Thumb:pointerover", ("BorderThickness", $"{inset - 1}"))
        + Style($"^{prefix} /template/ Thumb#PART_Thumb:pressed", ("BorderThickness", $"{inset - 2}"))
        + Style($"^{prefix}:focus-visible /template/ Thumb#PART_Thumb", ("BorderThickness", $"{inset - 2}"));

    private static string SliderRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Small, the default: a 12px thumb, so the knob is inset 14px. -->\n");
        o.Append(SliderStates("", 14));
        o.Append("\n    <!-- Colours: the thumb, the rail at 30%, the filled part and the ticks all follow\n");
        o.Append("         Foreground; the thumb's own Foreground is the value label's text. -->\n");

        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls}", ("Foreground", Res($"Fili{c}Brush"))));
            o.Append(Style($"^.{cls} /template/ Thumb#PART_Thumb", ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
        }

        foreach (var (size, rail, thumb) in SliderSizes)
        {
            var inset = (40 - thumb) / 2;
            o.Append($"\n    <!-- `.mud-slider-{size}`: a {rail}px rail and a {thumb}px thumb. -->\n");
            o.Append(Style($"^.{size}:horizontal /template/ RepeatButton", ("Height", $"{rail}")));
            o.Append(Style($"^.{size}:vertical /template/ RepeatButton", ("Width", $"{rail}")));
            o.Append(Style($"^.{size}:horizontal /template/ Border#PART_Rail", ("Height", $"{rail}")));
            o.Append(Style($"^.{size}:vertical /template/ Border#PART_Rail", ("Width", $"{rail}")));
            o.Append(Style($"^.{size}:horizontal /template/ ItemsControl#PART_TickMarks", ("Height", $"{rail}")));
            o.Append(Style($"^.{size}:vertical /template/ ItemsControl#PART_TickMarks", ("Width", $"{rail}")));
            o.Append(Style($"^.{size} /template/ Thumb#PART_Thumb", ("Padding", $"{inset}"), ("BorderThickness", $"{inset}")));
            o.Append(SliderStates($".{size}", inset));
        }

        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // ProgressBar.axaml: the @each $color loop. Track and bar are the same colour; the track sits
    // at 20% through the template's own opacity.
    // ---------------------------------------------------------------------------------------

    private static string ProgressRegion() =>
        string.Concat(Colours.Select(c => Style($"^.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}Brush")))));

    // ProgressCircular.axaml: `mud-{color}-text` on MudProgressCircular's root. The ring strokes in
    // the foreground and there is no track, so a colour is the foreground and nothing else.
    private static string CircularRegion() =>
        string.Concat(Colours.Select(c => Style($"^.{c.ToLowerInvariant()}", ("Foreground", Res($"Fili{c}Brush")))));

    // ---------------------------------------------------------------------------------------
    // Chip.axaml: MudChip, from _chip.scss. Filled is the default variant. The 12% tint of a text
    // chip is the colour itself on the state layer at Opacity 0.12, since it has no token.
    // Selected (ToggleButton :checked) is MudChip.GetVariant's swap: filled draws as text, text
    // draws as filled, outlined keeps its line and gains the colour's tint.
    // ---------------------------------------------------------------------------------------

    private static string ChipStates(string prefix, string part, params (string, string)[] setters) =>
        string.Concat(States.Select(s => Style($"^{prefix}{s} /template/ Border#{part}", setters)));

    /// <summary>The text look for one colour (or none): 6% fill, the colour as text, 12% on hover.</summary>
    private static string ChipTextLook(string prefix, string? colour)
    {
        var o = new StringBuilder();

        if (colour is null)
        {
            o.Append(Style($"^{prefix}", ("Background", Res("FiliActionDefaultHoverBrush")), ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderThickness", "0")));
            o.Append(ChipStates(prefix, "PART_Root", ("Background", Res("FiliActionDisabledBackgroundBrush"))));
            o.Append(ChipStates(prefix, "PART_StateLayer", ("Background", "Transparent")));
        }
        else
        {
            o.Append(Style($"^{prefix}", ("Background", Res($"Fili{colour}HoverBrush")), ("Foreground", Res($"Fili{colour}Brush")), ("BorderThickness", "0")));
            o.Append(ChipStates(prefix, "PART_Root", ("Background", "Transparent")));
            o.Append(ChipStates(prefix, "PART_StateLayer", ("Background", Res($"Fili{colour}Brush")), ("Opacity", "0.12")));
        }

        return o.ToString();
    }

    /// <summary>The filled look for one colour (or none): the fill, darkening on hover.</summary>
    private static string ChipFilledLook(string prefix, string? colour)
    {
        var o = new StringBuilder();

        if (colour is null)
        {
            o.Append(Style($"^{prefix}", ("Background", Res("FiliActionDisabledBackgroundBrush")), ("Foreground", Res("FiliTextPrimaryBrush"))));
            o.Append(ChipStates(prefix, "PART_Root", ("Background", Res("FiliActionDisabledBrush"))));
        }
        else
        {
            o.Append(Style($"^{prefix}", ("Background", Res($"Fili{colour}Brush")), ("Foreground", Res($"Fili{colour}ContrastTextBrush"))));
            o.Append(ChipStates(prefix, "PART_Root", ("Background", Res($"Fili{colour}DarkenBrush"))));
        }

        o.Append(ChipStates(prefix, "PART_StateLayer", ("Background", "Transparent")));
        return o.ToString();
    }

    /// <summary>
    /// MudToggleItem, which in 9.11 renders a MudButton: Variant.Outlined in the group's colour,
    /// Variant.Filled once selected, and `.mud-toggle-item` drops the button's own border, shadow
    /// and minimum width. Avalonia applies matching styles in ORDER, not by specificity, so the
    /// selected rules come after the unselected ones and disabled comes last.
    /// </summary>
    private static string ToggleRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Unselected, Variant.Outlined: the colour as text, `{color}-hover` under the pointer. -->\n");
        o.Append(ChipStates("", "PART_StateLayer", ("Background", Res("FiliPrimaryHoverBrush"))));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls}", ("Foreground", Res($"Fili{c}Brush"))));
            o.Append(ChipStates($".{cls}", "PART_StateLayer", ("Background", Res($"Fili{c}HoverBrush"))));
        }

        o.Append("\n    <!-- Selected, Variant.Filled: the colour with its contrast text, `{color}-darken` under the\n");
        o.Append("         pointer, and no shadow (`.mud-toggle-group > .mud-toggle-item { box-shadow: none }`). -->\n");
        o.Append(Style("^:checked", ("Background", Res("FiliPrimaryBrush")), ("Foreground", Res("FiliPrimaryContrastTextBrush"))));
        o.Append(ChipStates(":checked", "PART_Root", ("Background", Res("FiliPrimaryDarkenBrush"))));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls}:checked", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
            o.Append(ChipStates($".{cls}:checked", "PART_Root", ("Background", Res($"Fili{c}DarkenBrush"))));
        }

        o.Append(ChipStates(":checked", "PART_StateLayer", ("Background", "Transparent")));

        o.Append("\n    <!-- `.mud-toggle-group-outlined`: a 1px line in the colour (`border-opacity` is 1), absorbed\n");
        o.Append("         into the padding, so an outlined item is 5px (3px small, 7px large). -->\n");
        o.Append(Style("^.outlined", ("BorderBrush", Res("FiliPrimaryBrush")), ("BorderThickness", "1"), ("Padding", "5")));
        foreach (var c in Colours)
        {
            o.Append(Style($"^.outlined.{c.ToLowerInvariant()}", ("BorderBrush", Res($"Fili{c}Brush"))));
        }

        o.Append("\n    <!-- `.mud-toggle-item-size-*`: 4px / 8px, at the button's 13px / 15px. -->\n");
        o.Append(Sizes("4", "8"));
        o.Append(Style("^.outlined.small", ("Padding", "3")));
        o.Append(Style("^.outlined.large", ("Padding", "7")));

        o.Append("\n    <!-- Disabled, as MudButton: action-disabled text, and an action-disabled-background ground\n");
        o.Append("         or line where the variant has one. -->\n");
        o.Append(Style("^:disabled /template/ ContentPresenter#PART_ContentPresenter", ("Foreground", Res("FiliActionDisabledBrush"))));
        o.Append(Style("^:disabled /template/ Border#PART_StateLayer", ("Background", "Transparent")));
        o.Append(Style("^:checked:disabled", ("Background", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^.outlined:disabled", ("BorderBrush", Res("FiliActionDisabledBackgroundBrush"))));
        return o.ToString();
    }

    /// <summary>MudLink's Color, `mud-{color}-text`: every palette colour as the text.</summary>
    private static string LinkRegion() =>
        string.Concat(Colours.Select(c => Style($"^.{c.ToLowerInvariant()}", ("Foreground", Res($"Fili{c}Brush")))));

    /// <summary>
    /// MudList's and MudTreeView's Color on the selected row: the colour as the text over its
    /// hover tint. Styles, since they reach from the list to its rows, so they sit at the top
    /// level of FiliTheme.axaml; Primary is the rows' own theme. A list row's text is its
    /// Foreground and its theme deepens a selected row under the pointer, which a Style would
    /// otherwise override; a tree row's text is its header presenter's.
    /// </summary>
    private static string SelectionRegion(string owner, string row, string ground, bool list)
    {
        var o = new StringBuilder();
        foreach (var c in Colours.Where(c => c != "Primary"))
        {
            var sel = $"{owner}.{c.ToLowerInvariant()} {row}:selected";
            o.Append(list
                ? Style(sel, ("Foreground", Res($"Fili{c}Brush")))
                : Style($"{sel} /template/ ContentPresenter#PART_HeaderPresenter", ("Foreground", Res($"Fili{c}Brush"))));
            o.Append(Style($"{sel} /template/ {ground}", ("Background", Res($"Fili{c}HoverBrush"))));
            if (list)
            {
                o.Append(Style($"{sel}:pointerover /template/ {ground}", ("Background", Res("FiliOverlayPressedBrush"))));
            }
        }

        return TopLevel(o.ToString());
    }

    /// <summary>
    /// MudNavMenu's Color beyond the tree's selection colours: the active link's line and icon,
    /// an open group's arrow, and the colour at 12% under the pointer or keyboard focus. That 12%
    /// replaces the 6% hover tint in MudBlazor; here it is laid over it, so the layer is 6.4%,
    /// which with the 6% under it makes 12%.
    /// </summary>
    private static string NavRegion()
    {
        var o = new StringBuilder();
        foreach (var c in Colours.Where(c => c != "Primary"))
        {
            var nav = $"TreeView.nav.{c.ToLowerInvariant()} TreeViewItem";
            o.Append(Style($"{nav}:selected /template/ Border#SelectionBorder", ("BorderBrush", Res($"Fili{c}Brush"))));
            o.Append(Style($"{nav}:selected", ("theme:IconSlot.Brush", Res($"Fili{c}Brush"))));
            o.Append(Style($"{nav}[IsExpanded=True] /template/ Path#PART_ExpandIcon", ("Fill", Res($"Fili{c}Brush"))));
            o.Append(Style($"{nav}:selected /template/ Border#PART_Tint:pointerover", ("Background", Res($"Fili{c}Brush")), ("Opacity", "0.064")));
            o.Append(Style($"{nav}:selected:focus-visible /template/ Border#PART_Tint", ("Background", Res($"Fili{c}Brush")), ("Opacity", "0.064")));
        }

        return TopLevel(o.ToString());
    }

    /// <summary>A region in FiliTheme.axaml's top-level Styles is indented two spaces less.</summary>
    private static string TopLevel(string styles) =>
        styles.Replace("    <Style", "  <Style").Replace("      <Setter", "    <Setter").Replace("    </Style>", "  </Style>");

    private static string ChipRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Filled, the default: action-disabled-background, action-disabled on hover. -->\n");
        o.Append(ChipStates("", "PART_Root", ("Background", Res("FiliActionDisabledBrush"))));
        foreach (var c in Colours)
        {
            o.Append(ChipFilledLook($".{c.ToLowerInvariant()}", c));
        }

        o.Append("\n    <!-- Outlined: lines-inputs or the colour as a 1px line, the tint on hover. -->\n");
        o.Append(Style("^.outlined", ("Background", "Transparent"), ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderBrush", Res("FiliLinesInputsBrush")), ("BorderThickness", "1"),
            ("theme:Ripple.Opacity", Res("FiliRippleOpacity"))));
        o.Append(ChipStates(".outlined", "PART_Root", ("Background", "Transparent")));
        o.Append(ChipStates(".outlined", "PART_StateLayer", ("Background", Res("FiliActionDefaultHoverBrush"))));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.outlined.{cls}", ("Background", "Transparent"), ("Foreground", Res($"Fili{c}Brush")), ("BorderBrush", Res($"Fili{c}Brush"))));
            o.Append(ChipStates($".outlined.{cls}", "PART_StateLayer", ("Background", Res($"Fili{c}HoverBrush"))));
        }

        o.Append("\n    <!-- Text: the colour at 6% as fill and the colour as text; 12% on hover. -->\n");
        o.Append(ChipTextLook(".text", null));
        foreach (var c in Colours)
        {
            o.Append(ChipTextLook($".text.{c.ToLowerInvariant()}", c));
        }

        // Only `.mud-chip-filled` raises the ripple to --mud-ripple-opacity-secondary.
        o.Append(Style("^.text", ("theme:Ripple.Opacity", Res("FiliRippleOpacity"))));

        o.Append("\n    <!-- Selected: a filled chip draws as text, a text chip as filled. -->\n");
        o.Append(ChipTextLook(":checked", null));
        foreach (var c in Colours)
        {
            o.Append(ChipTextLook($".{c.ToLowerInvariant()}:checked", c));
        }

        o.Append(ChipFilledLook(".text:checked", null));
        foreach (var c in Colours)
        {
            o.Append(ChipFilledLook($".text.{c.ToLowerInvariant()}:checked", c));
        }

        o.Append("\n    <!-- A selected outlined chip keeps its line and gains the tint, 12% on hover. -->\n");
        o.Append(Style("^.outlined:checked", ("Background", "Transparent"), ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderThickness", "1")));
        o.Append(ChipStates(".outlined:checked", "PART_Root", ("Background", "Transparent")));
        o.Append(ChipStates(".outlined:checked", "PART_StateLayer", ("Background", Res("FiliActionDefaultHoverBrush")), ("Opacity", "1")));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.outlined.{cls}:checked", ("Background", Res($"Fili{c}HoverBrush")), ("Foreground", Res($"Fili{c}Brush")), ("BorderThickness", "1")));
            o.Append(ChipStates($".outlined.{cls}:checked", "PART_Root", ("Background", "Transparent")));
            o.Append(ChipStates($".outlined.{cls}:checked", "PART_StateLayer", ("Background", Res($"Fili{c}Brush")), ("Opacity", "0.12")));
        }

        o.Append("\n    <!-- `.mud-chip-size-small` / `-large`: 24px / 40px, radius half the height. -->\n");
        o.Append(Style("^.small", ("Height", "24"), ("CornerRadius", "12"), ("FontSize", "12"), ("Padding", "8,0")));
        o.Append(Style("^.large", ("Height", "40"), ("CornerRadius", "20"), ("FontSize", "16"), ("Padding", "16,0")));
        o.Append(IconSlots("chip"));
        o.Append("\n    <!-- `.mud-disabled`: half opacity. -->\n");
        o.Append(Style("^:disabled", ("Opacity", "0.5")));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // Fab.axaml: MudFab, from _fab.scss. Filled is the default variant and its ground changes on
    // hover, focus and press - no state layer, since every state has a token. Filled lifts 6 -> 8
    // (hover, focus) -> 12 (press); outlined and text never cast a shadow.
    // ---------------------------------------------------------------------------------------

    private static string FabStates(string prefix, params (string, string)[] setters) =>
        string.Concat(States.Select(s => Style($"^{prefix}{s} /template/ Border#PART_Root", setters)));

    /// <summary>No shadow at rest or in any state.</summary>
    private static string FabGrounded(string prefix) =>
        Style($"^{prefix} /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation0")))
        + FabStates(prefix, ("BoxShadow", Res("FiliElevation0")));

    private static string FabRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Filled, the default: lifts on hover and focus, more on press. -->\n");
        o.Append(Style("^:pointerover /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation8"))));
        o.Append(Style("^:focus-visible /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation8"))));
        o.Append(Style("^:pressed /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation12"))));
        o.Append("\n    <!-- No colour: grey, going to action-disabled-background. -->\n");
        o.Append(FabStates("", ("Background", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append("\n    <!-- `.mud-fab-{color}`: the colour, {color}-text on it, {color}-darken on hover, and the\n");
        o.Append("         ripple at the secondary ripple opacity. -->\n");
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush")),
                ("theme:Ripple.Opacity", Res("FiliRippleOpacitySecondary"))));
            o.Append(FabStates($".{cls}", ("Background", Res($"Fili{c}DarkenBrush"))));
        }

        o.Append("\n    <!-- `.mud-fab-text`: no ground and no shadow; the colour as content, its tint on hover. -->\n");
        o.Append(Style("^.text", ("Background", "Transparent"), ("Foreground", Res("FiliTextPrimaryBrush")),
            ("theme:Ripple.Opacity", Res("FiliRippleOpacity"))));
        o.Append(FabGrounded(".text"));
        o.Append(FabStates(".text", ("Background", Res("FiliActionDefaultHoverBrush"))));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.text.{cls}", ("Background", "Transparent"), ("Foreground", Res($"Fili{c}Brush"))));
            o.Append(FabStates($".text.{cls}", ("Background", Res($"Fili{c}HoverBrush"))));
        }

        o.Append("\n    <!-- `.mud-fab-outlined`: the text look plus a 1px line, text-primary or the colour\n");
        o.Append("         (BorderOpacity is 1.0, so the line IS the colour). -->\n");
        o.Append(Style("^.outlined", ("Background", "Transparent"), ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderBrush", Res("FiliTextPrimaryBrush")), ("BorderThickness", "1"),
            ("theme:Ripple.Opacity", Res("FiliRippleOpacity"))));
        o.Append(FabGrounded(".outlined"));
        o.Append(FabStates(".outlined", ("Background", Res("FiliActionDefaultHoverBrush"))));
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.outlined.{cls}", ("Background", "Transparent"), ("Foreground", Res($"Fili{c}Brush")), ("BorderBrush", Res($"Fili{c}Brush"))));
            o.Append(FabStates($".outlined.{cls}", ("Background", Res($"Fili{c}HoverBrush"))));
        }

        o.Append("\n    <!-- `.mud-fab-size-small` / `-medium`: 40px / 48px. -->\n");
        o.Append(Style("^.small", ("Width", "40"), ("Height", "40"), ("CornerRadius", "20")));
        o.Append(Style("^.medium", ("Width", "48"), ("Height", "48"), ("CornerRadius", "24")));
        o.Append("\n    <!-- `.mud-fab-extended`: a pill as wide as its label, 48 / 40 / 34px high. -->\n");
        o.Append(Style("^.extended", ("Width", "NaN"), ("Height", "48"), ("MinWidth", "48"), ("MinHeight", "0"), ("Padding", "16,0"), ("CornerRadius", "24")));
        o.Append(Style("^.extended.medium", ("Height", "40"), ("MinWidth", "40"), ("CornerRadius", "20")));
        o.Append(Style("^.extended.small", ("Height", "34"), ("MinWidth", "34"), ("Padding", "12,0"), ("CornerRadius", "17")));
        o.Append(IconSlots("fab"));
        o.Append("\n    <!-- `flat` is DropShadow=\"false\": `.mud-fab-disable-elevation` drops the shadow in every state. -->\n");
        o.Append(FabGrounded(".flat"));
        o.Append("\n    <!-- Disabled: action-disabled on action-disabled-background, flat; outlined and text keep\n");
        o.Append("         no ground, and outlined's line turns action-disabled-background. -->\n");
        o.Append(Style("^:disabled /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevation0")), ("Background", Res("FiliActionDisabledBackgroundBrush"))));
        o.Append(Style("^:disabled /template/ ContentPresenter#PART_ContentPresenter", ("Foreground", Res("FiliActionDisabledBrush"))));
        o.Append(Style("^.text:disabled /template/ Border#PART_Root", ("Background", "Transparent")));
        o.Append(Style("^.outlined:disabled /template/ Border#PART_Root", ("Background", "Transparent"), ("BorderBrush", Res("FiliActionDisabledBackgroundBrush"))));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // Badge.axaml: MudBadge, from _badge.scss. Origin hangs PART_BadgeAnchor off one edge per
    // axis - a 10000px panel past the edge by all but 4px (12px with `overlap`), the badge
    // aligned to its near side - or spans a centred axis with 10000px either side. Colour is
    // `.mud-theme-{color}` on the badge alone, never on what it decorates.
    // ---------------------------------------------------------------------------------------

    /// <summary>MudBlazor's Origin values but the default TopRight, as (class, vertical, horizontal).</summary>
    public static readonly (string Class, string Vertical, string Horizontal)[] BadgeOrigins =
    [
        ("top-left", "Top", "Left"), ("top-center", "Top", "Center"),
        ("center-left", "Center", "Left"), ("center-center", "Center", "Center"),
        ("center-right", "Center", "Right"),
        ("bottom-left", "Bottom", "Left"), ("bottom-center", "Bottom", "Center"),
        ("bottom-right", "Bottom", "Right"),
    ];

    /// <summary>How far PART_BadgeAnchor reaches past the edge it hangs from.</summary>
    public const int BadgeReach = 10000;

    /// <summary>
    /// One axis: the anchor's alignment, size and its near and far margins, and the badge's own
    /// alignment inside it. Hung off the right edge, the anchor's left side sits `inset` inside
    /// that edge and the badge hugs it; a centred axis spans the content with BadgeReach either side.
    /// </summary>
    private static (string Align, string Size, int Near, int Far, string Badge) BadgeAxis(string side, int inset) =>
        side switch
        {
            "Right" => ("Right", $"{BadgeReach}", 0, -(BadgeReach - inset), "Left"),
            "Bottom" => ("Bottom", $"{BadgeReach}", 0, -(BadgeReach - inset), "Top"),
            "Left" => ("Left", $"{BadgeReach}", -(BadgeReach - inset), 0, "Right"),
            "Top" => ("Top", $"{BadgeReach}", -(BadgeReach - inset), 0, "Bottom"),
            _ => ("Stretch", "NaN", -BadgeReach, -BadgeReach, "Center"),
        };

    private static string BadgeAnchor(string prefix, string vertical, string horizontal, int inset, bool alignments)
    {
        var (hAlign, width, left, right, hBadge) = BadgeAxis(horizontal, inset);
        var (vAlign, height, top, bottom, vBadge) = BadgeAxis(vertical, inset);
        var margin = $"{left},{top},{right},{bottom}";

        if (!alignments)
        {
            return Style($"^{prefix} /template/ Panel#PART_BadgeAnchor", ("Margin", margin));
        }

        return Style(
                $"^{prefix} /template/ Panel#PART_BadgeAnchor",
                ("HorizontalAlignment", hAlign), ("VerticalAlignment", vAlign),
                ("Width", width), ("Height", height), ("Margin", margin))
            + Style($"^{prefix} /template/ Border#PART_Badge", ("HorizontalAlignment", hBadge), ("VerticalAlignment", vBadge));
    }

    private static string BadgeRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Origin: `.mud-badge-{vertical}.{horizontal}`, calc(100% - 4px) from the opposite edge. -->\n");
        foreach (var (cls, vertical, horizontal) in BadgeOrigins)
        {
            o.Append(BadgeAnchor($".{cls}", vertical, horizontal, 4, alignments: true));
        }

        o.Append("\n    <!-- `.mud-badge-overlap`: calc(100% - 12px), 12px into the corner. A centred axis does not move. -->\n");
        o.Append(BadgeAnchor(".overlap", "Top", "Right", 12, alignments: false));
        foreach (var (cls, vertical, horizontal) in BadgeOrigins.Where(x => (x.Vertical, x.Horizontal) != ("Center", "Center")))
        {
            o.Append(BadgeAnchor($".overlap.{cls}", vertical, horizontal, 12, alignments: false));
        }

        o.Append("\n    <!-- `.mud-theme-{color}`: the colour, with its contrast text. -->\n");
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(Style($"^.{cls} /template/ Border#PART_Badge", ("Background", Res($"Fili{c}Brush"))));
            o.Append(Style($"^.{cls} /template/ ContentPresenter#PART_HeaderPresenter", ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
        }

        o.Append("\n    <!-- `mud-elevation-{n}`, on the badge. -->\n");
        foreach (var level in Elevations)
        {
            o.Append(Style($"^.elevation{level} /template/ Border#PART_Badge", ("BoxShadow", Res($"FiliElevation{level}"))));
        }

        return o.ToString();
    }

    // Optional/DataGrid/DataGrid.axaml: MudDataGrid's Elevation, on the Border its template draws the table with.
    private static string DataGridRegion() => ElevationRegion("DataGridBorder");

    /// <summary>`.mud-elevation-{n}` inside a control theme, on the named Border of its template.</summary>
    private static string ElevationRegion(string part) =>
        string.Concat(Elevations.Select(level => Style($"^.elevation{level} /template/ Border#{part}", ("BoxShadow", Res($"FiliElevation{level}")))));

    // FiliTheme.axaml: `.mud-elevation-{n}` on a Border - a paper, a card, an app bar.
    private static string PaperRegion() =>
        string.Concat(Elevations.Select(level => TopStyle($"Border.elevation{level}", ("BoxShadow", Res($"FiliElevation{level}")))));

    /// <summary>
    /// A control with `material-elevation-overlay` on, read on the control itself. The class sets
    /// ElevationOverlay.IsOn, which inherits: a descendant selector ending in a Border is matched
    /// against every Border in the window, watching each one's ancestors, and cost more than the
    /// rest of the theme's top-level styles together.
    /// </summary>
    private static string Overlay(string control) => $"{control}[(theme|ElevationOverlay.IsOn)=True]";

    // FiliTheme.axaml: `material-elevation-overlay`, Material's dark-theme overlay, on the surfaces
    // that paint their own surface colour - a paper or card, an expansion panel, and the popups'
    // elevation-8 sheets. A default level comes before the classes, so an elevation class wins.
    private static string OverlayRegion()
    {
        var o = new StringBuilder();
        o.Append(TopStyle($"{Overlay("Border.paper")}, {Overlay("Border.card")}", ("BoxShadow", Res("FiliElevationOverlay1"))));
        foreach (var level in Elevations)
        {
            o.Append(TopStyle($"{Overlay($"Border.paper.elevation{level}")}, {Overlay($"Border.card.elevation{level}")}", ("BoxShadow", Res($"FiliElevationOverlay{level}"))));
        }

        o.Append(TopStyle($"{Overlay("Expander")} /template/ Border#PART_Root", ("BoxShadow", Res("FiliElevationOverlay1"))));
        foreach (var level in Elevations)
        {
            o.Append(TopStyle($"{Overlay($"Expander.elevation{level}")} /template/ Border#PART_Root", ("BoxShadow", Res($"FiliElevationOverlay{level}"))));
        }

        o.Append(TopStyle(
            $"{Overlay("MenuItem")} /template/ Border#PART_PopupBorder, {Overlay("ComboBox")} /template/ Border#PART_PopupBorder, "
            + $"{Overlay("AutoCompleteBox")} /template/ Border#PART_PopupBorder, {Overlay("ContextMenu")} /template/ Border#PART_Border, "
            + $"{Overlay("FlyoutPresenter")} /template/ Border#PART_Border, {Overlay("MenuFlyoutPresenter")} /template/ Border#PART_Border",
            ("BoxShadow", Res("FiliElevationOverlay8"))));
        return o.ToString();
    }

    // Optional/DataGrid/DataGridStyles.axaml: the overlay on a grid, which is a surface at
    // elevation 1 unless outlined - an outlined grid has no shadow and gets no overlay.
    private static string DataGridOverlayRegion()
    {
        var o = new StringBuilder();
        o.Append(TopStyle($"{Overlay("DataGrid")}:not(.outlined) /template/ Border#DataGridBorder", ("BoxShadow", Res("FiliElevationOverlay1"))));
        foreach (var level in Elevations)
        {
            o.Append(TopStyle($"{Overlay($"DataGrid.elevation{level}")}:not(.outlined) /template/ Border#DataGridBorder", ("BoxShadow", Res($"FiliElevationOverlay{level}"))));
        }

        return o.ToString();
    }

    /// <summary>
    /// The elevation classes the theme publishes: every level _elevation.scss has, `mud-elevation-0`
    /// through `mud-elevation-25`.
    /// </summary>
    public static readonly int[] Elevations = [.. Enumerable.Range(0, 26)];

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudAvatar's Color, from _avatar.scss. Filled is the colour with its
    // contrast text; outlined the colour as text and as a 1px line.
    // ---------------------------------------------------------------------------------------

    private static string AvatarRegion()
    {
        var o = new StringBuilder();
        foreach (var c in Colours)
        {
            o.Append(TopStyle($"Border.avatar.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}Brush")), ("TextElement.Foreground", Res($"Fili{c}ContrastTextBrush"))));
        }

        foreach (var c in Colours)
        {
            o.Append(TopStyle($"Border.avatar.outlined.{c.ToLowerInvariant()}", ("Background", "Transparent"), ("TextElement.Foreground", Res($"Fili{c}Brush")), ("BorderBrush", Res($"Fili{c}Brush"))));
        }

        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudAlert's Severity, from _alert.scss. Text and outlined alerts draw
    // their text in the colour's darken shade and the icon in the colour; filled ones the colour
    // with its contrast text.
    // ---------------------------------------------------------------------------------------

    private static string TopStyle(string selector, params (string Property, string Value)[] setters) =>
        "  <Style Selector=\"" + Parts(selector) + "\">\n"
        + string.Concat(setters.Select(s => $"    <Setter Property=\"{s.Property}\" Value=\"{s.Value}\" />\n"))
        + "  </Style>\n";

    private static string AlertRegion()
    {
        var o = new StringBuilder();
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(TopStyle($"Border.alert.{cls}", ("Background", Res($"Fili{c}HoverBrush")), ("TextElement.Foreground", Res($"Fili{c}DarkenBrush"))));
            o.Append(TopStyle($"Border.alert.{cls} PathIcon", ("Foreground", Res($"Fili{c}Brush"))));
        }

        foreach (var c in Colours)
        {
            o.Append(TopStyle($"Border.alert.outlined.{c.ToLowerInvariant()}", ("Background", "Transparent"), ("BorderBrush", Res($"Fili{c}Brush"))));
        }

        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(TopStyle($"Border.alert.filled.{cls}", ("Background", Res($"Fili{c}Brush")), ("TextElement.Foreground", Res($"Fili{c}ContrastTextBrush"))));
            o.Append(TopStyle($"Border.alert.filled.{cls} PathIcon", ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
        }

        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudTabs' Color, from _tabs.scss's .mud-tabs-tabbar-{color}: the bar
    // in the colour, every tab and the indicator in its contrast text, a disabled tab still
    // text-disabled, and {color}-lighten on the active tab's hover. A tab's theme cannot see its
    // parent's class, so these are child selectors here rather than nested styles in the theme.
    // ---------------------------------------------------------------------------------------

    private static string TabsRegion()
    {
        var o = new StringBuilder();
        foreach (var c in Colours)
        {
            var cls = c.ToLowerInvariant();
            o.Append(TopStyle($"TabControl.{cls} /template/ Border#PART_TabBar", ("Background", Res($"Fili{c}Brush"))));
            o.Append(TopStyle($"TabStrip.{cls}", ("Background", Res($"Fili{c}Brush"))));

            foreach (var (parent, item) in new[] { ("TabControl", "TabItem"), ("TabStrip", "TabStripItem") })
            {
                var tab = $"{parent}.{cls} > {item}";
                o.Append(TopStyle(tab, ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
                o.Append(TopStyle($"{tab}:selected", ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
                o.Append(TopStyle($"{tab}:disabled", ("Foreground", Res("FiliTextDisabledBrush"))));
                o.Append(TopStyle($"{tab}:selected /template/ Border#PART_Indicator", ("Background", Res($"Fili{c}ContrastTextBrush"))));
                o.Append(TopStyle($"{tab}:selected:pointerover /template/ Border#PART_StateLayer", ("Background", Res($"Fili{c}LightenBrush"))));
                o.Append(TopStyle($"{tab}:selected:focus-visible /template/ Border#PART_StateLayer", ("Background", Res($"Fili{c}LightenBrush"))));
            }
        }

        // SliderColor, after the bar colours so it wins over a coloured bar's contrast indicator:
        // `.mud-tab-slider.mud-{color}`, the indicator alone in the colour.
        foreach (var c in Colours)
        {
            var cls = $"indicator-{c.ToLowerInvariant()}";
            o.Append(TopStyle($"TabControl.{cls} > TabItem /template/ Border#PART_Indicator", ("Background", Res($"Fili{c}Brush"))));
            o.Append(TopStyle($"TabStrip.{cls} > TabStripItem:selected /template/ Border#PART_Indicator", ("Background", Res($"Fili{c}Brush"))));
        }

        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudAppBar's Color, which is .mud-theme-{color} - the colour as the
    // ground and its contrast text, inherited by the bar's content.
    // ---------------------------------------------------------------------------------------

    private static string AppBarRegion() =>
        string.Concat(Colours.Select(c => TopStyle(
            $"Border.appbar.{c.ToLowerInvariant()}",
            ("Background", Res($"Fili{c}Brush")),
            ("TextElement.Foreground", Res($"Fili{c}ContrastTextBrush")))));

    // ---------------------------------------------------------------------------------------
    // ToggleButton.axaml region: MudToggleIconButton, which renders a MudIconButton with
    // Toggled ? (ToggledColor ?? Color) : Color. So the icon button's three themes, retargeted at
    // ToggleButton, with each colour's rules hung on `:checked` (ToggledColor) and on
    // `unchecked-{color}` while unchecked (Color).
    // ---------------------------------------------------------------------------------------

    private static string ToggleIconRegion()
    {
        static string[] Toggled(string c) => [$".{c}:checked", $".unchecked-{c}:unchecked"];

        return "\n" + TextTheme("FiliToggleIconButton", "ToggleButton", Toggled)
            + OutlinedTheme("FiliOutlinedToggleIconButton", "ToggleButton", Toggled, "FiliToggleIconButton")
            + FilledTheme("FiliFilledToggleIconButton", "ToggleButton", Toggled);
    }

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudIcon's Size and Color on a PathIcon. `.mud-icon-size-*` is
    // 1.25 / 1.5 / 2.25rem; a colour is `.mud-{color}-text`.
    // ---------------------------------------------------------------------------------------

    private static string IconRegion() =>
        TopStyle("PathIcon.small", ("Width", "20"), ("Height", "20"))
        + TopStyle("PathIcon.medium", ("Width", "24"), ("Height", "24"))
        + TopStyle("PathIcon.large", ("Width", "36"), ("Height", "36"))
        + string.Concat(Colours.Select(c => TopStyle($"PathIcon.{c.ToLowerInvariant()}", ("Foreground", Res($"Fili{c}Brush")))));

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudDrawer's Color, `.mud-theme-{color}`: the pane in the colour,
    // its content in the contrast text.
    // ---------------------------------------------------------------------------------------

    // Overlays.axaml region: MudTooltip's Color, `.mud-theme-{color}` on the chip. The arrow's
    // fill is the Background, so it follows.
    private static string TooltipRegion() =>
        string.Concat(Colours.Select(c => Style($"^.{c.ToLowerInvariant()}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush")))));

    // ToggleSwitch.axaml region: MudSwitch's ThumbIconColor, `thumb-{color}` on the switch.
    private static string ThumbIconRegion() =>
        string.Concat(Colours.Select(c => Style($"^.thumb-{c.ToLowerInvariant()} /template/ Path#PART_ThumbIcon", ("Fill", Res($"Fili{c}Brush")))));

    // FiliTheme.axaml region: MudList's and MudTreeView's CheckBoxColor, the list's colour class
    // while `multi-selection` is on. MudBlazor passes it as both Color and UncheckedColor, so both
    // glyphs and the halo take it; `.mud-treeview-checked-{color}` does the same for a tree. Before
    // the disabled rule, which must win.
    private static string MultiSelectionRegion()
    {
        var o = new StringBuilder();
        foreach (var c in Colours)
        {
            var lc = c.ToLowerInvariant();
            string Both(string tail) =>
                $"ListBox.multi-selection.{lc} ListBoxItem /template/ {tail}, TreeView.multi-selection.{lc} TreeViewItem /template/ {tail}";
            o.Append(TopStyle(Both("Panel#PART_CheckBox > Path"), ("Fill", Res($"Fili{c}Brush"))));
            o.Append(TopStyle(Both("Panel#PART_CheckBox:pointerover > Ellipse#PART_CheckHalo"), ("Fill", Res($"Fili{c}HoverBrush"))));
        }

        return o.ToString();
    }

    // FiliTheme.axaml: MudDrawer's Elevation, on the Border the fork's pane carries for it.
    private static string DrawerElevationRegion() =>
        string.Concat(Elevations.Select(level => TopStyle($"SplitView.elevation{level} /template/ Border#PART_PaneShadow", ("BoxShadow", Res($"FiliElevation{level}")))));

    private static string DrawerRegion() =>
        string.Concat(Colours.Select(c => TopStyle($"SplitView.{c.ToLowerInvariant()}", ("PaneBackground", Res($"Fili{c}Brush")))
            + TopStyle($"SplitView.{c.ToLowerInvariant()} /template/ ContentPresenter#PART_PanePresenter", ("TextElement.Foreground", Res($"Fili{c}ContrastTextBrush")))));

    // ---------------------------------------------------------------------------------------
    // Notifications.axaml region: MudSnackbar's Severity and SnackbarVariant. A snackbar is
    // `mud-alert-{variant}-{severity}`, plus `.mud-snackbar-surface` on a text or outlined one -
    // whose surface ground beats the alert's tint. NotificationType has four severities; Normal
    // is the `normal` class, filled `dark` and text-primary otherwise.
    // ---------------------------------------------------------------------------------------

    private static readonly (string Pseudo, string Colour)[] Severities =
        [(":information", "Info"), (":success", "Success"), (":warning", "Warning"), (":error", "Error")];

    private static string SnackbarRegion()
    {
        var o = new StringBuilder();
        o.Append("    <!-- Filled, the default: the colour, its contrast text, weight 500. -->\n");
        foreach (var (pseudo, c) in Severities)
        {
            o.Append(Style($"^{pseudo}", ("Background", Res($"Fili{c}Brush")), ("Foreground", Res($"Fili{c}ContrastTextBrush"))));
        }

        o.Append(Style("^.normal", ("Background", Res("FiliDarkBrush")), ("Foreground", Res("FiliDarkContrastTextBrush"))));
        o.Append("\n    <!-- Text and outlined: the surface, the text in the colour's darken shade at 400, and an\n");
        o.Append("         outlined card's 1px line in the colour. The close button is action-default. -->\n");
        foreach (var variant in new[] { ".text", ".outlined" })
        {
            o.Append(Style($"^{variant}", ("Background", Res("FiliSurfaceBrush")), ("FontWeight", "Normal")));
            foreach (var (pseudo, c) in Severities)
            {
                o.Append(Style($"^{variant}{pseudo}", ("Foreground", Res($"Fili{c}DarkenBrush")), ("BorderBrush", Res($"Fili{c}Brush"))));
            }

            o.Append(Style($"^{variant}.normal", ("Foreground", Res("FiliTextPrimaryBrush")), ("BorderBrush", Res("FiliTextPrimaryBrush"))));
            o.Append(Style($"^{variant} /template/ Panel#PART_Action", ("TextElement.Foreground", Res("FiliActionDefaultBrush"))));
        }

        o.Append(Style("^.outlined", ("BorderThickness", "1")));
        return o.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // FiliTheme.axaml region: MudText's Color, one class per palette colour.
    // ---------------------------------------------------------------------------------------

    private static string TextBlockRegion() =>
        string.Concat(Colours.Select(c =>
            $"  <Style Selector=\"TextBlock.{c.ToLowerInvariant()}\">\n"
            + $"    <Setter Property=\"Foreground\" Value=\"{Res($"Fili{c}Brush")}\" />\n"
            + "  </Style>\n"));
}
