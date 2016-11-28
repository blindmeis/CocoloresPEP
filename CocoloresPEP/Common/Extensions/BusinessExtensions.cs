using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using CocoloresPEP.Common.Entities;
using Itenso.TimePeriod;
using OfficeOpenXml.DataValidation;

namespace CocoloresPEP.Common.Extensions
{
    public static class BusinessExtensions
    {
        private const string ExternalResources = "ColoresPEPResourceDictionary.xaml";

        public static Tuple<int, int> GetZeitTuple(this string zeit)
        {
            var z = zeit.Split(':');

            if(z.Length != 2)
                throw new ArgumentException(@"Unzulässiger Wert für Dienstzeit in der app.config.", zeit);

            return new Tuple<int, int>(int.Parse(z[0]),int.Parse(z[1]));
        }

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

                    var gap = (int)Math.Round(gaps.First().Duration.TotalMinutes, MidpointRounding.ToEven); //sollte nur eine geben, sind ja nur 2 Zeiten

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

        public static string GetInfoPlanzeitInfo(this PlanItem planzeit, bool showStundenInfo = false)
        {
            var info = "";
            var stundenanzeige = "";

            if ((planzeit.Arbeitstag?.IsFeiertag ?? false)
                || planzeit.Dienst == DienstTyp.Frei 
                || (int)planzeit.Zeitraum.Duration.TotalMinutes == 0)
                return info;

            var pause = planzeit.NeedPause() ? "P" : " ";
            var grossteam = planzeit.HatGrossteam ? "G" : " ";

            if (showStundenInfo)
                stundenanzeige = $" ({(planzeit.Zeitraum.Duration.TotalMinutes/60).ToString("0.00")}h)";

            var zusatz = !string.IsNullOrWhiteSpace(pause) || !string.IsNullOrWhiteSpace(grossteam) ? " |" : "";
            info = $"{planzeit.Zeitraum.Start.ToString("HH:mm")}-{planzeit.Zeitraum.End.ToString("HH:mm")}{stundenanzeige}{zusatz}{pause}{grossteam}";

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

        public static bool CheckKernzeitAbgedeckt(this Arbeitstag arbeitstag, GruppenTyp gruppe)
        {
            if (!gruppe.IstFarbGruppe())
                return true;

            var gruppenzeiten = arbeitstag.GetMitarbeiterArbeitszeiten(gruppe);

            if (gruppenzeiten.Count == 0)
                return false;

            var tp = new TimePeriodCollection(gruppenzeiten);
            var obKernzeit = tp.HasInside(arbeitstag.KernzeitBasisRange);

            return obKernzeit;
        }

        public static bool CheckKernzeitDoppelBesetzungAbgedeckt(this Arbeitstag arbeitstag, GruppenTyp gruppe)
        {
            if (!gruppe.IstFarbGruppe())
                return true;

            var gruppenzeiten = arbeitstag.GetMitarbeiterArbeitszeiten(gruppe);

            if (gruppenzeiten.Count < 2)
                return false;

            if (gruppenzeiten.Count == 2)
            {
                var anzDoppelBesetzung = 0;
                foreach (var range in gruppenzeiten)
                {
                    var tp = new TimePeriodCollection(new List<ITimePeriod>() {range});
                    if (tp.HasInside(arbeitstag.KernzeitDoppelBesetzungRange))
                        anzDoppelBesetzung++;
                }
                return anzDoppelBesetzung >= 2;
            }
            
            //07:00-11:00
            //09:00-13:00
            //11:00-16:00
            var zeitzeiger = arbeitstag.KernzeitDoppelBesetzungRange.Start;
            while (zeitzeiger <= arbeitstag.KernzeitDoppelBesetzungRange.End)
            {
                var count = gruppenzeiten.Count(x => x.HasInside(zeitzeiger));
                if (count < 2)
                    return false;

                zeitzeiger = zeitzeiger.AddMinutes(1);
            }

            return true;
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
                   && (!plan.ErledigtDurch?.IsHelfer ?? false)
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
                arbeitstag.Planzeiten.Where(x => x.Gruppe== gruppe && !x.ErledigtDurch.IsHelfer)
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
                .Where(x => x.Gruppe  == gruppe 
                            && x.Dienst != DienstTyp.Frei
                            && (!x.ErledigtDurch?.IsHelfer??false))
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

        public static bool IstFarbGruppe(this GruppenTyp gruppe)
        {
            return (gruppe & GruppenTyp.Gelb) == GruppenTyp.Gelb
                   || (gruppe & GruppenTyp.Gruen) == GruppenTyp.Gruen
                   || (gruppe & GruppenTyp.Rot) == GruppenTyp.Rot
                   || (gruppe & GruppenTyp.Nest) == GruppenTyp.Nest;
        }


        public static ResourceDictionary GetExternalResources()
        {
            var fi = new FileInfo(ExternalResources);

            if (fi.Exists)
            {
                using (var reader = new StreamReader(ExternalResources))
                {
                    var xml = XamlReader.Load(reader.BaseStream) as ResourceDictionary;
                    return xml;
                }
            }
            return null;
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

            if (!gruppe.IstFarbGruppe())
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

        /// <summary>
        /// <SolidColorBrush x:Key="ColorSpaetdienst" Color="Red"/>
        /// <SolidColorBrush x:Key="ColorFruehdienst" Color="Yellow"/>
        /// <SolidColorBrush x:Key="ColorAchtUhrDienst" Color="DarkSeaGreen"/>
        /// <SolidColorBrush x:Key="ColorFsjFruehDienst" Color="DarkSeaGreen"/>
        /// <SolidColorBrush x:Key="ColorFsjSpaetDienst" Color="DarkSeaGreen"/>
        /// </summary>
        /// <param name="dienst"></param>
        /// <returns></returns>
        public static SolidColorBrush GetFarbeFromResources(this DienstTyp dienst)
        {
            var farbe = new SolidColorBrush();

            if (dienst == DienstTyp.None)
                return farbe;

            var key = "";
            switch (dienst)
            {
                case DienstTyp.Frühdienst:
                    key = "ColorFruehdienst";
                    break;
                case DienstTyp.SpätdienstEnde:
                    key = "ColorSpaetdienst";
                    break;
                case DienstTyp.AchtUhrDienst:
                    key = "ColorAchtUhrDienst";
                    break;
                case DienstTyp.FsjSpätdienst:
                    key = "ColorFsjSpaetDienst";
                    break;
                case DienstTyp.FsjFrühdienst:
                    key = "ColorFsjFruehDienst";
                    break;
                default:
                    return farbe;
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
