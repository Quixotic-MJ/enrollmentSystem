using Microsoft.AspNetCore.Mvc;
using enrollmentSystem.Models;
using System.Collections.Generic;
using enrollmentSystem.Data; // Added for AppDbContext
using Microsoft.EntityFrameworkCore; // Added for ToListAsync or ToList
using System.Linq; // Added for ToListAsync or ToList
using Microsoft.Extensions.Logging; // Added for logging

namespace enrollmentSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context; // Added DbContext field
        private readonly ILogger<StudentController> _logger; // Added logger field
        // Mock data for demonstration
        private static Student _currentStudent = new Student
        {
         
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

        public StudentController(AppDbContext context, ILogger<StudentController> logger) // Modified constructor for DI
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult StudentMenu()
        {
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

        public async Task<IActionResult> Enrollment() // Made method async
        {
            _logger.LogInformation("StudentController.Enrollment action started.");

            string studentId = HttpContext.Session.GetString("StudentID");
            Models.Student loggedInStudent = null;

            if (!string.IsNullOrEmpty(studentId))
            {
                _logger.LogInformation($"Attempting to fetch student data for StudentID: {studentId}");
                loggedInStudent = await _context.Student.FirstOrDefaultAsync(s => s.Stud_Id == studentId);
                if (loggedInStudent != null)
                {
                    _logger.LogInformation($"Student data found for {studentId}: {loggedInStudent.Stud_Fname} {loggedInStudent.Stud_Lname}");
                }
                else
                {
                    _logger.LogWarning($"No student data found in DB for StudentID: {studentId}.");
                }
            }
            else
            {
                _logger.LogWarning("StudentID not found in session. User might not be logged in or session expired.");
                // Optionally, redirect to login or show an error page
                // return RedirectToAction("Login", "Auth");
            }
            ViewBag.Student = loggedInStudent; // Assign fetched student (can be null)

            // Fetch distinct academic years from Curricula
            try
            {
                _logger.LogInformation("Attempting to fetch academic years from curricula.");
                var distinctAcademicYearStrings = await _context.Curricula
                    .Select(c => c.AcademicYear)
                    .Distinct()
                    .OrderBy(ay => ay) // Optional: order them
                    .ToListAsync();

                // Convert to List<AcademicYear> model if needed by the view, or adjust view to use strings
                // For now, assuming the view can adapt or we might need to create AcademicYear objects
                // If the view expects { Id, Year }, we need to map these strings.
                // Let's create AcademicYear objects for consistency with potential existing view logic.
                var dynamicAcademicYears = distinctAcademicYearStrings.Select((yearString, index) => 
                    new AcademicYear { Id = index + 1, Year = yearString } // Assign a dummy Id for now
                ).ToList();

                ViewBag.AcademicYears = dynamicAcademicYears;
                _logger.LogInformation($"Fetched {dynamicAcademicYears.Count} distinct academic years from curricula.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching academic years from curricula.");
                ViewBag.AcademicYears = new List<AcademicYear>(); // Ensure ViewBag is not null
            }

            // Initialize ViewBag.Programs as empty; will be populated by JS after academic year selection.
            _logger.LogInformation("Initializing ViewBag.Programs as an empty list. It will be populated dynamically.");
            ViewBag.Programs = new List<AcademicProgram>();

            _logger.LogInformation("StudentController.Enrollment action finished.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProgramsForAcademicYear(string academicYear)
        {
            _logger.LogInformation($"GetProgramsForAcademicYear called with academicYear: {academicYear}");
            if (string.IsNullOrEmpty(academicYear))
            {
                _logger.LogWarning("AcademicYear parameter is null or empty.");
                return Json(new List<AcademicProgram>()); // Return empty list or bad request
            }

            try
            {
                var programCodesInCurricula = await _context.Curricula
                    .Where(c => c.AcademicYear == academicYear)
                    .Select(c => c.Program) // Assuming Curriculum.Program stores the ProgramCode
                    .Distinct()
                    .ToListAsync();

                if (!programCodesInCurricula.Any())
                {
                    _logger.LogInformation($"No curricula found for academic year {academicYear}, returning empty list of programs.");
                    return Json(new List<AcademicProgram>());
                }

                var programs = await _context.Programs
                    .Where(p => programCodesInCurricula.Contains(p.ProgramCode))
                    .OrderBy(p => p.ProgramName)
                    .ToListAsync();
                
                _logger.LogInformation($"Found {programs.Count} programs for academic year {academicYear}.");
                return Json(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching programs for academic year {academicYear}.");
                return StatusCode(500, "Internal server error fetching programs.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableCourses()
        {
            _logger.LogInformation("GetAvailableCourses called.");
            try
            {
                var courses = await _context.Courses
                    .OrderBy(c => c.Crs_Code) // Optional: Order by course code
                    .Select(c => new { c.Crs_Code, c.Crs_Title, c.Crs_Units }) // Select only needed fields
                    .ToListAsync();
                
                _logger.LogInformation($"Found {courses.Count} available courses.");
                return Json(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available courses.");
                return StatusCode(500, "Internal server error fetching courses.");
            }
        }
    }
}