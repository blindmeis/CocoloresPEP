using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    [Flags]
    public enum SollTyp
    {
        Blau= 1 << 0,
        Gruen = 1 << 1,
        Rot = 1 << 2,
        Nest = 1 << 3,
        Frühdienst = 1 << 4,
        Spätdienst = 1 << 5,
        AchtUhrDienst = 1<< 6,
        AchtUhr30Dienst = 1<< 7,
        NeunUhrDienst = 1<< 8,
        ZehnUhrDienst = 1<< 9,
    }
}
