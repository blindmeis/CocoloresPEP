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
                var tmp = new List<ValidierungsItem>();

                #region Frühdienst

                var obGenauEinFrühdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.Frühdienst && x.Zeitraum.Start == x.Arbeitstag.Frühdienst).ToList();
                if (obGenauEinFrühdienst.Count != 1)
                {
                    var keinMehr = obGenauEinFrühdienst.Count == 0 ? "Kein" : "Mehr als ein";
                    var msg = $"{keinMehr} {DienstTyp.Frühdienst.GetDisplayname()} geplant für {arbeitstag.Frühdienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidierungsItem();
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
                }
                #endregion

                #region Spätdienst

                var obGenauZweiSpätdienste = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.SpätdienstEnde && x.Zeitraum.End == x.Arbeitstag.SpätdienstEnde).ToList();
                if (obGenauZweiSpätdienste.Count != 2)
                {
                    var keinMehr = obGenauEinFrühdienst.Count == 0 ? "Kein" : obGenauEinFrühdienst.Count == 1 ? "Nur ein" : "Mehr als ein";
                    var msg = $"{keinMehr} {DienstTyp.SpätdienstEnde.GetDisplayname()} geplant bis {arbeitstag.SpätdienstEnde.ToString("HH:mm")}Uhr.";
                    var v = new ValidierungsItem();
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
                }
                #endregion

                #region 8Uhr Dienst

                var obGenauEin8Uhrdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.AchtUhrDienst && x.Zeitraum.Start == x.Arbeitstag.AchtUhrDienst).ToList();
                if (obGenauEin8Uhrdienst.Count == 0)
                {
                    var msg = $"Kein {DienstTyp.AchtUhrDienst.GetDisplayname()} geplant für {arbeitstag.AchtUhrDienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidierungsItem();
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
                }
                #endregion

                #region 16Uhr Dienst

                var obGenauEin16Uhrdienst = arbeitstag.Planzeiten.Where(x => x.Dienst == DienstTyp.SechszehnUhrDienst && x.Zeitraum.End == x.Arbeitstag.SechzehnUhrDienst).ToList();
                if (!arbeitstag.HasGrossteam && obGenauEin16Uhrdienst.Count == 0)
                {
                    var msg = $"Kein {DienstTyp.SechszehnUhrDienst.GetDisplayname()} geplant bis {arbeitstag.SechzehnUhrDienst.ToString("HH:mm")}Uhr.";
                    var v = new ValidierungsItem();
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
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
                        var msg = $"Gruppe: {gruppe.Key.GetDisplayname()} Kernzeit nicht abgedeckt";
                        var v = new ValidierungsItem();
                        v.Messages.Add(new ValidationMessage() { Message = msg });
                        tmp.Add(v);


                    }
                    else if (!arbeitstag.CheckKernzeitDoppelBesetzungAbgedeckt(gruppe.Key))
                    {
                        var msgDoppel = $"Gruppe: {gruppe.Key.GetDisplayname()} Doppelbesetzung in Kernzeit nicht abgedeckt.";
                        var vDoppel = new ValidierungsItem();
                        vDoppel.Messages.Add(new ValidationMessage() { Message = msgDoppel });
                        tmp.Add(vDoppel);
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
                    var msg =
                        $"Dienst Frei geplant mit {planItem.GetArbeitsminutenAmKindOhnePause()}Minuten ({planstunden}h). Tagessatz: {planItem.ErledigtDurch.TagesSollMinuten}Minuten";
                    var v = new ValidierungsItem() { Displayname = planItem.ErledigtDurch.Name };
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
                }
                #endregion

                #region Check Mindestarbeitszeit
                var mindestArbeitszeiten =
                            arbeitstag.Planzeiten.Where(
                                x => x.Dienst != DienstTyp.Frei && !x.Zeitraum.Duration.IsMindestzeitAbgedeckt()).ToList();

                foreach (var planItem in mindestArbeitszeiten)
                {
                    var planstunden = (planItem.GetArbeitsminutenAmKindOhnePause() / 60).ToString("#.##");
                    var msg =
                        $"Mindestarbeitzeit nicht abgedeckt {planItem.GetArbeitsminutenAmKindOhnePause()}Minuten ({planstunden}h).";
                    var v = new ValidierungsItem() { Displayname = planItem.ErledigtDurch?.Name };
                    v.Messages.Add(new ValidationMessage() { Message = msg });
                    tmp.Add(v);
                }
                #endregion

                foreach (var item in tmp.GroupBy(x => x.Displayname).ToList())
                {
                    aa.ValidierungsItems.Add(new ValidierungsItem()
                    {
                        Displayname = item.Key,
                        Messages = new List<ValidationMessage>(item.SelectMany(x => x.Messages))
                    });
                }

                if (aa.ValidierungsItems.Count > 0)
                    result.Auswertungen.Add(aa);
                else
                {
                    var io = new ValidierungsItem();
                    io.Messages.Add(new ValidationMessage() { Message = "Alles Prima :)" });
                    aa.ValidierungsItems.Add(io);
                    result.Auswertungen.Add(aa);
                }
            }

            return result;
        }


    }
}
