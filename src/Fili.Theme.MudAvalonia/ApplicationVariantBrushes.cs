using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia;

/// <summary>
/// Every brush once more, outside the variant dictionaries, answered only for the application's
/// own variant: what a variant switch costs.
///
/// <para>
/// Every brush is declared in each variant's dictionary, so a window or <c>ThemeVariantScope</c>
/// with its own variant paints with that variant's. Found for every lookup, those made a switch
/// hand each control a new brush in every brush property it has, up to three times the work of
/// recolouring one brush. This provider is searched first and answers a lookup for the
/// application's variant, or one naming no variant (<c>FindResource(key)</c>), with
/// <c>FallbackBrushes.axaml</c>: one brush per key whose colour follows the application's variant.
/// A switch then recolours those brushes and a control keeps the one it has. A lookup for any
/// other variant falls through to that variant's dictionary, as before.
/// </para>
/// <para>
/// A control under a window or scope whose own variant WAS the application's holds one of these
/// brushes, and would follow the application into the other variant: its variant did not change,
/// so it does not look the brush up again. So every element that requests a variant of its own is
/// kept, weakly, and after a switch each one whose variant is no longer the application's gets a
/// resource notification for its subtree, where a control finds its own variant's brush. A
/// notification for the whole application would make every control look up every brush again,
/// which cost as much as the switch.
/// </para>
/// </summary>
internal sealed class ApplicationVariantBrushes : ResourceProvider
{
    private static readonly List<WeakReference<StyledElement>> Requesters = [];

    private Application? _watched;

    static ApplicationVariantBrushes() =>
        ThemeVariantScope.RequestedThemeVariantProperty.Changed.AddClassHandler<StyledElement>(OnRequested);

    /// <summary>The brushes: <c>FallbackBrushes.axaml</c>, set once from XAML.</summary>
    [Content]
    public IResourceProvider? Brushes { get; set; }

    public override bool HasResources => Brushes?.HasResources ?? false;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (Brushes is not null
            && (theme is null || theme == ThemeVariant.Default || theme == Application.Current?.ActualThemeVariant))
        {
            return Brushes.TryGetResource(key, theme, out value);
        }

        value = null;
        return false;
    }

    protected override void OnAddOwner(IResourceHost owner)
    {
        Brushes?.AddOwner(owner);
        if (Application.Current is { } application)
        {
            _watched = application;
            application.ActualThemeVariantChanged += OnApplicationVariantChanged;
        }

        base.OnAddOwner(owner);
    }

    protected override void OnRemoveOwner(IResourceHost owner)
    {
        Brushes?.RemoveOwner(owner);
        if (_watched is not null)
        {
            _watched.ActualThemeVariantChanged -= OnApplicationVariantChanged;
            _watched = null;
        }

        base.OnRemoveOwner(owner);
    }

    private static void OnRequested(StyledElement element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is ThemeVariant variant && variant != ThemeVariant.Default
            && !Requesters.Exists(kept => kept.TryGetTarget(out var known) && known == element))
        {
            Requesters.Add(new WeakReference<StyledElement>(element));
        }
    }

    private void OnApplicationVariantChanged(object? sender, EventArgs e)
    {
        var application = Application.Current?.ActualThemeVariant;
        Requesters.RemoveAll(kept => !kept.TryGetTarget(out _));
        foreach (var kept in Requesters.ToList())
        {
            if (kept.TryGetTarget(out var element)
                && element.GetValue(ThemeVariantScope.RequestedThemeVariantProperty) is { } requested
                && requested != ThemeVariant.Default
                && element.ActualThemeVariant != application)
            {
                ((IResourceHost)element).NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());
            }
        }
    }
}
