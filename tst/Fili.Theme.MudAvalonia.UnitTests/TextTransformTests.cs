using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudBlazor's <c>ButtonTypography.TextTransform</c> is <c>uppercase</c>, and <c>.mud-button</c>
/// and <c>.mud-fab</c> apply it: every button, the toggle items and menu activators that are
/// MudButtons, and an extended FAB. A tab is uppercase too, but <c>.mud-tab</c> hard-codes it
/// (_tabs.scss). A chip, a link, a checkbox and a menu row are not buttons in its sense and keep
/// their case. <c>FiliButtonTextTransform</c> is the switch, as the typography setting is; an app
/// that sets it to <c>none</c> keeps its button labels, and its tabs stay uppercase.
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

    /// <summary>What a TextBlock draws: the run the transform drew, or else its own Text.</summary>
    private static string? Shown(TextBlock block) =>
        block.Inlines is { Count: > 0 } inlines
            ? string.Concat(inlines.OfType<Run>().Select(r => r.Text))
            : block.Text;

    private static (Button Button, TextBlock Label) IconButton(string classes, string label)
    {
        var text = new TextBlock { Text = label };
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { new PathIcon { Classes = { "start-icon" } }, text },
            },
        };
        button.Classes.AddRange(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Matrix.Show(button);
        return (button, text);
    }

    /// <summary>
    /// MudButton's StartIcon idiom - an icon and a label in a panel - reads in capitals too, as CSS's
    /// inherited text-transform makes it. The TextBlock's Text stays the app's, so a binding on it
    /// is untouched; only what it draws is in case.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("outlined")]
    [InlineData("filled primary")]
    [InlineData("fab extended")]
    public Task ALabelBesideAnIconIsUppercase(string classes) => UiThread.RunAsync(() =>
    {
        var (_, label) = IconButton(classes, "Store reconciliation");

        Assert.Equal("Store reconciliation", label.Text);
        Assert.Equal("STORE RECONCILIATION", Shown(label));
    });

    /// <summary>A new Text clears the TextBlock's inlines; the label is drawn in case again.</summary>
    [Fact]
    public Task ALabelThatChangesStaysUppercase() => UiThread.RunAsync(() =>
    {
        var (_, label) = IconButton("", "Save");

        label.Text = "Save as draft";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Save as draft", label.Text);
        Assert.Equal("SAVE AS DRAFT", Shown(label));
    });

    /// <summary>The switch reaches a label in a panel as it does a string, both ways and live.</summary>
    [Fact]
    public Task TheResourceTurnsOffALabelBesideAnIcon() => UiThread.RunAsync(() =>
    {
        var (button, label) = IconButton("", "Save changes");
        Assert.Equal("SAVE CHANGES", Shown(label));

        button.Resources["FiliButtonTextTransform"] = "none";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Save changes", Shown(label));
        Assert.Equal("Save changes", label.Text);

        button.Resources.Remove("FiliButtonTextTransform");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("SAVE CHANGES", Shown(label));
    });

    /// <summary>A TextBlock with runs of the app's own is its formatting, and left as written.</summary>
    [Fact]
    public Task ALabelWithItsOwnRunsIsLeftAsWritten() => UiThread.RunAsync(() =>
    {
        var label = new TextBlock { Inlines = new InlineCollection { new Run("Pay "), new Run("now") { FontWeight = FontWeight.Bold } } };
        Matrix.Show(new Button { Content = new StackPanel { Children = { label } } });

        Assert.Equal("Pay now", Shown(label));
        Assert.Equal(2, label.Inlines!.Count);
    });

    /// <summary>A chip's label beside its icon keeps its case, as a chip's string does.</summary>
    [Fact]
    public Task AChipsLabelBesideAnIconKeepsItsCase() => UiThread.RunAsync(() =>
    {
        var (_, label) = IconButton("chip", "Groceries");

        Assert.Equal("Groceries", Shown(label));
    });

    /// <summary>Typo.button on a TextBlock is uppercase, as it is in MudBlazor.</summary>
    [Fact]
    public Task TheButtonTypeClassIsUppercase() => UiThread.RunAsync(() =>
    {
        var label = new TextBlock { Text = "Read more", Classes = { "button" } };
        Matrix.Show(label);

        Assert.Equal("READ MORE", Shown(label));
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
        // `.mud-tab { text-transform: uppercase }` is a literal: the switch does not reach a tab.
        Assert.Contains("INBOX", Drawn(tab));

        region.Resources.Remove("FiliButtonTextTransform");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("SAVE CHANGES", Drawn(button));
    });

    /// <summary>
    /// `.mud-tab` hard-codes `text-transform: uppercase` (_tabs.scss) where `.mud-button` reads the
    /// typography's: a tab and a tab strip's item stay uppercase under any FiliButtonTextTransform.
    /// </summary>
    [Theory]
    [InlineData("TabItem", "none")]
    [InlineData("TabItem", "capitalize")]
    [InlineData("TabStripItem", "none")]
    [InlineData("TabStripItem", "lowercase")]
    public Task ATabIsUppercaseWhateverTheSwitchSays(string spec, string textCase) => UiThread.RunAsync(() =>
    {
        var tabs = Create(spec, "Inbox");
        var region = new StackPanel { Children = { tabs } };
        region.Resources["FiliButtonTextTransform"] = textCase;
        Matrix.Show(region);

        Assert.Contains("INBOX", Drawn(tabs));
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
