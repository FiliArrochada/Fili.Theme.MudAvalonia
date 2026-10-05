using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Fili.Theme.MudAvalonia;

namespace Fili.Theme.MudAvalonia.Gallery;

/// <summary>
/// The gallery itself: the app bar with its variant selector, and the tabs.
/// Desktop hosts it in <see cref="MainWindow"/>; the browser build sets it as the single view.
/// </summary>
public partial class MainView : UserControl
{
    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
        this.FindControl<TextBlock>("ThemeVersion")!.Text = ThemeVersionText();
    }

    /// <summary>
    /// "v0.4.0", or "v0.4.0 · 6d8ed36" for a build the SDK could stamp with its commit - which
    /// is every build from a clone, the published gallery included. It reads the THEME's
    /// assembly, not the gallery's, so it names the package the page is showing; between
    /// releases the commit is what tells two builds of the same version apart.
    /// </summary>
    internal static string ThemeVersionText()
    {
        var assembly = typeof(FiliThemeVariants).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrEmpty(informational))
        {
            return $"v{assembly.GetName().Version?.ToString(3)}";
        }

        var plus = informational.IndexOf('+');
        if (plus < 0)
        {
            return $"v{informational}";
        }

        var commit = informational[(plus + 1)..];
        return $"v{informational[..plus]} · {commit[..System.Math.Min(7, commit.Length)]}";
    }

    /// <summary>
    /// Switches the application theme variant at runtime. This is the single most useful thing
    /// the gallery does: every token that was wired with StaticResource instead of
    /// DynamicResource stops following the theme here, visibly, in one click.
    /// </summary>
    /// <remarks>
    /// The variant goes on the APPLICATION and not on the window, and that is not a shortcut.
    /// Every brush in the theme is one shared application-level object whose Color is a
    /// DynamicResource, so a window asking for a different variant gets the same brush instance
    /// and therefore the same colour.
    /// </remarks>
    private void OnVariantChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Application.Current is null || sender is not ComboBox selector)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = selector.SelectedIndex switch
        {
            1 => ThemeVariant.Dark,
            2 => FiliThemeVariants.HighContrast,
            _ => ThemeVariant.Light,
        };
    }
}
