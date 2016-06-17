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

                //Nur richtige Mitarbeiter die auch da sind
                var alledieDaSind = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && !x.IsHelfer).ToList();

                var schonEingeteilt = new List<Mitarbeiter>();

                #region Frühdienst

                var maFrüh = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.Frühdienst);
                if (maFrüh != null)
                {
                    schonEingeteilt.Add(maFrüh);
                    var istFrüh = CreatePlanItem(maFrüh, arbeitstag.Frühdienst, maFrüh.TagesQuarterTicks, maFrüh.DefaultGruppe, DienstTyp.Frühdienst );
                    arbeitstag.Planzeiten.Add(istFrüh);
                }

                #endregion

                #region Spätdienst 2Mitarbeiter

                var maSpät1 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde);

                if (maSpät1 != null)
                {
                    schonEingeteilt.Add(maSpät1);
                    var spätdienstStart1 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät1.TagesQuarterTicks);
                    var istSpät1 = CreatePlanItem(maSpät1, spätdienstStart1, maSpät1.TagesQuarterTicks, maSpät1.DefaultGruppe, DienstTyp.SpätdienstEnde );
                    arbeitstag.Planzeiten.Add(istSpät1);
                }

                var maSpät2 = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.SpätdienstEnde);

                if (maSpät2 != null)
                {
                    schonEingeteilt.Add(maSpät2);
                    var spätdienstStart2 = arbeitstag.SpätdienstEnde.AddMinutes(-1 * 15 * maSpät2.TagesQuarterTicks);
                    var istSpät2 = CreatePlanItem(maSpät2, spätdienstStart2, maSpät2.TagesQuarterTicks, maSpät2.DefaultGruppe, DienstTyp.SpätdienstEnde );
                    arbeitstag.Planzeiten.Add(istSpät2);
                }

                #endregion

                #region 8 uhr Dienst

                var ma8UhrErster = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.AchtUhrDienst);
                schonEingeteilt.Add(ma8UhrErster);
                var ist8UhrErster = CreatePlanItem(ma8UhrErster, arbeitstag.AchtUhrDienst, ma8UhrErster.TagesQuarterTicks,ma8UhrErster.DefaultGruppe, DienstTyp.AchtUhrDienst );
                arbeitstag.Planzeiten.Add(ist8UhrErster);

                var ma8UhrZweiter = NextMitarbeiter(GetMitarbeiterDieNichtAlleinSindProGruppe(alledieDaSind, schonEingeteilt), schonEingeteilt, DienstTyp.AchtUhrDienst);
                schonEingeteilt.Add(ma8UhrZweiter);
                var ist8UhrZweiter = CreatePlanItem(ma8UhrZweiter, arbeitstag.AchtUhrDienst, ma8UhrZweiter.TagesQuarterTicks,ma8UhrZweiter.DefaultGruppe, DienstTyp.AchtUhrDienst );
                arbeitstag.Planzeiten.Add(ist8UhrZweiter);

                #endregion

                #region Gruppendienste

                var restMas = alledieDaSind.Where(x => !schonEingeteilt.Contains(x)).ToList();

                var nestMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Nest)).ToList();
                FillGruppenDienste(nestMas, arbeitstag, GruppenTyp.Nest, schonEingeteilt);

                var blauMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Blau)).ToList();
                FillGruppenDienste(blauMas, arbeitstag, GruppenTyp.Blau, schonEingeteilt);

                var gruenMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Gruen)).ToList();
                FillGruppenDienste(gruenMas, arbeitstag, GruppenTyp.Gruen, schonEingeteilt);

                var rotMas = restMas.Where(x => x.DefaultGruppe.HasFlag(GruppenTyp.Rot)).ToList();
                FillGruppenDienste(rotMas, arbeitstag, GruppenTyp.Rot, schonEingeteilt);

                var rest = alledieDaSind.Where(x => !schonEingeteilt.Contains(x)).ToList();
                FillGruppenDienste(rest, arbeitstag, GruppenTyp.None, schonEingeteilt);

                #endregion

                #region Buftis und Co

                var helferleins = maList.Where(x => !x.NichtDaZeiten.Any(dt => dt == arbeitstag.Datum) && x.IsHelfer).ToList();
                foreach (var helferlein in helferleins)
                {
                    var planitems = arbeitstag.Planzeiten.Where(x => (x.ErledigtDurch.DefaultGruppe & helferlein.DefaultGruppe) == helferlein.DefaultGruppe).ToList();

                    var planitemOhneHelfer = planitems.GroupBy(x => x.Startzeit).FirstOrDefault(x => !x.Any(h => h.ErledigtDurch.IsHelfer))?.FirstOrDefault();

                    if (planitemOhneHelfer == null)
                    {
                        schonEingeteilt.Add(helferlein);
                        var plan = CreatePlanItem(helferlein, arbeitstag.AchtUhr30Dienst, helferlein.TagesQuarterTicks, helferlein.DefaultGruppe, DienstTyp.AchtUhr30Dienst );
                        arbeitstag.Planzeiten.Add(plan);
                    }
                    else
                    {
                        schonEingeteilt.Add(helferlein);
                        var plan = CreatePlanItem(helferlein, planitemOhneHelfer.Startzeit, helferlein.TagesQuarterTicks,helferlein.DefaultGruppe, planitemOhneHelfer.Dienst );
                        arbeitstag.Planzeiten.Add(plan);
                    }
                }
                #endregion
            }

            //Arbeitswochenstunden checken, brauch man gar nicht wird ja immer der Tagessatz draufgeplant...
            //var zeiten = woche.Arbeitstage.SelectMany(x => x.Planzeiten).ToList();
            //foreach (var maZeiten in zeiten.GroupBy(x=>x.ErledigtDurch))
            //{
            //    var anzArbeitstage = woche.Arbeitstage.Count(x => !maZeiten.Key.NichtDaZeiten.Contains(x.Datum));//todo oder Feiertag
            //    var planTicks = maZeiten.Sum(x=>x.QuarterTicks);
            //    var sollTicks = maZeiten.Key.TagesQuarterTicks*anzArbeitstage;

            //    if(planTicks >= sollTicks)
            //        continue;

            //    //weniger im Plan als man müsste
            //    var addTicks = sollTicks - planTicks;
            //}
        }

        private static Mitarbeiter NextMitarbeiter(IList<Mitarbeiter> alleDieDaSind, IList<Mitarbeiter> schonEingeteilt, DienstTyp ma4Diensttyp = DienstTyp.None)
        {
            var topf = alleDieDaSind.Except(schonEingeteilt).ToList();

            if (topf.Count == 0)
                return null;

            //die mit Wunschdienst
            var mitarbeiter = topf.Where(x => (x.Wunschdienste & ma4Diensttyp) == ma4Diensttyp).ToList();

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

        private static PlanItem CreatePlanItem(Mitarbeiter ma, DateTime startzeit, short quarterticks, GruppenTyp gruppe, DienstTyp dienst)
        {
            var result = new PlanItem();

            result.ErledigtDurch = ma;
            result.Startzeit = startzeit;
            result.QuarterTicks = quarterticks;
            result.Gruppe = gruppe;
            result.Dienst = dienst;

            //Pause nachbereiten, Startzeit für den Spätdienst vorziehen
            if (result.BreakTicks != 0)
            {
                if ((result.Dienst & DienstTyp.SpätdienstEnde) ==DienstTyp.SpätdienstEnde)
                {
                    result.Startzeit = startzeit.AddMinutes(-1 * 15 * result.BreakTicks);
                }
            }

            return result;
        }

        private static void FillGruppenDienste(List<Mitarbeiter> maList, Arbeitstag arbeitstag, GruppenTyp gruppe, List<Mitarbeiter> schonEingeteilt)
        {
            //Todo eigentlich müsste man rechnen ob wer von der startzeit drunter liegt und mit Ticks bis zu KernzeitStart kommt
            if (!arbeitstag.Planzeiten.Any(x =>
                        (x.Dienst.HasFlag(DienstTyp.Frühdienst) || x.Dienst.HasFlag(DienstTyp.AchtUhrDienst) || x.Dienst.HasFlag(DienstTyp.AchtUhr30Dienst))
                        && x.ErledigtDurch.DefaultGruppe.HasFlag(gruppe)))
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.AchtUhr30Dienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(ma, arbeitstag.AchtUhr30Dienst, ma.TagesQuarterTicks, ma.DefaultGruppe, DienstTyp.AchtUhr30Dienst );
                arbeitstag.Planzeiten.Add(plan);
            }

            //ab hier gibts ein "Frühen Dienst"
            var ma9 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
            if (ma9 == null)
                return;

            schonEingeteilt.Add(ma9);
            var plan9 = CreatePlanItem(ma9, arbeitstag.NeunUhrDienst, ma9.TagesQuarterTicks, ma9.DefaultGruppe, DienstTyp.NeunUhrDienst );
            arbeitstag.Planzeiten.Add(plan9);

            var ma10 = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.ZehnUhrDienst);
            if (ma10 == null)
                return;

            schonEingeteilt.Add(ma10);
            var plan10 = CreatePlanItem(ma10, arbeitstag.ZehnUhrDienst, ma10.TagesQuarterTicks, ma10.DefaultGruppe, DienstTyp.ZehnUhrDienst );
            arbeitstag.Planzeiten.Add(plan10);

            //hmm wenn immernoch welche übrig sind dann, halt um 9Uhr kommen
            while (true)
            {
                var ma = NextMitarbeiter(maList, schonEingeteilt, DienstTyp.NeunUhrDienst);
                if (ma == null)
                    return;

                schonEingeteilt.Add(ma);
                var plan = CreatePlanItem(ma, arbeitstag.NeunUhrDienst, ma.TagesQuarterTicks, ma.DefaultGruppe, DienstTyp.NeunUhrDienst );
                arbeitstag.Planzeiten.Add(plan);
            }
        } 
        #endregion

        

        public static void CheckPlanung(Arbeitswoche woche)
        {
            //prüfen ob alle Gruppen in der Kernzeit (8:30-15:30) besetzt sind
            foreach (var arbeitstag in woche.Arbeitstage)
            {
                DateTime startzeit;
                short ticks;

                if (!CheckKernzeitAbgedeckt(arbeitstag, GruppenTyp.Gruen, out startzeit, out ticks))
                {
                   

                    //erstmal beim Tag schauen in andern Gruppen
                    var wirHabenvlltZeit = arbeitstag.Planzeiten.Where(x => x.Gruppe != GruppenTyp.Rot).GroupBy(g => g.Gruppe).OrderBy(o => o.Count(c=>!c.ErledigtDurch.IsHelfer));

                    foreach (var gruppe in wirHabenvlltZeit)
                    {
                        
                    }
                }


            }
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

            var zeiten = arbeitstag.Planzeiten.Where(x => x.Gruppe == gruppe && !x.ErledigtDurch.IsHelfer).OrderBy(x=>x.Startzeit).ToList();
            
            //Wenn es keine Leutz gibt dann ist halt niemand da :)
            if (zeiten.Count == 0)
                return false;

            var planStartzeit = zeiten.Min(x => x.Startzeit);
            var planEndzeit = zeiten.Max(x => x.Startzeit.AddMinutes(15 * x.AllTicks));
            
            if (planStartzeit > arbeitstag.KernzeitGruppeStart || planEndzeit < arbeitstag.KernzeitGruppeEnde)
            {
                foreach (var z in zeiten)
                {
                    startzeitNichtAbgedeckt = z.Startzeit.AddMinutes(15*z.AllTicks);
                }

                ticksNichtAbgedeckt = (short)((arbeitstag.KernzeitGruppeEnde - startzeitNichtAbgedeckt).TotalMinutes / 15);

                return false;
            }

            return true;
        }
    }
}
