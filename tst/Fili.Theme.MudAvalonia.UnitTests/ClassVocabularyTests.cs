using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Every style class this theme claims, pinned.
///
/// <para>
/// This is the one risk the gallery cannot show, because the gallery has no app styles to collide
/// with. The class names are MudBlazor's vocabulary on purpose — <c>primary</c> is
/// <c>Color.Primary</c>, <c>filled</c> is <c>Variant.Filled</c> — and they are deliberately not
/// namespaced, so an adopter that already uses one of these words has its controls restyled the
/// moment it includes the theme.
/// </para>
///
/// <para>
/// A collision does not fail loudly. The app wins for the setters it declares, and every setter
/// it does NOT declare leaks through from the theme. One adopting app hit this twice: `primary`
/// in 20 places, and `h2` rendering 19px headings inside a 72px line box, because the app set
/// FontSize while the theme supplied LineHeight.
/// </para>
///
/// <para>
/// So the list below is a published interface rather than an implementation detail. Adding a
/// class costs adopters something, and this test is what makes that deliberate — as well as
/// giving an adopter one place to grep against. Note how ordinary the words are: <c>small</c>,
/// <c>flat</c>, <c>light</c>, <c>vertical</c>, <c>error</c>. That is the whole hazard.
/// </para>
/// </summary>
public class ClassVocabularyTests
{
    private static readonly string[] Vocabulary =
    [
        // Type ramp, on TextBlock: every Typo but inherit.
        "body1", "body2", "button", "caption", "h1", "h2", "h3", "h4", "h5", "h6",
        "overline", "subtitle1", "subtitle2",

        // Surfaces, on Border, and the Square of MudCard, MudAvatar and MudExpansionPanels, which
        // reach the card's media, the avatar's image and every panel. Elsewhere Square is
        // CornerRadius="0", and no class.
        "appbar", "square", "paper",

        // MudOverlay, on a Border: DarkBackground and LightBackground.
        "overlay", "dark-background", "light-background",

        // Elevation: every level _elevation.scss has, mud-elevation-0 to mud-elevation-25.
        "elevation0", "elevation1", "elevation2", "elevation3", "elevation4", "elevation5",
        "elevation6", "elevation7", "elevation8", "elevation9", "elevation10", "elevation11",
        "elevation12", "elevation13", "elevation14", "elevation15", "elevation16", "elevation17",
        "elevation18", "elevation19", "elevation20", "elevation21", "elevation22", "elevation23",
        "elevation24", "elevation25",

        // Colour, from MudBlazor's Color enum.
        "dark", "error", "info", "inherit", "primary", "secondary", "success", "tertiary",
        "warning",

        // Shape, from MudBlazor's Variant enum and the per-control parameters.
        "filled", "flat", "outlined", "rounded", "text",

        // Size, from MudBlazor's Size enum.
        "dense", "large", "medium", "small",

        // Placement and decoration, from MudDivider, MudLink and MudTabs.
        "border", "centered", "hide-indicator", "inset", "light", "middle", "no-underline", "underline",
        "vertical",

        // Material's fixed tabs, which MudTabs does not offer, named for where they come from so
        // that no one takes the class for MudBlazor's.
        "material-fixed-tabs",

        // Material's dark-theme elevation overlay, which MudBlazor does not offer.
        "material-elevation-overlay",

        // Components that are a class on an existing control: MudIconButton, MudChip and MudFab
        // on a Button, MudAlert, MudAvatar, MudCard, MudSkeleton and MudToolBar on a Border, with
        // MudSkeleton's SkeletonType, MudProgressCircular on a ProgressBar, MudBadge on a
        // HeaderedContentControl, MudNavMenu on a TreeView and MudExpansionPanels on a StackPanel.
        "alert", "avatar", "badge", "card", "chip", "circle", "circular", "counter",
        "expansion-panels", "fab", "icon", "nav", "rectangle", "skeleton", "toolbar",

        // MudSnackbar's Severity.Normal, on a notification card.
        "normal",

        // MudDrawerHeader, after its CSS class.
        "drawer-header",

        // MudCard's parts, after the CSS classes that draw them.
        "card-actions", "card-content", "card-header", "card-header-actions",
        "card-header-avatar", "card-header-content", "card-media",

        // MudFab's Label, and MudBadge's Dot, Bordered, Overlap and Origin.
        "bordered", "dot", "extended", "overlap",
        "bottom-center", "bottom-left", "bottom-right", "center-center", "center-left",
        "center-right", "top-center", "top-left",

        // Behaviour parameters named as MudBlazor names them: HelperTextOnFocus, Required and
        // ShrinkLabel on a field, MudSkeleton's Animation.False and Animation.Wave, MudDataGrid's
        // Hover, Gutters="false", HideIcon, MudSnackbar's ShowCloseIcon="false",
        // MudExpansionPanels' Outlined="false" and MudTabs' SliderAnimation="false", which is
        // no-animation too.
        "helper-on-focus", "hide-close", "hide-icon", "hover", "no-animation", "no-gutters",
        "no-outline", "required", "shrink-label", "wave",

        // MudSlider's ValueLabel, MudTooltip's Arrow, MudSelect's Dense on its options,
        // SelectionMode.MultiSelection on MudList and MudTreeView, and MudExpansionPanels'
        // MultiExpansion.
        "arrow", "dense-items", "multi-expansion", "multi-selection", "value-label",

        // LabelPlacement on MudCheckBox, MudRadio and MudSwitch; End is the default.
        "label-bottom", "label-start", "label-top",

        // MudCheckBox's and MudRadio's UncheckedColor, one per palette colour.
        "unchecked-dark", "unchecked-error", "unchecked-info", "unchecked-primary",
        "unchecked-secondary", "unchecked-success", "unchecked-tertiary", "unchecked-warning",

        // MudSwitch's ThumbIconColor, one per palette colour.
        "thumb-dark", "thumb-error", "thumb-info", "thumb-primary",
        "thumb-secondary", "thumb-success", "thumb-tertiary", "thumb-warning",

        // MudTabs' SliderColor, one per palette colour.
        "indicator-dark", "indicator-error", "indicator-info", "indicator-primary",
        "indicator-secondary", "indicator-success", "indicator-tertiary", "indicator-warning",

        // MudProgressLinear's Striped, and MudButton's and MudChip's icon placement.
        "end-icon", "start-icon", "striped",
    ];

    /// <summary>
    /// Classes the theme reads in code rather than in a selector: IconSlot sets a host's icon size,
    /// gap and colour on the icon that carries one, since a style ending in the icon would be
    /// matched against every control in the window.
    /// </summary>
    private static readonly string[] ReadInCode = ["end-icon", "start-icon"];

    /// <summary>
    /// The classes the theme actually uses must match the declared list exactly, in both
    /// directions. An undeclared class is a collision nobody signed off on; a declared one that
    /// no selector uses means the list is lying to adopters.
    /// </summary>
    [Fact]
    public Task TheClaimedVocabularyIsExactlyWhatTheThemeUses() => UiThread.RunAsync(() =>
    {
        var selectors = new List<string>();

        // Top-level styles: the type ramp, the surface classes, the drawer, the pickers.
        Walk(Application.Current!.Styles, selectors);

        // And the classes declared INSIDE control themes, which live in a resource dictionary
        // rather than in the styles collection. Those are the variant names, which are most of
        // them — walking only Application.Styles finds 28 of the 44.
        foreach (var key in StandaloneReadinessTests.ThemeKeys)
        {
            Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value);
            Walk(((ControlTheme)value!).Children, selectors);
        }

        var used = selectors
            // Attribute selectors carry a dotted property path —
            // [(DataValidationErrors.HasErrors)=True] — which otherwise reads as a class.
            .Select(s => Regex.Replace(s, @"\[[^\]]*\]", string.Empty))
            .SelectMany(s => Regex.Matches(s, @"\.([A-Za-z][A-Za-z0-9-]*)").Select(m => m.Groups[1].Value))
            .Concat(ReadInCode)
            .Distinct()
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(Vocabulary.OrderBy(c => c, StringComparer.Ordinal).ToArray(), used);
    });

    /// <summary>
    /// No class is one fixed value on its own control. MudPaper's Square is
    /// <c>CornerRadius="0"</c> and MudDataGrid's Bordered <c>GridLinesVisibility="All"</c>: the
    /// property says it, and a class that says it again is one more word to collide with. A class
    /// earns its place on a control by reaching a template part or a child, by carrying a token,
    /// by setting more than one property, by a value that changes with another class -
    /// <c>inset</c> moves to the top of a <c>vertical</c> divider, <c>no-gutters</c> on a row
    /// halves with <c>dense</c> - or by being one of a family that has to work alike everywhere.
    /// </summary>
    /// <remarks>
    /// Counted per selector, by its control and the classes it names, so <c>Border.card.square</c>
    /// and <c>Border.card.square Border.card-media</c> are one use and <c>Border.alert.square</c>
    /// another. A pseudo-class is a state of the same control.
    /// </remarks>
    [Fact]
    public Task NoClassIsOnePropertyOfItsOwnControl() => UiThread.RunAsync(() =>
    {
        var styles = new List<(string Selector, Style Style, string Target)>();
        WalkStyles(Application.Current!.Styles, "*", styles);
        foreach (var key in StandaloneReadinessTests.ThemeKeys)
        {
            Application.Current!.TryFindResource(key, ThemeVariant.Light, out var value);
            var theme = (ControlTheme)value!;
            WalkStyles(theme.Children, theme.TargetType?.Name ?? "*", styles);
        }

        // Every selector, by the control and the classes it names before any descendant or part.
        var uses = styles
            .SelectMany(s => s.Selector.Split(',').Select(part => (Part: part.Trim(), s.Style, s.Target)))
            .Select(s => (Control: ControlOf(s.Part, s.Target), Classes: ClassesOf(s.Part).Order().ToArray(), s.Part, s.Style))
            .Where(s => s.Classes.Length > 0)
            .GroupBy(s => $"{s.Control}.{string.Join(".", s.Classes)}")
            .Select(g => (g.Key, g.First().Control, g.First().Classes, Uses: g.ToArray(),
                Setters: g.SelectMany(s => s.Style.Setters.OfType<Setter>()).ToArray()))
            .ToArray();

        // One fixed value of the control's own: what a property on it would say.
        var single = uses
            .Where(u => u.Uses.All(s => IsOwn(s.Part)) && u.Setters.All(Fixed))
            .Where(u => u.Setters.Select(x => (x.Property, x.Value)).Distinct().Count() == 1)
            .ToArray();

        var redundant = single
            .Where(u => !u.Classes.Any(Families.Contains))
            // Unless another such class combined with it gives another value: `inset` is 72px at
            // the left, and `vertical inset` 72px at the top, so the one class follows the other.
            .Where(u => !single.Any(v => v.Key != u.Key && v.Control == u.Control
                && (v.Classes.All(u.Classes.Contains) || u.Classes.All(v.Classes.Contains))
                && v.Setters[0].Property == u.Setters[0].Property && !Equals(v.Setters[0].Value, u.Setters[0].Value)))
            .Select(u => u.Key)
            .ToArray();

        Assert.True(redundant.Length == 0, "One fixed property on its own control: " + string.Join(", ", redundant));
    });

    /// <summary>
    /// The words that must mean the same on every control that takes them: MudBlazor's Size and
    /// Variant, and its components' parts, whose names are what a Razor page is ported from.
    /// </summary>
    private static readonly HashSet<string> Families =
    [
        "small", "medium", "large", "dense", "text", "filled", "outlined",
        "card-actions", "card-content", "card-header", "card-header-content",
    ];

    /// <summary>The selector's own control and its classes, before any descendant, child or part.</summary>
    private static string Head(string selector) =>
        Regex.Replace(Regex.Split(selector, @"\s|>|/template/")[0], @"\[[^\]]*\]|:[a-z-]+(\([^)]*\))?", string.Empty);

    private static IEnumerable<string> ClassesOf(string selector) =>
        Regex.Matches(Head(selector), @"\.([A-Za-z][A-Za-z0-9-]*)").Select(m => m.Groups[1].Value);

    private static string ControlOf(string selector, string target) =>
        Regex.Match(Head(selector), @"^[A-Za-z][A-Za-z0-9]*") is { Success: true } m ? m.Value : target;

    /// <summary>Styles the selected control itself: no descendant, child or template part.</summary>
    private static bool IsOwn(string selector) => !Regex.IsMatch(selector.Trim(), @"\s|>|/template/");

    /// <summary>
    /// A literal an app could write itself: not a resource or a binding, which a theme could
    /// repaint, nor a template or a theme, nor one of the theme's own internal properties.
    /// </summary>
    private static bool Fixed(Setter setter) =>
        setter.Value is not (BindingBase or ITemplate or ControlTheme)
        && setter.Property?.OwnerType.Assembly != typeof(FiliThemeVariants).Assembly;

    private static void WalkStyles(IEnumerable<IStyle> styles, string target, List<(string, Style, string)> into)
    {
        foreach (var style in styles)
        {
            switch (style)
            {
                case Style s:
                    if (s.Selector is not null)
                    {
                        into.Add((s.Selector.ToString()!, s, target));
                    }

                    WalkStyles(s.Children, target, into);
                    break;

                case ControlTheme theme:
                    WalkStyles(theme.Children, theme.TargetType?.Name ?? target, into);
                    break;

                case StyleInclude include when include.Loaded is { } loaded:
                    WalkStyles([loaded], target, into);
                    break;

                case Styles group:
                    WalkStyles(group, target, into);
                    break;
            }
        }
    }

    /// <summary>
    /// Walks every style in the tree, including those behind a StyleInclude and those nested in
    /// a Style or a ControlTheme.
    /// </summary>
    private static void Walk(IEnumerable<IStyle> styles, List<string> into)
    {
        foreach (var style in styles)
        {
            switch (style)
            {
                case Style s:
                    if (s.Selector is not null)
                    {
                        into.Add(s.Selector.ToString()!);
                    }

                    Walk(s.Children, into);
                    break;

                case ControlTheme theme:
                    Walk(theme.Children, into);
                    break;

                // A StyleInclude is a placeholder until it resolves, and its Loaded style is the
                // real content. Missing this case is why the first version of this test walked
                // the whole application and found nothing at all.
                case StyleInclude include when include.Loaded is { } loaded:
                    Walk([loaded], into);
                    break;

                case Styles group:
                    Walk(group, into);
                    break;
            }
        }
    }
}
