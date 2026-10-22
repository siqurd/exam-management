using Exam.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.Api.Data;

public sealed class ExamDbContext(DbContextOptions<ExamDbContext> options) : DbContext(options)
{
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<ExamResult> Exams => Set<ExamResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

        var lesson = modelBuilder.Entity<Lesson>();
        lesson.ToTable("Lessons", table =>
        {
            table.HasCheckConstraint("CK_Lessons_Code", "LEN([Code]) = 3");
            table.HasCheckConstraint("CK_Lessons_Class", "[Class] BETWEEN 1 AND 99");
        });
        lesson.HasKey(x => x.Code);
        lesson.Property(x => x.Code).HasColumnType("char(3)");
        lesson.Property(x => x.Name).HasColumnType("varchar(30)");
        lesson.Property(x => x.Class).HasConversion<decimal>().HasPrecision(2, 0);
        lesson.Property(x => x.TeacherFirstName).HasColumnType("varchar(20)");
        lesson.Property(x => x.TeacherLastName).HasColumnType("varchar(20)");

        var student = modelBuilder.Entity<Student>();
        student.ToTable("Students", table =>
        {
            table.HasCheckConstraint("CK_Students_Number", "[Number] BETWEEN 1 AND 99999");
            table.HasCheckConstraint("CK_Students_Class", "[Class] BETWEEN 1 AND 99");
        });
        student.HasKey(x => x.Number);
        student.Property(x => x.Number).HasConversion<decimal>().HasPrecision(5, 0).ValueGeneratedNever();
        student.Property(x => x.FirstName).HasColumnType("varchar(30)");
        student.Property(x => x.LastName).HasColumnType("varchar(30)");
        student.Property(x => x.Class).HasConversion<decimal>().HasPrecision(2, 0);

        var exam = modelBuilder.Entity<ExamResult>();
        exam.ToTable("Exams", table =>
            table.HasCheckConstraint("CK_Exams_Grade", "[Grade] BETWEEN 0 AND 9"));
        exam.HasKey(x => new { x.LessonCode, x.StudentNumber, x.ExamDate });
        exam.Property(x => x.LessonCode).HasColumnType("char(3)");
        exam.Property(x => x.StudentNumber).HasConversion<decimal>().HasPrecision(5, 0);
        exam.Property(x => x.ExamDate).HasColumnType("date");
        exam.Property(x => x.Grade).HasConversion<decimal>().HasPrecision(1, 0);
        exam.HasOne<Lesson>().WithMany().HasForeignKey(x => x.LessonCode).OnDelete(DeleteBehavior.Restrict);
        exam.HasOne<Student>().WithMany().HasForeignKey(x => x.StudentNumber).OnDelete(DeleteBehavior.Restrict);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(entity => entity.GetProperties())
                     .Where(property => property.ClrType == typeof(string)))
        {
            property.SetCollation("Latin1_General_100_CI_AS_SC_UTF8");
        }
    }
}
