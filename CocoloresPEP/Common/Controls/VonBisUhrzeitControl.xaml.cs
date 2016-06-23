using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CocoloresPEP.Common.Controls
{
    /// <summary>
    /// Interaktionslogik für VonBisUhrzeitControl.xaml
    /// </summary>
    public partial class VonBisUhrzeitControl : UserControl
    {
        public VonBisUhrzeitControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedHourVonProperty =
      DependencyProperty.Register("SelectedHourVon", typeof(int), typeof(VonBisUhrzeitControl));

        public int SelectedHourVon
        {
            get { return (int)GetValue(SelectedHourVonProperty); }
            set { SetValue(SelectedHourVonProperty, value); }
        }


        public static readonly DependencyProperty SelectedHourBisProperty =
       DependencyProperty.Register("SelectedHourBis", typeof(int), typeof(VonBisUhrzeitControl));

        public int SelectedHourBis
        {
            get { return (int)GetValue(SelectedHourBisProperty); }
            set { SetValue(SelectedHourBisProperty, value); }
        }

        public static readonly DependencyProperty SelectedMinuteVonProperty =
       DependencyProperty.Register("SelectedMinuteVon", typeof(int), typeof(VonBisUhrzeitControl));

        public int SelectedMinuteVon
        {
            get { return (int)GetValue(SelectedMinuteVonProperty); }
            set { SetValue(SelectedMinuteVonProperty, value); }
        }


        public static readonly DependencyProperty SelectedMinuteBisProperty =
       DependencyProperty.Register("SelectedMinuteBis", typeof(int), typeof(VonBisUhrzeitControl));

        public int SelectedMinuteBis
        {
            get { return (int)GetValue(SelectedMinuteBisProperty); }
            set { SetValue(SelectedMinuteBisProperty, value); }
        }
    }
}
