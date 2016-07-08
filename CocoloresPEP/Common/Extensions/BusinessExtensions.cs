﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CocoloresPEP.Common.Entities;
using Itenso.TimePeriod;

namespace CocoloresPEP.Common.Extensions
{
    public static class BusinessExtensions
    {

        public static int GetGrossteamZeitInMinmuten(this PlanItem planzeit)
        {
            if (planzeit.HatGrossteam)
            {
                var gtMinuten = (int)planzeit.Arbeitstag.Grossteam.Duration.TotalMinutes;
                var overlap = 0;

                if (planzeit.Zeitraum.IntersectsWith(planzeit.Arbeitstag.Grossteam))
                    overlap = (int)planzeit.Zeitraum.GetIntersection(planzeit.Arbeitstag.Grossteam).Duration.TotalMinutes;

                return gtMinuten - overlap;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gibt für die Arbeitszeit bzw. Dienst Frei die Minuten ohne Pause zurück
        /// </summary>
        /// <param name="dienst"></param>
        /// <returns></returns>
        public static int GetArbeitsminutenAmKindOhnePause(this PlanItem dienst)
        {
            var minuten = (int)dienst.Zeitraum.Duration.TotalMinutes;
            int pause;

            if(dienst.NeedPause(out pause))
                return minuten - pause;

            return minuten;
        }

        public static bool NeedPause(this PlanItem dienst, out int pause)
        {
            pause = 0;

            if (dienst.Zeitraum.Duration.NeedPause())
            {
                pause = 30;
                return true;
            }

            //wenn keine Pause aber Großteam noch ist
            if (dienst.HatGrossteam)
            {
                var tp = new TimePeriodCollection(new List<ITimePeriod>() { dienst.Zeitraum, dienst.Arbeitstag.Grossteam });

                if (tp.HasGaps())
                {
                    var gapCalculator = new TimeGapCalculator<TimeRange>(new TimeCalendar());
                    var gaps = gapCalculator.GetGaps(tp);

                    var gap = (int) gaps.First().Duration.TotalMinutes; //sollte nur eine geben, sind ja nur 2 Zeiten

                    if (gap < 30)
                    {
                        pause = 30 - gap;
                        return true;
                    }
                }
                else
                {
                    var periodCombiner = new TimePeriodCombiner<TimeRange>();
                    var combinedPeriods = periodCombiner.CombinePeriods(tp);

                    if (combinedPeriods.First().Duration.NeedPause())
                    {
                        pause = 30;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool NeedPause(this PlanItem dienst)
        {
            int pause;
            return dienst.NeedPause(out pause);
        }

        private static bool NeedPause(this TimeSpan duration)
        {
            var minuten = (int)duration.TotalMinutes;

            if (minuten > 360)
                return true;

            return false;
        }

        public static string GetInfoPlanzeitInfo(this PlanItem planzeit)
        {
            var info = "";
            if ((planzeit.Arbeitstag?.IsFeiertag ?? false)
                || (planzeit.Dienst & DienstTyp.Frei) == DienstTyp.Frei 
                || (int)planzeit.Zeitraum.Duration.TotalMinutes == 0)
                return info;

            var pause = planzeit.NeedPause() ? "P" : " ";
            var grossteam = planzeit.HatGrossteam ? "G" : " ";

            var zusatz = !string.IsNullOrWhiteSpace(pause) || !string.IsNullOrWhiteSpace(grossteam) ? " |" : "";
            info = $"{planzeit.Zeitraum.Start.ToString("HH:mm")}-{planzeit.Zeitraum.End.ToString("HH:mm")}{zusatz}{pause}{grossteam}";

            return info;
        }

        /// <summary>
        /// Es sollte jeder Mitarbeiter mindestens 4h da sein
        /// </summary>
        /// <param name="planzeit"></param>
        /// <returns></returns>
        public static bool IsMindestzeitAbgedeckt(this TimeSpan planzeit)
        {
            var minuten = (int)planzeit.TotalMinutes;

            if (minuten >= 240)
                return true;

            return false;
        }

        public static int GibtFreiMinutenBzglDerGeplantenDienste(this PlanItem planzeit)
        {
            var freiminuten = 0;
            switch (planzeit.Dienst)
            {
                case DienstTyp.Frühdienst:
                    if (planzeit.Zeitraum.Start < planzeit.Arbeitstag.Frühdienst)
                        freiminuten = (int)(planzeit.Arbeitstag.Frühdienst - planzeit.Zeitraum.Start).TotalMinutes;
                    break;
                case DienstTyp.AchtUhrDienst:
                    if (planzeit.Zeitraum.Start < planzeit.Arbeitstag.AchtUhrDienst)
                        freiminuten = (int)(planzeit.Arbeitstag.AchtUhrDienst - planzeit.Zeitraum.Start).TotalMinutes;
                    break;
                case DienstTyp.KernzeitStartDienst:
                    if (planzeit.Zeitraum.Start < planzeit.Arbeitstag.KernzeitGruppeStart)
                        freiminuten = (int)(planzeit.Arbeitstag.KernzeitGruppeStart - planzeit.Zeitraum.Start).TotalMinutes;
                    break;
                case DienstTyp.NeunUhrDienst:
                    if (planzeit.Zeitraum.Start < planzeit.Arbeitstag.NeunUhrDienst)
                        freiminuten = (int)(planzeit.Arbeitstag.NeunUhrDienst - planzeit.Zeitraum.Start).TotalMinutes;
                    break;
                case DienstTyp.ZehnUhrDienst:
                    if (planzeit.Zeitraum.Start < planzeit.Arbeitstag.ZehnUhrDienst)
                        freiminuten = (int)(planzeit.Arbeitstag.ZehnUhrDienst - planzeit.Zeitraum.Start).TotalMinutes;
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    if (planzeit.Zeitraum.End > planzeit.Arbeitstag.KernzeitGruppeEnde)
                        freiminuten = (int)(planzeit.Zeitraum.End - planzeit.Arbeitstag.KernzeitGruppeEnde).TotalMinutes;
                    break;
                case DienstTyp.SechszehnUhrDienst:
                    if (planzeit.Zeitraum.End > planzeit.Arbeitstag.SechzehnUhrDienst)
                        freiminuten = (int)(planzeit.Zeitraum.End - planzeit.Arbeitstag.SechzehnUhrDienst).TotalMinutes;
                    break;
                case DienstTyp.SpätdienstEnde:
                    if (planzeit.Zeitraum.End > planzeit.Arbeitstag.SpätdienstEnde)
                        freiminuten = (int)(planzeit.Zeitraum.End - planzeit.Arbeitstag.SpätdienstEnde).TotalMinutes;
                    break;
            }

          

            return freiminuten;
        }

        public static void SetHatGrossteam(this PlanItem plan)
        {
            plan.HatGrossteam = plan.Arbeitstag.HasGrossteam
                                && !plan.ErledigtDurch.IsHelfer 
                                && (plan.Dienst & DienstTyp.Frei) != DienstTyp.Frei;
        }

        public static bool CanSetHatGrossteam(this PlanItem plan)
        {
            return plan.Arbeitstag.HasGrossteam
                   && !plan.ErledigtDurch.IsHelfer
                   && (plan.Dienst & DienstTyp.Frei) != DienstTyp.Frei;
        }

        /// <summary>
        /// Gibt die Mitarbeiter Planitems für eine Arbeitstag und eine Gruppe zurück
        /// </summary>
        /// <param name="arbeitstag"></param>
        /// <param name="gruppe"></param>
        /// <returns></returns>
        public static IList<PlanItem> GetMitarbeiterArbeitsplanzeiten(this Arbeitstag arbeitstag, GruppenTyp gruppe)
        {
            return
                arbeitstag.Planzeiten.Where(x => (x.Gruppe & gruppe) == gruppe && !x.ErledigtDurch.IsHelfer)
                    .Select(x => x)
                    .ToList();
        }

        /// <summary>
        /// Gibt die Mitarbeiter Arbeitszeiten für eine Arbeitstag und eine Gruppe zurück
        /// </summary>
        /// <param name="arbeitstag"></param>
        /// <param name="gruppe"></param>
        /// <returns></returns>
        public static IList<TimeRange> GetMitarbeiterArbeitszeiten(this Arbeitstag arbeitstag, GruppenTyp gruppe)
        {
            return
                arbeitstag.Planzeiten
                .Where(x => (x.Gruppe & gruppe) == gruppe 
                            && (x.Dienst & DienstTyp.Frei) != DienstTyp.Frei
                            && !x.ErledigtDurch.IsHelfer)
                    .Select(x => x.Zeitraum)
                    .ToList();
        } 
        /// <summary>
        /// Früh-, 8uhr-, 16Uhr-, Spätdienst
        /// </summary>
        /// <param name="dienst"></param>
        /// <returns></returns>
        public static bool IstRandDienst(this DienstTyp dienst)
        {
            return (dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                   || (dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
                   || (dienst & DienstTyp.SechszehnUhrDienst) == DienstTyp.SechszehnUhrDienst
                   || (dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde;

        }
        /// <summary>
        /// Früh-, Spätdienst
        /// </summary>
        /// <param name="dienst"></param>
        /// <returns></returns>
        public static bool IstRandRandDienst(this DienstTyp dienst)
        {
            return (dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                   || (dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde;

        }

        /// <summary>
        ///    <SolidColorBrush x:Key="ColorGruppeGelb" Color="DarkGoldenrod"/>
        ///    <SolidColorBrush x:Key="ColorGruppeGruen" Color="LightGreen"/>
        ///    <SolidColorBrush x:Key="ColorGruppeRot" Color="LightCoral"/>
        ///    <SolidColorBrush x:Key="ColorGruppeNest" Color="LightBlue"/>
        /// </summary>
        /// <param name="gruppe"></param>
        /// <returns></returns>
        public static SolidColorBrush GetFarbeFromResources(this GruppenTyp gruppe)
        {
            var farbe = new SolidColorBrush();

            if (gruppe == GruppenTyp.None)
                return farbe;

            var key = "";
            switch (gruppe)
            {
                    case GruppenTyp.Gelb:
                    key = "ColorGruppeGelb";
                    break;
                case GruppenTyp.Gruen:
                    key = "ColorGruppeGruen";
                    break;
                case GruppenTyp.Rot:
                    key = "ColorGruppeRot";
                    break;
                case GruppenTyp.Nest:
                    key = "ColorGruppeNest";
                    break;
            }

            try
            {
                var color = Application.Current.TryFindResource(key) as SolidColorBrush;

                if (color == null)
                    return farbe;

                farbe = color;
                return farbe;
            }
            catch 
            {
                return farbe;
            }
          

        }
    }
}
