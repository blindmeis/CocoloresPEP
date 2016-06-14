using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Extensions
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// Gibt alle(rekursiv) exception.Message als einen string mit Leerzeile getrennt zurück
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetAllErrorMessages(this Exception error)
        {
            var errmsg = "";
            var ex = error;
            while (ex != null)
            {
                errmsg += ex.InnerException != null ? ex.Message + Environment.NewLine : ex.Message;
                ex = ex.InnerException;
            }

            return errmsg;
        }
    }
}
