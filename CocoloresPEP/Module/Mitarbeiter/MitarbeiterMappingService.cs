using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public static class MitarbeiterMappingService
    {
        public static MitarbeiterViewmodel MapMitarbeiterToViewmodel(this Common.Entities.Mitarbeiter ma)
        {
            var vm = new MitarbeiterViewmodel()
            {
                Name = ma.Name,
                WochenStunden = ma.WochenStunden,
                DefaultGruppe = ma.DefaultGruppe,
                Wunschdienste = ma.Wunschdienste,
                SaldoWochenStunden = ma.SaldoWochenStunden,
                IsHelfer = ma.IsHelfer
            };

           vm.NichtDaZeiten = new ObservableCollection<DateTime>(ma.NichtDaZeiten);

            return vm;

        }

        public static Common.Entities.Mitarbeiter MapViewmodelToMitarbeiter(this MitarbeiterViewmodel vm)
        {
            var ma = new Common.Entities.Mitarbeiter()
            {
                Name = vm.Name,
                WochenStunden = vm.WochenStunden,
                DefaultGruppe = vm.DefaultGruppe,
                Wunschdienste = vm.Wunschdienste,
                SaldoWochenStunden = vm.SaldoWochenStunden,
                IsHelfer = vm.IsHelfer
            };

            ma.NichtDaZeiten = new ObservableCollection<DateTime>(vm.NichtDaZeiten);

            return ma;
        }
    }
}
