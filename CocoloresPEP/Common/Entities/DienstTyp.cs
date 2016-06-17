using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
 [Flags]
    public enum DienstTyp
    {
        None = 1 << 0,
        Frühdienst = 1 << 1,
        SpätdienstEnde = 1 << 2,
        AchtUhrDienst = 1 << 3,
        AchtUhr30Dienst = 1 << 4,
        NeunUhrDienst = 1 << 5,
        ZehnUhrDienst = 1 << 6,
        KernzeitEndeDienst = 1 << 6,
    }
}
