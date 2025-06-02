using Microsoft.AspNetCore.Mvc;
using enrollmentSystem.Models;
using System.Collections.Generic;

namespace enrollmentSystem.Controllers
{
    public class StudentController : Controller
    {
        // Mock data for demonstration
        private static Student _currentStudent = new Student
        {
            StudentId = "2023-0001",
            LastName = "Doe",
            FirstName = "John",
            MiddleName = "Smith",
            HomeAddress = "123 Main St, Cityville",
            ContactNumber = "09123456789",
            Email = "john.doe@example.com",
            YearLevel = "1st Year",
            Program = "BS Computer Science"
        };

        private static List<AcademicYear> _academicYears = new List<AcademicYear>
        {
            new AcademicYear { Id = 1, Year = "2023-2024" },
            new AcademicYear { Id = 2, Year = "2024-2025" }
        };

        private static List<AcademicProgram> _programs = new List<AcademicProgram>
        {
            new AcademicProgram { Id = 1, Name = "BS Computer Science" },
            new AcademicProgram { Id = 2, Name = "BS Information Technology" },
            new AcademicProgram { Id = 3, Name = "BS Business Administration" }
        };
        
        private static List<Subject> _sampleSubjects = new List<Subject>
        {
            new Subject { Code = "CS101", Title = "Introduction to Programming",  Units = 3 },
            new Subject { Code = "CS102", Title = "Data Structures",  Units = 3 },
            new Subject { Code = "MATH101", Title = "College Algebra",  Units = 3 },
            new Subject { Code = "ENG101", Title = "Communication Arts",  Units = 3 },
            new Subject { Code = "CS201", Title = "Algorithms",  Units = 3 },
            new Subject { Code = "CS202", Title = "Database Systems",  Units = 3 },
            new Subject { Code = "PHYS101", Title = "Physics for CS",  Units = 3 }
        };

        private static List<Subject> _availableSubjects = new List<Subject>
        {
            // 1st Semester Subjects
            new Subject { Code = "CS101", Title = "Intro to Programming",  Units = 3, Semester = "1st Semester" },
            new Subject { Code = "MATH101", Title = "College Algebra",  Units = 3, Semester = "1st Semester" },
    
            // 2nd Semester Subjects
            new Subject { Code = "CS102", Title = "Data Structures",  Units = 3, Semester = "2nd Semester" },
            new Subject { Code = "PHYS101", Title = "Physics",  Units = 3, Semester = "2nd Semester" },
    
            // Summer Subjects
            new Subject { Code = "ENG101", Title = "Communication Arts",  Units = 3, Semester = "Summer" }
        };

        public StudentController()
        {
            // Constructor logic (optional)
        }

        public IActionResult StudentMenu()
        {
            return View();
        }

        public IActionResult Enrollment()
        {
            // Get student ID from session
            var studentId = HttpContext.Session.GetString("StudentID") ?? "123456789"; // Fallback for testing
    
            // Update your student model with the logged-in ID
            _currentStudent.StudentId = studentId;

            ViewBag.Student = _currentStudent;
            ViewBag.AcademicYears = _academicYears;
            ViewBag.Programs = _programs;
            return View();
        }

        [HttpGet]
        public IActionResult GetSubjects(string yearLevel, string program, string semester)
        {
            try
            {
                var filteredSubjects = _availableSubjects
                    .Where(s => s.Semester.Equals(semester, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Json(new {
                    success = true,
                    data = filteredSubjects
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    error = ex.Message
                });
            }
        }

        public IActionResult Schedule()
        {
            return View();
        }
        
        public IActionResult Profile()
        {
            return View();
        }
    }
}