using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
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

        // The data grid samples. A collection view carries the sort and the grouping, which is
        // what lights a header's arrow and draws the group rows. Two levels, so a nested group's
        // header shows its 48px indent.
        var sorted = new DataGridCollectionView(GalleryElement.FirstSix);
        sorted.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(GalleryElement.Name)));
        PlainGrid.ItemsSource = sorted;

        OptionsGrid.ItemsSource = GalleryElement.FirstSix;
        OptionsGrid.SelectedIndex = 2;

        var grouped = new DataGridCollectionView(GalleryElement.FirstSix);
        grouped.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(GalleryElement.Group)));
        grouped.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(GalleryElement.Position)));
        GroupedGrid.ItemsSource = grouped;

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

            // A collection view makes its first item current, and the grid selects what is current.
            // Only the options sample is meant to show a selected row.
            PlainGrid.SelectedIndex = -1;
            GroupedGrid.SelectedIndex = -1;
            ((IPseudoClasses)FocusedText.Classes).Add(":focus-visible");
        };
    }

    /// <summary>The section headings, top to bottom: TextBlocks tagged "section".</summary>
    private TextBlock[] _headings = [];

    /// <summary>True while scrolling moves the index's selection, which is not a request to go there.</summary>
    private bool _followingScroll;

    /// <summary>
    /// The index on the left: one row per section heading, in a pane that stays put while the
    /// samples scroll. Choosing a row brings its heading to the top - by setting the offset
    /// itself, since this page ignores bring-into-view requests that do not come from keyboard
    /// focus (see the constructor) - and scrolling selects the row of the section in view.
    /// </summary>
    private void BuildSectionIndex()
    {
        _headings = PageContent.GetLogicalDescendants().OfType<TextBlock>().Where(t => Equals(t.Tag, "section")).ToArray();

        foreach (var heading in _headings)
        {
            SectionIndex.Items.Add(new ListBoxItem
            {
                Content = new TextBlock { Text = heading.Text, TextWrapping = TextWrapping.Wrap },
                Classes = { "dense" },
            });
        }

        SectionIndex.SelectionChanged += (_, _) =>
        {
            if (!_followingScroll)
            {
                ScrollToSection(SectionIndex.SelectedIndex);
            }
        };

        // Choosing the row that is already selected changes nothing, yet the reader may have
        // scrolled past the top of that section and wants its heading back.
        SectionIndex.Tapped += (_, _) => ScrollToSection(SectionIndex.SelectedIndex);

        Page.ScrollChanged += (_, _) => FollowScroll();
    }

    private void ScrollToSection(int index)
    {
        if (index < 0)
        {
            return;
        }

        var top = _headings[index].TranslatePoint(default, PageContent)?.Y ?? 0;

        // Back off a little, so the divider above the heading shows too.
        Page.Offset = new Vector(Page.Offset.X, Math.Max(0, top + PageContent.Margin.Top - 16));
    }

    /// <summary>
    /// Selects the last section whose heading has reached the top third of the page, or none on a
    /// page that does not scroll - a screenshot frame sized to the whole content.
    /// </summary>
    private void FollowScroll()
    {
        if (Page.Extent.Height <= Page.Viewport.Height)
        {
            SelectSection(-1);
            return;
        }

        var line = Page.Viewport.Height / 3;
        var current = -1;

        for (var i = 0; i < _headings.Length; i++)
        {
            var top = _headings[i].TranslatePoint(default, Page)?.Y;
            if (top is null || top > line)
            {
                break;
            }

            current = i;
        }

        // At the very bottom the last sections can never reach the line, so the last one wins.
        if (Page.Offset.Y > 0 && Page.Offset.Y >= Page.Extent.Height - Page.Viewport.Height - 1)
        {
            current = _headings.Length - 1;
        }

        SelectSection(current);
    }

    private void SelectSection(int index)
    {
        _followingScroll = true;
        SectionIndex.SelectedIndex = index;
        _followingScroll = false;
    }
}
