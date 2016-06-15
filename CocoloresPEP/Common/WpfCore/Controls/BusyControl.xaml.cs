using System.Windows;
using System.Windows.Controls;

namespace CocoloresPEP.Common.WpfCore.Controls
{
    /// <summary>
    /// Interaktionslogik für BusyControl.xaml
    /// </summary>
    public partial class BusyControl : UserControl
    {
        public BusyControl()
        {
            InitializeComponent();
        }

        public readonly static DependencyProperty IsBusyProperty = DependencyProperty.Register(
        "IsBusy", typeof(bool), typeof(BusyControl), new PropertyMetadata(false));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public readonly static DependencyProperty ProgressbarSizeProperty = DependencyProperty.Register(
        "ProgressbarSize", typeof(double), typeof(BusyControl), new PropertyMetadata(40d));

        public double ProgressbarSize
        {
            get { return (double)GetValue(ProgressbarSizeProperty); }
            set { SetValue(ProgressbarSizeProperty, value); }
        }
    }
}
