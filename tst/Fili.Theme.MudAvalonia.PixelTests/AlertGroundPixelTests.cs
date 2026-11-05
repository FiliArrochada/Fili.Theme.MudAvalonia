using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.PixelTests;

/// <summary>
/// The ground an alert's text is measured against is the ground Skia paints. The unit suite's
/// <c>AlertTextContrastTests</c> composites the alert's tint over the page or a surface in
/// arithmetic; this renders every text and outlined alert in every variant, reads the pixel inside
/// it, holds that to the arithmetic within one level, and checks the alert-text token's 4.5:1
/// against the painted pixel itself.
/// </summary>
public class AlertGroundPixelTests
{
    private const double Text = 4.5;

    private static readonly string[] Colours = ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    private static readonly string[] Grounds = ["FiliBackground", "FiliSurface"];

    public static TheoryData<string> Variants => ["Light", "Dark", "HighContrast"];

    [Theory]
    [MemberData(nameof(Variants))]
    public Task EveryAlertsTextReadsOnThePaintedGround(string name) => UiThread.RunAsync(() =>
    {
        var variant = name switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => FiliThemeVariants.HighContrast,
        };

        var cases = new List<(string What, Border Alert, Color Expected, Color TextColour)>();
        var column = new StackPanel();
        foreach (var ground in Grounds)
        {
            var groundColour = Resource($"{ground}Color", variant);
            var alerts = new StackPanel();
            column.Children.Add(new Border { Child = alerts, [!Border.BackgroundProperty] = new DynamicResourceExtension($"{ground}Brush") });
            foreach (var colour in Colours)
            {
                var textColour = Resource($"Fili{colour}AlertTextColor", variant);
                foreach (var outlined in new[] { false, true })
                {
                    var alert = new Border { Width = 40, Height = 30 };
                    alert.Classes.AddRange(outlined ? ["alert", "outlined", colour.ToLowerInvariant()] : ["alert", colour.ToLowerInvariant()]);
                    alerts.Children.Add(alert);
                    var expected = outlined ? groundColour : Over(Resource($"Fili{colour}HoverColor", variant), groundColour);
                    cases.Add(($"{(outlined ? "outlined " : string.Empty)}{colour} on {ground}", alert, expected, textColour));
                }
            }
        }

        var window = new Window { RequestedThemeVariant = variant, Content = column, Width = 100, SizeToContent = SizeToContent.Height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var size = new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height);
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            bitmap.Render(window);
            var pixels = Frames.Read(bitmap);

            var failures = new List<string>();
            foreach (var (what, alert, expected, textColour) in cases)
            {
                // The middle of the alert: inside its corners and its outline, where only the ground is.
                var centre = alert.TranslatePoint(new Point(alert.Bounds.Width / 2, alert.Bounds.Height / 2), window)!.Value;
                var i = ((int)centre.Y * pixels.Stride) + ((int)centre.X * 4);
                var painted = Color.FromRgb(pixels.Bytes[i + 2], pixels.Bytes[i + 1], pixels.Bytes[i]);

                var drift = Math.Max(Math.Abs(painted.R - expected.R), Math.Max(Math.Abs(painted.G - expected.G), Math.Abs(painted.B - expected.B)));
                if (drift > Frames.Tolerance)
                {
                    failures.Add($"{name}, {what}: painted {painted}, composited {expected}.");
                }

                var ratio = Ratio(Over(textColour, painted), painted);
                if (ratio < Text)
                {
                    failures.Add($"{name}, {what}: {textColour} on the painted {painted} is {ratio:0.00}:1, under {Text}:1.");
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures));
        }
        finally
        {
            window.Close();
        }
    });

    private static Color Resource(string key, ThemeVariant variant) =>
        Application.Current!.TryGetResource(key, variant, out var value) && value is Color colour
            ? colour
            : throw new InvalidOperationException($"{key} does not resolve in {variant}.");

    private static Color Over(Color top, Color ground)
    {
        var a = top.A / 255d;
        byte Mix(byte t, byte g) => (byte)Math.Round((a * t) + ((1 - a) * g));
        return Color.FromRgb(Mix(top.R, ground.R), Mix(top.G, ground.G), Mix(top.B, ground.B));
    }

    private static double Ratio(Color a, Color b)
    {
        static double Linear(byte channel)
        {
            var v = channel / 255d;
            return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        static double Luminance(Color c) => (0.2126 * Linear(c.R)) + (0.7152 * Linear(c.G)) + (0.0722 * Linear(c.B));

        var (la, lb) = (Luminance(a), Luminance(b));
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }
}
