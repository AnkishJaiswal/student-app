using Microsoft.EntityFrameworkCore;
using student_app.Model;

namespace student_app.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Salary> Salaries { get; set; }

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

            modelBuilder.Entity<Salary>()
                .Property(s => s.BasicSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Salary>()
                .Property(s => s.Allowances)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Salary>()
                .Property(s => s.Deductions)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Salary>()
                .Property(s => s.NetSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Salary>()
                .HasOne(s => s.Teacher)
                .WithMany(t => t.Salaries)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Salary>()
                .HasIndex(s => new { s.TeacherId, s.SalaryMonth, s.SalaryYear })
                .IsUnique();
        }
    }
}
