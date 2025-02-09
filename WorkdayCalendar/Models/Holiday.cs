using System.ComponentModel.DataAnnotations;

namespace WorkdayCalendar.Models
{
    public class Holiday
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; }
    }
}
