using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                cb.AssociatedObject.BlackoutDates.Clear();
                foreach (var dateTime in liste)
                {
                    cb.AssociatedObject.BlackoutDates.Add(new CalendarDateRange(dateTime));
                }

                liste.CollectionChanged += (sender, args) =>
                {
                    cb.AssociatedObject.BlackoutDates.Clear();
                    foreach (var dateTime in liste)
                    {
                        cb.AssociatedObject.BlackoutDates.Add(new CalendarDateRange(dateTime));
                    }
                };
            }
        }

        public IList<DateTime> SelectedDatesCollection
        {
            get { return (IList<DateTime>)GetValue(SelectedDatesCollectionProperty); }
            set { SetValue(SelectedDatesCollectionProperty, value); }
        }


     

    }
}
