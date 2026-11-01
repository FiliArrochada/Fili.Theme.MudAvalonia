using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using Xunit;

namespace Fili.Theme.MudAvalonia.UnitTests;

/// <summary>
/// A control is one Tab stop at most: itself or the one part that stands for it, as a TextBox is
/// inside an AutoCompleteBox. A part that is focusable as well adds a stop the keyboard has to
/// walk through for nothing. The first found: a slider's two track halves, 2px tall, each took
/// focus and drew its ring round a hairline - two extra stops per slider.
/// </summary>
public class TabStopTests
{
    [Theory]
    [MemberData(nameof(BindingErrorTests.Controls), MemberType = typeof(BindingErrorTests))]
    public Task AControlIsOneTabStopAtMost(string spec) => UiThread.RunAsync(() =>
    {
        var control = Matrix.Show(BindingErrorTests.Create(spec));

        var stops = control.GetSelfAndVisualDescendants().OfType<InputElement>()
            .Where(e => e == control || e.TemplatedParent == control)
            .Where(e => e.Focusable && e.IsTabStop && e.IsEffectivelyVisible && e.IsEffectivelyEnabled)
            .Select(e => $"{e.GetType().Name}#{e.Name}")
            .ToList();

        Assert.True(stops.Count <= 1, $"{spec}: " + string.Join(", ", stops));
    });

    /// <summary>
    /// The controls a user acts on, of those above: a progress bar and a badge are read, not
    /// operated, and take no stop.
    /// </summary>
    public static readonly TheoryData<string> Operable = [.. BindingErrorTests.Controls
        .Select(row => row.Data)
        .Where(spec => !spec.StartsWith("ProgressBar", StringComparison.Ordinal) && !spec.StartsWith("HeaderedContentControl", StringComparison.Ordinal))];

    /// <summary>
    /// And at least one: Tab from the button before a control lands in it, and Tab from inside it
    /// reaches the button after. "At most one" alone passes a control a template made unreachable,
    /// a part marked <c>Focusable="False"</c> one too many, and a control that traps focus.
    /// </summary>
    [Theory]
    [MemberData(nameof(Operable))]
    public Task EveryOperableControlIsReachedByTab(string spec) => UiThread.RunAsync(() =>
    {
        var before = new Button { Content = "Before" };
        var control = BindingErrorTests.Create(spec);
        var after = new Button { Content = "After" };
        Matrix.Show(new StackPanel { Children = { before, control, after } });
        var window = (Window)TopLevel.GetTopLevel(control)!;
        before.Focus(NavigationMethod.Tab);

        var landed = Tab(window);
        Assert.True(landed is not null && (landed == control || landed.GetVisualAncestors().Contains(control)),
            $"{spec}: Tab from the button before landed on {landed?.GetType().Name ?? "nothing"}#{(landed as Control)?.Name}.");

        // Walk on through the control's own stops (a pager's buttons, a menu's items), and out.
        var at = landed;
        for (var step = 0; step < 10 && at != after; step++)
        {
            at = Tab(window);
        }

        Assert.True(at == after, $"{spec}: Tab never left it; it stopped on {at?.GetType().Name}#{(at as Control)?.Name}.");

        static Visual? Tab(Window window)
        {
            window.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            return window.FocusManager!.GetFocusedElement() as Visual;
        }
    });
}
