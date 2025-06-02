namespace enrollmentSystem.Models
{
    public class Student
    {
        public string StudentId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string HomeAddress { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string YearLevel { get; set; }
        public string Program { get; set; }
    }

    public class AcademicYear
    {
        public int Id { get; set; }
        public string Year { get; set; }
    }

    public class AcademicProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

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