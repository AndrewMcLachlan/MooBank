using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Models;

public enum ScheduleFrequency
{
    [Display(Name = "Daily")]
    Daily = 1,

    [Display(Name = "Weekly")]
    Weekly = 2,

    [Display(Name = "Monthly")]
    Monthly = 3,
}
