using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Services
{
    public class SollzeitenService
    {
        public void AddSollItem(Arbeitstag tag,string displayname, DateTime startzeit, short quarterticks, SollTyp typ)
        {
            tag.SollItems.Add(new SollItem(displayname, startzeit,quarterticks,typ));
        }
      
    }
}
