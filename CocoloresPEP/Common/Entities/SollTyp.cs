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
        None = 1 << 0,
        Blau= 1 << 1,
        Gruen = 1 << 2,
        Rot = 1 << 3,
        Nest = 1 << 4,
        Frühdienst = 1 << 5,
        Spätdienst = 1 << 6,
        AchtUhrDienst = 1<< 7,
        AchtUhr30Dienst = 1<< 8,
        NeunUhrDienst = 1<< 9,
        ZehnUhrDienst = 1<< 10,
    }
}
