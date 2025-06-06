using enrollmentSystem.Data;
using enrollmentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace enrollmentSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        #region Section Management
        [HttpGet]
        public async Task<IActionResult> GetSections(string curriculumCode, string semester, string yearLevel)
        {
            var sections = await _context.Sections
                .Where(s => s.CurriculumCode == curriculumCode &&
                           (string.IsNullOrEmpty(semester) || s.Semester == semester) &&
                           (string.IsNullOrEmpty(yearLevel) || s.YearLevel == yearLevel))
                .Join(_context.Curricula,
                    s => s.CurriculumCode,
                    c => c.CurriculumCode,
                    (s, c) => new {
                        id = s.Id,
                        curriculumCode = s.CurriculumCode,
                        programName = c.Program,
                        section = s.SectionName,
                        semester = s.Semester,
                        yearLevel = s.YearLevel,
                        instructor = s.Instructor
                    })
                .ToListAsync();

            return Json(sections);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] SectionCreateModel model)
        {
            try
            {
                var section = new Section
                {
                    CurriculumCode = model.CurriculumCode,
                    SectionName = model.Section,
                    Semester = model.Semester,
                    YearLevel = model.YearLevel,
                    Instructor = "To be assigned"
                };

                _context.Sections.Add(section);
                await _context.SaveChangesAsync();

                var curriculum = await _context.Curricula.FindAsync(section.CurriculumCode);

                return Json(new { 
                    success = true,
                    section = new {
                        id = section.Id,
                        curriculumCode = section.CurriculumCode,
                        programName = curriculum?.Program,
                        section = section.SectionName,
                        semester = section.Semester,
                        yearLevel = section.YearLevel,
                        instructor = section.Instructor
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] SectionCreateModel model)
        {
            try
            {
                var section = await _context.Sections.FindAsync(id);
                if (section == null)
                {
                    return Json(new { success = false, message = "Section not found" });
                }

                section.SectionName = model.Section;
                section.Semester = model.Semester;
                section.YearLevel = model.YearLevel;

                await _context.SaveChangesAsync();

                var curriculum = await _context.Curricula.FindAsync(section.CurriculumCode);

                return Json(new { 
                    success = true,
                    section = new {
                        id = section.Id,
                        curriculumCode = section.CurriculumCode,
                        programName = curriculum?.Program,
                        section = section.SectionName,
                        semester = section.Semester,
                        yearLevel = section.YearLevel,
                        instructor = section.Instructor
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                var section = await _context.Sections.FindAsync(id);
                if (section == null)
                {
                    return Json(new { success = false, message = "Section not found" });
                }

                // First delete all schedules for this section - comparing int to int
                var schedules = await _context.Schedules
                    .Where(s => s.SectionId == id.ToString()) // Convert int to string if SectionId is string
                    .ToListAsync();

                if (schedules.Any())
                {
                    _context.Schedules.RemoveRange(schedules);
                }

                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Schedule Management
        [HttpGet]
        public async Task<IActionResult> GetSchedules(int sectionId) // Changed parameter type from string to int
        {
            var schedules = await _context.Schedules
                .Where(s => s.SectionId == sectionId.ToString()) // Convert int to string for comparison
                .Include(s => s.Sessions)
                .Include(s => s.Course)
                .Select(s => new {
                    id = s.Id, 
                    scheduleId = s.Id, 
                    curriculumCode = s.CurriculumCode,
                    courseCode = s.CourseCode,
                    courseTitle = s.Course.Crs_Title,
                    sectionId = s.SectionId,
                    room = s.Room,
                    instructor = s.Instructor,
                    sessions = s.Sessions.Select(ss => new {
                        id = ss.Id,
                        dayOfWeek = ss.DayOfWeek,
                        startTime = ss.StartTime.ToString(@"hh\:mm"),
                        endTime = ss.EndTime.ToString(@"hh\:mm")
                    })
                })
                .ToListAsync();

            // Flatten the sessions for easier display in the table
            var result = new List<dynamic>();
            foreach (var schedule in schedules)
            {
                foreach (var session in schedule.sessions)
                {
                    result.Add(new {
                        id = session.id,
                        scheduleId = schedule.id,
                        curriculumCode = schedule.curriculumCode,
                        courseCode = schedule.courseCode,
                        courseTitle = schedule.courseTitle,
                        sectionId = schedule.sectionId,
                        dayOfWeek = session.dayOfWeek,
                        startTime = session.startTime,
                        endTime = session.endTime,
                        room = schedule.room,
                        instructor = schedule.instructor
                    });
                }
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleCreateModel model)
        {
            try
            {
                // First, check if a schedule already exists for this course and section
                var existingSchedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => 
                        s.SectionId == model.SectionId && 
                        s.CourseCode == model.CourseCode);

                if (existingSchedule == null)
                {
                    // Create new schedule
                    var schedule = new Schedule
                    {
                        CurriculumCode = model.CurriculumCode,
                        CourseCode = model.CourseCode,
                        SectionId = model.SectionId,
                        Room = model.Room,
                        Instructor = model.Instructor
                    };

                    _context.Schedules.Add(schedule);
                    await _context.SaveChangesAsync();

                    // Add the session - Fix: Use schedule.SectionId (string) instead of converting to int
                    var session = new ScheduleSession
                    {
                        ScheduleId = schedule.Id, // Use the actual Schedule ID
                        DayOfWeek = model.DayOfWeek,
                        StartTime = TimeSpan.Parse(model.StartTime),
                        EndTime = TimeSpan.Parse(model.EndTime)
                    };

                    _context.ScheduleSessions.Add(session);
                    await _context.SaveChangesAsync();

                    return Json(new { 
                        success = true, 
                        id = session.Id,
                        schedule = new {
                            courseCode = schedule.CourseCode,
                            courseTitle = (await _context.Courses.FindAsync(schedule.CourseCode))?.Crs_Title,
                            instructor = schedule.Instructor,
                            room = schedule.Room,
                            dayOfWeek = session.DayOfWeek,
                            startTime = session.StartTime.ToString(@"hh\:mm"),
                            endTime = session.EndTime.ToString(@"hh\:mm")
                        }
                    });
                }
                else
                {
                    // Add session to existing schedule - Fix: Use proper ScheduleId
                    var session = new ScheduleSession
                    {
                        ScheduleId = existingSchedule.Id,  // Convert string to int
                        DayOfWeek = model.DayOfWeek,
                        StartTime = TimeSpan.Parse(model.StartTime),
                        EndTime = TimeSpan.Parse(model.EndTime)
                    };

                    _context.ScheduleSessions.Add(session);
                    await _context.SaveChangesAsync();

                    return Json(new { 
                        success = true, 
                        id = session.Id,
                        schedule = new {
                            courseCode = existingSchedule.CourseCode,
                            courseTitle = (await _context.Courses.FindAsync(existingSchedule.CourseCode))?.Crs_Title,
                            instructor = existingSchedule.Instructor,
                            room = existingSchedule.Room,
                            dayOfWeek = session.DayOfWeek,
                            startTime = session.StartTime.ToString(@"hh\:mm"),
                            endTime = session.EndTime.ToString(@"hh\:mm")
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleCreateModel model)
        {
            try
            {
                var session = await _context.ScheduleSessions.FindAsync(id);
                if (session == null)
                {
                    return Json(new { success = false, message = "Schedule session not found" });
                }

                var schedule = await _context.Schedules.FindAsync(session.ScheduleId); 
                if (schedule == null)
                {
                    return Json(new { success = false, message = "Schedule not found" });
                }

                // Update schedule details
                schedule.Room = model.Room;
                schedule.Instructor = model.Instructor;

                // Update session details
                session.DayOfWeek = model.DayOfWeek;
                session.StartTime = TimeSpan.Parse(model.StartTime);
                session.EndTime = TimeSpan.Parse(model.EndTime);

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true,
                    schedule = new {
                        courseCode = schedule.CourseCode,
                        courseTitle = (await _context.Courses.FindAsync(schedule.CourseCode))?.Crs_Title,
                        instructor = schedule.Instructor,
                        room = schedule.Room,
                        dayOfWeek = session.DayOfWeek,
                        startTime = session.StartTime.ToString(@"hh\:mm"),
                        endTime = session.EndTime.ToString(@"hh\:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var session = await _context.ScheduleSessions.FindAsync(id);
                if (session == null)
                {
                    return Json(new { success = false, message = "Schedule session not found" });
                }

                _context.ScheduleSessions.Remove(session);
                
                // Check if this was the last session for the schedule
                var hasOtherSessions = await _context.ScheduleSessions
                    .AnyAsync(ss => ss.ScheduleId == session.ScheduleId);
                
                if (!hasOtherSessions)
                {
                    var schedule = await _context.Schedules.FindAsync(session.ScheduleId); 
                    if (schedule != null)
                    {
                        _context.Schedules.Remove(schedule);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Curriculum Management
        public async Task<IActionResult> ViewCurriculum()
        {
            var curricula = await _context.Curricula
                .Join(_context.Programs,
                    c => c.Program,
                    p => p.ProgramCode,
                    (c, p) => new CurriculumViewModel
                    {
                        CurriculumCode = c.CurriculumCode,
                        ProgramCode = c.Program,
                        ProgramName = p.ProgramName,
                        AcademicYear = c.AcademicYear
                    })
                .OrderBy(c => c.ProgramName)
                .ThenBy(c => c.AcademicYear)
                .ToListAsync();

            return View(curricula);
        }

        [HttpPost]
        public async Task<IActionResult> AddCurriculum(string program, string academicYear)
        {
            var curriculumCode = $"{program}{academicYear.Split('-')[0]}";
            
            var curriculum = new Curriculum
            {
                CurriculumCode = curriculumCode,
                Program = program,
                AcademicYear = academicYear
            };

            _context.Curricula.Add(curriculum);
            await _context.SaveChangesAsync();

            return Json(new { success = true, curriculum });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCurriculum(string curriculumCode)
        {
            try
            {
                // First, remove all curriculum courses associated with this curriculum
                var curriculumCourses = await _context.CurriculumCourses
                    .Where(cc => cc.CurriculumCode == curriculumCode)
                    .ToListAsync();
        
                if (curriculumCourses.Any())
                {
                    _context.CurriculumCourses.RemoveRange(curriculumCourses);
                }
        
                // Then remove the curriculum itself
                var curriculum = await _context.Curricula
                    .FirstOrDefaultAsync(c => c.CurriculumCode == curriculumCode);
        
                if (curriculum == null)
                {
                    return Json(new { success = false, message = "Curriculum not found" });
                }
        
                _context.Curricula.Remove(curriculum);
                await _context.SaveChangesAsync();
        
                return Json(new { success = true, message = "Curriculum removed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error removing curriculum: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAcademicYears()
        {
            var academicYears = await _context.Curricula
                .Select(c => c.AcademicYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            return Json(academicYears);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurriculaByAcademicYear(string academicYear)
        {
            var curricula = await _context.Curricula
                .Where(c => c.AcademicYear == academicYear)
                .Join(_context.Programs,
                    c => c.Program,
                    p => p.ProgramCode,
                    (c, p) => new {
                        curriculumCode = c.CurriculumCode,
                        programName = p.ProgramName
                    })
                .ToListAsync();

            return Json(curricula);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurricula()
        {
            var curricula = await _context.Curricula
                .Join(_context.Programs,
                    c => c.Program,
                    p => p.ProgramCode,
                    (c, p) => new {
                        curriculumCode = c.CurriculumCode,
                        programCode = c.Program,
                        programName = p.ProgramName,
                        academicYear = c.AcademicYear
                    })
                .OrderBy(c => c.programName)
                .ThenBy(c => c.academicYear)
                .ToListAsync();

            return Json(curricula);
        }
        #endregion

        #region Curriculum Course Methods
        [HttpGet]
        public async Task<IActionResult> GetCurriculumCourses(string curriculumCode)
        {
            var courses = await _context.CurriculumCourses
                .Where(cc => cc.CurriculumCode == curriculumCode)
                .Include(cc => cc.Course)
                .Select(cc => new
                {
                    id = cc.Id,
                    code = cc.CourseCode,
                    title = cc.Course.Crs_Title,
                    units = cc.Course.Crs_Units,
                    year = cc.YearLevel,
                    semester = cc.Semester
                })
                .ToListAsync();

            return Json(courses);
        }

        [HttpPost]
        public async Task<IActionResult> AddCurriculumCourse(string curriculumCode, string courseCode, int yearLevel, string semester)
        {
            var course = await _context.Courses.FindAsync(courseCode);
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }

            var curriculumCourse = new CurriculumCourse
            {
                CurriculumCode = curriculumCode,
                CourseCode = courseCode,
                YearLevel = yearLevel,
                Semester = semester
            };

            _context.CurriculumCourses.Add(curriculumCourse);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true,
                course = new {
                    id = curriculumCourse.Id,
                    code = courseCode,
                    title = course.Crs_Title,
                    units = course.Crs_Units,
                    year = yearLevel,
                    semester = semester
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCurriculumCourse(int id)
        {
            var course = await _context.CurriculumCourses.FindAsync(id);
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found in curriculum" });
            }

            _context.CurriculumCourses.Remove(course);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        #endregion

        #region Course Methods
        [HttpGet]
        public async Task<IActionResult> AddCourse()
        {
            var viewModel = new AddCourseViewModel
            {
                Categories = await _context.CourseCategories
                    .OrderBy(c => c.Ctg_Name)
                    .ToListAsync(),

                Courses = await _context.Courses
                    .Include(c => c.CourseCategory)
                    .OrderBy(c => c.Crs_Code)
                    .Select(c => new CourseDropdownItem
                    {
                        Crs_Code = c.Crs_Code,
                        Crs_Title = c.Crs_Title,
                        CategoryName = c.CourseCategory.Ctg_Name
                    })
                    .ToListAsync(),

                NewCourse = new Course()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(AddCourseViewModel model)
        {
            // Always repopulate dropdown data
            model.Categories = await _context.CourseCategories
                .OrderBy(c => c.Ctg_Name)
                .ToListAsync();
    
            model.Courses = await _context.Courses
                .Include(c => c.CourseCategory)
                .OrderBy(c => c.Crs_Code)
                .Select(c => new CourseDropdownItem
                {
                    Crs_Code = c.Crs_Code,
                    Crs_Title = c.Crs_Title,
                    CategoryName = c.CourseCategory!.Ctg_Name
                })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var newCourse = new Course
                {
                    Ctg_Code = model.NewCourse.Ctg_Code,
                    Crs_Code = model.NewCourse.Ctg_Code + model.NewCourse.Crs_Code,
                    Crs_Title = model.NewCourse.Crs_Title,
                    Preq_Crs_Code = string.IsNullOrWhiteSpace(model.NewCourse.Preq_Crs_Code) ? null : model.NewCourse.Preq_Crs_Code,
                    Crs_Units = model.NewCourse.Crs_Units,
                    Crs_Lec = model.NewCourse.Crs_Lec,
                    Crs_Lab = model.NewCourse.Crs_Lab
                };

                _context.Courses.Add(newCourse);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Course {newCourse.Crs_Code} added successfully!";
                return RedirectToAction(nameof(AddCourse));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Error saving course: {ex.InnerException?.Message ?? ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses
                .OrderBy(c => c.Crs_Code)
                .Select(c => new {
                    crs_Code = c.Crs_Code,
                    crs_Title = c.Crs_Title
                })
                .ToListAsync();
    
            return Json(courses);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrograms()
        {
            var programs = await _context.Programs
                .OrderBy(p => p.ProgramName)
                .Select(p => new {
                    code = p.ProgramCode,
                    name = p.ProgramName
                })
                .ToListAsync();
    
            return Json(programs);
        }
        #endregion

        #region View Methods
        public IActionResult AdminMenu()
        {
            return View();
        }

        public IActionResult ApproveEnrollment()
        {
            return View();
        }

        public IActionResult CreateSchedule()
        {
            return View();
        }

        public IActionResult ViewAllSchedule()
        {
            return View();
        }
        #endregion

        #region View Models
        public class CurriculumViewModel
        {
            public string CurriculumCode { get; set; }
            public string ProgramCode { get; set; }
            public string ProgramName { get; set; }
            public string AcademicYear { get; set; }
        }

        // public class AddCourseViewModel
        // {
        //     public List<CourseCategory> Categories { get; set; } = new List<CourseCategory>();
        //     public List<CourseDropdownItem> Courses { get; set; } = new List<CourseDropdownItem>();
        //     public Course NewCourse { get; set; } = new Course();
        // }

        public class CourseDropdownItem
        {
            public string Crs_Code { get; set; }
            public string Crs_Title { get; set; }
            public string CategoryName { get; set; }
        }

        public class ScheduleCreateModel
        {
            public string CurriculumCode { get; set; }
            public string CourseCode { get; set; }
            public string SectionId { get; set; }
            public string Room { get; set; }
            public string Instructor { get; set; }
            public int DayOfWeek { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }

        public class SectionCreateModel
        {
            public string CurriculumCode { get; set; }
            public string Section { get; set; }
            public string Semester { get; set; }
            public string YearLevel { get; set; }
        }
        #endregion
    }
}