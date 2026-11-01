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
/// <para>
/// The ring is built once per variant and kept: every elevation lookup in high contrast used to
/// allocate one and run a nested application lookup for the line colour. The application's own
/// resource notification checks the kept line colours, and only when one changed drops it and
/// raises this provider's own notification, so the elevated controls look the rings up again
/// after the change rather than in the middle of it.
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

    private readonly Dictionary<ThemeVariant, (Color Line, BoxShadows Ring)> _rings = new();
    private Application? _watched;

    public override bool HasResources => true;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (key is not string name || Level(name) is not { } level)
        {
            value = null;
            return false;
        }

        value = level == 0 ? None : Ring(theme);
        return true;
    }

    private BoxShadows Ring(ThemeVariant? theme)
    {
        Watch();
        var variant = theme ?? ThemeVariant.Default;
        if (!_rings.TryGetValue(variant, out var kept))
        {
            var line = Line(theme);
            kept = (line, new BoxShadows(new BoxShadow { Spread = 1, Color = line }));
            _rings[variant] = kept;
        }

        return kept.Ring;
    }

    private void Watch()
    {
        if (_watched is null && Application.Current is { } application)
        {
            _watched = application;
            application.ResourcesChanged += OnApplicationResourcesChanged;
        }
    }

    private void OnApplicationResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        var stale = _rings.Where(kept => kept.Value.Line != Line(kept.Key)).Select(kept => kept.Key).ToList();
        if (stale.Count == 0)
        {
            return;
        }

        foreach (var variant in stale)
        {
            _rings.Remove(variant);
        }

        RaiseResourcesChanged();
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
