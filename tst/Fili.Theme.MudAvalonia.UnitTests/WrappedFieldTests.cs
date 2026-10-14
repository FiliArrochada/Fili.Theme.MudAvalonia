using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// MudAutocomplete and MudNumericField are MudInputs, so they take MudTextField's Variant,
/// Margin, Error, Counter and HelperText. Here they are an <c>AutoCompleteBox</c> and a
/// <c>NumericUpDown</c>, whose field is a <c>TextBox</c> in their template: the text field's
/// classes and helper text, set on the outer control, must reach it. Until 0.9 none did.
/// </summary>
public class WrappedFieldTests
{
    public static readonly TheoryData<string> Wrappers = ["AutoCompleteBox", "NumericUpDown"];

    private static TemplatedControl Show(string wrapper, params string[] classes)
    {
        TemplatedControl control = wrapper == "AutoCompleteBox"
            ? new AutoCompleteBox { Width = 220, PlaceholderText = "Search" }
            : new NumericUpDown { Width = 220, PlaceholderText = "Amount" };
        control.Classes.AddRange(classes);
        return Matrix.Show(control);
    }

    private static TextBox Field(TemplatedControl control) => Matrix.Part<TextBox>(control, "PART_TextBox");

    private static ControlTheme Theme(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, out var value), $"{key} did not resolve.");
        return Assert.IsType<ControlTheme>(value);
    }

    [Theory]
    [MemberData(nameof(Wrappers))]
    public Task TheVariantsReachTheField(string wrapper) => UiThread.RunAsync(() =>
    {
        Assert.Same(Theme("FiliFilledTextBox"), Field(Show(wrapper, "filled")).Theme);
        Assert.Same(Theme("FiliOutlinedTextBox"), Field(Show(wrapper, "outlined")).Theme);
        Assert.DoesNotContain("filled", Field(Show(wrapper)).Classes);
    });

    [Theory]
    [MemberData(nameof(Wrappers))]
    public Task EveryTextFieldClassReachesTheField(string wrapper) => UiThread.RunAsync(() =>
    {
        foreach (var name in new[] { "dense", "error", "counter", "helper-on-focus" })
        {
            Assert.Contains(name, Field(Show(wrapper, name)).Classes);
        }
    });

    /// <summary>MudTextField's Error is a parameter an app flips as validation runs.</summary>
    [Theory]
    [MemberData(nameof(Wrappers))]
    public Task AClassSetLaterFollows(string wrapper) => UiThread.RunAsync(() =>
    {
        var control = Show(wrapper);
        var field = Field(control);

        control.Classes.Add("error");
        Assert.Contains("error", field.Classes);

        control.Classes.Remove("error");
        Assert.DoesNotContain("error", field.Classes);
    });

    [Theory]
    [MemberData(nameof(Wrappers))]
    public Task TheHelperTextReachesTheField(string wrapper) => UiThread.RunAsync(() =>
    {
        var control = Show(wrapper);
        AutomationProperties.SetHelpText(control, "Pick one");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Pick one", AutomationProperties.GetHelpText(Field(control)));
    });

    /// <summary>A NumericUpDown's validation error shows in the field's helper line, with its red rule.</summary>
    [Fact]
    public Task ANumericFieldsErrorIsTheFieldsOwn() => UiThread.RunAsync(() =>
    {
        var control = Show("NumericUpDown");
        DataValidationErrors.SetErrors(control, ["Too large"]);
        Dispatcher.UIThread.RunJobs();

        Assert.True(DataValidationErrors.GetHasErrors(Field(control)));
    });
}
