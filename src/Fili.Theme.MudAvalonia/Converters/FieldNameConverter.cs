using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fili.Theme.MudAvalonia.Converters;

/// <summary>
/// A field's name to a screen reader: its label, <c>PlaceholderText</c>, unless the app labels it
/// with <c>AutomationProperties.LabeledBy</c>. Takes the label and the labelling control, in that
/// order.
/// </summary>
///
/// <remarks>
/// <para>
/// Avalonia's automation peer reads <c>AutomationProperties.Name</c> first and turns to
/// <c>LabeledBy</c> only when the name is blank, so a name set from the label would hide the
/// control an app named the field with. Null here leaves that control to name it
/// (<c>FieldNameTests</c>).
/// </para>
/// <para>
/// Internal, like <see cref="FactorConverter"/>: a template needs it, a consumer does not.
/// </para>
/// </remarks>
internal sealed class FieldNameConverter : IMultiValueConverter
{
    public static readonly FieldNameConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count == 2 && values[1] is null && values[0] is string label ? label : null;
}
