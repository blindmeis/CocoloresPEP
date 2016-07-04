using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using Itenso.TimePeriod;

namespace CocoloresPEP.Services
{
    public static class PlanService
    {
        private static readonly Random Zufall = new Random();

        #region Erstellung Wochenplan
        public static void ErstelleWochenplan(Arbeitswoche woche, IList<Mitarbeiter> maList)
        {
            foreach (var arbeitstag in woche.Arbeitstage)
            {
                arbeitstag.Planzeiten.Clear();

                //Samstag und Sontag ignorieren bei Planung
                if (arbeitstag.Datum.DayOfWeek == DayOfWeek.Saturday || arbeitstag.Datum.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (arbeitstag.IsFeiertag)
                {
                    //Wenn Feiertag, dann seinen Tagessatz minutengenau
                    foreach (var mitarbeiter in maList)
                    {
                        var tr = new TimeRange(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeStart.AddMinutes(mitarbeiter.TagesSollMinutenMitPause));
                        arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, tr,GruppenTyp.None, DienstTyp.Frei));
                    }
                    continue;
                }

                //Wenn wer nicht da ist, dann seinen Tagessatz minutengenau
                var mitarbeiterNichtDa = maList.Where(x => x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum)).ToList();
                foreach (var mitarbeiter in mitarbeiterNichtDa)
                {
                    var tr = new TimeRange(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeStart.AddMinutes(mitarbeiter.TagesSollMinutenMitPause));
                    arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, tr, GruppenTyp.None, DienstTyp.Frei));
                }

                //Nur richtige Mitarbeiter die auch da sind
                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && !x.IsHelfer).ToList();

                var schonEingeteilt = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.Frühdienst);
                if (maFrüh != null)
                {
                    schonEingeteilt.Add(maFrüh);
                    var trFrüh = new TimeRange(arbeitstag.Frühdienst, arbeitstag.Frühdienst.AddMinutes(15* maFrüh.TagesQuarterTicks));
                    var istFrüh = CreatePlanItem(arbeitstag, maFrüh,trFrüh,maFrüh.DefaultGruppe, DienstTyp.Frühdienst);
                    arbeitstag.Planzeiten.Add(istFrüh);
                }

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde);

                if (maSpät1 != null)
                {
                    schonEingeteilt.Add(maSpät1);
                    var spätdienstStart1 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät1.TagesQuarterTicks);
                    var trSpät1 = new TimeRange(spätdienstStart1, arbeitstag.SpätdienstEnde);
                    var istSpät1 = CreatePlanItem(arbeitstag, maSpät1, trSpät1, maSpät1.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät1);
                }

                var maSpät2 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde, GibAndereEtage(maSpät1));

                if (maSpät2 != null)
                {
                    schonEingeteilt.Add(maSpät2);
                    var spätdienstStart2 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät2.TagesQuarterTicks);
                    var trSpät2 = new TimeRange(spätdienstStart2, arbeitstag.SpätdienstEnde);
                    var istSpät2 = CreatePlanItem(arbeitstag, maSpät2, trSpät2, maSpät2.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät2);
                }

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.AchtUhrDienst, GibAndereEtage(maFrüh));
                if (ma8UhrErster != null)
                {
                    schonEingeteilt.Add(ma8UhrErster);
                    var tr8UhrErster = new TimeRange(arbeitstag.AchtUhrDienst, arbeitstag.AchtUhrDienst.AddMinutes(15* ma8UhrErster.TagesQuarterTicks));
                    var ist8UhrErster = CreatePlanItem(arbeitstag, ma8UhrErster, tr8UhrErster, ma8UhrErster.DefaultGruppe, DienstTyp.AchtUhrDienst);
                    arbeitstag.Planzeiten.Add(ist8UhrErster);
                }

                #endregion

                #region Gruppendienste

                var restMas = alledieDaSind.Where(x => !schonEingeteilt.Contains(x)).ToList();

                var nestMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Nest)).ToList();
                FillGruppenDiensteMitKernzeitPrio(nestMas, arbeitstag, GruppenTyp.Nest, schonEingeteilt);

                var blauMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Gelb)).ToList();
                FillGruppenDiensteMitKernzeitPrio(blauMas, arbeitstag, GruppenTyp.Gelb, schonEingeteilt);

                var gruenMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Gruen)).ToList();
                FillGruppenDiensteMitKernzeitPrio(gruenMas, arbeitstag, GruppenTyp.Gruen, schonEingeteilt);

                var rotMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Rot)).ToList();
                FillGruppenDiensteMitKernzeitPrio(rotMas, arbeitstag, GruppenTyp.Rot, schonEingeteilt);

                var rest = alledieDaSind.Where(x => !schonEingeteilt.Contains(x)).ToList();
                FillGruppenDiensteMitKernzeitPrio(rest, arbeitstag, GruppenTyp.None, schonEingeteilt);

                #endregion

                #region Buftis und Co

                var helferleins = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && x.IsHelfer).ToList();

                //Frühdiesnt
                var h1 = helferleins.Except(schonEingeteilt).FirstOrDefault();

                if (h1 != null)
                {
                    schonEingeteilt.Add(h1);
                    var tr1Früh = new TimeRange(arbeitstag.FrühdienstFsj, arbeitstag.FrühdienstFsj.AddMinutes(15 * h1.TagesQuarterTicks));
                    var h1Früh = CreatePlanItem(arbeitstag, h1, tr1Früh, h1.DefaultGruppe, DienstTyp.FsjFrühdienst);
                    arbeitstag.Planzeiten.Add(h1Früh);
                }

                var h2 = helferleins.Except(schonEingeteilt).FirstOrDefault();

                if (h2 != null)
                {
                    schonEingeteilt.Add(h2);
                    var spätdienstStartFsj = arbeitstag.SpätdienstEndeFsj.AddMinutes(-1 * 15 * h2.TagesQuarterTicks);
                    var tr2Spät = new TimeRange(spätdienstStartFsj, arbeitstag.SpätdienstEndeFsj);
                    var h2Spät = CreatePlanItem(arbeitstag, h2, tr2Spät, h2.DefaultGruppe, DienstTyp.FsjSpätdienst);
                    arbeitstag.Planzeiten.Add(h2Spät);
                }

                foreach (var helferlein in helferleins.Except(schonEingeteilt).ToList())
                {
                    schonEingeteilt.Add(helferlein);
                    var trhelfer = new TimeRange(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeStart.AddMinutes(15 * helferlein.TagesQuarterTicks));
                    var plan = CreatePlanItem(arbeitstag, helferlein, trhelfer, helferlein.DefaultGruppe, DienstTyp.KernzeitStartDienst);
                    arbeitstag.Planzeiten.Add(plan);
                }
                #endregion

                #region Grossteam

                if (arbeitstag.HasGrossteam)
                {
                    foreach (var mitarbeiter in alledieDaSind)
                    {
                        var gt = new PlanItem();

                        gt.Arbeitstag = arbeitstag;
                        gt.ErledigtDurch = mitarbeiter;
                        gt.Zeitraum = arbeitstag.Grossteam;
                        gt.Gruppe = mitarbeiter.DefaultGruppe;
                        gt.Dienst = DienstTyp.Großteam;

                        arbeitstag.Planzeiten.Add(gt);
                    }
                }

                #endregion
            }

            #region nach "Minusstunden" gucken, passiert weil TagesTicks  und Tagesminuten nicht immer passen
            //nach "Minusstunden" gucken, passiert weil TagesTicks abgerundet werden
            //foreach (var mitarbeiter in maList)
            //{
            //    var planzeiten = woche.Arbeitstage.SelectMany(x => x.Planzeiten.Where(p => p.ErledigtDurch == mitarbeiter)).ToList();

            //    var minuten = planzeiten.Sum(x => (int)x.Zeitraum.Duration.TotalMinutes);

            //    var saldoInMinuten = minuten - (mitarbeiter.WochenStunden * 60);

            //    //vorkommazahl bleibt übrig
            //    var tickstoAdd = Math.Abs((int)saldoInMinuten / 15);

            //    if (tickstoAdd < 1)
            //        continue;

            //    var planzeitenProTag = planzeiten.Where(x => x.ObGruppenDienst).GroupBy(x => x.Zeitraum.Start.Day).ToList();
            //    var ticksAdded = 0;
            //    for (int i = 0; i < tickstoAdd; i++)
            //    {
            //        if (planzeitenProTag.Count <= i)
            //            break;

            //        var planitem = planzeitenProTag[i].First();

            //        if ((planitem.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
            //            || (planitem.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
            //            || (planitem.Dienst & DienstTyp.KernzeitStartDienst) == DienstTyp.KernzeitStartDienst)
            //        {
            //            planitem.Zeitraum.ExpandEndTo(planitem.Zeitraum.End.AddMinutes(15));
            //            AdjustEndzeitSpaetdienstEnde(planitem);
            //        }
            //        else
            //        {
            //            planitem.Zeitraum.ExpandStartTo(planitem.Zeitraum.Start.AddMinutes(-15));
            //        }

            //        ticksAdded++;
            //    }
            //    //Fallback wenn Tage "fehlen"
            //    if (tickstoAdd != ticksAdded)
            //    {
            //        var ticks = tickstoAdd - ticksAdded;
            //        var planitem = planzeitenProTag.Select(x => x.First()).First();


            //        if ((planitem.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
            //            || (planitem.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
            //            || (planitem.Dienst & DienstTyp.KernzeitStartDienst) == DienstTyp.KernzeitStartDienst)
            //        {

            //            planitem.Zeitraum.ExpandEndTo(planitem.Zeitraum.End.AddMinutes(15 * ticks));
            //            AdjustEndzeitSpaetdienstEnde(planitem);
            //        }
            //        else
            //        {
            //            planitem.Zeitraum.ExpandStartTo(planitem.Zeitraum.Start.AddMinutes(-15 * ticks));
            //        }
            //    }
            //}
            #endregion

            //CheckKernzeitAbgedecktMitMitarbeiternVomTag(woche);

            //KindFreieZeitPlanen(woche);
        }



        private static GruppenTyp GibAndereEtage(Mitarbeiter ma)
        {
            var result = GruppenTyp.Nest | GruppenTyp.Gelb | GruppenTyp.Nest | GruppenTyp.Gelb;

            if (ma == null)
                return result;

            if ((ma.DefaultGruppe & GruppenTyp.Nest) == GruppenTyp.Nest || (ma.DefaultGruppe & GruppenTyp.Gelb) == GruppenTyp.Gelb)
                return GruppenTyp.Nest | GruppenTyp.Gelb;

            if ((ma.DefaultGruppe & GruppenTyp.Gruen) == GruppenTyp.Gruen || (ma.DefaultGruppe & GruppenTyp.Rot) == GruppenTyp.Rot)
                return GruppenTyp.Nest | GruppenTyp.Gelb;

            return result;
        }

        private static Mitarbeiter NextMitarbeiter(IList<Mitarbeiter> alleDieDaSind, IList<Mitarbeiter> schonEingeteilt, DienstTyp ma4Diensttyp = DienstTyp.None, GruppenTyp etage = GruppenTyp.Gelb | GruppenTyp.Gruen | GruppenTyp.Nest | GruppenTyp.Rot)
        {
            var topf = alleDieDaSind.Except(schonEingeteilt).ToList();

            if (topf.Count == 0)
                return null;

            //die mit Wunschdienst aus Etage
            var mitarbeiter = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp && (etage & x.DefaultGruppe) == x.DefaultGruppe).ToList();

            //die von Etage
            if (mitarbeiter.Count == 0)
                mitarbeiter = topf.Where(x => (etage & x.DefaultGruppe) == x.DefaultGruppe).ToList();

            //Wunschdienst
            if (mitarbeiter.Count == 0)
                mitarbeiter = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

            if (mitarbeiter.Count == 0)//bei allen gucken wenn keiner will
                mitarbeiter = topf;


            int ichBinDran = Zufall.Next(0, mitarbeiter.Count);
#if DEBUG
            Console.WriteLine($"{ma4Diensttyp}: {ichBinDran} von {mitarbeiter.Count}");
#endif
            return mitarbeiter[ichBinDran];
        }

        private static IList<Mitarbeiter> GetMitarbeiterDieNichtAlleinSindProGruppe(IList<Mitarbeiter> alleDieDaSind, IList<Mitarbeiter> schonEingeteilt)
        {
            var result = alleDieDaSind.Except(schonEingeteilt)
                             .GroupBy(x => x.DefaultGruppe)
                             .Where(x => x.Count() > 1)
                             .SelectMany(x => x).ToList();

            if (result.Count == 0)
                return alleDieDaSind;

            return result;
        }

        private static PlanItem CreatePlanItem(Arbeitstag arbeitstag, Mitarbeiter ma, TimeRange zeitraum, GruppenTyp gruppe, DienstTyp dienst)
        {
            var result = new PlanItem();
            
            result.Arbeitstag = arbeitstag;
            result.ErledigtDurch = ma;
            result.Zeitraum = zeitraum;
            result.Gruppe = gruppe;
            result.Dienst = dienst;

            //Pausen mit Planen
            if (result.Zeitraum.Duration.TotalMinutes > 360)
                result.Zeitraum.End = result.Zeitraum.End.AddMinutes(30);

            AdjustEndzeitSpaetdienstEnde(result);

            return result;
        }

        private static void AdjustEndzeitSpaetdienstEnde(PlanItem planItem)
        {
            if (planItem.Zeitraum.End > planItem.Arbeitstag.SpätdienstEnde)
            {
                var minuten = planItem.Arbeitstag.SpätdienstEnde - planItem.Zeitraum.End;
                planItem.Zeitraum.Move(minuten);
            }
        }

        private static void FillGruppenDiensteMitKernzeitPrio(List<Mitarbeiter> maList, Arbeitstag arbeitstag, GruppenTyp gruppe, List<Mitarbeiter> schonEingeteilt)
        {
            if (!arbeitstag.Planzeiten.Any(x =>
                        ((x.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                          || (x.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
                          || (x.Dienst & DienstTyp.KernzeitStartDienst) == DienstTyp.KernzeitStartDienst)
                        && (x.ErledigtDurch.DefaultGruppe & gruppe) == gruppe
                        && !x.ErledigtDurch.IsHelfer))
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.KernzeitStartDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var trMa = new TimeRange(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeStart.AddMinutes(15 * ma.TagesQuarterTicks));
                var plan = CreatePlanItem(arbeitstag, ma, trMa, ma.DefaultGruppe, DienstTyp.KernzeitStartDienst);
                arbeitstag.Planzeiten.Add(plan);
            }


            DateTime startzeit;
            short ticks;
            while (!CheckKernzeitAbgedeckt(arbeitstag, gruppe, out startzeit, out ticks))
            {
                var maKernzeitende = NextMitarbeiter(maList, schonEingeteilt);
                if (maKernzeitende == null)
                    return;

                var kernzeitEndeStart = arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * 15 * maKernzeitende.TagesQuarterTicks);
                if (kernzeitEndeStart < startzeit)
                    startzeit = kernzeitEndeStart;

                schonEingeteilt.Add(maKernzeitende);
                var trMa = new TimeRange(startzeit, startzeit.AddMinutes(15 * maKernzeitende.TagesQuarterTicks));
                var planKernzeitende = CreatePlanItem(arbeitstag, maKernzeitende, trMa, maKernzeitende.DefaultGruppe, DienstTyp.KernzeitEndeDienst);
                arbeitstag.Planzeiten.Add(planKernzeitende);
            }

            //ab hier gibts ein "Frühen Dienst" und wenn möglich ein Dienst bis Kernzeitende oder drüber
            var ma9 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
            if (ma9 == null)
                return;

            schonEingeteilt.Add(ma9);
            var trplan9 = new TimeRange(arbeitstag.NeunUhrDienst, arbeitstag.NeunUhrDienst.AddMinutes(15 * ma9.TagesQuarterTicks));
            var plan9 = CreatePlanItem(arbeitstag, ma9, trplan9, ma9.DefaultGruppe, DienstTyp.NeunUhrDienst);
            arbeitstag.Planzeiten.Add(plan9);

            var ma10 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.ZehnUhrDienst);
            if (ma10 == null)
                return;

            schonEingeteilt.Add(ma10);
            var trplan10 = new TimeRange(arbeitstag.ZehnUhrDienst, arbeitstag.ZehnUhrDienst.AddMinutes(15 * ma10.TagesQuarterTicks));
            var plan10 = CreatePlanItem(arbeitstag, ma10, trplan10, ma10.DefaultGruppe, DienstTyp.ZehnUhrDienst);
            arbeitstag.Planzeiten.Add(plan10);

            //hmm wenn immernoch welche übrig sind dann, halt um 9Uhr kommen
            while (true)
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var trplan = new TimeRange(arbeitstag.NeunUhrDienst, arbeitstag.NeunUhrDienst.AddMinutes(15 * ma.TagesQuarterTicks));
                var plan = CreatePlanItem(arbeitstag, ma, trplan, ma.DefaultGruppe, DienstTyp.NeunUhrDienst);
                arbeitstag.Planzeiten.Add(plan);
            }
        }
        #endregion

        private static void CheckKernzeitAbgedecktMitMitarbeiternVomTag(Arbeitswoche woche)
        {
            var gruppen = woche.Mitarbeiter.Select(x => x.DefaultGruppe).Distinct().ToList();
            //prüfen ob alle Gruppen in der Kernzeit besetzt sind
            foreach (var arbeitstag in woche.Arbeitstage)
            {
                //Samstag und Sontag ignorieren bei Planung
                if (arbeitstag.Datum.DayOfWeek == DayOfWeek.Saturday
                    || arbeitstag.Datum.DayOfWeek == DayOfWeek.Sunday
                    || arbeitstag.IsFeiertag)
                    continue;

                foreach (var gruppe in gruppen)
                {
                    if (gruppe == 0)
                        continue;

                    DateTime startzeit;
                    short ticks;

                    if (!CheckKernzeitAbgedeckt(arbeitstag, gruppe, out startzeit, out ticks))
                    {
                        //beim Tag schauen in andern Gruppen
                        var wirHabenvlltZeit = arbeitstag.Planzeiten.Where(x => (x.Gruppe & gruppe) != gruppe && !x.ErledigtDurch.IsHelfer && !x.Dienst.HasFlag(DienstTyp.Frei))
                                                                    .GroupBy(g => g.Gruppe)
                                                                    .OrderByDescending(o => o.Count())
                                                                    .ToList();

                        foreach (var vllt in wirHabenvlltZeit)
                        {
                            var mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden = new List<Mitarbeiter>();
                            var nachDienstbegin = vllt.OrderBy(x => x.Zeitraum.Start);

                            var erster = nachDienstbegin.FirstOrDefault();

                            if (erster == null)
                                continue;

                            mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Add(erster.ErledigtDurch);
                            var bisAbgedeckt = erster.Zeitraum.End;

                            //gucken ohne Zeiten zu ändern
                            while (bisAbgedeckt < arbeitstag.KernzeitGruppeEnde)
                            {
                                var nächster = vllt.FirstOrDefault(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch)
                                                                        && x.Zeitraum.End >= arbeitstag.KernzeitGruppeEnde);

                                if (nächster == null)
                                {
                                    var alle = vllt.Where(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch))
                                                   .Select(x => x.ErledigtDurch);
                                    mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.AddRange(alle);
                                    break;
                                }

                                mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Add(nächster.ErledigtDurch);
                                bisAbgedeckt = nächster.Zeitraum.End;
                            }

                            var kanditaten = vllt.Where(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch)).ToList();

                            if (kanditaten.Count == 0)
                                continue;

                            var mindauer = (int)(startzeit.AddMinutes(ticks) - startzeit).TotalMinutes;
                            var erstbesten = kanditaten.Where(x => x.Zeitraum.Duration.TotalMinutes >= mindauer).OrderBy(x => x.Zeitraum.Start).FirstOrDefault();
                            if (erstbesten == null)
                                continue;

                            var dauer = (int)(erstbesten.Zeitraum.Duration).TotalMinutes;
                            erstbesten.Gruppe = gruppe;

                            //wenn die Zeiten nicht passen, dann anpassen
                            if (erstbesten.Zeitraum.Start > startzeit)
                            {
                                erstbesten.Zeitraum.Start = startzeit;
                                erstbesten.Zeitraum.End = startzeit.AddMinutes(dauer);
                            }

                            if (erstbesten.Zeitraum.End < arbeitstag.KernzeitGruppeEnde)
                            {
                                erstbesten.Zeitraum.End = arbeitstag.KernzeitGruppeEnde;
                                erstbesten.Zeitraum.Start = arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * dauer);
                            }

                            break;
                        }
                    }
                }


            }
        }

        private static void KindFreieZeitPlanen(Arbeitswoche woche)
        {
            var mitarbeiterPlanzeiten = woche.Arbeitstage.SelectMany(x => x.Planzeiten).GroupBy(x => x.ErledigtDurch).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var mapl in mitarbeiterPlanzeiten)
            {
                var kfz = mapl.Key.KindFreieZeit;

                if (kfz == 0)
                    continue;

                //der Plan ist erstmal volle KFZ auf ein Tag
                //wenn nicht dann halt immer weniger

                //var volleTage = mapl.Value.Where(x => (x.Dienst & DienstTyp.Frei) != DienstTyp.Frei)
                //                          .GroupBy(x => x.Startzeit.ToString("yyyyMMdd"))
                //                          .Count();
                var volleTage = 5;
                var kfzMinutenTag = (int)(kfz * 60 * volleTage / 5);
                var kfzAufteilen = 0;
                while (kfzMinutenTag > 0)
                {
                    var aufteilen = new List<int> { kfzMinutenTag, kfzAufteilen };

                    foreach (var kfzMinute in aufteilen)
                    {
                        if (kfzMinute <= 0)
                            continue;

                        var spätdienste = mapl.Value.Where(x => (x.Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde).ToList();
                        if (PlanzeitReduzierenOhneKernzeitVerletzung(spätdienste, kfzMinute))
                        {
                            kfzMinutenTag = 0;
                            continue;
                        }

                        var frühdienste = mapl.Value.Where(x => (x.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                                                  || (x.Dienst & DienstTyp.AchtUhrDienst) != DienstTyp.AchtUhrDienst
                                                  || (x.Dienst & DienstTyp.KernzeitStartDienst) != DienstTyp.KernzeitStartDienst).ToList();

                        if (PlanzeitReduzierenOhneKernzeitVerletzung(frühdienste, kfzMinute))
                        {
                            kfzMinutenTag = 0;
                            continue;
                        }

                        var rest = mapl.Value.Except(spätdienste).ToList();
                        rest = rest.Except(frühdienste).ToList();

                        if (PlanzeitReduzierenOhneKernzeitVerletzung(rest, kfzMinute))
                            kfzMinutenTag = 0;
                    }

                    kfzMinutenTag -= 15;
                    kfzAufteilen += 15;
                }


            }
        }

        private static bool PlanzeitReduzierenOhneKernzeitVerletzung(List<PlanItem> dienste, int kfzMinutenTag)
        {
            if (dienste.Count > 0)
            {
                foreach (var planItem in dienste)
                {
                    var arbeitstag = planItem.Arbeitstag;

                    var oldStartzeit = planItem.Zeitraum.Start;
                    var oldEndzeit = planItem.Zeitraum.End;

                    if (planItem.Zeitraum.Start <= arbeitstag.KernzeitGruppeStart)
                    {
                        planItem.Zeitraum.End = planItem.Zeitraum.End.AddMinutes(-1 * kfzMinutenTag);
                    }
                    else
                    {
                        planItem.Zeitraum.Start = planItem.Zeitraum.Start.AddMinutes(kfzMinutenTag);
                    }

                    if (CheckKernzeitAbgedeckt(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeEnde, planItem.Gruppe, arbeitstag.Planzeiten))
                    {
                        return true;
                    }

                    planItem.Zeitraum.Start = oldStartzeit;
                    planItem.Zeitraum.End = oldEndzeit;
                }

                //mhh hat nich geklappt :(
            }
            return false;
        }

        /// <summary>
        /// Per Definition gilt das wenn mindestens eine Planzeit existiert das diese die KernzeitStart erfüllt
        /// Per Definition in der Planung können keine Lücken in der Kernzeit entstehen
        /// </summary>
        /// <param name="arbeitstag"></param>
        /// <param name="gruppe"></param>
        /// <param name="startzeitNichtAbgedeckt"></param>
        /// <param name="ticksNichtAbgedeckt"></param>
        /// <returns></returns>
        private static bool CheckKernzeitAbgedeckt(Arbeitstag arbeitstag, GruppenTyp gruppe, out DateTime startzeitNichtAbgedeckt, out short ticksNichtAbgedeckt)
        {
            startzeitNichtAbgedeckt = arbeitstag.KernzeitGruppeStart;
            ticksNichtAbgedeckt = (short)((arbeitstag.KernzeitGruppeEnde - arbeitstag.KernzeitGruppeStart).TotalMinutes / 15);

            if (!CheckKernzeitAbgedeckt(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeEnde, gruppe, arbeitstag.Planzeiten))
            {
                var zeiten = arbeitstag.Planzeiten.Where(x => (x.Gruppe & gruppe) == gruppe && !x.ErledigtDurch.IsHelfer).OrderBy(x => x.Zeitraum.Start).ToList();
                var ende = startzeitNichtAbgedeckt;
                foreach (var z in zeiten)
                {
                    if (z.Zeitraum.Start > ende)
                        break;

                    startzeitNichtAbgedeckt = ende = z.Zeitraum.End;
                }

                ticksNichtAbgedeckt = (short)((arbeitstag.KernzeitGruppeEnde - startzeitNichtAbgedeckt).TotalMinutes / 15);

                return false;
            }

            return true;
        }

        private static bool CheckKernzeitAbgedeckt(DateTime kernzeitGruppenStart, DateTime kernzeitGruppenEnde, GruppenTyp gruppe, IList<PlanItem> gruppenPlanzeiten)
        {
            var zeiten = gruppenPlanzeiten.Where(x => (x.Gruppe & gruppe) == gruppe && !x.ErledigtDurch.IsHelfer).OrderBy(x => x.Zeitraum.Start).ToList();

            //Wenn es keine Leutz gibt dann ist halt niemand da :)
            if (zeiten.Count == 0)
                return false;

            var obKernzeitStartAbgedeckt = zeiten.Min(x => x.Zeitraum.Start) <= kernzeitGruppenStart;

            if (!obKernzeitStartAbgedeckt)
                return false;

            var ende = zeiten.First().Zeitraum.End;
            while (true)
            {
                if (ende >= kernzeitGruppenEnde)
                {
                    return true;
                }

                var höchsteEndzeit = zeiten.Where(x => x.Zeitraum.Start <= ende).OrderByDescending(x => x.Zeitraum.End).FirstOrDefault();

                if (höchsteEndzeit == null || höchsteEndzeit.Zeitraum.End <= ende)
                {
                    return false;
                }

                ende = höchsteEndzeit.Zeitraum.End;
            }
        }
    }
}
