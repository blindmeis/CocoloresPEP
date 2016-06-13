using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Services
{
    public class ArbeitswochenService
    {
        public Arbeitswoche CreateArbeitswoche(int jahr, int woche)
        {
            var aw =new Arbeitswoche(jahr, woche);
            
            var montag = DateTimeExtensions.FirstDateOfWeekIso8601(jahr, woche);

            aw.Arbeitstage.Add(new Arbeitstag(montag));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(1)));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(2)));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(3)));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(4)));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(5)));
            aw.Arbeitstage.Add(new Arbeitstag(montag.AddDays(6)));

            return aw;
        }
    }
}
