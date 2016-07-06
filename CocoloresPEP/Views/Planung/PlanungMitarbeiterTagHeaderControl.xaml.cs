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
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP.Views.Planung
{
    /// <summary>
    /// Interaktionslogik für PlanungMitarbeiterTagHeaderControl.xaml
    /// </summary>
    public partial class PlanungMitarbeiterTagHeaderControl : UserControl
    {
        public PlanungMitarbeiterTagHeaderControl()
        {
            InitializeComponent();
        }

        public readonly static DependencyProperty WochentagProperty =
           DependencyProperty.Register("Wochentag", typeof(string), typeof(PlanungMitarbeiterTagHeaderControl));

        public string Wochentag
        {
            get { return (string)GetValue(WochentagProperty); }
            set { SetValue(WochentagProperty, value); }
        }

        public readonly static DependencyProperty IsFeiertagProperty =
                DependencyProperty.Register("IsFeiertag", typeof(bool), typeof(PlanungMitarbeiterTagHeaderControl));

        public bool IsFeiertag
        {
            get { return (bool)GetValue(IsFeiertagProperty); }
            set { SetValue(IsFeiertagProperty, value); }
        }

        public readonly static DependencyProperty HasGrossteamProperty =
               DependencyProperty.Register("HasGrossteam", typeof(bool), typeof(PlanungMitarbeiterTagHeaderControl));

        public bool HasGrossteam
        {
            get { return (bool)GetValue(HasGrossteamProperty); }
            set { SetValue(HasGrossteamProperty, value); }
        }

        public readonly static DependencyProperty ArbeitstagWrapperProperty =
              DependencyProperty.Register("ArbeitstagWrapper", typeof(ArbeitstagWrapper), typeof(PlanungMitarbeiterTagHeaderControl));

        public ArbeitstagWrapper ArbeitstagWrapper
        {
            get { return (ArbeitstagWrapper)GetValue(ArbeitstagWrapperProperty); }
            set { SetValue(ArbeitstagWrapperProperty, value); }
        }
    }
}
