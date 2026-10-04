using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia.Gallery.Views;

public partial class ControlStatesView : UserControl
{
    /// <summary>
    /// True while the page itself moves focus for a screenshot prop, which is not somewhere the
    /// reader asked to go either.
    /// </summary>
    private bool _placingFocus;

    public ControlStatesView()
    {
        InitializeComponent();

        BuildSectionIndex();

        // The page opens at the top. Every sample with a selection - the tab strips, the drawer's
        // list, the pips - asks to bring its selected item into view, and the ones with no scroll
        // area of their own pass that request up to this page's ScrollViewer, which obeyed the
        // last of them and opened the tab near the bottom. A selection set in markup is a sample,
        // not a place the reader asked to go, so only a request from where keyboard focus is -
        // tabbing, typing - is let through. The handler sits on the content, BELOW the
        // ScrollViewer, because the scroller handles the request on its way up.
        PageContent.AddHandler(RequestBringIntoViewEvent, (_, e) =>
        {
            var focused = e.TargetObject?.GetSelfAndVisualAncestors()
                .OfType<InputElement>()
                .Any(element => element.IsFocused) == true;

            if (!focused || _placingFocus)
            {
                e.Handled = true;
            }
        });

        // A real validation failure, set the way a failing binding sets one. It is done here
        // rather than in markup because DataValidationErrors.Errors takes a live collection and
        // x:Array does not survive Avalonia's compiled XAML - and because a hand-set `.error`
        // class would prove nothing: the point of this sample is that the RULE turns red and the
        // MESSAGE appears without the app asking for either.
        DataValidationErrors.SetError(
            InvalidField,
            new InvalidOperationException("Must be a valid email address"));

        // Keyboard focus, pinned so it shows up in a capture. NavigationMethod.Tab is what sets
        // :focus-visible; focusing with Pointer deliberately does not, which is the whole reason
        // the theme keys off :focus-visible rather than :focus.
        //
        // Only one element can hold focus, so the second sample is faked with the pseudo-class
        // directly. That is not something an app should do - it is a screenshot prop.
        Loaded += (_, _) =>
        {
            // Without the guard, focusing it scrolled the page to it - below the fold in a browser
            // tab, so the page opened halfway down its first screen.
            _placingFocus = true;
            FocusedSample.Focus(NavigationMethod.Tab);
            _placingFocus = false;
            SelectedNode.IsSelected = true;
            ((IPseudoClasses)FocusedText.Classes).Add(":focus-visible");
        };
    }

    /// <summary>
    /// One link per section heading (a TextBlock tagged "section"), at the top of the page. The
    /// page is long enough that a reader looking for one control should not have to scroll for
    /// it. A link sets the scroll offset itself, because this page ignores bring-into-view
    /// requests that do not come from keyboard focus - see the constructor.
    /// </summary>
    private void BuildSectionIndex()
    {
        foreach (var heading in PageContent.GetLogicalDescendants().OfType<TextBlock>().Where(t => Equals(t.Tag, "section")))
        {
            var link = new Button { Content = heading.Text, Classes = { "primary", "small" } };

            link.Click += (_, _) =>
            {
                var top = heading.TranslatePoint(default, PageContent)?.Y ?? 0;

                // Back off a little, so the divider above the heading shows too.
                Page.Offset = new Vector(Page.Offset.X, Math.Max(0, top + PageContent.Margin.Top - 16));
            };

            SectionIndex.Children.Add(link);
        }
    }
}
