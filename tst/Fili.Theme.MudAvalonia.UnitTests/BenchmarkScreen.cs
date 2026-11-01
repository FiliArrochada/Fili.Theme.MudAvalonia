using Avalonia.Controls;
using Avalonia.Layout;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// The style-cost benchmark's screen: 1,600 ordinary controls, as an app's form-heavy page has
/// them. A hundred cards, each with a heading, body text, the three button variants, a labelled
/// field, a select, a checkbox, a radio, a switch, a slider, a progress bar, a chip, an icon
/// button, a link and a divider - the controls nearly every screen shows, with the classes they
/// are usually given.
///
/// <para>
/// One definition, two users: <c>StyleCostTests</c> counts the style instances it holds, a
/// deterministic number CI can gate on, and the Benchmarks project times building it, which a
/// count cannot stand in for (docs/traps.md, "Fewer style instances is not always less time").
/// </para>
/// </summary>
public static class BenchmarkScreen
{
    public const int Cards = 100;
    public const int ControlsPerCard = 16;

    public static Control Build()
    {
        var cards = new StackPanel { Spacing = 16, Margin = new(24) };
        for (var i = 0; i < Cards; i++)
        {
            cards.Children.Add(Card(i));
        }

        return new ScrollViewer { Content = cards };
    }

    private static Control Card(int index)
    {
        var body = new StackPanel { Spacing = 8 };
        body.Children.Add(new TextBlock { Text = $"Card {index}", Classes = { "h6" } });
        body.Children.Add(new TextBlock { Text = "Supporting text for the card.", Classes = { "body2" } });

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(new Button { Content = "Save", Classes = { "filled", "primary" } });
        buttons.Children.Add(new Button { Content = "Edit", Classes = { "outlined", "primary" } });
        buttons.Children.Add(new Button { Content = "Cancel" });
        body.Children.Add(buttons);

        body.Children.Add(new TextBox { PlaceholderText = "Name" });
        body.Children.Add(new ComboBox { PlaceholderText = "Choose", ItemsSource = new[] { "One", "Two", "Three" } });
        body.Children.Add(new CheckBox { Content = "Remember", IsChecked = index % 2 == 0 });
        body.Children.Add(new RadioButton { Content = "Option", GroupName = $"group{index}" });
        body.Children.Add(new ToggleSwitch { IsChecked = index % 3 == 0 });
        body.Children.Add(new Slider { Value = index % 100 });
        body.Children.Add(new ProgressBar { Value = index % 100 });
        body.Children.Add(new ContentControl { Content = "Chip", Classes = { "chip" } });
        body.Children.Add(new Button { Content = new PathIcon(), Classes = { "icon" } });
        body.Children.Add(new HyperlinkButton { Content = "Details" });
        body.Children.Add(new Separator());

        return new Border { Classes = { "card" }, Padding = new(16), Child = body };
    }
}
