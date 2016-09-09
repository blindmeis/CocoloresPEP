using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using Itenso.TimePeriod;

namespace CocoloresPEP.Services
{
    public static class PlanService
    {
        private static readonly Random Zufall = new Random();

        #region Erstellung Wochenplan
        public static void ErstelleWochenplan(Arbeitswoche woche, IList<Mitarbeiter> maList)
        {
            foreach (var item in woche.Arbeitstage)
            {
                item.Planzeiten.Clear();
            }

            foreach (var arbeitstag in woche.Arbeitstage)
            {
                //Samstag und Sontag ignorieren bei Planung
                if (arbeitstag.Datum.DayOfWeek == DayOfWeek.Saturday || arbeitstag.Datum.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (arbeitstag.IsFeiertag)
                {
                    //Wenn Feiertag, dann seinen Tagessatz minutengenau
                    foreach (var mitarbeiter in maList)
                    {
                        arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, GruppenTyp.None, DienstTyp.Frei));
                    }
                    continue;
                }

                //Wenn wer nicht da ist, dann seinen Tagessatz minutengenau
                var mitarbeiterNichtDa = maList.Where(x => x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum)).ToList();
                foreach (var mitarbeiter in mitarbeiterNichtDa)
                {
                    arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, GruppenTyp.None, DienstTyp.Frei));
                }

                //Nur richtige Mitarbeiter die auch da sind
                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && !x.IsHelfer).ToList();

                var schonEingeteilt = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(alledieDaSind, schonEingeteilt, arbeitstag, woche, DienstTyp.Frühdienst);
                if (maFrüh != null)
                {
                    schonEingeteilt.Add(maFrüh);
                    var istFrüh = CreatePlanItem(arbeitstag, maFrüh, maFrüh.DefaultGruppe, DienstTyp.Frühdienst);
                    arbeitstag.Planzeiten.Add(istFrüh);
                }

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(alledieDaSind, schonEingeteilt, arbeitstag, woche, DienstTyp.SpätdienstEnde);

                if (maSpät1 != null)
                {
                    schonEingeteilt.Add(maSpät1);
                    var istSpät1 = CreatePlanItem(arbeitstag, maSpät1, maSpät1.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät1);
                }

                var maSpät2 = NextMitarbeiter(alledieDaSind, schonEingeteilt, arbeitstag, woche, DienstTyp.SpätdienstEnde, GibAndereEtage(maSpät1));

                if (maSpät2 != null)
                {
                    schonEingeteilt.Add(maSpät2);
                    var istSpät2 = CreatePlanItem(arbeitstag, maSpät2, maSpät2.DefaultGruppe, DienstTyp.SpätdienstEnde);
                    arbeitstag.Planzeiten.Add(istSpät2);
                }

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(alledieDaSind, schonEingeteilt, arbeitstag, woche, DienstTyp.AchtUhrDienst, GibAndereEtage(maFrüh));
                if (ma8UhrErster != null)
                {
                    schonEingeteilt.Add(ma8UhrErster);
                    var ist8UhrErster = CreatePlanItem(arbeitstag, ma8UhrErster, ma8UhrErster.DefaultGruppe, DienstTyp.AchtUhrDienst);
                    arbeitstag.Planzeiten.Add(ist8UhrErster);
                }

                #endregion

                #region 16 Uhr Dienst

                if (!arbeitstag.HasGrossteam)
                {
                    var ma16Uhr = NextMitarbeiter(alledieDaSind, schonEingeteilt, arbeitstag, woche, DienstTyp.SechszehnUhrDienst);

                    if (ma16Uhr != null)
                    {
                        schonEingeteilt.Add(ma16Uhr);
                        var ist16Uhr = CreatePlanItem(arbeitstag, ma16Uhr, ma16Uhr.DefaultGruppe, DienstTyp.SechszehnUhrDienst);
                        arbeitstag.Planzeiten.Add(ist16Uhr);
                    }
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
                foreach (var mitarbeiter in rest)
                {
                    arbeitstag.Planzeiten.Add(CreatePlanItem(arbeitstag, mitarbeiter, GruppenTyp.None, DienstTyp.KernzeitStartDienst));
                }

                #endregion

                #region FSJ Dienste

                var helferleins = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && x.IsHelfer).ToList();

                //Frühdienst
                if (!arbeitstag.HasGrossteam)
                {
                    var h1 = NextMitarbeiter(helferleins, schonEingeteilt, arbeitstag, woche, DienstTyp.FsjFrühdienst);

                    if (h1 != null)
                    {
                        schonEingeteilt.Add(h1);
                        var h1Früh = CreatePlanItem(arbeitstag, h1, h1.DefaultGruppe, DienstTyp.FsjFrühdienst);
                        arbeitstag.Planzeiten.Add(h1Früh);
                    }

                    var h2 = NextMitarbeiter(helferleins, schonEingeteilt, arbeitstag, woche, DienstTyp.FsjSpätdienst);

                    if (h2 != null)
                    {
                        schonEingeteilt.Add(h2);
                        var h2Früh = CreatePlanItem(arbeitstag, h2, h2.DefaultGruppe, DienstTyp.FsjSpätdienst);
                        arbeitstag.Planzeiten.Add(h2Früh);
                    }


                    foreach (var helferlein in helferleins.Except(schonEingeteilt).ToList())
                    {
                        schonEingeteilt.Add(helferlein);
                        var h = CreatePlanItem(arbeitstag, helferlein, helferlein.DefaultGruppe, DienstTyp.FsjKernzeitdienst);
                        arbeitstag.Planzeiten.Add(h);
                    }
                }
                else
                {
                    foreach (var helferlein in helferleins.Except(schonEingeteilt).ToList())
                    {
                        schonEingeteilt.Add(helferlein);
                        var hSpät = CreatePlanItem(arbeitstag, helferlein, helferlein.DefaultGruppe, DienstTyp.FsjSpätdienst);
                        arbeitstag.Planzeiten.Add(hSpät);
                    }
                }
               
                #endregion

            }

            CheckKernzeitAbgedecktMitMitarbeiternVomTag(woche);

            NichtVerplanteZeitenPlanen(woche);
        }



        private static GruppenTyp GibAndereEtage(Mitarbeiter ma)
        {
            var result = GruppenTyp.Nest | GruppenTyp.Gelb | GruppenTyp.Gruen | GruppenTyp.Rot;

            if (ma == null)
                return result;

            if ((ma.DefaultGruppe & GruppenTyp.Nest) == GruppenTyp.Nest || (ma.DefaultGruppe & GruppenTyp.Gelb) == GruppenTyp.Gelb)
                return (GruppenTyp.Gruen | GruppenTyp.Rot) & ~ma.DefaultGruppe;

            if ((ma.DefaultGruppe & GruppenTyp.Gruen) == GruppenTyp.Gruen || (ma.DefaultGruppe & GruppenTyp.Rot) == GruppenTyp.Rot)
                return (GruppenTyp.Nest | GruppenTyp.Gelb) & ~ma.DefaultGruppe;

            return result;
        }

        private static Mitarbeiter NextMitarbeiter(IList<Mitarbeiter> alleDieDaSind, IList<Mitarbeiter> schonEingeteilt, Arbeitstag arbeitstag, Arbeitswoche woche = null, DienstTyp ma4Diensttyp = DienstTyp.None, GruppenTyp etage = GruppenTyp.Gelb | GruppenTyp.Gruen | GruppenTyp.Nest | GruppenTyp.Rot)
        {
            var arbeitstagPlanzeiten = arbeitstag.Planzeiten.ToList();

            var topf = alleDieDaSind.Except(schonEingeteilt).Where(x => x.DefaultGruppe.IstFarbGruppe()).ToList();

            if (topf.Count == 0)
                return null;

            //alle die nicht allein sind in der gruppe
            var sonderTopf = topf.ToList();
            var mitarbeiter = new List<Mitarbeiter>();

            //Wenn RandDienst dann gilt erstmal die Wunschdienst REgel zwingend
            if (ma4Diensttyp.IstRandDienst())
                mitarbeiter = sonderTopf = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

            //erstma gucken für die ganz RandRand Dienste
            //alle verfügbaren wo mehr als einer noch in der Gruppe ist
            //und in der Gruppe darf noch keiner einen RandRandDienst haben
            if (sonderTopf.Count == 0 && ma4Diensttyp.IstRandRandDienst())
                sonderTopf = topf.GroupBy(x => x.DefaultGruppe).Where(x => x.Count() > 1)
                                 .SelectMany(x => x.Where(ma => !arbeitstagPlanzeiten.Any(y => y.Gruppe == ma.DefaultGruppe && y.Dienst.IstRandRandDienst()))).ToList();

            //dann gucken für die ganz Rand Dienste
            if (sonderTopf.Count == 0 && ma4Diensttyp.IstRandDienst())
                sonderTopf = topf.GroupBy(x => x.DefaultGruppe).Where(x => x.Count() > 1)
                                .SelectMany(x => x.Where(ma => !arbeitstagPlanzeiten.Any(y => y.Gruppe == ma.DefaultGruppe && y.Dienst.IstRandDienst()))).ToList();

            //Probieren einen zu Finden aus einer Gruppen die noch keinen Randdienst hat
            if (sonderTopf.Count == 0 && ma4Diensttyp.IstRandDienst())
                sonderTopf = topf.GroupBy(x => x.DefaultGruppe)
                                .SelectMany(x => x.Where(ma => !arbeitstagPlanzeiten.Any(y => y.Gruppe == ma.DefaultGruppe && y.Dienst.IstRandDienst()))).ToList();

            //die mit Wunschdienst aus Etage
            if (mitarbeiter.Count == 0)
                mitarbeiter = sonderTopf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp && (etage & x.DefaultGruppe) == x.DefaultGruppe).ToList();

            //die von Etage
            if (mitarbeiter.Count == 0)
                mitarbeiter = sonderTopf.Where(x => (etage & x.DefaultGruppe) == x.DefaultGruppe).ToList();

            //Wunschdienst
            if (mitarbeiter.Count == 0)
                mitarbeiter = sonderTopf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

            //Wenn mit der RandDienstlogik niemand gefunden wurde, dann vllt vorher nochmal schauen ob ein Wunschdienstler Lust hat
            if (mitarbeiter.Count == 0 && ma4Diensttyp.IstRandDienst())
                sonderTopf = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

            if (mitarbeiter.Count == 0)
                mitarbeiter = sonderTopf;

            if (mitarbeiter.Count == 0)//bei allen gucken wenn keiner will
                mitarbeiter = topf;

            //RandDienste Gleichverteilen über die Woche
            if (woche != null && ma4Diensttyp.IstRandDienst())
            {
                var geplanteRanddienste = woche.Arbeitstage.SelectMany(x => x.Planzeiten.Where(p => p.Dienst == ma4Diensttyp)).ToList();
                var mitarbeiterMitRanddiensten = geplanteRanddienste.Select(x => x.ErledigtDurch).Distinct().ToList();

                //irgendein Mitarbeiter da, der noch gar nix hat
                if (mitarbeiter.Except(mitarbeiterMitRanddiensten).Any())
                {
                    mitarbeiter = mitarbeiter.Except(mitarbeiterMitRanddiensten).ToList();
                }
                else
                {
                    var mitarbeiterMitAnzahlRanddienstInderWoche = geplanteRanddienste.GroupBy(g => g.ErledigtDurch).ToDictionary(x => x.Key, c => c.Count());
                    var minCount = mitarbeiterMitAnzahlRanddienstInderWoche.Min(x => x.Value);
                    var mitarbeiterMitWenigstenCounts = mitarbeiterMitAnzahlRanddienstInderWoche.Where(x => x.Value == minCount).Select(x => x.Key).ToList();
                    var mitarbeiterMitMehrcounts = mitarbeiterMitAnzahlRanddienstInderWoche.Where(x => x.Value != minCount).Select(x => x.Key).ToList();
                    
                    if (mitarbeiter.Intersect(mitarbeiterMitWenigstenCounts).Any())
                    {
                        mitarbeiter = mitarbeiter.Intersect(mitarbeiterMitWenigstenCounts).ToList();
                    }
                    else if (mitarbeiter.Except(mitarbeiterMitRanddiensten.Except(mitarbeiterMitMehrcounts)).Any())
                    {
                        mitarbeiter = mitarbeiter.Except(mitarbeiterMitRanddiensten.Except(mitarbeiterMitMehrcounts)).ToList();
                    }

                    //sofern noch einer übrig bleibt soll derjenige der am Vortag das gemacht, nicht nochmal dran sein
                    var vortag = woche.Arbeitstage.Single(x => x.Datum == arbeitstag.Datum.AddDays(-1));
                    var letzte = vortag?.Planzeiten?.Where(x => x.Dienst == ma4Diensttyp).Select(x => x.ErledigtDurch).ToList() ?? new List<Mitarbeiter>();

                    if (mitarbeiter.Except(letzte).Any())
                        mitarbeiter = mitarbeiter.Except(letzte).ToList();
                }

                //am gleichen Tag vllt nicht unbedingt auch noch aus der gleichen gruppe
                var ma4Gruppencheck = arbeitstag.Planzeiten.Where(x => x.Dienst == ma4Diensttyp).Select(x=>x.ErledigtDurch).ToList();
                var maGleicherGruppe = mitarbeiter.Where(x => ma4Gruppencheck.Any(m => m.DefaultGruppe == x.DefaultGruppe)).ToList();
                if (mitarbeiter.Except(maGleicherGruppe).Any())
                {
                    mitarbeiter = mitarbeiter.Except(maGleicherGruppe).ToList();
                }
            }

            int ichBinDran = Zufall.Next(0, mitarbeiter.Count);
#if DEBUG
            Console.WriteLine($"{ma4Diensttyp}: {ichBinDran} von {mitarbeiter.Count}");
#endif
            return mitarbeiter[ichBinDran];
        }



        private static PlanItem CreatePlanItem(Arbeitstag arbeitstag, Mitarbeiter ma, GruppenTyp gruppe, DienstTyp dienst, TimeRange vorgabe = null)
        {
            #region Planzeit ausrechnen
            var duration = ma.TagesQuarterTicks * 15;
            var start = arbeitstag.KernzeitGruppeStart;
            var ende = arbeitstag.KernzeitGruppeEnde;

            switch (dienst)
            {
                case DienstTyp.Frühdienst:
                    start = arbeitstag.Frühdienst;
                    ende = arbeitstag.Frühdienst.AddMinutes(duration);
                    break;
                case DienstTyp.AchtUhrDienst:
                    start = arbeitstag.AchtUhrDienst;
                    ende = arbeitstag.AchtUhrDienst.AddMinutes(duration);
                    break;
                case DienstTyp.KernzeitStartDienst:
                    start = arbeitstag.KernzeitGruppeStart;
                    ende = arbeitstag.KernzeitGruppeStart.AddMinutes(duration);
                    break;
                case DienstTyp.NeunUhrDienst:
                    start = arbeitstag.NeunUhrDienst;
                    ende = arbeitstag.NeunUhrDienst.AddMinutes(duration);
                    break;
                case DienstTyp.ZehnUhrDienst:
                    start = arbeitstag.ZehnUhrDienst;
                    ende = arbeitstag.ZehnUhrDienst.AddMinutes(duration);
                    break;
                case DienstTyp.FsjFrühdienst:
                    start = arbeitstag.FrühdienstFsj;
                    ende = arbeitstag.FrühdienstFsj.AddMinutes(duration);
                    break;
                case DienstTyp.FsjKernzeitdienst:
                    start = arbeitstag.NormaldienstFsj;
                    ende = arbeitstag.NormaldienstFsj.AddMinutes(duration);
                    break;
                case DienstTyp.FsjSpätdienst:
                    start = arbeitstag.SpätdienstEndeFsj.AddMinutes(-1 * duration);
                    ende = arbeitstag.SpätdienstEndeFsj;
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    start = arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * duration);
                    ende = arbeitstag.KernzeitGruppeEnde;
                    break;
                case DienstTyp.SechszehnUhrDienst:
                    start = arbeitstag.SechzehnUhrDienst.AddMinutes(-1 * duration);
                    ende = arbeitstag.SechzehnUhrDienst;
                    break;
                case DienstTyp.SpätdienstEnde:
                    start = arbeitstag.SpätdienstEnde.AddMinutes(-1 * duration);
                    ende = arbeitstag.SpätdienstEnde;
                    break;
                case DienstTyp.Frei:
                    duration = ma.TagesSollMinuten;
                    start = arbeitstag.KernzeitGruppeStart;
                    ende = arbeitstag.KernzeitGruppeStart.AddMinutes(duration);
                    break;
                default:
                    break;
            }
            var zeitraum = new TimeRange(start, ende);
            #endregion

            var result = new PlanItem();

            result.Arbeitstag = arbeitstag;
            result.ErledigtDurch = ma;
            result.Thema = ma?.DefaultThema ?? Themen.None;
            result.Zeitraum = vorgabe ?? zeitraum;
            result.Gruppe = gruppe;
            result.Dienst = dienst;

            result.SetHatGrossteam();

            //Pausen mit Planen
            if (result.Zeitraum.Duration.TotalMinutes > 360)
            {
                if ((dienst & DienstTyp.KernzeitEndeDienst) == DienstTyp.KernzeitEndeDienst
                    || (dienst & DienstTyp.SechszehnUhrDienst) == DienstTyp.SechszehnUhrDienst
                    || (dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde
                    || (dienst & DienstTyp.FsjSpätdienst) == DienstTyp.FsjSpätdienst)

                {
                    result.Zeitraum.Start = result.Zeitraum.Start.AddMinutes(-1 * 30);
                }
                else
                {
                    result.Zeitraum.End = result.Zeitraum.End.AddMinutes(30);
                }
            }

            //Endzeiten einhalten
            AdjustEndzeitEnde(result);

            return result;
        }

        private static void AdjustEndzeitEnde(PlanItem planItem)
        {
            var ende = planItem.Arbeitstag.SpätdienstEnde;

            switch (planItem.Dienst)
            {
                case DienstTyp.Frühdienst:
                case DienstTyp.AchtUhrDienst:
                case DienstTyp.KernzeitStartDienst:
                case DienstTyp.NeunUhrDienst:
                case DienstTyp.ZehnUhrDienst:
                    ende = planItem.Arbeitstag.KernzeitGruppeEnde;
                    break;
                case DienstTyp.FsjFrühdienst:
                case DienstTyp.FsjKernzeitdienst:
                case DienstTyp.FsjSpätdienst:
                    ende = planItem.Arbeitstag.SpätdienstEndeFsj;
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    ende = planItem.Arbeitstag.KernzeitGruppeEnde;
                    break;
                case DienstTyp.SechszehnUhrDienst:
                    ende = planItem.Arbeitstag.SechzehnUhrDienst;
                    break;
                case DienstTyp.SpätdienstEnde:
                    ende = planItem.Arbeitstag.SpätdienstEnde;
                    break;
                default:
                    break;
            }

            if (planItem.Zeitraum.End > ende)
            {
                var minuten = ende - planItem.Zeitraum.End;
                planItem.Zeitraum.Move(minuten);
            }
        }

        private static void FillGruppenDiensteMitKernzeitPrio(List<Mitarbeiter> maList, Arbeitstag arbeitstag, GruppenTyp gruppe, List<Mitarbeiter> schonEingeteilt)
        {
            if (!arbeitstag.Planzeiten.Any(x =>
                        ((x.Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                          || (x.Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
                          || (x.Dienst & DienstTyp.KernzeitStartDienst) == DienstTyp.KernzeitStartDienst)
                        && (x.Gruppe & gruppe) == gruppe
                        && !x.ErledigtDurch.IsHelfer))
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, arbeitstag, ma4Diensttyp: DienstTyp.KernzeitStartDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(arbeitstag, ma, ma.DefaultGruppe, DienstTyp.KernzeitStartDienst);
                arbeitstag.Planzeiten.Add(plan);
            }
            //ab hier gibt es einen Frühen vogel :)

            DateTime startzeit;
            short ticks;
            while (!CheckKernzeitAbgedeckt(arbeitstag, gruppe, out startzeit, out ticks))
            {
                var maKernzeitende = NextMitarbeiter(maList, schonEingeteilt, arbeitstag);
                if (maKernzeitende == null)
                    return;

                var kernzeitEndeStart = arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * 15 * maKernzeitende.TagesQuarterTicks);
                if (kernzeitEndeStart < startzeit)
                    startzeit = kernzeitEndeStart;

                schonEingeteilt.Add(maKernzeitende);
                var trMa = new TimeRange(startzeit, startzeit.AddMinutes(15 * maKernzeitende.TagesQuarterTicks));
                var planKernzeitende = CreatePlanItem(arbeitstag, maKernzeitende, maKernzeitende.DefaultGruppe, DienstTyp.KernzeitEndeDienst, trMa);
                arbeitstag.Planzeiten.Add(planKernzeitende);
            }

            //ab hier gibts ein "Frühen Dienst" und wenn möglich ein Dienst bis Kernzeitende oder drüber
            var ma9 = NextMitarbeiter(maList, schonEingeteilt, arbeitstag, ma4Diensttyp: DienstTyp.NeunUhrDienst);
            if (ma9 == null)
                return;

            schonEingeteilt.Add(ma9);
            var plan9 = CreatePlanItem(arbeitstag, ma9, ma9.DefaultGruppe, DienstTyp.NeunUhrDienst);
            arbeitstag.Planzeiten.Add(plan9);

            var ma10 = NextMitarbeiter(maList, schonEingeteilt, arbeitstag, ma4Diensttyp: DienstTyp.ZehnUhrDienst);
            if (ma10 == null)
                return;

            schonEingeteilt.Add(ma10);
            var plan10 = CreatePlanItem(arbeitstag, ma10, ma10.DefaultGruppe, DienstTyp.ZehnUhrDienst);
            arbeitstag.Planzeiten.Add(plan10);

            //hmm wenn immernoch welche übrig sind dann, halt um 9Uhr kommen
            while (true)
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, arbeitstag, ma4Diensttyp: DienstTyp.NeunUhrDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(arbeitstag, ma, ma.DefaultGruppe, DienstTyp.NeunUhrDienst);
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
                    if (gruppe == GruppenTyp.None)
                        continue;

                    DateTime startzeit;
                    short ticks;

                    if (!CheckKernzeitAbgedeckt(arbeitstag, gruppe, out startzeit, out ticks))
                    {
                        //beim Tag schauen in andern Gruppen
                        var wirHabenvlltZeit = arbeitstag.Planzeiten.Where(x => (x.ErledigtDurch.DefaultGruppe & gruppe) != gruppe && !x.ErledigtDurch.IsHelfer && !x.Dienst.HasFlag(DienstTyp.Frei))
                                                                    .GroupBy(g => g.Gruppe)
                                                                    .OrderByDescending(o => o.Count())
                                                                    .ToList();

                        foreach (var vllt in wirHabenvlltZeit)
                        {
                            if (vllt.Count() < 2)
                                continue;

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
                            var erstbesten = kanditaten.Where(x => x.Zeitraum.Duration.TotalMinutes >= mindauer && !x.Dienst.IstRandDienst()).OrderBy(x => x.Zeitraum.Start).FirstOrDefault()
                                             ?? kanditaten.Where(x => x.Zeitraum.Duration.TotalMinutes >= mindauer).OrderBy(x => x.Zeitraum.Start).FirstOrDefault();

                            if (erstbesten == null)
                                continue;

                            //Wenn nur randdienstler gefunden, dann darf der nicht "verschoben" werden in der Zeit
                            if (erstbesten.Dienst.IstRandDienst())
                            {
                                if (erstbesten.Zeitraum.Start > startzeit || erstbesten.Zeitraum.End < arbeitstag.KernzeitGruppeEnde)
                                    continue;
                            }

                            //einen Gefunden
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

        private static void NichtVerplanteZeitenPlanen(Arbeitswoche woche)
        {
            var mitarbeiterPlanzeiten =
                woche.Arbeitstage
                    .SelectMany(x => x.Planzeiten)
                    .GroupBy(x => x.ErledigtDurch)
                    .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var mapl in mitarbeiterPlanzeiten)
            {
                var kfz = (int)(mapl.Key.KindFreieZeit * 60);
                var grossteam = mapl.Value.Where(x => x.HatGrossteam).Select(x => x.Arbeitstag.Grossteam).Sum(s => (int)s.Duration.TotalMinutes);
                var wochenstundenAngeordnet = (int)(mapl.Key.WochenStunden * 60);
                var geplant = mapl.Value.Sum(x => (int)x.GetArbeitsminutenAmKindOhnePause());
                var saldo = geplant + kfz + grossteam - wochenstundenAngeordnet;

                if (saldo < 15 && saldo > -15)
                    continue;

                if (saldo >= 15)
                {
                    var aufzuteilenMin = (int)(saldo);

                    if (aufzuteilenMin < 15)
                        continue;


                    aufzuteilenMin = ((int)aufzuteilenMin / 15) * 15;
                    var tage = mapl.Value.Count(x=>(x.Dienst & DienstTyp.Frei) != DienstTyp.Frei);

                    if(tage == 0)
                        continue;

                    var tagAufteilung = ((int)((int)(aufzuteilenMin / tage)) / 15) * 15;

                    //1. Kleine Zeiten auf alle Teile
                    foreach (var planItem in mapl.Value)
                    {
                        if ((planItem.Dienst & DienstTyp.Frei) == DienstTyp.Frei)
                            continue;

                        var aufteilen = 15;
                        while (aufteilen <= tagAufteilung)
                        {
                            if (PlanzeitReduzierenOhneKernzeitVerletzung(planItem, 15))
                            {
                                aufzuteilenMin -= 15;
                            }
                            else
                            {
                                break;
                            }

                            aufteilen += 15;
                        }
                    }
                    //2. Rest irgendwie
                    foreach (var planItem in mapl.Value)
                    {
                        if ((planItem.Dienst & DienstTyp.Frei) == DienstTyp.Frei)
                            continue;

                        var aufteilen = aufzuteilenMin;
                        while (aufteilen > 0)
                        {
                            if (PlanzeitReduzierenOhneKernzeitVerletzung(planItem, 15))
                            {
                                aufzuteilenMin -= 15;
                                break;
                            }

                            aufteilen -= 15;
                        }
                    }
                }
            }
        }

        private static bool PlanzeitReduzierenOhneKernzeitVerletzung(PlanItem dienst, int aufzuteilen)
        {
            var oldStartzeit = dienst.Zeitraum.Start;
            var oldEndzeit = dienst.Zeitraum.End;

            switch (dienst.Dienst)
            {
                case DienstTyp.None:
                case DienstTyp.Frühdienst:
                case DienstTyp.AchtUhrDienst:
                case DienstTyp.KernzeitStartDienst:
                    dienst.Zeitraum.End = dienst.Zeitraum.End.AddMinutes(-1 * aufzuteilen);
                    break;
                case DienstTyp.NeunUhrDienst:
                    //für 9Uhr Dienst schauen ob man besser beim Ende oder beim Anfang die Zeit abzieht
                    if (dienst.Zeitraum.Start.AddMinutes(aufzuteilen) <= dienst.Arbeitstag.NeunUhrDienst)
                    {
                        dienst.Zeitraum.Start = dienst.Zeitraum.Start.AddMinutes(aufzuteilen);
                    }
                    else
                    {
                        dienst.Zeitraum.End = dienst.Zeitraum.End.AddMinutes(-1 * aufzuteilen);
                    }
                    break;
                case DienstTyp.ZehnUhrDienst:
                    //für 10Uhr Dienst schauen ob man besser beim Ende oder beim Anfang die Zeit abzieht
                    if (dienst.Zeitraum.Start.AddMinutes(aufzuteilen) <= dienst.Arbeitstag.ZehnUhrDienst)
                    {
                        dienst.Zeitraum.Start = dienst.Zeitraum.Start.AddMinutes(aufzuteilen);
                    }
                    else
                    {
                        dienst.Zeitraum.End = dienst.Zeitraum.End.AddMinutes(-1 * aufzuteilen);
                    }

                    break;
                case DienstTyp.KernzeitEndeDienst:
                case DienstTyp.SechszehnUhrDienst:
                case DienstTyp.SpätdienstEnde:
                    dienst.Zeitraum.Start = dienst.Zeitraum.Start.AddMinutes(aufzuteilen);
                    break;
                case DienstTyp.Frei:
                    return false;
                default:
                    break;
            }

            if (!dienst.Zeitraum.Duration.IsMindestzeitAbgedeckt())
            {
                dienst.Zeitraum.Start = oldStartzeit;
                dienst.Zeitraum.End = oldEndzeit;
                return false;
            }

            if (!dienst.Arbeitstag.CheckKernzeitAbgedeckt(dienst.Gruppe))
            {
                dienst.Zeitraum.Start = oldStartzeit;
                dienst.Zeitraum.End = oldEndzeit;
                return false;
            }
            if (dienst.HatGrossteam)
                return true;

            if (!dienst.Arbeitstag.CheckKernzeitDoppelBesetzungAbgedeckt(dienst.Gruppe))
            {
                dienst.Zeitraum.Start = oldStartzeit;
                dienst.Zeitraum.End = oldEndzeit;
                return false;
            }

            return true;
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

            if (!arbeitstag.CheckKernzeitAbgedeckt(gruppe))
            {
                var planzeiten = new TimePeriodCollection(arbeitstag.GetMitarbeiterArbeitszeiten(gruppe));
                var gapCalculator = new TimeGapCalculator<TimeRange>(new TimeCalendar());
                var gap = gapCalculator.GetGaps(planzeiten, arbeitstag.KernzeitBasisRange).FirstOrDefault();

                if (gap == null)
                    return false;

                startzeitNichtAbgedeckt = gap.Start;
                ticksNichtAbgedeckt = (short)(gap.Duration.TotalMinutes / 15);

                return false;
            }
            return true;
        }




    }
}
