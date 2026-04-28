using Microsoft.EntityFrameworkCore;
using student_app.Model;

namespace student_app.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
        }
    }
}
