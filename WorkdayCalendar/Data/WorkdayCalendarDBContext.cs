using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.Data
{
    public class WorkdayCalendarDBContext: DbContext
    {
        public DbSet<Holiday> Holiday { get; set; }

        public WorkdayCalendarDBContext(DbContextOptions<WorkdayCalendarDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Holiday>().HasKey(e => e.Id);
        }
    }
}
