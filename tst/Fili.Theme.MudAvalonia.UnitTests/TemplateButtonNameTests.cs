using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// Every icon-only button the theme's own templates draw has a name a screen reader can say. With
/// none, Avalonia's peer falls back to the content's type, and a reader announced the time
/// picker's OK as "Avalonia.Controls.PathIcon". The names are <c>FiliString*</c> resources, so an
/// app translates them by setting the key; a label shown in upper case still reads as written.
/// </summary>
public class TemplateButtonNameTests
{
    private static string Name(Control control) => ControlAutomationPeer.CreatePeerForElement(control).GetName();

    private static Control Part(Control host, string name) =>
        host.GetVisualDescendants().OfType<Control>().First(c => c.Name == name);

    /// <summary>The part a host shows, by the key that names it and the English default.</summary>
    public static TheoryData<string, string, string, string> Parts => new()
    {
        { "TextBox", "PART_ClearButton", "FiliStringClear", "Clear" },
        { "NotificationCard", "PART_CloseButton", "FiliStringClose", "Close" },
        { "Calendar", "PART_PreviousButton", "FiliStringPrevious", "Previous" },
        { "Calendar", "PART_NextButton", "FiliStringNext", "Next" },
        { "Carousel", "PART_PreviousButton", "FiliStringPrevious", "Previous" },
        { "Carousel", "PART_NextButton", "FiliStringNext", "Next" },
        { "NumericUpDown", "PART_IncreaseButton", "FiliStringIncrease", "Increase" },
        { "NumericUpDown", "PART_DecreaseButton", "FiliStringDecrease", "Decrease" },
        { "TimePicker", "PART_AcceptButton", "FiliStringAccept", "OK" },
        { "TimePicker", "PART_DismissButton", "FiliStringDismiss", "Cancel" },
    };

    /// <summary>Shows <paramref name="type"/> and returns the control whose template holds the part.</summary>
    private static Control Host(string type)
    {
        Control control = type switch
        {
            "TextBox" => new TextBox { Classes = { "clearable" }, Text = "Ada", Width = 200 },
            "NotificationCard" => new NotificationCard { Content = "Saved", Width = 300 },
            "Calendar" => new Calendar(),
            "Carousel" => new Carousel { Width = 300, Height = 120, ItemsSource = new[] { "One", "Two" } },
            "NumericUpDown" => new NumericUpDown { Width = 200 },
            "TimePicker" => new TimePicker { Width = 260, SelectedTime = new TimeSpan(14, 30, 0) },
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
        Matrix.Show(control);

        if (control is TimePicker picker)
        {
            Matrix.Part<Button>(picker, "PART_FlyoutButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            return (Control)picker.GetVisualDescendants().OfType<Popup>().Single(p => p.Name == "PART_Popover").Child!;
        }

        return control;
    }

    [Theory]
    [MemberData(nameof(Parts))]
    public Task AnIconButtonInATemplateHasAName(string type, string part, string key, string english) => UiThread.RunAsync(() =>
    {
        var host = Host(type);

        Assert.Equal(english, Name(Part(host, part)));
        Assert.True(Application.Current!.TryFindResource(key, out var value) && (string?)value == english);
    });

    [Theory]
    [MemberData(nameof(Parts))]
    public Task AnAppTranslatesTheNameBySettingItsKey(string type, string part, string key, string english) => UiThread.RunAsync(() =>
    {
        Application.Current!.Resources[key] = "Übersetzt";
        try
        {
            var host = Host(type);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("Übersetzt", Name(Part(host, part)));
            Assert.NotEqual(english, Name(Part(host, part)));
        }
        finally
        {
            Application.Current!.Resources.Remove(key);
        }
    });

    /// <summary>
    /// The time picker's icon button is hidden from automation, as the date picker's is: the field
    /// is the picker's one stop and name, and a second unnamed control would only be noise.
    /// </summary>
    [Fact]
    public Task TheTimePickersIconButtonIsNotASecondControl() => UiThread.RunAsync(() =>
    {
        var picker = Matrix.Show(new TimePicker { Width = 260 });

        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(Matrix.Part<Button>(picker, "PART_FlyoutButton")));
    });

    /// <summary>
    /// A button's and a tab's label is drawn upper case (MudBlazor's text-transform), but its
    /// content stays as written, so a screen reader reads "Save", not "SAVE", which some spell out.
    /// </summary>
    [Fact]
    public Task AnUpperCaseLabelReadsAsWritten() => UiThread.RunAsync(() =>
    {
        var button = new Button { Content = "Save draft", Classes = { "filled", "primary" } };
        var tab = new TabItem { Header = "Account", Content = "body" };
        var tabs = new TabControl { Width = 400, Items = { tab } };
        Matrix.Show(new StackPanel { Children = { button, tabs } });

        var shown = button.GetVisualDescendants().OfType<TextBlock>().Concat(tab.GetVisualDescendants().OfType<TextBlock>())
            .Select(t => t.Inlines is { Count: > 0 } inlines ? string.Concat(inlines.OfType<Avalonia.Controls.Documents.Run>().Select(r => r.Text)) : t.Text)
            .ToArray();
        Assert.Contains("SAVE DRAFT", shown);
        Assert.Contains("ACCOUNT", shown);

        Assert.Equal("Save draft", Name(button));
        Assert.Equal("Account", Name(tab));
    });
}
