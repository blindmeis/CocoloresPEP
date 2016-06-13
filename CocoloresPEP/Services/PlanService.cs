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

                var frühdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 7, 0,0);
                var achtuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 8,0, 0);
                var achtuhr30dienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day,8, 30, 0);
                var neunuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 9,0, 0);
                var zehnuhrdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day,10, 0, 0);
                var spätdienst = new DateTime(arbeitstag.Datum.Year, arbeitstag.Datum.Month, arbeitstag.Datum.Day, 17,15, 0);



                var listMaDieNichtMehr = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                if (maFrüh != null)
                {
                    listMaDieNichtMehr.Add(maFrüh);
                    arbeitstag.Istzeiten.Add(new Ist()
                    {
                        ErledigtDurch = maFrüh,
                        Startzeit = frühdienst,
                        QuarterTicks = maFrüh.TagesQuarterTicks,
                        Typ = SollTyp.Frühdienst | maFrüh.DefaultGruppe
                    });

                }

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                if (maSpät1 != null)
                {
                    listMaDieNichtMehr.Add(maSpät1);
                    arbeitstag.Istzeiten.Add(new Ist()
                    {
                        ErledigtDurch = maSpät1,
                        Startzeit = spätdienst.AddMinutes(-1*15*maSpät1.TagesQuarterTicks),
                        QuarterTicks = maSpät1.TagesQuarterTicks,
                        Typ = SollTyp.Spätdienst | maSpät1.DefaultGruppe
                    });
                }

                var maSpät2 = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                if (maSpät2 != null)
                {
                    listMaDieNichtMehr.Add(maSpät2);
                    arbeitstag.Istzeiten.Add(new Ist()
                    {
                        ErledigtDurch = maSpät2,
                        Startzeit = spätdienst.AddMinutes(-1*15*maSpät2.TagesQuarterTicks),
                        QuarterTicks = maSpät2.TagesQuarterTicks,
                        Typ = SollTyp.Spätdienst | maSpät2.DefaultGruppe
                    });

                }

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                listMaDieNichtMehr.Add(ma8UhrErster);
                arbeitstag.Istzeiten.Add(new Ist()
                {
                    ErledigtDurch = ma8UhrErster,
                    Startzeit = achtuhrdienst,
                    QuarterTicks = ma8UhrErster.TagesQuarterTicks,
                    Typ = SollTyp.AchtUhrDienst | ma8UhrErster.DefaultGruppe
                });

                var ma8UhrZweiter = NextMitarbeiter(alledieDaSind, listMaDieNichtMehr);
                listMaDieNichtMehr.Add(ma8UhrZweiter);
                arbeitstag.Istzeiten.Add(new Ist()
                {
                    ErledigtDurch = ma8UhrZweiter,
                    Startzeit = achtuhrdienst,
                    QuarterTicks = ma8UhrZweiter.TagesQuarterTicks,
                    Typ = SollTyp.AchtUhrDienst | ma8UhrZweiter.DefaultGruppe
                });

                #endregion

                var restMas = alledieDaSind.Where(x => !listMaDieNichtMehr.Contains(x)).ToList();


                var nestMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Nest)).ToList();
                FülleRestlicheMitarbeiter(nestMas, arbeitstag, SollTyp.Nest, neunuhrdienst, achtuhr30dienst, zehnuhrdienst, listMaDieNichtMehr);

                var blauMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Blau)).ToList();
                FülleRestlicheMitarbeiter(blauMas, arbeitstag, SollTyp.Blau, neunuhrdienst, achtuhr30dienst, zehnuhrdienst, listMaDieNichtMehr);

                var gruenMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Gruen)).ToList();
                FülleRestlicheMitarbeiter(gruenMas, arbeitstag, SollTyp.Gruen, neunuhrdienst, achtuhr30dienst, zehnuhrdienst, listMaDieNichtMehr);

                var rotMas = restMas.Where(x => x.DefaultGruppe.HasFlag(SollTyp.Rot)).ToList();
                FülleRestlicheMitarbeiter(rotMas, arbeitstag, SollTyp.Rot, neunuhrdienst, achtuhr30dienst, zehnuhrdienst, listMaDieNichtMehr);



            }
        }

        private static void FülleRestlicheMitarbeiter(List<Mitarbeiter> maList, Arbeitstag arbeitstag, SollTyp gruppe, 
            DateTime neunuhrdienst,
            DateTime achtuhr30dienst, 
            DateTime zehnuhrdienst,
            List<Mitarbeiter> listMaDieNichtMehr) 
        {
            foreach (var ma in maList)
            {
                if (arbeitstag.Istzeiten.Any(
                    x =>(   x.Typ.HasFlag(SollTyp.Frühdienst) 
                            || x.Typ.HasFlag(SollTyp.AchtUhrDienst) 
                            || x.Typ.HasFlag(SollTyp.AchtUhr30Dienst)
                        )
                         && x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
                {
                    if (maList.Count == 3 && arbeitstag.Istzeiten.Any(x => x.Typ.HasFlag(SollTyp.NeunUhrDienst) &&
                                                                           x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
                    {
                        arbeitstag.Istzeiten.Add(new Ist()
                        {
                            ErledigtDurch = ma,
                            Startzeit = zehnuhrdienst,
                            QuarterTicks = ma.TagesQuarterTicks,
                            Typ = SollTyp.ZehnUhrDienst | gruppe
                        });
                        listMaDieNichtMehr.Add(ma);
                    }
                    else
                    {
                         arbeitstag.Istzeiten.Add(new Ist()
                        {
                            ErledigtDurch = ma,
                            Startzeit = neunuhrdienst,
                            QuarterTicks = ma.TagesQuarterTicks,
                            Typ = SollTyp.NeunUhrDienst | gruppe
                        });
                        listMaDieNichtMehr.Add(ma);
                    }
                   
                }
                else
                {
                    arbeitstag.Istzeiten.Add(new Ist()
                    {
                        ErledigtDurch = ma,
                        Startzeit = achtuhr30dienst,
                        QuarterTicks = ma.TagesQuarterTicks,
                        Typ = SollTyp.AchtUhr30Dienst | gruppe
                    });
                    listMaDieNichtMehr.Add(ma);
                }
            }
        }
    }
}
