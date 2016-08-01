using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{

    [Flags]
    public enum GruppenTyp
    {
        [Display(Name = @" ")]
        None = 0,
        [Display(Name = @"Gelb")]
        Gelb = 1 << 1,
        [Display(Name = @"Grün")]
        Gruen = 1 << 2,
        [Display(Name = @"Rot")]
        Rot = 1 << 3,
        [Display(Name = @"Nest")]
        Nest = 1 << 4,
    }
}
