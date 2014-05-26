using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Models
{
    public enum Schedule
    {
        [Display(Name = "Daily")]
        Daily = 1,

        [Display(Name = "Weekly")]
        Weekly = 2,

        [Display(Name = "Monthly")]
        Monthly = 3,
    }
}
