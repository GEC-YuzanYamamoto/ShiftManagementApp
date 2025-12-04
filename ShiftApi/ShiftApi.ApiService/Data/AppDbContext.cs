using Microsoft.EntityFrameworkCore;
using ShiftApi.ApiService.Models;

namespace ShiftApi.ApiService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<ShiftRequest> ShiftRequests { get; set; }
        public DbSet<ShiftSchedule> ShiftSchedules { get; set; }
    }
}
