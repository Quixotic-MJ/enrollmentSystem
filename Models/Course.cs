using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using enrollmentSystem.Controllers;

namespace enrollmentSystem.Models
{
    public class CourseCategory
    {
        [Key]
        [Column("ctg_code")]
        public string Ctg_Code { get; set; }

        [Column("ctg_name")]
        public string Ctg_Name { get; set; }

        [Column("ctg_description")]
        public string Ctg_Description { get; set; }
    }
    
    public class Section
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("curriculum_code")]
        public string CurriculumCode { get; set; }

        [ForeignKey("CurriculumCode")]
        public Curriculum Curriculum { get; set; }

        [Required]
        [Column("section_name")]
        public string SectionName { get; set; }

        [Required]
        [Column("semester")]
        public string Semester { get; set; }

        [Required]
        [Column("year_level")]
        public string YearLevel { get; set; }

        [Column("instructor")]
        public string Instructor { get; set; }
    }

    // In Models/Course.cs
    public class Course
    {
        [Key]
        [Column("crs_code")]
        public string Crs_Code { get; set; }
    
        [ForeignKey("CourseCategory")]
        [Column("ctg_code")]
        [Required(ErrorMessage = "Course category is required")]
        public string Ctg_Code { get; set; }
    
        // Remove the [Required] attribute from the navigation property
        public CourseCategory? CourseCategory { get; set; } // Made nullable
    
        [Column("crs_title")]
        [Required(ErrorMessage = "Course title is required")]
        public string Crs_Title { get; set; }

        [Column("preq_crs_code")]
        public string? Preq_Crs_Code { get; set; }

        [Column("crs_units")]
        [Range(1, 10, ErrorMessage = "Units must be between 1 and 10")]
        public int Crs_Units { get; set; }

        [Column("crs_lec")]
        [Range(0, 10, ErrorMessage = "Lecture hours must be between 0 and 10")]
        public int Crs_Lec { get; set; }

        [Column("crs_lab")]
        [Range(0, 10, ErrorMessage = "Lab hours must be between 0 and 10")]
        public int Crs_Lab { get; set; }
    }
    
    public class AddCourseViewModel
    {
        public List<CourseCategory> Categories { get; set; } = new List<CourseCategory>();
        public List<AdminController.CourseDropdownItem> Courses { get; set; } = new List<AdminController.CourseDropdownItem>();
        public Course NewCourse { get; set; } = new Course();
    }
    
    public class AcademicProgram  // Changed from Program to avoid conflict
    {
        [Key]
        [Column("program_code")]
        public string ProgramCode { get; set; }

        [Column("program_name")]
        public string ProgramName { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
    
    public class Schedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("section_id")]
        public string SectionId { get; set; }
        
        [Required]
        [Column("curriculum_code")]
        public string CurriculumCode { get; set; }

        [ForeignKey("CurriculumCode")]
        public Curriculum Curriculum { get; set; }

        [Required]
        [Column("course_code")]  // Add this mapping
        public string CourseCode { get; set; }

        [ForeignKey("CourseCode")]
        public Course Course { get; set; }

        [Required]
        [Column("section")]
        public string Section { get; set; }

        [Column("room")]
        public string Room { get; set; }

        [Column("instructor")]
        public string Instructor { get; set; }

        public ICollection<ScheduleSession> Sessions { get; set; } = new List<ScheduleSession>();
    }

    public class ScheduleSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public Schedule Schedule { get; set; }
        
        [Required]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
    
    public class Curriculum
    {
        [Key]
        public string CurriculumCode { get; set; }

        [Required]
        public string Program { get; set; }

        [Required]
        public string AcademicYear { get; set; }

        public ICollection<CurriculumCourse> Courses { get; set; } = new List<CurriculumCourse>();
    }

    public class CurriculumCourse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CurriculumCode { get; set; }

        [ForeignKey("CurriculumCode")]
        public Curriculum Curriculum { get; set; }

        [Required]
        public string CourseCode { get; set; }

        [ForeignKey("CourseCode")]
        public Course Course { get; set; }

        [Required]
        public int YearLevel { get; set; }

        [Required]
        public string Semester { get; set; }
    }
    
}