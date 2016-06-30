using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
    public class CalendarBehavior :  Behavior<Calendar>
    {

        public static readonly DependencyProperty SelectedDatesCollectionProperty =
            DependencyProperty.Register("SelectedDatesCollection", typeof(IList<DateTime>), typeof(CalendarBehavior), new FrameworkPropertyMetadata(OnCollectionChanged));

        private static void OnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cb = d as CalendarBehavior;
            var liste = e.NewValue as ObservableCollection<DateTime>;

            if (cb != null && liste != null)
            {
                //Initial die Blackoutliste aufbauen
                cb.AssociatedObject.BlackoutDates.Clear();
                foreach (var dateTime in liste)
                {
                    cb.AssociatedObject.BlackoutDates.Add(new CalendarDateRange(dateTime));
                }
            }
        }

        public IList<DateTime> SelectedDatesCollection
        {
            get { return (IList<DateTime>)GetValue(SelectedDatesCollectionProperty); }
            set { SetValue(SelectedDatesCollectionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseRightButtonDown += AssociatedObjectOnPreviewMouseRightButtonDown;
            AssociatedObject.SelectedDatesChanged += AssociatedObjectOnSelectedDatesChanged;
            AssociatedObject.GotMouseCapture += AssociatedObjectOnGotMouseCapture;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseRightButtonDown -= AssociatedObjectOnPreviewMouseRightButtonDown;
            AssociatedObject.SelectedDatesChanged -= AssociatedObjectOnSelectedDatesChanged;
            AssociatedObject.GotMouseCapture -= AssociatedObjectOnGotMouseCapture;
        }

        private void AssociatedObjectOnSelectedDatesChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selected = AssociatedObject.SelectedDates;
            var toBlackOutDates = new List<DateTime>();

            foreach (var dt in selected)
            {
                if (!SelectedDatesCollection.Contains(dt))
                {
                    SelectedDatesCollection.Add(dt);
                    toBlackOutDates.Add(dt);
                }
            }

            AssociatedObject.SelectedDates.Clear();

            foreach (var dt in toBlackOutDates)
            {
                AssociatedObject.BlackoutDates.Add(new CalendarDateRange(dt));
            }
        }

        private void AssociatedObjectOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {

            var pi = AssociatedObject.GetType()
                        .GetProperty("MonthControl", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(AssociatedObject);

            var mi = pi?.GetType().GetMethod("GetCalendarDayButtons", BindingFlags.Instance | BindingFlags.NonPublic);

            var days = mi?.Invoke(pi, new object[] {}) as IEnumerable<CalendarDayButton>;

            var focused = days?.SingleOrDefault(x => x.IsFocused);

            if (focused != null && focused.IsBlackedOut)
            {
                var dt = (DateTime)focused.DataContext;
                var bd = AssociatedObject.BlackoutDates.SingleOrDefault(x => x.Start == dt && x.End == dt);

                if (bd != null)
                {
                   if (SelectedDatesCollection.Contains(dt))
                     if (SelectedDatesCollection.Remove(dt))
                        AssociatedObject.BlackoutDates.Remove(bd);
                }
            }
        }

        private void AssociatedObjectOnGotMouseCapture(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.OriginalSource is CalendarDayButton || mouseEventArgs.OriginalSource is CalendarItem)
            {
                UIElement originalElement = mouseEventArgs.OriginalSource as UIElement;
                originalElement?.ReleaseMouseCapture();
            }
        }
    }

   
}
