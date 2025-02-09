using System.ComponentModel.DataAnnotations;

namespace WorkdayCalendar.Models
{
    public class WorkingHours
    {

        [Required]
        public TimeSpan Start { get; set; }

        [Required]
        public TimeSpan End { get; set; }
    }
}