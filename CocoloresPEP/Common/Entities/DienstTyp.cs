using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
 [Flags]
    public enum DienstTyp
    {
        None = 1 << 0,
        [Display(Name = @"Frühdienst")]
        Frühdienst = 1 << 1,
        [Display(Name = @"Spätdienst")]
        SpätdienstEnde = 1 << 2,
        [Display(Name = @"08:00Uhr Dienst")]
        AchtUhrDienst = 1 << 3,
        [Display(Name = @"Kernzeitstart Dienst")]
        KernzeitStartDienst = 1 << 4,
        [Display(Name = @"09:00Uhr Dienst")]
        NeunUhrDienst = 1 << 5,
        [Display(Name = @"10.00Uhr Dienst")]
        ZehnUhrDienst = 1 << 6,
        [Display(Name = @"Kernzeitende Dienst")]
        KernzeitEndeDienst = 1 << 7,
        Frei = 1 << 8,
        FsjFrühdienst = 1 << 9,
        FsjSpätdienst = 1 << 10,
        Großteam = 1 << 11,
    }
}
