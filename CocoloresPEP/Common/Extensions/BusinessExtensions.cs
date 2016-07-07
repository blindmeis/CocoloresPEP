using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Common.Extensions
{
    public static class BusinessExtensions
    {
        public static int GetAbzurechnendeZeitInMinmuten(this PlanItem planzeit)
        {
            if (planzeit.HatGrossteam)
            {
                var gtMinuten = planzeit.Arbeitstag.Grossteam.Duration.GetArbeitsminutenOhnePause();
                var arbeit = planzeit.Zeitraum.Duration.GetArbeitsminutenOhnePause();
                var overlap = 0;

                if(planzeit.Zeitraum.IntersectsWith(planzeit.Arbeitstag.Grossteam))
                    overlap = (int)planzeit.Zeitraum.GetIntersection(planzeit.Arbeitstag.Grossteam).Duration.TotalMinutes;

                return arbeit + gtMinuten - overlap;
            }
            else
            {
                return planzeit.Zeitraum.Duration.GetArbeitsminutenOhnePause();
            }
        }

        public static int GetArbeitsminutenOhnePause(this TimeSpan dauerMitPause)
        {
            var minuten = (int)dauerMitPause.TotalMinutes;

            if (minuten > 360)
                return minuten - 30;

            return minuten;
        }

        public static bool NeedPause(this TimeSpan dauerMitPause)
        {
            var minuten = (int)dauerMitPause.TotalMinutes;

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

            var pause = planzeit.Zeitraum.Duration.NeedPause() ? "P" : " ";
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

        public static bool IstRandDienst(this DienstTyp dienst)
        {
            return (dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                   || (dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
                   || (dienst & DienstTyp.SechszehnUhrDienst) == DienstTyp.SechszehnUhrDienst
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
