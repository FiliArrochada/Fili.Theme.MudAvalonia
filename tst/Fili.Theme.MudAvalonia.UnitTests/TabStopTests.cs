using Avalonia.Controls;
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
}
