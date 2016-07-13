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
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP.Views.Planung
{
    /// <summary>
    /// Interaktionslogik für PlanungMitarbeiterTagControl.xaml
    /// </summary>
    public partial class PlanungMitarbeiterTagControl : UserControl
    {
        public PlanungMitarbeiterTagControl()
        {
            InitializeComponent();
        }

        public readonly static DependencyProperty MitarbeiterViewmodelProperty = 
            DependencyProperty.Register("MitarbeiterViewmodel", typeof(MitarbeiterViewmodel), typeof(PlanungMitarbeiterTagControl));

        public MitarbeiterViewmodel MitarbeiterViewmodel
        {
            get { return (MitarbeiterViewmodel)GetValue(MitarbeiterViewmodelProperty); }
            set { SetValue(MitarbeiterViewmodelProperty, value); }
        }

        public readonly static DependencyProperty PlanungstagViewmodelProperty = 
            DependencyProperty.Register("PlanungstagViewmodel", typeof(PlanungstagViewmodel), typeof(PlanungMitarbeiterTagControl));

        public PlanungstagViewmodel PlanungstagViewmodel
        {
            get { return (PlanungstagViewmodel)GetValue(PlanungstagViewmodelProperty); }
            set { SetValue(PlanungstagViewmodelProperty, value); }
        }

        public readonly static DependencyProperty ShowThemenProperty =
           DependencyProperty.Register("ShowThemen", typeof(bool), typeof(PlanungMitarbeiterTagControl));

        public bool ShowThemen
        {
            get { return (bool)GetValue(ShowThemenProperty); }
            set { SetValue(ShowThemenProperty, value); }
        }
    }
}
