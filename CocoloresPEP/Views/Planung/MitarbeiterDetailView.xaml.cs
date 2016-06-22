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
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP.Views.Planung
{
    /// <summary>
    /// Interaktionslogik für MitarbeiterDetailView.xaml
    /// </summary>
    public partial class MitarbeiterDetailView : UserControl
    {
        public MitarbeiterDetailView()
        {
            InitializeComponent();
        }

        public readonly static DependencyProperty MitarbeiterViewmodelProperty =
           DependencyProperty.Register("MitarbeiterViewmodel", typeof(MitarbeiterViewmodel), typeof(MitarbeiterDetailView));

        public MitarbeiterViewmodel MitarbeiterViewmodel
        {
            get { return (MitarbeiterViewmodel)GetValue(MitarbeiterViewmodelProperty); }
            set { SetValue(MitarbeiterViewmodelProperty, value); }
        }

        public readonly static DependencyProperty DisplayDateStartProperty =
        DependencyProperty.Register("DisplayDateStart", typeof(DateTime?), typeof(MitarbeiterDetailView), new PropertyMetadata(OnDisplayDateStartChanged));

        private static void OnDisplayDateStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public DateTime? DisplayDateStart
        {
            get { return (DateTime?)GetValue(DisplayDateStartProperty); }
            set { SetValue(DisplayDateStartProperty, value); }
        }

        public readonly static DependencyProperty DisplayDateEndProperty =
        DependencyProperty.Register("DisplayDateEnd", typeof(DateTime?), typeof(MitarbeiterDetailView));

        public DateTime? DisplayDateEnd
        {
            get { return (DateTime?)GetValue(DisplayDateEndProperty); }
            set { SetValue(DisplayDateEndProperty, value); }
        }
    }
}
