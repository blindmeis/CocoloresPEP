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
    public class PlanService
    {
        private static Random Zufall = new Random();

        private Func<IList<Mitarbeiter>, List<Mitarbeiter>, Mitarbeiter> NextMitarbeiter = (alleDieDaSind, dieNicht) =>
        {
            if (dieNicht.Count == alleDieDaSind.Count)
                return null;

            int? result = Zufall.Next(alleDieDaSind.Count);

            while (dieNicht.Contains(alleDieDaSind[result.Value]))
            {
                result = Zufall.Next(alleDieDaSind.Count);
            }

            return alleDieDaSind[result.Value];
        };

        public void ErstelleWochenplan(Arbeitswoche woche, IList<Mitarbeiter> maList)
        {
            foreach (var arbeitstag in woche.Arbeitstage)
            {

                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum)).ToList();

                var frühdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 7, 0, 0);
                var achtuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 8, 0, 0);
                var achtuhr30Dienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 8, 30, 0);
                var neunuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 9, 0, 0);
                var zehnuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 10, 0, 0);
                var spätdienstEnde = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 17, 15, 0);



                var listMaDieNichtMehr = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);

                listMaDieNichtMehr.Add(maFrüh);
                var istFrüh = CreateIstItem(maFrüh, frühdienst, maFrüh.TagesQuarterTicks, SollTyp.Frühdienst | maFrüh.DefaultGruppe);
                arbeitstag.Istzeiten.Add(istFrüh);

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);

                listMaDieNichtMehr.Add(maSpät1);
                var spätdienstStart1 = spätdienstEnde.AddMinutes(-1 * 15 * maSpät1.TagesQuarterTicks);
                var istSpät1 = CreateIstItem(maSpät1, spätdienstStart1, maSpät1.TagesQuarterTicks, SollTyp.Spätdienst | maSpät1.DefaultGruppe);
                arbeitstag.Istzeiten.Add(istSpät1);

                var maSpät2 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);

                listMaDieNichtMehr.Add(maSpät2);
                var spätdienstStart2 = spätdienstEnde.AddMinutes(-1 * 15 * maSpät2.TagesQuarterTicks);
                var istSpät2 = CreateIstItem(maSpät2, spätdienstStart2, maSpät2.TagesQuarterTicks, SollTyp.Spätdienst | maSpät2.DefaultGruppe);
                arbeitstag.Istzeiten.Add(istSpät2);

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                listMaDieNichtMehr.Add(ma8UhrErster);
                var ist8UhrErster = CreateIstItem(ma8UhrErster, achtuhrdienst, ma8UhrErster.TagesQuarterTicks, SollTyp.AchtUhrDienst | ma8UhrErster.DefaultGruppe);
                arbeitstag.Istzeiten.Add(ist8UhrErster);

                var ma8UhrZweiter = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                listMaDieNichtMehr.Add(ma8UhrZweiter);
                var ist8UhrZweiter = CreateIstItem(ma8UhrZweiter, achtuhrdienst, ma8UhrZweiter.TagesQuarterTicks, SollTyp.AchtUhrDienst | ma8UhrZweiter.DefaultGruppe);
                arbeitstag.Istzeiten.Add(ist8UhrZweiter);

                #endregion

                var restMas = alledieDaSind.Where(x => !listMaDieNichtMehr.Contains(x)).ToList();


                var nestMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Nest)).ToList();
                FülleRestlicheMitarbeiter(nestMas, arbeitstag, SollTyp.Nest, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var blauMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Blau)).ToList();
                FülleRestlicheMitarbeiter(blauMas, arbeitstag, SollTyp.Blau, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var gruenMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Gruen)).ToList();
                FülleRestlicheMitarbeiter(gruenMas, arbeitstag, SollTyp.Gruen, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);

                var rotMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Rot)).ToList();
                FülleRestlicheMitarbeiter(rotMas, arbeitstag, SollTyp.Rot, neunuhrdienst, achtuhr30Dienst, zehnuhrdienst, listMaDieNichtMehr);



            }
        }

        private static Ist CreateIstItem(Mitarbeiter ma, DateTime startzeit, short quarterticks, SollTyp typ)
        {
            var result = new Ist();

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

        private static void FülleRestlicheMitarbeiter(List<Mitarbeiter> maList, Arbeitstag arbeitstag, SollTyp gruppe,
            DateTime neunuhrdienst,
            DateTime achtuhr30Dienst,
            DateTime zehnuhrdienst,
            List<Mitarbeiter> listMaDieNichtMehr)
        {
            foreach (var ma in maList)
            {
                if (arbeitstag.Istzeiten.Any(
                    x => (x.Typ.HasFlag(SollTyp.Frühdienst)
                            || x.Typ.HasFlag(SollTyp.AchtUhrDienst)
                            || x.Typ.HasFlag(SollTyp.AchtUhr30Dienst)
                        )
                         && x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
                {

                    if (maList.Count == 3 && arbeitstag.Istzeiten.Any(x => x.Typ.HasFlag(SollTyp.NeunUhrDienst) && x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
                    {
                        var ist = CreateIstItem(ma, zehnuhrdienst, ma.TagesQuarterTicks, SollTyp.ZehnUhrDienst | gruppe);
                        arbeitstag.Istzeiten.Add(ist);
                        listMaDieNichtMehr.Add(ma);
                    }
                    else
                    {
                        var ist = CreateIstItem(ma, neunuhrdienst, ma.TagesQuarterTicks, SollTyp.NeunUhrDienst | gruppe);
                        arbeitstag.Istzeiten.Add(ist);
                        listMaDieNichtMehr.Add(ma);
                    }

                }
                else
                {
                    var ist = CreateIstItem(ma, achtuhr30Dienst, ma.TagesQuarterTicks, SollTyp.AchtUhr30Dienst | gruppe);
                    arbeitstag.Istzeiten.Add(ist);
                    listMaDieNichtMehr.Add(ma);
                }
            }
        }
    }
}
