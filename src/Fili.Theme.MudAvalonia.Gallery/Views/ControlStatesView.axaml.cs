using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Fili.Theme.MudAvalonia.Gallery.Views;

public partial class ControlStatesView : UserControl
{
    public ControlStatesView()
    {
        InitializeComponent();

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

            if (!focused)
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
            FocusedSample.Focus(NavigationMethod.Tab);
            SelectedNode.IsSelected = true;
            ((IPseudoClasses)FocusedText.Classes).Add(":focus-visible");
        };
    }
}
