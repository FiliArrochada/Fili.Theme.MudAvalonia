using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
    /// <summary>
    /// `.mud-input-numeric-spin`: the spin column sits inside the input box, top to bottom, and the
    /// box runs on under it, so the underline, the filled ground and the outline reach the field's
    /// edge; the text keeps 24px clear of it, as `padding-right: 24px` does. The helper line below
    /// runs the whole width.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("filled")]
    [InlineData("outlined")]
    public Task TheSpinColumnSitsInsideTheInputBox(string variant) => UiThread.RunAsync(() =>
    {
        var field = (NumericUpDown)Show("NumericUpDown", variant.Length > 0 ? [variant, "counter"] : ["counter"]);
        Dispatcher.UIThread.RunJobs();
        var box = Field(field);
        var root = Matrix.Part<Border>(box, "PART_Root");
        var column = Matrix.Part<Control>(Matrix.Part<ButtonSpinner>(field, "PART_Spinner"), "PART_SpinnerPanel");
        var text = Matrix.Part<Control>(box, "PART_ScrollViewer");
        double Left(Visual v) => v.TranslatePoint(default, field)!.Value.X;
        double Top(Visual v) => v.TranslatePoint(default, field)!.Value.Y;

        Assert.Equal(field.Bounds.Width, box.Bounds.Width, 0.5);
        Assert.Equal(field.Bounds.Width, root.Bounds.Width, 0.5);
        Assert.Equal(field.Bounds.Width, Left(column) + column.Bounds.Width, 0.5);
        Assert.Equal(Top(root), Top(column), 0.5);
        Assert.Equal(root.Bounds.Height, column.Bounds.Height, 0.5);
        Assert.InRange(Left(text) + text.Bounds.Width, 0, field.Bounds.Width - 24 + 0.5);
        // The helper line keeps its own inset under filled and outlined, and nothing more.
        var helper = Matrix.Part<Control>(box, "PART_HelperLine");
        Assert.Equal(field.Bounds.Width - helper.Margin.Right, Left(helper) + helper.Bounds.Width, 0.5);
    });

    /// <summary>
    /// MudInput's NumericUpIcon and NumericDownIcon are KeyboardArrowUp and KeyboardArrowDown,
    /// chevrons with keys of their own: not the select's ArrowDropUp and ArrowDropDown.
    /// </summary>
    [Fact]
    public Task TheSpinButtonsDrawMudBlazorsChevrons() => UiThread.RunAsync(() =>
    {
        var field = (NumericUpDown)Show("NumericUpDown");
        Dispatcher.UIThread.RunJobs();
        var spinner = Matrix.Part<ButtonSpinner>(field, "PART_Spinner");
        Application.Current!.TryFindResource("FiliKeyboardArrowUpGeometry", out var up);
        Application.Current!.TryFindResource("FiliKeyboardArrowDownGeometry", out var down);

        Assert.NotNull(up);
        Assert.Same(up, ((Avalonia.Controls.Shapes.Path)Matrix.Part<RepeatButton>(spinner, "PART_IncreaseButton").Content!).Data);
        Assert.Same(down, ((Avalonia.Controls.Shapes.Path)Matrix.Part<RepeatButton>(spinner, "PART_DecreaseButton").Content!).Data);
    });

    /// <summary>With no spin buttons the box is the field, and the text runs to its padding.</summary>
    [Fact]
    public Task WithoutSpinButtonsTheTextHasTheWholeBox() => UiThread.RunAsync(() =>
    {
        var field = (NumericUpDown)Show("NumericUpDown");
        field.ShowButtonSpinner = false;
        Dispatcher.UIThread.RunJobs();
        var box = Field(field);

        Assert.Equal(field.Bounds.Width, box.Bounds.Width, 0.5);
        Assert.Equal(default, Matrix.Part<ContentPresenter>(box, "PART_InnerRightContent").Margin);
        Assert.Equal(default, Matrix.Part<ContentPresenter>(box, "PART_InnerLeftContent").Margin);
    });

    /// <summary>Spin buttons on the left: the column at the start of the box, the text clear of it there.</summary>
    [Fact]
    public Task SpinButtonsOnTheLeftSitInsideTheBoxThere() => UiThread.RunAsync(() =>
    {
        var field = (NumericUpDown)Show("NumericUpDown");
        field.ButtonSpinnerLocation = Location.Left;
        Dispatcher.UIThread.RunJobs();
        var box = Field(field);
        var column = Matrix.Part<Control>(Matrix.Part<ButtonSpinner>(field, "PART_Spinner"), "PART_SpinnerPanel");

        Assert.Equal(0, column.TranslatePoint(default, field)!.Value.X, 0.5);
        Assert.Equal(0, box.TranslatePoint(default, field)!.Value.X, 0.5);
        Assert.Equal(new Thickness(24, 0, 0, 0), Matrix.Part<ContentPresenter>(box, "PART_InnerLeftContent").Margin);
        Assert.Equal(default, Matrix.Part<ContentPresenter>(box, "PART_InnerRightContent").Margin);
    });
}
