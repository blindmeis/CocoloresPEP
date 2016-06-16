using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Services
{
    public static class PlanService
    {
        private static Random Zufall = new Random();

        public static void ErstelleWochenplan(Arbeitswoche woche, IList<Mitarbeiter> maList)
        {
            foreach (var arbeitstag in woche.Arbeitstage)
            {
                arbeitstag.Planzeiten.Clear();

                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum)).ToList();

                var frühdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 7, 0, 0);
                var achtuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 8, 0, 0);
                var achtuhr30Dienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 8, 30, 0);
                var neunuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 9, 0, 0);
                var zehnuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 10, 0, 0);
                var spätdienstEnde = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 17, 15, 0);



                var listMaDieNichtMehr = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr, SollTyp.Frühdienst);

                listMaDieNichtMehr.Add(maFrüh);
                var istFrüh = CreatePlanItem(maFrüh, frühdienst, maFrüh.TagesQuarterTicks, SollTyp.Frühdienst | maFrüh.DefaultGruppe);
                arbeitstag.Planzeiten.Add(istFrüh);

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr, SollTyp.Spätdienst);

                listMaDieNichtMehr.Add(maSpät1);
                var spätdienstStart1 = spätdienstEnde.AddMinutes(-1 * 15 * maSpät1.TagesQuarterTicks);
                var istSpät1 = CreatePlanItem(maSpät1, spätdienstStart1, maSpät1.TagesQuarterTicks, SollTyp.Spätdienst | maSpät1.DefaultGruppe);
                arbeitstag.Planzeiten.Add(istSpät1);

                var maSpät2 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr, SollTyp.Spätdienst);

                listMaDieNichtMehr.Add(maSpät2);
                var spätdienstStart2 = spätdienstEnde.AddMinutes(-1 * 15 * maSpät2.TagesQuarterTicks);
                var istSpät2 = CreatePlanItem(maSpät2, spätdienstStart2, maSpät2.TagesQuarterTicks, SollTyp.Spätdienst | maSpät2.DefaultGruppe);
                arbeitstag.Planzeiten.Add(istSpät2);

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr, SollTyp.AchtUhrDienst);
                listMaDieNichtMehr.Add(ma8UhrErster);
                var ist8UhrErster = CreatePlanItem(ma8UhrErster, achtuhrdienst, ma8UhrErster.TagesQuarterTicks, SollTyp.AchtUhrDienst | ma8UhrErster.DefaultGruppe);
                arbeitstag.Planzeiten.Add(ist8UhrErster);

                var ma8UhrZweiter = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr, SollTyp.AchtUhrDienst);
                listMaDieNichtMehr.Add(ma8UhrZweiter);
                var ist8UhrZweiter = CreatePlanItem(ma8UhrZweiter, achtuhrdienst, ma8UhrZweiter.TagesQuarterTicks, SollTyp.AchtUhrDienst | ma8UhrZweiter.DefaultGruppe);
                arbeitstag.Planzeiten.Add(ist8UhrZweiter);

                #endregion

                var restMas = alledieDaSind.Where(x => !listMaDieNichtMehr.Contains(x)).ToList();


                var nestMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Nest)).ToList();
                FillGruppenDienste(nestMas, arbeitstag, SollTyp.Nest, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var blauMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Blau)).ToList();
                FillGruppenDienste(blauMas, arbeitstag, SollTyp.Blau, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var gruenMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Gruen)).ToList();
                FillGruppenDienste(gruenMas, arbeitstag, SollTyp.Gruen, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var rotMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Rot)).ToList();
                FillGruppenDienste(rotMas, arbeitstag, SollTyp.Rot, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);


                var rest = alledieDaSind.Where(x => !listMaDieNichtMehr.Contains(x)).ToList();
                FillGruppenDienste(rest, arbeitstag, SollTyp.None, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);
            }
        }

        private static Mitarbeiter NextMitarbeiter(IList<Mitarbeiter> alleDieDaSind, IList<Mitarbeiter> schonEingeteilt, SollTyp ma4Diensttyp = SollTyp.None)
        {
            var topf = alleDieDaSind.Except(schonEingeteilt).ToList();

            if (topf.Count == 0)
                return null;

            //die mit Wunschdienst
            var mitarbeiter = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

            if (mitarbeiter.Count == 0)//bei allen gucken wenn keiner will
                mitarbeiter = topf;

            int ichBinDran = Zufall.Next(0,mitarbeiter.Count);
#if DEBUG
            Console.WriteLine($"{ma4Diensttyp}: {ichBinDran} von {mitarbeiter.Count}");
#endif
            return mitarbeiter[ichBinDran];
        }

        private static PlanItem CreatePlanItem(Mitarbeiter ma, DateTime startzeit, short quarterticks, SollTyp typ)
        {
            var result = new PlanItem();

            result.ErledigtDurch = ma;
            result.Startzeit = startzeit;
            result.QuarterTicks = quarterticks;
            result.Typ = typ;

            //Pause nachbereiten, Startzeit für den Spätdienst vorziehen
            if (result.BreakTicks != 0)
            {
                if (result.Typ.HasFlag(SollTyp.Spätdienst))
                {
                    result.Startzeit = startzeit.AddMinutes(-1 * 15 * result.BreakTicks);
                }
            }

            return result;
        }

        private static void FillGruppenDienste(List<Mitarbeiter> maList, Arbeitstag arbeitstag, SollTyp gruppe,
            DateTime neunuhrdienst,
            DateTime achtuhr30Dienst,
            DateTime zehnuhrdienst,
            List<Mitarbeiter> schonEingeteilt)
        {
            if (!arbeitstag.Planzeiten.Any(x =>
                        (x.Typ.HasFlag(SollTyp.Frühdienst) || x.Typ.HasFlag(SollTyp.AchtUhrDienst) || x.Typ.HasFlag(SollTyp.AchtUhr30Dienst))
                        && x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, SollTyp.AchtUhr30Dienst);
                if(ma==null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(ma, achtuhr30Dienst, ma.TagesQuarterTicks, SollTyp.AchtUhr30Dienst | ma.DefaultGruppe);
                arbeitstag.Planzeiten.Add(plan);
            }

            //ab hier gibts ein "Frühen Dienst"
            var ma9 = NextMitarbeiter(maList, schonEingeteilt, SollTyp.NeunUhrDienst);
            if (ma9 == null)
                return;

            schonEingeteilt.Add(ma9);
            var plan9 = CreatePlanItem(ma9, neunuhrdienst, ma9.TagesQuarterTicks, SollTyp.NeunUhrDienst | ma9.DefaultGruppe);
            arbeitstag.Planzeiten.Add(plan9);

            var ma10 = NextMitarbeiter(maList, schonEingeteilt, SollTyp.ZehnUhrDienst);
            if (ma10 == null)
                return;

            schonEingeteilt.Add(ma10);
            var plan10 = CreatePlanItem(ma10, zehnuhrdienst, ma10.TagesQuarterTicks, SollTyp.ZehnUhrDienst | ma10.DefaultGruppe);
            arbeitstag.Planzeiten.Add(plan10);

            //hmm wenn immernoch welche übrig sind dann, halt um 9Uhr kommen
            while (true)
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, SollTyp.NeunUhrDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(ma, neunuhrdienst, ma.TagesQuarterTicks, SollTyp.NeunUhrDienst | ma.DefaultGruppe);
                arbeitstag.Planzeiten.Add(plan);
            }
        }
    }
}
