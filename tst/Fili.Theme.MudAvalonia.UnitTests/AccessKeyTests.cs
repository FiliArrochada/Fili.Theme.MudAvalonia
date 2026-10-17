using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// "_Save" is Save with S as its access key, as Avalonia's own themes draw it. MudBlazor has no
/// access keys, but the base this theme forks honours them, and the hand-written templates had
/// dropped them: every button, toggle, checkbox, radio, switch, tab, expander and menu row showed
/// the underscore and took no Alt shortcut. A control whose presenter recognises access keys
/// shows a string through an <see cref="AccessText"/>, which is what is asserted.
/// </summary>
public class AccessKeyTests
{
    public static readonly TheoryData<string> Controls =
    [
        "Button", "Button.filled", "Button.chip", "Button.fab", "ToggleButton", "CheckBox", "RadioButton",
        "ToggleSwitch", "SplitButton", "DropDownButton", "HyperlinkButton", "TabItem", "Expander",
    ];

    private static Control Create(string spec)
    {
        var parts = spec.Split('.');
        Control control = parts[0] switch
        {
            "Button" => new Button { Content = "_Save" },
            "ToggleButton" => new ToggleButton { Content = "_Save" },
            "CheckBox" => new CheckBox { Content = "_Save" },
            "RadioButton" => new RadioButton { Content = "_Save" },
            "ToggleSwitch" => new ToggleSwitch { Content = "_Save" },
            "SplitButton" => new SplitButton { Content = "_Save" },
            "DropDownButton" => new DropDownButton { Content = "_Save" },
            "HyperlinkButton" => new HyperlinkButton { Content = "_Save" },
            "TabItem" => new TabControl { Items = { new TabItem { Header = "_Save" } } },
            "Expander" => new Expander { Header = "_Save", Content = "Body" },
            _ => throw new ArgumentException(spec),
        };
        control.Classes.AddRange(parts.Skip(1));
        return control;
    }

    private static void AssertRecognised(Control control, string what)
    {
        // A button's label is uppercase (TextTransformTests); the key is the same letter.
        var text = control.GetVisualDescendants().OfType<AccessText>()
            .FirstOrDefault(t => t.Text?.ToLowerInvariant() is "_save" or "_file" or "_open");
        Assert.True(text is not null, $"{what} draws its text without an AccessText, underscore and all.");
        Assert.False(string.IsNullOrEmpty(text!.AccessKey), $"{what} has no access key.");
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public Task ContentRecognisesItsAccessKey(string spec) => UiThread.RunAsync(() =>
        AssertRecognised(Matrix.Show(Create(spec)), spec));

    [Fact]
    public Task MenuHeadersRecogniseTheirAccessKeys() => UiThread.RunAsync(() =>
    {
        var row = new MenuItem { Header = "_Open" };
        var top = new MenuItem { Header = "_File", Items = { row } };
        Matrix.Show(new Menu { Items = { top } });
        top.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();

        AssertRecognised(top, "A menu bar's item");
        AssertRecognised(row, "A menu row");
    });
}
