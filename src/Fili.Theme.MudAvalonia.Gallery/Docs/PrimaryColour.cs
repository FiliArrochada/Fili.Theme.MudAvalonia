using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Fili.Theme.MudAvalonia.Gallery.Docs;

/// <summary>
/// The app bar's primary colour picker: a colour becomes the five primary tokens an app overrides
/// (see the Theming page), in light and dark, so every page shows the theme in that colour. High
/// contrast keeps its own primary, which is derived for contrast rather than chosen.
/// </summary>
public static class PrimaryColour
{
    /// <summary>
    /// MudBlazor's primary first, which puts the theme's own tokens back, then Material colours
    /// dark enough to carry white text at AA, as MudBlazor's primary does.
    /// </summary>
    public static IReadOnlyList<(string Name, Color Colour)> Swatches { get; } =
    [
        ("MudBlazor", Color.Parse("#594AE2")),
        ("Blue", Color.Parse("#1565C0")),
        ("Teal", Color.Parse("#00796B")),
        ("Green", Color.Parse("#2E7D32")),
        ("Deep orange", Color.Parse("#BF360C")),
        ("Red", Color.Parse("#C62828")),
        ("Pink", Color.Parse("#AD1457")),
        ("Purple", Color.Parse("#6A1B9A")),
        ("Brown", Color.Parse("#6D4C41")),
        ("Blue grey", Color.Parse("#455A64")),
    ];

    /// <summary>The colour picked, or null for the theme's own.</summary>
    public static Color? Current { get; private set; }

    /// <summary>
    /// Sets the primary tokens in the application's light and dark resources, or removes them for
    /// null. Application resources are looked up before the theme's, as an app's overrides are.
    /// High contrast falls back to dark, so it is declared too, with the tokens it ships.
    /// </summary>
    public static void Apply(Application app, Color? colour)
    {
        var variants = new[] { ThemeVariant.Light, ThemeVariant.Dark, FiliThemeVariants.HighContrast };
        foreach (var variant in variants)
        {
            app.Resources.ThemeDictionaries.Remove(variant);
        }

        Current = colour;
        if (colour is not { } c)
        {
            return;
        }

        var highContrast = new ResourceDictionary();
        foreach (var key in Tokens(c, ThemeVariant.Dark).Keys)
        {
            if (app.TryGetResource(key, FiliThemeVariants.HighContrast, out var shipped))
            {
                highContrast[key] = shipped;
            }
        }

        app.Resources.ThemeDictionaries[ThemeVariant.Light] = Tokens(c, ThemeVariant.Light);
        app.Resources.ThemeDictionaries[ThemeVariant.Dark] = Tokens(c, ThemeVariant.Dark);
        app.Resources.ThemeDictionaries[FiliThemeVariants.HighContrast] = highContrast;
    }

    /// <summary>
    /// What an app overriding the primary sets: the colour, its text, its 6% hover, its darken and
    /// lighten shades by MudBlazor's rule, the light app bar, which is the primary in MudBlazor, and
    /// the forked base's accent for the controls the theme leaves to it.
    /// </summary>
    public static ResourceDictionary Tokens(Color colour, ThemeVariant variant)
    {
        var tokens = new ResourceDictionary
        {
            ["FiliPrimaryColor"] = colour,
            ["FiliPrimaryContrastTextColor"] = ContrastText(colour),
            ["FiliPrimaryHoverColor"] = WithAlpha(colour, 0x0F),
            ["FiliPrimaryDarkenColor"] = MudColor.Darken(colour),
            ["FiliPrimaryLightenColor"] = MudColor.Lighten(colour),
            ["ThemeAccentColor"] = colour,
            ["ThemeAccentColor2"] = WithAlpha(colour, 0x99),
            ["ThemeAccentColor3"] = WithAlpha(colour, 0x66),
        };
        if (variant == ThemeVariant.Light)
        {
            tokens["FiliAppbarBackgroundColor"] = colour;
        }

        return tokens;
    }

    /// <summary>White, as MudBlazor's primary text is, unless black reads better on the colour.</summary>
    public static Color ContrastText(Color colour) =>
        Contrast.Ratio(Colors.White, colour) >= Contrast.Ratio(Colors.Black, colour) ? Colors.White : Colors.Black;

    private static Color WithAlpha(Color colour, byte alpha) => Color.FromArgb(alpha, colour.R, colour.G, colour.B);

    /// <summary>
    /// MudBlazor's <c>ColorRgbDarken</c> and <c>ColorRgbLighten</c>, from <c>MudColor.cs</c>: HSL
    /// lightness 0.075 down or up, with hue rounded to a degree and saturation and lightness to two
    /// places on the way in and on the way out, by .NET's own half-to-even rounding.
    /// </summary>
    public static class MudColor
    {
        private const double Epsilon = 0.000000000000001;

        public static Color Darken(Color color) => ChangeLightness(color, -0.075);

        public static Color Lighten(Color color) => ChangeLightness(color, +0.075);

        private static Color ChangeLightness(Color color, double amount)
        {
            var (h, s, l) = RgbToHsl(color.R, color.G, color.B);
            var (r, g, b) = HslToRgb(h, s, Math.Max(0, Math.Min(1, l + amount)));

            return Color.FromRgb(r, g, b);
        }

        private static (double H, double S, double L) RgbToHsl(byte r, byte g, byte b)
        {
            var h = 0d;
            var s = 0d;

            var rn = r / 255d;
            var gn = g / 255d;
            var bn = b / 255d;

            var max = Math.Max(rn, Math.Max(gn, bn));
            var min = Math.Min(rn, Math.Min(gn, bn));

            if (Math.Abs(max - min) < Epsilon)
            {
                h = 0d;
            }
            else if (Math.Abs(max - rn) < Epsilon && gn >= bn)
            {
                h = 60d * (gn - bn) / (max - min);
            }
            else if (Math.Abs(max - rn) < Epsilon && gn < bn)
            {
                h = (60d * (gn - bn) / (max - min)) + 360d;
            }
            else if (Math.Abs(max - gn) < Epsilon)
            {
                h = (60d * (bn - rn) / (max - min)) + 120d;
            }
            else if (Math.Abs(max - bn) < Epsilon)
            {
                h = (60d * (rn - gn) / (max - min)) + 240d;
            }

            var l = (max + min) / 2d;

            if (Math.Abs(l) < Epsilon || Math.Abs(max - min) < Epsilon)
            {
                s = 0d;
            }
            else if (l is > 0d and <= .5d)
            {
                s = (max - min) / (max + min);
            }
            else if (l > .5d)
            {
                s = (max - min) / (2d - (max + min));
            }

            return (Math.Round(Range(h, 360), 0), Math.Round(Range(s, 1), 2), Math.Round(Range(l, 1), 2));
        }

        private static (byte R, byte G, byte B) HslToRgb(double h, double s, double l)
        {
            // MudColor's HSL constructor rounds again before converting.
            h = Math.Round(Range(h, 360), 0);
            s = Math.Round(Range(s, 1), 2);
            l = Math.Round(Range(l, 1), 2);

            if (Math.Abs(s) < Epsilon)
            {
                var grey = (byte)Math.Max(0, Math.Min(255, (int)Math.Ceiling(l * 255d)));
                return (grey, grey, grey);
            }

            var hn = h / 360d;
            var t2 = l <= 0.5d ? l * (1.0d + s) : l + s - (l * s);
            var t1 = (2.0d * l) - t2;

            return (Channel(HueToRgb(t1, t2, hn + (1.0d / 3.0d))),
                    Channel(HueToRgb(t1, t2, hn)),
                    Channel(HueToRgb(t1, t2, hn - (1.0d / 3.0d))));

            static byte Channel(double value) => (byte)Math.Max(0, Math.Min(255, (int)Math.Round(value * 255d)));
        }

        private static double HueToRgb(double t1, double t2, double hue)
        {
            if (hue < 0.0d)
            {
                hue += 1.0d;
            }

            if (hue > 1.0d)
            {
                hue -= 1.0d;
            }

            return hue switch
            {
                < 1.0d / 6.0d => t1 + ((t2 - t1) * 6.0d * hue),
                < 1.0d / 2.0d => t2,
                < 2.0d / 3.0d => t1 + ((t2 - t1) * ((2.0d / 3.0d) - hue) * 6.0d),
                _ => t1,
            };
        }

        private static double Range(double value, double max) => Math.Max(0, Math.Min(max, value));
    }
}
