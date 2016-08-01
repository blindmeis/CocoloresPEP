using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP.Services
{
    public static class PlanzeitenValidationService
    {
        public static AuswertungViewmodel ValidateArbeitswoche(this Arbeitswoche woche)
        {
            //frei nicht weniger als tagessatz
            //Nicht weniger als 4h
            //1 Frühdienst
            //1 8Uhr Dienst
            //2 Spätdienst
            //16Uhr Dienst

            //Kernzeit 1MA 8:30-15:30
            //Kernzeit 2MA 9:00-13:00

            var result = new AuswertungViewmodel();

            foreach (var arbeitstag in woche.Arbeitstage)
            {
                //Samstag und Sontag ignorieren bei Planung
                if (arbeitstag.Datum.DayOfWeek == DayOfWeek.Saturday || arbeitstag.Datum.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var aa = new ArbeitstagAuswertung { Wochentag = arbeitstag.Wochentag };

                #region Frühdienst

                var obGenauEinFrühdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.Frühdienst && x.Zeitraum.Start == x.Arbeitstag.Frühdienst).ToList();
                if (obGenauEinFrühdienst.Count != 1)
                {
                    var keinMehr = obGenauEinFrühdienst.Count == 0 ? "Kein" : "Mehr als ein";
                    var msg = $"{keinMehr} {DienstTyp.Frühdienst.GetDisplayname()} geplant für {arbeitstag.Frühdienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion

                #region Spätdienst

                var obGenauZweiSpätdienste = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.SpätdienstEnde && x.Zeitraum.End == x.Arbeitstag.SpätdienstEnde).ToList();
                if (obGenauZweiSpätdienste.Count != 2)
                {
                    var keinMehr = obGenauEinFrühdienst.Count == 0 ? "Kein" : obGenauEinFrühdienst.Count == 1 ? "Nur ein" : "Mehr als ein";
                    var msg = $"{keinMehr} {DienstTyp.SpätdienstEnde.GetDisplayname()} geplant bis {arbeitstag.SpätdienstEnde.ToString("HH:mm")}Uhr.";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion

                #region 8Uhr Dienst

                var obGenauEin8Uhrdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.AchtUhrDienst && x.Zeitraum.Start == x.Arbeitstag.AchtUhrDienst).ToList();
                if (obGenauEin8Uhrdienst.Count == 0)
                {
                    var msg = $"Kein {DienstTyp.AchtUhrDienst.GetDisplayname()} geplant für {arbeitstag.AchtUhrDienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion

                #region 16Uhr Dienst

                var obGenauEin16Uhrdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.SechszehnUhrDienst && x.Zeitraum.End == x.Arbeitstag.SechzehnUhrDienst).ToList();
                if (!arbeitstag.HasGrossteam && obGenauEin16Uhrdienst.Count == 0)
                {
                    var msg = $"Kein {DienstTyp.SechszehnUhrDienst.GetDisplayname()} geplant bis {arbeitstag.SechzehnUhrDienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion

                #region Kernzeit
                var gruppen =
                            arbeitstag.Planzeiten.Where(x => x.Gruppe != GruppenTyp.None)
                            .GroupBy(x => x.Gruppe)
                            .ToList();

                foreach (var gruppe in gruppen)
                {
                    if (!arbeitstag.CheckKernzeitAbgedeckt(gruppe.Key))
                    {
                        var zeitraum = $"{arbeitstag.KernzeitBasisRange.Start.ToString("HH:mm")}-{arbeitstag.KernzeitBasisRange.End.ToString("HH:mm")}";
                        var msg = $"Gruppe: {gruppe.Key.GetDisplayname()} Kernzeit  ({zeitraum}) nicht abgedeckt";
                        var v = new ValidationMessage() { Message = msg };
                        aa.Messages.Add(v);


                    }
                    else if (!arbeitstag.CheckKernzeitDoppelBesetzungAbgedeckt(gruppe.Key))
                    {
                        var doppelzeitraum = $"{arbeitstag.KernzeitDoppelBesetzungRange.Start.ToString("HH:mm")}-{arbeitstag.KernzeitDoppelBesetzungRange.End.ToString("HH:mm")}";
                        var msgDoppel = $"Gruppe: {gruppe.Key.GetDisplayname()} Doppelbesetzung ({doppelzeitraum}) nicht abgedeckt.";
                        var vDoppel = new ValidationMessage() { Message = msgDoppel };
                        aa.Messages.Add(vDoppel);
                    }
                }
                #endregion

                #region Check Frei Tagessollsatz
                var wenigerZeitAlsFreiTagessatz =
                            arbeitstag.Planzeiten.Where(
                                x => x.Dienst == DienstTyp.Frei && x.GetArbeitsminutenAmKindOhnePause() < x.ErledigtDurch.TagesSollMinuten).ToList();

                foreach (var planItem in wenigerZeitAlsFreiTagessatz)
                {
                    var planstunden = (planItem.GetArbeitsminutenAmKindOhnePause() / 60).ToString("#.##");
                    var msg = $"{planItem.ErledigtDurch.Name}: Dienst Frei geplant mit {planItem.GetArbeitsminutenAmKindOhnePause()}Minuten ({planstunden}h). Tagessatz: {planItem.ErledigtDurch.TagesSollMinuten}Minuten";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion

                #region Check Mindestarbeitszeit
                var mindestArbeitszeiten =
                            arbeitstag.Planzeiten.Where(
                                x => x.Dienst != DienstTyp.Frei && !x.Zeitraum.Duration.IsMindestzeitAbgedeckt()).ToList();

                foreach (var planItem in mindestArbeitszeiten)
                {
                    var planstunden = (planItem.GetArbeitsminutenAmKindOhnePause() / 60).ToString("#.##");
                    var msg = $"{planItem.ErledigtDurch?.Name}: Mindestarbeitzeit nicht abgedeckt {planItem.GetArbeitsminutenAmKindOhnePause()}Minuten ({planstunden}h).";
                    var v = new ValidationMessage() { Message = msg };
                    aa.Messages.Add(v);
                }
                #endregion



                if (aa.Messages.Count > 0)
                    result.Auswertungen.Add(aa);
                else
                {
                    aa.Messages.Add(new ValidationMessage() { Message = "Alles Prima :)" });
                    result.Auswertungen.Add(aa);
                }
            }

            return result;
        }


    }
}
