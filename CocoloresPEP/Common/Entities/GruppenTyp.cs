using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{

    [Flags]
    public enum GruppenTyp
    {
        None = 1 << 0,
        Blau= 1 << 1,
        Gruen = 1 << 2,
        Rot = 1 << 3,
        Nest = 1 << 4,
    }
}
