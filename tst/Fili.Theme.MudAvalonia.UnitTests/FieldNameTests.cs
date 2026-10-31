using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A field's label is its name to a screen reader, as MudBlazor's <c>&lt;label for&gt;</c> makes
/// it: the field and the text box inside it that takes focus both read "Email", and both read an
/// app's own name over it. Without it every field was announced as an unnamed edit box.
/// </summary>
public class FieldNameTests
{
    public static TheoryData<string> Fields => ["TextBox", "AutoCompleteBox", "NumericUpDown", "ComboBox", "CalendarDatePicker"];

    private static Control Field(string type, string? label) => type switch
    {
        "TextBox" => new TextBox { PlaceholderText = label },
        "AutoCompleteBox" => new AutoCompleteBox { PlaceholderText = label },
        "NumericUpDown" => new NumericUpDown { PlaceholderText = label },
        "ComboBox" => new ComboBox { PlaceholderText = label },
        "CalendarDatePicker" => new CalendarDatePicker { PlaceholderText = label },
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    private static T Show<T>(T field) where T : Control
    {
        new Window { Content = field }.Show();
        Dispatcher.UIThread.RunJobs();
        return field;
    }

    private static string Name(Control control) => ControlAutomationPeer.CreatePeerForElement(control).GetName();

    [Theory]
    [MemberData(nameof(Fields))]
    public Task TheLabelIsTheName(string type) => UiThread.RunAsync(() =>
    {
        var field = Show(Field(type, "Email"));

        Assert.Equal("Email", Name(field));
        foreach (var inner in field.GetVisualDescendants().OfType<TextBox>())
        {
            Assert.Equal("Email", Name(inner));
        }
    });

    [Theory]
    [MemberData(nameof(Fields))]
    public Task TheAppsOwnNameWins(string type) => UiThread.RunAsync(() =>
    {
        var field = Field(type, "Email");
        AutomationProperties.SetName(field, "Work email");

        Assert.Equal("Work email", Name(Show(field)));
        Assert.All(field.GetVisualDescendants().OfType<TextBox>(), inner => Assert.Equal("Work email", Name(inner)));
    });

    /// <summary>
    /// A field the app labels with another control keeps that control's text as its name: the
    /// peer reads the name first, so one set from the label would hide it.
    /// </summary>
    [Theory]
    [MemberData(nameof(Fields))]
    public Task LabeledByWins(string type) => UiThread.RunAsync(() =>
    {
        var label = new TextBlock { Text = "Work email" };
        var field = Field(type, "Email");
        AutomationProperties.SetLabeledBy(field, label);
        new Window { Content = new StackPanel { Children = { label, field } } }.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Work email", Name(field));
        Assert.All(field.GetVisualDescendants().OfType<TextBox>(), inner => Assert.Equal("Work email", Name(inner)));
    });

    /// <summary>The name follows the label when it changes.</summary>
    [Fact]
    public Task TheNameFollowsTheLabel() => UiThread.RunAsync(() =>
    {
        var field = Show(new TextBox { PlaceholderText = "Email" });

        field.PlaceholderText = "Phone";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Phone", Name(field));
    });

    /// <summary>A time picker's label is its name already; the text box inside reads it too.</summary>
    [Fact]
    public Task ATimePickersFieldReadsItsLabel() => UiThread.RunAsync(() =>
    {
        var picker = new TimePicker();
        AutomationProperties.SetName(picker, "Start");
        Show(picker);

        Assert.All(picker.GetVisualDescendants().OfType<TextBox>(), inner => Assert.Equal("Start", Name(inner)));
    });
}
