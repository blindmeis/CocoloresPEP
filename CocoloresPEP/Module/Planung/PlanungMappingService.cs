using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Services;

namespace CocoloresPEP.Module.Planung
{
    public static class PlanungMappingService
    {
        public static ArbeitswocheViewmodel MapArbeitswocheToViewmodel(this Arbeitswoche aw)
        {
            var vm = new ArbeitswocheViewmodel(aw.Jahr, aw.KalenderWoche);
            vm.Mitarbeiter = new List<MitarbeiterViewmodel>(aw.Mitarbeiter.Select(x => x.MapMitarbeiterToViewmodel()));

            var pwmvms = new List<PlanungswocheMitarbeiterViewmodel>();

            foreach (var ma in vm.Mitarbeiter)
            {
                var pwmvm = new PlanungswocheMitarbeiterViewmodel() {Mitarbeiter = ma};

                foreach (var arbeitstag in aw.Arbeitstage)
                {
                    var dow = arbeitstag.Datum.DayOfWeek;
                    var ptvm = new PlanungstagViewmodel()
                    {
                        Datum = arbeitstag.Datum,
                        Planzeiten = arbeitstag.Planzeiten.Where(x => x.ErledigtDurch.Name == ma.Name).ToList()
                    };

                    switch (dow)
                    {
                        case DayOfWeek.Monday:
                            pwmvm.Montag = ptvm;
                            break;
                        case DayOfWeek.Tuesday:
                            pwmvm.Dienstag = ptvm;
                            break;
                        case DayOfWeek.Wednesday:
                            pwmvm.Mittwoch = ptvm;
                            break;
                        case DayOfWeek.Thursday:
                            pwmvm.Donnerstag = ptvm;
                            break;
                        case DayOfWeek.Friday:
                            pwmvm.Freitag = ptvm;
                            break;
                        case DayOfWeek.Saturday:

                            break;
                        case DayOfWeek.Sunday:

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                pwmvms.Add(pwmvm);
            }

            vm.PlanungProMitarbeiterListe = new List<PlanungswocheMitarbeiterViewmodel>(pwmvms);

            return vm;
        }

        public static Arbeitswoche MapViewmodelToArbeitswoche(this ArbeitswocheViewmodel vm)
        {
            var aw = new ArbeitswochenService().CreateArbeitswoche(vm.Jahr, vm.KalenderWoche);

            aw.Mitarbeiter = new List<Common.Entities.Mitarbeiter>(vm.Mitarbeiter.Select(x=>x.MapViewmodelToMitarbeiter()));

            aw.Arbeitstage.Single(x=>x.Datum.DayOfWeek == DayOfWeek.Monday).Planzeiten = 
                new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.SelectMany(x => x.Montag?.Planzeiten ?? new List<PlanItem>()).ToList());

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).Planzeiten =
               new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.SelectMany(x => x.Dienstag?.Planzeiten ?? new List<PlanItem>()));

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).Planzeiten =
                 new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.SelectMany(x => x.Mittwoch?.Planzeiten ?? new List<PlanItem>()));

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).Planzeiten =
               new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.SelectMany(x => x.Donnerstag?.Planzeiten ?? new List<PlanItem>()));

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).Planzeiten =
               new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.SelectMany(x => x.Freitag?.Planzeiten ?? new List<PlanItem>()));

            return aw;
        }


    }
}
