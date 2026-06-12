using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data;

public class SchoolContext : DbContext
{
    public SchoolContext(DbContextOptions<SchoolContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<OfficeAssignment> OfficeAssignments => Set<OfficeAssignment>();
    public DbSet<CourseAssignment> CourseAssignments => Set<CourseAssignment>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>().ToTable("Course");
        modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
        modelBuilder.Entity<Department>().ToTable("Department");
        modelBuilder.Entity<OfficeAssignment>().ToTable("OfficeAssignment");
        modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment");
        modelBuilder.Entity<Notification>().ToTable("Notification");

        modelBuilder.Entity<Person>()
            .ToTable("Person")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Student>("Student")
            .HasValue<Instructor>("Instructor");

        modelBuilder.Entity<CourseAssignment>()
            .HasKey(c => new { c.CourseID, c.InstructorID });

        modelBuilder.Entity<CourseAssignment>()
            .HasOne(m => m.Course)
            .WithMany(t => t.CourseAssignments)
            .HasForeignKey(m => m.CourseID);

        modelBuilder.Entity<CourseAssignment>()
            .HasOne(m => m.Instructor)
            .WithMany(t => t.CourseAssignments)
            .HasForeignKey(m => m.InstructorID);

        modelBuilder.Entity<Instructor>()
            .HasOne(s => s.OfficeAssignment)
            .WithOne(ad => ad.Instructor)
            .HasForeignKey<OfficeAssignment>(ad => ad.InstructorID);
    }
}
