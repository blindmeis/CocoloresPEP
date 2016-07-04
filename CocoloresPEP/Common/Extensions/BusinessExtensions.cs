using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Extensions
{
    public static class BusinessExtensions
    {
        public static int GetArbeitsminutenOhnePause(this TimeSpan dauerMitPause)
        {
            var minuten = (int)dauerMitPause.TotalMinutes;

            if (minuten > 360)
                return minuten - 30;

            return minuten;
        }
    }
}
