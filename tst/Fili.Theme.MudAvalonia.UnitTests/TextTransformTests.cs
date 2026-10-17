using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudBlazor's <c>ButtonTypography.TextTransform</c> is <c>uppercase</c>, and <c>.mud-button</c>,
/// <c>.mud-fab</c> and <c>.mud-tab</c> apply it: every button, the toggle items and menu
/// activators that are MudButtons, an extended FAB and a tab. A chip, a link, a checkbox and a menu
/// row are not buttons in its sense and keep their case. <c>FiliButtonTextTransform</c> is the
/// switch, as the typography setting is; an app that sets it to <c>none</c> keeps its labels.
/// </summary>
public class TextTransformTests
{
    public static readonly TheoryData<string> Uppercase =
    [
        "Button", "Button.outlined", "Button.filled.primary", "Button.fab.extended", "ToggleButton",
        "SplitButton", "DropDownButton", "TabItem", "TabStripItem",
    ];

    public static readonly TheoryData<string> AsWritten =
    [
        "Button.chip", "ToggleButton.chip", "HyperlinkButton", "CheckBox", "RadioButton", "ToggleSwitch", "Expander",
    ];

    private static Control Create(string spec, string label)
    {
        var parts = spec.Split('.');
        Control control = parts[0] switch
        {
            "Button" => new Button { Content = label },
            "ToggleButton" => new ToggleButton { Content = label },
            "SplitButton" => new SplitButton { Content = label },
            "DropDownButton" => new DropDownButton { Content = label },
            "TabItem" => new TabControl { Items = { new TabItem { Header = label, Content = "Body" } } },
            "TabStripItem" => new TabStrip { Items = { new TabStripItem { Content = label } } },
            "HyperlinkButton" => new HyperlinkButton { Content = label },
            "CheckBox" => new CheckBox { Content = label },
            "RadioButton" => new RadioButton { Content = label },
            "ToggleSwitch" => new ToggleSwitch { Content = label },
            "Expander" => new Expander { Header = label, Content = "Body" },
            _ => throw new ArgumentException(spec),
        };
        control.Classes.AddRange(parts.Skip(1));
        return control;
    }

    /// <summary>The texts the control draws, whatever presented them.</summary>
    private static List<string?> Drawn(Control control) =>
        control.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text).ToList();

    [Theory]
    [MemberData(nameof(Uppercase))]
    public Task AButtonsLabelIsUppercase(string spec) => UiThread.RunAsync(() =>
    {
        var control = Matrix.Show(Create(spec, "Save changes"));

        Assert.Contains("SAVE CHANGES", Drawn(control));
        Assert.DoesNotContain("Save changes", Drawn(control));
    });

    [Theory]
    [MemberData(nameof(AsWritten))]
    public Task WhatIsNotAButtonKeepsItsCase(string spec) => UiThread.RunAsync(() =>
    {
        var control = Matrix.Show(Create(spec, "Save changes"));

        Assert.Contains("Save changes", Drawn(control));
    });

    /// <summary>A control as content is the app's own: an icon, or a label it built itself.</summary>
    [Fact]
    public Task ContentThatIsAControlIsLeftAsItIs() => UiThread.RunAsync(() =>
    {
        var label = new TextBlock { Text = "Create" };
        var fab = Matrix.Show(new Button { Classes = { "fab", "extended" }, Content = new StackPanel { Children = { label } } });

        Assert.Equal("Create", label.Text);
        Assert.Same(label, fab.GetVisualDescendants().OfType<TextBlock>().Single());
    });

    /// <summary>The underscore survives the transform, and with it the access key.</summary>
    [Fact]
    public Task AnAccessKeySurvives() => UiThread.RunAsync(() =>
    {
        var button = Matrix.Show(new Button { Content = "_Save" });
        var text = button.GetVisualDescendants().OfType<AccessText>().Single();

        Assert.Equal("_SAVE", text.Text);
        Assert.Equal("S", text.AccessKey);
    });

    /// <summary>
    /// <c>none</c> keeps the labels as written, theme-wide or in one region, as MudBlazor's
    /// <c>TextTransform = "none"</c> does; and the switch is live, like every resource here.
    /// </summary>
    [Fact]
    public Task TheResourceTurnsItOff() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save changes" };
        var tab = new TabItem { Header = "Inbox", Content = "Body" };
        var region = new StackPanel { Children = { button, new TabControl { Items = { tab } } } };
        Matrix.Show(region);
        Assert.Contains("SAVE CHANGES", Drawn(button));

        region.Resources["FiliButtonTextTransform"] = "none";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("Save changes", Drawn(button));
        Assert.Contains("Inbox", Drawn(tab));

        region.Resources.Remove("FiliButtonTextTransform");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("SAVE CHANGES", Drawn(button));
    });

    [Fact]
    public Task TheSwitchIsUppercaseByDefault() => UiThread.RunAsync(() =>
    {
        Assert.True(Application.Current!.TryFindResource("FiliButtonTextTransform", out var value));
        Assert.Equal("uppercase", value);
    });

    /// <summary>CSS's four values, the same words MudBlazor's TextTransform takes; capitalize raises each word's first letter and leaves the rest.</summary>
    [Theory]
    [InlineData("uppercase", "Save as draft", "SAVE AS DRAFT")]
    [InlineData("lowercase", "Save As Draft", "save as draft")]
    [InlineData("capitalize", "save as dRAFT", "Save As DRAFT")]
    [InlineData("none", "Save as Draft", "Save as Draft")]
    public Task EachCaseIsCssS(string textCase, string label, string expected) => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = label };
        var region = new StackPanel { Children = { button } };
        region.Resources["FiliButtonTextTransform"] = textCase;
        Matrix.Show(region);

        Assert.Equal(expected, Matrix.Part<ContentPresenter>(button, "PART_ContentPresenter").Content);
    });
}
