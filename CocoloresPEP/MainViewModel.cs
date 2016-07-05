using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.WpfCore;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Common.WpfCore.Service.MessageBox;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP
{
    public class MainViewModel : ViewmodelBase
    {
        private readonly ObservableCollection<MitarbeiterViewmodel> _mitarbeiters;
        private WpfMessageBoxService _msg;

        public MainViewModel()
        {
            MitarbeiterVm = new MitarbeiterManager();
            PlanungVm = new PlanungManager();
            _msg = new WpfMessageBoxService();

            _mitarbeiters = MitarbeiterVm.MitarbeiterCollection;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var hasChangesMitarbeiter = MitarbeiterVm.HasChanges();
            var hasChangesPlanung = PlanungVm.HasChanges();

            if (hasChangesMitarbeiter || hasChangesPlanung)
            {
                var result = _msg.ShowYesNoCancel($"Es liegen noch ungespeicherte Änderungen vor.{Environment.NewLine}Wollen Sie das Programm trotzdem beenden?", CustomDialogIcons.Warning);
                if (result != CustomDialogResults.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        public MitarbeiterManager MitarbeiterVm { get; set; }
        public PlanungManager PlanungVm { get; set; }

     
    }
}
