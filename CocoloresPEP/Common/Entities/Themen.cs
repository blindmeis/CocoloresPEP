using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public enum Themen
    {
        [Display(Name = @"")]
        None = 1 << 0,
        [Display(Name = @"Garten")]
        Garten = 1 << 1,
        [Display(Name = @"Musik")]
        Musik = 1 << 2,
        [Display(Name = @"Geschichten")]
        Geschichten = 1 << 3,
        [Display(Name = @"Bauen")]
        Bauen = 1 << 4,
        [Display(Name = @"Atelier")]
        Atelier = 1 << 5,
        [Display(Name = @"Theater")]
        Theater = 1 << 6,
        [Display(Name = @"Experimente")]
        Experimente = 1 << 7,
        [Display(Name = @"Hauswirtschaft")]
        Hauswirtschaft = 1 << 8,
    }
}
