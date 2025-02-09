using System.ComponentModel.DataAnnotations;

namespace WorkdayCalendar.Models
{
    public class WorkdayCalculation
    {

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public double WorkingDays { get; set; }

        public List<Holiday>? Holidays { get; set; }

        public WorkingHours? WorkingHours { get; set; }

    }
}
