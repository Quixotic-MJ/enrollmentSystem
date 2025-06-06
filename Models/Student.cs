using System.ComponentModel.DataAnnotations;

namespace enrollmentSystem.Models
{
    public class Student
    {
        [Key]
        [Required(ErrorMessage = "Student ID is required")]
        public string Stud_Id { get; set; }  
        
        [Required(ErrorMessage = "Last name is required")]
        public string Stud_Lname { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public string Stud_Fname { get; set; }
        
        public string? Stud_Mname { get; set; } 
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateOnly Stud_DOB { get; set; }
        
        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Stud_Contact { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Stud_Email { get; set; }
        
        [Required(ErrorMessage = "Address is required")]
        public string Stud_Address { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Stud_Password { get; set; }
        
        
    }

    public class studentInfo
    {
        public string YearLevel { get; set; }
        public string Program { get; set; }
    }
    

    public class AcademicYear
    {
        public int Id { get; set; }
        public string Year { get; set; }
    }

    // public class AcademicProgram
    // {
    //     public int Id { get; set; }
    //     public string Name { get; set; }
    // }

    public class Subject
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int Units { get; set; }
        
        public string Semester { get; set; } = string.Empty; // Add this
    }
    
    // Add these model classes at the bottom of the controller file
    public class EnrollmentData
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string StudentStatus { get; set; }
        public string EnrollmentStatus { get; set; }
        public string YearLevel { get; set; }
        public string Program { get; set; }
        public List<Subject> Subjects { get; set; }
    }

    public class Enrollment
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string StudentStatus { get; set; }
        public string EnrollmentStatus { get; set; }
        public string YearLevel { get; set; }
        public string Program { get; set; }
        public List<Subject> Subjects { get; set; }
        public bool IsApproved { get; set; }
    }
}