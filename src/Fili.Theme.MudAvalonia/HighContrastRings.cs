using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// High contrast's elevation ladder: a 1px ring at every level but 0, in the line colour.
///
/// <para>
/// A <c>BoxShadows</c> string cannot carry a <c>DynamicResource</c>, so the rings are not written
/// out in Elevation.axaml: this provider builds each one when it is looked up, from the
/// <c>FiliLinesDefaultColor</c> the application resolves. An app that repaints the line for high
/// contrast repaints the rings with it, and a change at run time reaches them through the
/// application's own resource notification. It is merged into the high-contrast dictionary, not
/// beside it, because a variant's inherited Dark dictionary is searched before the outer
/// dictionary's merged ones.
/// </para>
/// </summary>
internal sealed class HighContrastRings : ResourceProvider
{
    private const string ElevationKey = "FiliElevation";
    private const string OverlayKey = "FiliElevationOverlay";
    private const string LineKey = "FiliLinesDefaultColor";
    private const int TopLevel = 25;

    private static readonly BoxShadows None = BoxShadows.Parse("none");
    private static readonly Color Fallback = Color.Parse("#E8E8EE");

    public override bool HasResources => true;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (key is not string name || Level(name) is not { } level)
        {
            value = null;
            return false;
        }

        value = level == 0 ? None : new BoxShadows(new BoxShadow { Spread = 1, Color = Line(theme) });
        return true;
    }

    private static int? Level(string name)
    {
        var digits = name.StartsWith(OverlayKey, StringComparison.Ordinal) ? name[OverlayKey.Length..]
            : name.StartsWith(ElevationKey, StringComparison.Ordinal) ? name[ElevationKey.Length..]
            : null;

        return digits is not null
            && int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var level)
            && level <= TopLevel
            ? level
            : null;
    }

    private static Color Line(ThemeVariant? theme) =>
        Application.Current is { } application
        && application.TryGetResource(LineKey, theme, out var value)
        && value is Color colour
            ? colour
            : Fallback;
}
