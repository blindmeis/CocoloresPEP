using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungstagViewmodel : ViewmodelBase
    {
        public DateTime Datum { get; set; }
        public IEnumerable<PlanItem> Planzeiten { get; set; }

        public string IstZeitenInfo
        {
            get
            {
                var sb = (from zeiten in Planzeiten
                          let endzeit = zeiten.Startzeit.AddMinutes(15*zeiten.AllTicks)
                          select $"{zeiten.Startzeit.ToString("HH:mm")}-{endzeit.ToString("HH:mm")}"
                          ).ToList();

                return string.Join(Environment.NewLine, sb);
            }
        }

        public SollTyp EingeteiltSollTyp
        {
            get
            {
                var s = SollTyp.None;
                foreach (var planItem in Planzeiten)
                {
                    if((s & planItem.Typ)==planItem.Typ)
                        continue;

                    s |= planItem.Typ;
                }

                return s;
            }
        }
    }
}
