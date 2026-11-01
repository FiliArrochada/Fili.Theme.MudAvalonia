using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A field's validation error reaches a screen reader, not only the eye. MudBlazor wires its
/// error text into the input's aria-describedby; on screen the theme replaces the helper text with
/// the error, but the automation help text is the app's own helper text, so the error is the
/// field's <c>ItemStatus</c> while it shows, and gone with it.
/// </summary>
public class FieldErrorAutomationTests
{
    private static string? Status(Control control) => ControlAutomationPeer.CreatePeerForElement(control).GetItemStatus();

    private static T Fail<T>(T control, params object[] errors) where T : Control
    {
        DataValidationErrors.SetErrors(control, errors);
        Dispatcher.UIThread.RunJobs();
        return control;
    }

    [Fact]
    public Task ATextFieldsErrorIsItsStatus() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { PlaceholderText = "Email" });
        AutomationProperties.SetHelpText(box, "We never share it");

        Fail(box, new InvalidOperationException("Must be a valid email address"));

        Assert.Equal("Must be a valid email address", Status(box));
        Assert.Equal("We never share it", AutomationProperties.GetHelpText(box));
    });

    [Fact]
    public Task TheStatusGoesWithTheError() => UiThread.RunAsync(() =>
    {
        var box = Fail(Matrix.Show(new TextBox { PlaceholderText = "Email" }), "Required");
        Assert.Equal("Required", Status(box));

        DataValidationErrors.ClearErrors(box);
        Dispatcher.UIThread.RunJobs();

        Assert.True(string.IsNullOrEmpty(Status(box)));
    });

    [Fact]
    public Task EveryErrorIsRead() => UiThread.RunAsync(() =>
    {
        var box = Fail(Matrix.Show(new TextBox { PlaceholderText = "Password" }), "Too short.", "Needs a digit.");

        Assert.Equal("Too short. Needs a digit.", Status(box));
    });

    [Fact]
    public Task AnAppsOwnStatusWins() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { PlaceholderText = "Email" });
        AutomationProperties.SetItemStatus(box, "Checked by the server");

        Fail(box, "Required");

        Assert.Equal("Checked by the server", Status(box));
    });

    [Fact]
    public Task ASelectsErrorIsItsStatus() => UiThread.RunAsync(() =>
    {
        var select = Fail(Matrix.Show(new ComboBox { Width = 200, PlaceholderText = "Fruit", ItemsSource = new[] { "Apple" } }), "Pick a fruit");

        Assert.Equal("Pick a fruit", Status(select));
    });

    /// <summary>
    /// A picker or a wrapped field takes focus in its own text box, which its error reaches, so
    /// that is where the error is read.
    /// </summary>
    [Theory]
    [InlineData("CalendarDatePicker")]
    [InlineData("TimePicker")]
    [InlineData("NumericUpDown")]
    [InlineData("AutoCompleteBox")]
    public Task APickersErrorIsItsTextBoxsStatus(string type) => UiThread.RunAsync(() =>
    {
        TemplatedControl picker = type switch
        {
            "CalendarDatePicker" => new CalendarDatePicker { Width = 220 },
            "TimePicker" => new TimePicker { Width = 220 },
            "NumericUpDown" => new NumericUpDown { Width = 220 },
            _ => new AutoCompleteBox { Width = 220 },
        };
        Fail(Matrix.Show(picker), "Out of range");

        Assert.Equal("Out of range", Status(Matrix.Part<TextBox>(picker, "PART_TextBox")));
    });

    /// <summary>An app's own TextBox in its own template, not the theme's field, is left alone.</summary>
    [Fact]
    public Task ABareTextBoxIsLeftAlone() => UiThread.RunAsync(() =>
    {
        var box = Matrix.Show(new TextBox { Theme = new Avalonia.Styling.ControlTheme(typeof(TextBox)) });

        Fail(box, "Required");

        Assert.True(string.IsNullOrEmpty(Status(box)));
    });
}
