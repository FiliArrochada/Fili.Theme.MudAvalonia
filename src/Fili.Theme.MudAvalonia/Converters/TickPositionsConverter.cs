using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// Where a slider's tick marks go: one offset per tick, along the track, for the dot's top-left.
/// </summary>
///
/// <remarks>
/// <para>
/// MudSlider draws its <c>TickMarks</c> as dots ON the rail, one per <c>Step</c>, the rail's own
/// thickness and the slider's colour (<c>.mud-slider-track-tick</c> in _slider.scss). Avalonia's
/// <c>TickBar</c> can only draw lines, so the dots are an ItemsControl on a Canvas, and their
/// positions need arithmetic a binding cannot do - the same reason <see cref="FactorConverter"/>
/// exists. Internal for the same reason too: nothing here is an API.
/// </para>
/// <para>
/// The values are, in order: Minimum, Maximum, TickFrequency, Ticks, IsDirectionReversed, the
/// track's length along its axis, and the dot's size. The parameter is the orientation. The
/// thumb is 40px, so its centre travels from 20px in to 20px short of the far end - exactly
/// where a tick for the minimum and the maximum must sit. A vertical slider runs bottom to top.
/// </para>
/// </remarks>
internal sealed class TickPositionsConverter : IMultiValueConverter
{
    /// <summary>Half the thumb's 40px hit area: the inset of the first and last tick.</summary>
    private const double Inset = 20;

    /// <summary>Enough for any slider a person can read; a runaway frequency draws nothing.</summary>
    private const int MaxTicks = 1000;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 7
            || values[0] is not double min
            || values[1] is not double max
            || values[4] is not bool reversed
            || values[5] is not double length
            || values[6] is not double dot
            || max <= min
            || length <= 2 * Inset)
        {
            return Array.Empty<double>();
        }

        var frequency = values[2] as double? ?? 0;
        var explicitTicks = values[3] as IEnumerable<double>;
        var vertical = parameter is Orientation.Vertical
            || (parameter is string text && text.Equals(nameof(Orientation.Vertical), StringComparison.Ordinal));

        IEnumerable<double> ticks;

        if (explicitTicks?.Any() == true)
        {
            ticks = explicitTicks.Where(t => t >= min && t <= max);
        }
        else if (frequency > 0 && (max - min) / frequency < MaxTicks)
        {
            var count = (int)Math.Floor((max - min) / frequency + 1e-9);
            ticks = Enumerable.Range(0, count + 1).Select(i => min + i * frequency);
        }
        else
        {
            return Array.Empty<double>();
        }

        var travel = length - 2 * Inset;

        return ticks
            .Select(tick =>
            {
                var fraction = (tick - min) / (max - min);

                // Horizontal runs left to right, vertical bottom to top; reversing flips either.
                if (vertical != reversed)
                {
                    fraction = 1 - fraction;
                }

                return Inset + fraction * travel - dot / 2;
            })
            .ToArray();
    }
}
