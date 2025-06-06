using Microsoft.EntityFrameworkCore;
using enrollmentSystem.Models;

namespace enrollmentSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<Admin> Admin { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
        
        public DbSet<Curriculum> Curricula { get; set; }
        public DbSet<CurriculumCourse> CurriculumCourses { get; set; }
        
        public DbSet<AcademicProgram> Programs { get; set; }  
        
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleSession> ScheduleSessions { get; set; }
        
        public DbSet<Section> Sections { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Admin entity
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");
                entity.Property(e => e.Admin_Id).HasColumnName("admin_id");
                entity.Property(e => e.Admin_Lname).HasColumnName("admin_lname");
                entity.Property(e => e.Admin_Fname).HasColumnName("admin_fname");
                entity.Property(e => e.Admin_Mname).HasColumnName("admin_mname");
                entity.Property(e => e.Admin_DOB)
                    .HasColumnName("admin_dob")
                    .HasColumnType("date")
                    .HasConversion(
                        d => d.ToDateTime(TimeOnly.MinValue),
                        d => DateOnly.FromDateTime(d));
                entity.Property(e => e.Admin_Contact).HasColumnName("admin_contact");
                entity.Property(e => e.Admin_Email).HasColumnName("admin_email");
                entity.Property(e => e.Admin_Address).HasColumnName("admin_address");
                entity.Property(e => e.Admin_Password).HasColumnName("admin_password");
            });
            
            // Configure CourseCategory
            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.ToTable("course_categories"); // Ensure this matches your actual table name
                entity.Property(e => e.Ctg_Code).HasColumnName("ctg_code");
                entity.Property(e => e.Ctg_Name).HasColumnName("ctg_name");
                entity.Property(e => e.Ctg_Description).HasColumnName("ctg_description");
            });
            
            // Configure Section entity
            modelBuilder.Entity<Section>(entity =>
            {
                entity.ToTable("section");
                entity.Property(e => e.Id)
                    .HasColumnName("id");  // Maps to 'id' column in section table

                entity.Property(e => e.CurriculumCode)
                    .HasColumnName("curriculum_code");
    
                entity.Property(e => e.SectionName)
                    .HasColumnName("section_name");
    
                entity.Property(e => e.Semester)
                    .HasColumnName("semester");
    
                entity.Property(e => e.YearLevel)
                    .HasColumnName("year_level");
    
                entity.Property(e => e.Instructor)
                    .HasColumnName("instructor");

                // Relationship configuration
                entity.HasMany(s => s.Schedules)
                    .WithOne(s => s.Section)
                    .HasForeignKey(s => s.SectionId)  // Refers to Schedule.SectionId property
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("schedules");
    
                entity.Property(e => e.Id)
                    .HasColumnName("id");
    
                // THIS IS THE CRITICAL FIX:
                entity.Property(e => e.SectionId)
                    .HasColumnName("section_id");  // Explicitly maps to 'section_id' column in schedules table
    
                entity.Property(e => e.CurriculumCode)
                    .HasColumnName("curriculum_code");
    
                entity.Property(e => e.CourseCode)
                    .HasColumnName("course_code");
    
                entity.Property(e => e.Room)
                    .HasColumnName("room");
    
                entity.Property(e => e.Instructor)
                    .HasColumnName("instructor");
            });
            
            modelBuilder.Entity<ScheduleSession>(entity =>
            {
                entity.ToTable("schedulesession");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
            });
            
            // Configure Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("courses");
                entity.Property(e => e.Crs_Code).HasColumnName("crs_code");
                entity.Property(e => e.Ctg_Code).HasColumnName("ctg_code");
                entity.Property(e => e.Crs_Title).HasColumnName("crs_title");
                entity.Property(e => e.Preq_Crs_Code).HasColumnName("preq_crs_code");
                entity.Property(e => e.Crs_Units).HasColumnName("crs_units");
                entity.Property(e => e.Crs_Lec).HasColumnName("crs_lec");
                entity.Property(e => e.Crs_Lab).HasColumnName("crs_lab");
            });
            
            modelBuilder.Entity<Curriculum>(entity =>
            {
                entity.ToTable("curricula");
                entity.Property(e => e.CurriculumCode).HasColumnName("curriculum_code");
                entity.Property(e => e.Program).HasColumnName("program");
                entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            });

            modelBuilder.Entity<CurriculumCourse>(entity =>
            {
                entity.ToTable("curriculum_courses");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CurriculumCode).HasColumnName("curriculum_code");
                entity.Property(e => e.CourseCode).HasColumnName("course_code");
                entity.Property(e => e.YearLevel).HasColumnName("year_level");
                entity.Property(e => e.Semester).HasColumnName("semester");
            });
            
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");
                entity.Property(e => e.Stud_Id).HasColumnName("stud_id");
                entity.Property(e => e.Stud_Lname).HasColumnName("stud_lname");
                entity.Property(e => e.Stud_Fname).HasColumnName("stud_fname");
                entity.Property(e => e.Stud_Mname).HasColumnName("stud_mname");
                entity.Property(e => e.Stud_Contact).HasColumnName("stud_contact");
                entity.Property(e => e.Stud_Email).HasColumnName("stud_email");
                entity.Property(e => e.Stud_Address).HasColumnName("stud_address");
                entity.Property(e => e.Stud_Password).HasColumnName("stud_password");
                
                entity.Property(e => e.Stud_DOB)
                    .HasColumnName("stud_dob")
                    .HasColumnType("date")
                    .HasConversion(
                        d => d.ToDateTime(TimeOnly.MinValue),
                        d => DateOnly.FromDateTime(d));
            });

            // Configure AcademicProgram if it exists
            modelBuilder.Entity<AcademicProgram>(entity =>
            {
                entity.ToTable("academic_programs");
                entity.Property(e => e.ProgramCode).HasColumnName("program_code");
                entity.Property(e => e.ProgramName).HasColumnName("program_name");
                entity.Property(e => e.Description).HasColumnName("description");
            });
        }
    }
}