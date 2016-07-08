using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.WpfCore.Service.MessageBox;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Services;

namespace CocoloresPEP.Module.Planung
{
    public static class PlanungMappingService
    {
        public static ArbeitswocheViewmodel MapArbeitswocheToViewmodel(this Arbeitswoche aw)
        {
            var msgService = new WpfMessageBoxService();
            var vm = new ArbeitswocheViewmodel(aw.Jahr, aw.KalenderWoche);
            vm.Mitarbeiter = new List<MitarbeiterViewmodel>(aw.Mitarbeiter.Select(x => x.MapMitarbeiterToViewmodel()));

            vm.Montag = new ArbeitstagWrapper(msgService) {Arbeitstag = aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Monday)};
            vm.Dienstag = new ArbeitstagWrapper(msgService) { Arbeitstag = aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday)};
            vm.Mittwoch = new ArbeitstagWrapper(msgService) { Arbeitstag = aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday)};
            vm.Donnerstag = new ArbeitstagWrapper(msgService) { Arbeitstag = aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday)};
            vm.Freitag = new ArbeitstagWrapper(msgService) { Arbeitstag = aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday)};

            vm.IsMontagFeiertag = vm.Montag.Arbeitstag.IsFeiertag;
            vm.IsDienstagFeiertag = vm.Dienstag.Arbeitstag.IsFeiertag;
            vm.IsMittwochFeiertag = vm.Mittwoch.Arbeitstag.IsFeiertag;
            vm.IsDonnerstagFeiertag = vm.Donnerstag.Arbeitstag.IsFeiertag;
            vm.IsFreitagFeiertag = vm.Freitag.Arbeitstag.IsFeiertag;

            vm.HasMontagGrossteam = vm.Montag.Arbeitstag.HasGrossteam;
            vm.HasDienstagGrossteam = vm.Dienstag.Arbeitstag.HasGrossteam;
            vm.HasMittwochGrossteam = vm.Mittwoch.Arbeitstag.HasGrossteam;
            vm.HasDonnerstagGrossteam = vm.Donnerstag.Arbeitstag.HasGrossteam;
            vm.HasFreitagGrossteam = vm.Freitag.Arbeitstag.HasGrossteam;


            var pwmvms = new List<PlanungswocheMitarbeiterViewmodel>();

            foreach (var ma in vm.Mitarbeiter)
            {
                var pwmvm = new PlanungswocheMitarbeiterViewmodel() {Mitarbeiter = ma};

                foreach (var arbeitstag in aw.Arbeitstage)
                {
                    var dow = arbeitstag.Datum.DayOfWeek;
                    var ptvm = new PlanungstagViewmodel(ma,msgService, new Action(()=>pwmvm.Refresh()))
                    {
                        Datum = arbeitstag.Datum,
                        Planzeit = arbeitstag.Planzeiten.SingleOrDefault(x => x.ErledigtDurch?.Name == ma.Name) ?? arbeitstag.EmptyPlanzeitOhneMitarbeiter,
                        IsFeiertag =  arbeitstag.IsFeiertag,
                        HasGrossteam =  arbeitstag.HasGrossteam
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

            aw.Arbeitstage.Single(x=>x.Datum.DayOfWeek == DayOfWeek.Monday).Planzeiten = new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.Select(x => x.Montag?.Planzeit ?? new PlanItem()));
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Monday).IsFeiertag = vm.IsMontagFeiertag;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Monday).HasGrossteam = vm.HasMontagGrossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Monday).Grossteam = vm.Montag.Arbeitstag.Grossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Monday).KernzeitDoppelBesetzungRange = vm.Montag.Arbeitstag.KernzeitDoppelBesetzungRange;

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).Planzeiten = new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.Select(x => x.Dienstag?.Planzeit ?? new PlanItem()));
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).IsFeiertag = vm.IsDienstagFeiertag;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).HasGrossteam = vm.HasDienstagGrossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).Grossteam = vm.Dienstag.Arbeitstag.Grossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Tuesday).KernzeitDoppelBesetzungRange = vm.Dienstag.Arbeitstag.KernzeitDoppelBesetzungRange;

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).Planzeiten = new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.Select(x => x.Mittwoch?.Planzeit ?? new PlanItem()));
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).IsFeiertag = vm.IsMittwochFeiertag;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).HasGrossteam = vm.HasMittwochGrossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).Grossteam = vm.Mittwoch.Arbeitstag.Grossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Wednesday).KernzeitDoppelBesetzungRange = vm.Mittwoch.Arbeitstag.KernzeitDoppelBesetzungRange;

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).Planzeiten = new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.Select(x => x.Donnerstag?.Planzeit ?? new PlanItem()));
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).IsFeiertag = vm.IsDonnerstagFeiertag;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).HasGrossteam = vm.HasDonnerstagGrossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).Grossteam = vm.Donnerstag.Arbeitstag.Grossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Thursday).KernzeitDoppelBesetzungRange = vm.Donnerstag.Arbeitstag.KernzeitDoppelBesetzungRange;

            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).Planzeiten = new ObservableCollection<PlanItem>(vm.PlanungProMitarbeiterListe.Select(x => x.Freitag?.Planzeit ?? new PlanItem()));
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).IsFeiertag = vm.IsFreitagFeiertag;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).HasGrossteam = vm.HasFreitagGrossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).Grossteam = vm.Freitag.Arbeitstag.Grossteam;
            aw.Arbeitstage.Single(x => x.Datum.DayOfWeek == DayOfWeek.Friday).KernzeitDoppelBesetzungRange = vm.Freitag.Arbeitstag.KernzeitDoppelBesetzungRange;

            return aw;
        }


    }
}
