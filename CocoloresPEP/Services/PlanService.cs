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
                        arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, arbeitstag.KernzeitGruppeStart, mitarbeiter.TagesQuarterTicks, GruppenTyp.None, DienstTyp.Frei, arbeitstag.KernzeitGruppeStart.AddMinutes(mitarbeiter.TagesSollMinutenMitPause)));
                    }
                    continue;
                }

                //Wenn wer nicht da ist, dann seinen Tagessatz minutengenau
                var mitarbeiterNichtDa = maList.Where(x => x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum)).ToList();
                foreach (var mitarbeiter in mitarbeiterNichtDa)
                {
                    arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, arbeitstag.KernzeitGruppeStart, mitarbeiter.TagesQuarterTicks, GruppenTyp.None, DienstTyp.Frei, arbeitstag.KernzeitGruppeStart.AddMinutes(mitarbeiter.TagesSollMinutenMitPause)));
                }

                //Nur richtige Mitarbeiter die auch da sind
                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && !x.IsHelfer).ToList();

                var schonEingeteilt = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.Frühdienst);
                if (maFrüh != null)
                {
                    schonEingeteilt.Add(maFrüh);
                    var istFrüh = CreatePlanItem(arbeitstag, maFrüh, arbeitstag.Frühdienst, maFrüh.TagesQuarterTicks, maFrüh.DefaultGruppe, DienstTyp.Frühdienst);
                    arbeitstag.Planzeiten.Add(istFrüh);
                }

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde);

                if (maSpät1 != null)
                {
                    schonEingeteilt.Add(maSpät1);
                    var spätdienstStart1 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät1.TagesQuarterTicks);
                    var istSpät1 = CreatePlanItem(arbeitstag, maSpät1, spätdienstStart1, maSpät1.TagesQuarterTicks, maSpät1.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät1);
                }

                var maSpät2 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde, GibAndereEtage(maSpät1));

                if (maSpät2 != null)
                {
                    schonEingeteilt.Add(maSpät2);
                    var spätdienstStart2 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät2.TagesQuarterTicks);
                    var istSpät2 = CreatePlanItem(arbeitstag, maSpät2, spätdienstStart2, maSpät2.TagesQuarterTicks, maSpät2.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät2);
                }

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.AchtUhrDienst, GibAndereEtage(maFrüh));
                if (ma8UhrErster!=null)
                {
                    schonEingeteilt.Add(ma8UhrErster);
                    var ist8UhrErster = CreatePlanItem(arbeitstag, ma8UhrErster, arbeitstag.AchtUhrDienst, ma8UhrErster.TagesQuarterTicks, ma8UhrErster.DefaultGruppe, DienstTyp.AchtUhrDienst);
                    arbeitstag.Planzeiten.Add(ist8UhrErster); 
                }

                //var ma8UhrZweiter = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.AchtUhrDienst);
                //schonEingeteilt.Add(ma8UhrZweiter);
                //var ist8UhrZweiter = CreatePlanItem(ma8UhrZweiter, arbeitstag.AchtUhrDienst, ma8UhrZweiter.TagesQuarterTicks, ma8UhrZweiter.DefaultGruppe, DienstTyp.AchtUhrDienst);
                //arbeitstag.Planzeiten.Add(ist8UhrZweiter);

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
                foreach (var helferlein in helferleins)
                {
                    var planitems = arbeitstag.Planzeiten.Where(x => (x.Gruppe & helferlein.DefaultGruppe) == helferlein.DefaultGruppe).ToList();

                    var planitemMitarbeiter = planitems.GroupBy(x => x.Startzeit).FirstOrDefault(x => !x.Any(h => h.ErledigtDurch.IsHelfer))?.FirstOrDefault();

                    //if (planitemMitarbeiter == null)
                    //{
                        schonEingeteilt.Add(helferlein);
                        var helferleinEndzeit = arbeitstag.KernzeitGruppeStart.AddMinutes(15 * helferlein.TagesQuarterTicks);
                        var plan = CreatePlanItem(arbeitstag, helferlein, arbeitstag.KernzeitGruppeStart, helferlein.TagesQuarterTicks, helferlein.DefaultGruppe, DienstTyp.KernzeitStartDienst, helferleinEndzeit);
                        arbeitstag.Planzeiten.Add(plan);
                    //}
                    //else
                    //{
                    //    var helferleinStartzeit = planitemMitarbeiter.Startzeit;
                    //    if (helferlein.TagesQuarterTicks > planitemMitarbeiter.AllTicks)
                    //        helferleinStartzeit = helferleinStartzeit.AddMinutes(-1 * 15 * (helferlein.TagesQuarterTicks - planitemMitarbeiter.AllTicks));

                    //    if (helferleinStartzeit < arbeitstag.Frühdienst)
                    //        helferleinStartzeit = arbeitstag.Frühdienst;

                    //    if (helferleinStartzeit.AddMinutes(15 * helferlein.TagesQuarterTicks) > arbeitstag.KernzeitGruppeEnde)
                    //        helferleinStartzeit = arbeitstag.KernzeitGruppeStart;

                    //    schonEingeteilt.Add(helferlein);
                    //    var plan = CreatePlanItem(arbeitstag, helferlein, helferleinStartzeit, helferlein.TagesQuarterTicks, helferlein.DefaultGruppe, planitemMitarbeiter.Dienst);
                    //    arbeitstag.Planzeiten.Add(plan);
                    //}
                }
                #endregion
            }

            //WICHTIG AB HIER GIBT ES EINE DEFINIERTE ENDZEIT, TAGESTICKSÄNDERUNGEN HABEN KEINE AUSWIRKUNG MEHR

            #region nach "Minusstunden" gucken, passiert weil TagesTicks  und Tagesminuten nicht immer passen
            //nach "Minusstunden" gucken, passiert weil TagesTicks abgerundet werden
            //foreach (var mitarbeiter in maList)
            //{
            //    var planzeiten = woche.Arbeitstage.SelectMany(x => x.Planzeiten.Where(p => p.ErledigtDurch == mitarbeiter)).ToList();

            //    var minuten = planzeiten.Sum(x => x.ArbeitszeitOhnePauseInMinuten());

            //    var saldoInMinuten = minuten - (mitarbeiter.WochenStunden * 60);

            //    //vorkommazahl bleibt übrig
            //    var tickstoAdd = Math.Abs((int)saldoInMinuten / 15);

            //    if (tickstoAdd < 1)
            //        continue;

            //    var planzeitenProTag = planzeiten.Where(x => x.ObGruppenDienst).GroupBy(x => x.Startzeit.Day).ToList();
            //    var ticksAdded = 0;
            //    for (int i = 0; i < tickstoAdd; i++)
            //    {
            //        if (planzeitenProTag.Count <= i)
            //            break;

            //        var planitem = planzeitenProTag[i].First();

            //        if ((planitem.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst ||
            //            (planitem.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst)
            //        {
            //            planitem.QuarterTicks += 1;
            //        }
            //        else
            //        {
            //            planitem.Startzeit = planitem.Startzeit.AddMinutes(-15);
            //            planitem.QuarterTicks += 1;
            //        }

            //        ticksAdded++;
            //    }
            //    //Fallback wenn Tage "fehlen"
            //    if (tickstoAdd != ticksAdded)
            //    {
            //        var ticks = tickstoAdd - ticksAdded;
            //        var planitem = planzeitenProTag.Select(x => x.First()).First();


            //        if ((planitem.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst ||
            //            (planitem.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst)
            //        {
            //            planitem.QuarterTicks += (short)ticks;
            //        }
            //        else
            //        {
            //            planitem.Startzeit = planitem.Startzeit.AddMinutes(-15 * ticks);
            //            planitem.QuarterTicks += (short)ticks;
            //        }
            //    }
            //}
            #endregion

            CheckKernzeitAbgedecktMitMitarbeiternVomTag(woche);

            KindFreieZeitPlanen(woche);
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

        private static PlanItem CreatePlanItem(Arbeitstag arbeitstag, Mitarbeiter ma, DateTime startzeit, short quarterticks, GruppenTyp gruppe, DienstTyp dienst, DateTime? endzeit = null)
        {
            var result = new PlanItem();

            result.ErledigtDurch = ma;
            result.Startzeit = startzeit;
            result.QuarterTicks = quarterticks;
            result.Gruppe = gruppe;
            result.Dienst = dienst;

            result.Endzeit = endzeit.HasValue ? endzeit : startzeit.AddMinutes(15*result.AllTicks);

            if (result.GetEndzeit() > arbeitstag.SpätdienstEnde)
            {
                var minuten = (int)(result.GetEndzeit() - arbeitstag.SpätdienstEnde).TotalMinutes;

                result.Startzeit = startzeit.AddMinutes(-1 * minuten);
            }

            return result;
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
                var plan = CreatePlanItem(arbeitstag, ma, arbeitstag.KernzeitGruppeStart, ma.TagesQuarterTicks, ma.DefaultGruppe, DienstTyp.KernzeitStartDienst);
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
                var planKernzeitende = CreatePlanItem(arbeitstag, maKernzeitende, startzeit, maKernzeitende.TagesQuarterTicks, maKernzeitende.DefaultGruppe, DienstTyp.KernzeitEndeDienst);
                arbeitstag.Planzeiten.Add(planKernzeitende);
            }

            //ab hier gibts ein "Frühen Dienst" und wenn möglich ein Dienst bis Kernzeitende oder drüber
            var ma9 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
            if (ma9 == null)
                return;

            schonEingeteilt.Add(ma9);
            var plan9 = CreatePlanItem(arbeitstag, ma9, arbeitstag.NeunUhrDienst, ma9.TagesQuarterTicks, ma9.DefaultGruppe, DienstTyp.NeunUhrDienst);
            arbeitstag.Planzeiten.Add(plan9);

            var ma10 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.ZehnUhrDienst);
            if (ma10 == null)
                return;

            schonEingeteilt.Add(ma10);
            var plan10 = CreatePlanItem(arbeitstag, ma10, arbeitstag.ZehnUhrDienst, ma10.TagesQuarterTicks, ma10.DefaultGruppe, DienstTyp.ZehnUhrDienst);
            arbeitstag.Planzeiten.Add(plan10);

            //hmm wenn immernoch welche übrig sind dann, halt um 9Uhr kommen
            while (true)
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(arbeitstag, ma, arbeitstag.NeunUhrDienst, ma.TagesQuarterTicks, ma.DefaultGruppe, DienstTyp.NeunUhrDienst);
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
                //var gruppen = arbeitstag.Planzeiten.Select(x => x.ErledigtDurch.DefaultGruppe).Distinct().ToList();
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
                            var nachDienstbegin = vllt.OrderBy(x => x.Startzeit);

                            var erster = nachDienstbegin.FirstOrDefault();

                            if (erster == null)
                                continue;

                            mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Add(erster.ErledigtDurch);
                            var bisAbgedeckt = erster.GetEndzeit();

                            //gucken ohne Zeiten zu ändern
                            while (bisAbgedeckt < arbeitstag.KernzeitGruppeEnde)
                            {
                                var nächster = vllt.FirstOrDefault(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch)
                                                                        && x.GetEndzeit() >= arbeitstag.KernzeitGruppeEnde);

                                if (nächster == null)
                                {
                                    var alle = vllt.Where(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch))
                                                   .Select(x => x.ErledigtDurch);
                                    mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.AddRange(alle);
                                    break;
                                }

                                mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Add(nächster.ErledigtDurch);
                                bisAbgedeckt = nächster.GetEndzeit();
                            }

                            var kanditaten = vllt.Where(x => !mitarbeiterDieInDerEigentlichenGruppeGebrauchtWerden.Contains(x.ErledigtDurch)).ToList();

                            if (kanditaten.Count == 0)
                                continue;


                            var erstbesten = kanditaten.Where(x => x.AllTicks >= ticks).OrderBy(x => x.Startzeit).FirstOrDefault();
                            if (erstbesten == null)
                                continue;

                            var eigentlicherDienstschluss = erstbesten.GetEndzeit();
                            erstbesten.Gruppe = gruppe;

                            //wenn die Zeiten nicht passen, dann anpassen
                            if (erstbesten.Startzeit > startzeit || eigentlicherDienstschluss < arbeitstag.KernzeitGruppeEnde)
                            {
                                if (erstbesten.Startzeit > startzeit)
                                    erstbesten.Startzeit = startzeit;

                                if (eigentlicherDienstschluss < arbeitstag.KernzeitGruppeEnde)
                                    erstbesten.Startzeit = arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * 15 * erstbesten.AllTicks);
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

                var volleTage = mapl.Value.Where(x => (x.Dienst & DienstTyp.Frei) != DienstTyp.Frei)
                                          .GroupBy(x => x.Startzeit.ToString("yyyyMMdd"))
                                          .Count();

                var kfzMinutenTag = kfz * 60 * volleTage / 5;
                
                //prio ist Spätdienst
                var spätdienste = mapl.Value.Where(x => (x.Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde).ToList();
                if (PlanzeitReduzierenOhneKernzeitVerletzung(woche, spätdienste, kfzMinutenTag))
                    continue;

                var frühdienste = mapl.Value.Where(x => (x.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                                                        || (x.Dienst & DienstTyp.AchtUhrDienst) != DienstTyp.AchtUhrDienst
                                                        || (x.Dienst & DienstTyp.KernzeitStartDienst) != DienstTyp.KernzeitStartDienst).ToList();

                if (PlanzeitReduzierenOhneKernzeitVerletzung(woche, frühdienste, kfzMinutenTag))
                    continue;

            }
        }

        private static bool PlanzeitReduzierenOhneKernzeitVerletzung(Arbeitswoche woche, List<PlanItem> dienste, decimal kfzMinutenTag)
        {
            if (dienste.Count > 0)
            {
                foreach (var planItem in dienste)
                {
                    var arbeitstag = woche.Arbeitstage.Single(x => x.Datum.ToString("yyyyMMdd") == planItem.Startzeit.ToString("yyyyMMdd"));

                    var oldStartzeit = planItem.Startzeit;
                    var oldEndzeit = planItem.GetEndzeit();

                    if (planItem.Startzeit <= arbeitstag.KernzeitGruppeStart)
                    {
                        planItem.Endzeit = oldEndzeit.AddMinutes(-1*(int) kfzMinutenTag);
                    }
                    else
                    {
                        planItem.Startzeit = oldStartzeit.AddMinutes((int)kfzMinutenTag); 
                        planItem.Endzeit = oldEndzeit;
                    }
                   
                    if (CheckKernzeitAbgedeckt(arbeitstag.KernzeitGruppeStart, arbeitstag.KernzeitGruppeEnde, planItem.Gruppe,
                        arbeitstag.Planzeiten))
                    {
                        return true;
                    }

                    planItem.Startzeit = oldStartzeit;
                    planItem.Endzeit = oldEndzeit;
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
                var zeiten = arbeitstag.Planzeiten.Where(x => (x.Gruppe & gruppe) == gruppe && !x.ErledigtDurch.IsHelfer).OrderBy(x => x.Startzeit).ToList();
                foreach (var z in zeiten)
                {
                    startzeitNichtAbgedeckt = z.Startzeit.AddMinutes(15 * z.AllTicks);
                }

                ticksNichtAbgedeckt = (short)((arbeitstag.KernzeitGruppeEnde - startzeitNichtAbgedeckt).TotalMinutes / 15);

                return false;
            }

            return true;
        }

        private static bool CheckKernzeitAbgedeckt(DateTime kernzeitGruppenStart, DateTime kernzeitGruppenEnde, GruppenTyp gruppe, IList<PlanItem> gruppenPlanzeiten)
        {
            var zeiten = gruppenPlanzeiten.Where(x => (x.Gruppe & gruppe) == gruppe && !x.ErledigtDurch.IsHelfer).OrderBy(x => x.Startzeit).ToList();

            //Wenn es keine Leutz gibt dann ist halt niemand da :)
            if (zeiten.Count == 0)
                return false;

            var obKernzeitStartAbgedeckt = zeiten.Min(x => x.Startzeit) <= kernzeitGruppenStart;

            if (!obKernzeitStartAbgedeckt)
                return false;

            var ende = zeiten.First().GetEndzeit();
            while (true)
            {
                if (ende >= kernzeitGruppenEnde)
                {
                    return true;
                }

                var höchsteEndzeit = zeiten.Where(x => x.Startzeit <= ende).OrderByDescending(x => x.GetEndzeit()).FirstOrDefault();

                if (höchsteEndzeit == null || höchsteEndzeit.GetEndzeit() <= ende)
                {
                    return false;
                }

                ende = höchsteEndzeit.GetEndzeit();
            }
        }
    }
}
