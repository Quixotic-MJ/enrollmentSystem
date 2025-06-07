using enrollmentSystem.Data;
using enrollmentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace enrollmentSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Schedule Management

        
        
        [HttpDelete("Admin/DeleteScheduleItem/{id}")]
        public async Task<IActionResult> DeleteScheduleItem(int id)
        {
            try
            {
                // First find the session to be deleted
                var session = await _context.ScheduleSessions
                    .Include(s => s.Schedule)
                    .FirstOrDefaultAsync(s => s.Id == id);
                    
                if (session == null)
                {
                    return NotFound(new { success = false, message = "Schedule session not found." });
                }

                // Get the schedule ID before deleting the session
                var scheduleId = session.ScheduleId;
                
                // Remove the session
                _context.ScheduleSessions.Remove(session);
                
                // Check if this was the last session for the schedule
                var hasOtherSessions = await _context.ScheduleSessions
                    .AnyAsync(ss => ss.ScheduleId == scheduleId);
                
                // If no more sessions, delete the schedule as well
                if (!hasOtherSessions)
                {
                    var schedule = await _context.Schedules.FindAsync(scheduleId);
                    if (schedule != null)
                    {
                        _context.Schedules.Remove(schedule);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Schedule item deleted successfully.",
                    scheduleId = scheduleId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule item");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while deleting the schedule item.",
                    error = ex.Message
                });
            }
        }
        
        #endregion
        
        #region Section Management
        [HttpGet]
        public async Task<IActionResult> GetSections(string curriculumCode = null, string semester = null, string yearLevel = null)
        {
            try
            {
                var query = _context.Sections.AsQueryable();

                // Apply filters if provided
                if (!string.IsNullOrEmpty(curriculumCode))
                    query = query.Where(s => s.CurriculumCode != null && s.CurriculumCode.ToUpper() == curriculumCode.ToUpper());
                
                if (!string.IsNullOrEmpty(semester))
                    query = query.Where(s => s.Semester != null && s.Semester.ToUpper() == semester.ToUpper());
                
                if (!string.IsNullOrEmpty(yearLevel))
                    query = query.Where(s => s.YearLevel != null && s.YearLevel.ToUpper() == yearLevel.ToUpper());

                var sections = await query
                    .Include(s => s.Curriculum)
                    .Include(s => s.Schedules)
                        .ThenInclude(sc => sc.Course)
                    .Include(s => s.Schedules)
                        .ThenInclude(sc => sc.Sessions)
                    .Select(s => new 
                    {
                        id = s.Id,
                        curriculumCode = s.CurriculumCode,
                        programName = s.Curriculum != null ? s.Curriculum.Program : "N/A (Curriculum Missing)",
                        sectionName = s.SectionName,
                        semester = s.Semester,
                        yearLevel = s.YearLevel,
                        instructor = s.Instructor,
                        curriculum = new 
                        {
                            code = s.Curriculum != null ? s.Curriculum.CurriculumCode : "N/A",
                            program = s.Curriculum != null ? s.Curriculum.Program : "N/A",
                            academicYear = s.Curriculum != null ? s.Curriculum.AcademicYear : "N/A",
                            courses = _context.Courses
                                .Select(c => new 
                                {
                                    id = c.Crs_Code,
                                    code = c.Crs_Code,
                                    name = c.Crs_Title
                                })
                                .OrderBy(c => c.code)
                                .ToList()
                        },
                        schedules = s.Schedules.Select(sc => new 
                        {
                            id = sc.Id,
                            courseCode = sc.CourseCode,
                            courseName = sc.Course != null ? sc.Course.Crs_Title : "N/A",
                            room = sc.Room,
                            instructor = sc.Instructor,
                            sessions = sc.Sessions.Select(sess => new 
                            {
                                day = sess.DayOfWeek,
                                startTime = sess.StartTime,
                                endTime = sess.EndTime,
                                id = sess.Id
                            }).ToList()
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sections");
                return StatusCode(500, new { message = "An error occurred while fetching sections." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] SectionCreateModel model)
        {
            try
            {
                var section = new Section
                {
                    CurriculumCode = model.CurriculumCode,
                    SectionName = model.SectionName,
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

                section.SectionName = model.SectionName;
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

        [HttpDelete("Admin/DeleteSection/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            try
            {
                var section = await _context.Sections
                    .Include(s => s.Schedules)
                        .ThenInclude(sch => sch.Sessions) // Eagerly load schedules and their sessions
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (section == null)
                {
                    return NotFound(new { message = "Section not found." });
                }

                // Iterate over a copy of schedules for safe removal
                foreach (var schedule in section.Schedules.ToList())
                {
                    // Remove all sessions associated with this schedule
                    if (schedule.Sessions.Any())
                    {
                        _context.ScheduleSessions.RemoveRange(schedule.Sessions);
                    }
                    // Remove the schedule itself
                    _context.Schedules.Remove(schedule);
                }

                // Finally, remove the section
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Section and all associated schedules and sessions deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting section with ID {id}.");
                return StatusCode(500, new { message = "An error occurred while deleting the section: " + ex.Message });
            }
        }
        #endregion

        #region Schedule Management
        [HttpGet]
        public async Task<IActionResult> GetSchedules(int sectionId)
        {
            var schedules = await _context.Schedules
                .Where(s => s.SectionId == sectionId)
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                _logger.LogError($"CreateSchedule: Model validation failed. Errors: {string.Join(", ", errors)}");
                return BadRequest(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                if (model == null)
                {
                    _logger.LogError("CreateSchedule: Model is null");
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                _logger.LogInformation($"CreateSchedule called with model: {System.Text.Json.JsonSerializer.Serialize(model)}");
                
                // Validate model properties
                if (model.SectionId <= 0)
                {
                    _logger.LogError($"Invalid SectionId: {model.SectionId}");
                    return Json(new { success = false, message = "Invalid section ID" });
                }

                if (string.IsNullOrEmpty(model.CurriculumCode))
                {
                    _logger.LogError("CurriculumCode is null or empty");
                    return Json(new { success = false, message = "Curriculum code is required" });
                }

                // First, verify the section exists
                var section = await _context.Sections.FindAsync(model.SectionId);
                if (section == null)
                {
                    _logger.LogError($"Section not found with ID: {model.SectionId}");
                    return Json(new { success = false, message = "Section not found" });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(model.CourseCode))
                {
                    _logger.LogError("CourseCode is required");
                    return Json(new { success = false, message = "Course code is required" });
                }

                if (model.DayOfWeek < 0 || model.DayOfWeek > 6)
                {
                    _logger.LogError($"Invalid DayOfWeek: {model.DayOfWeek}");
                    return Json(new { success = false, message = "Invalid day of week" });
                }

                // Check if a schedule already exists for this course and section
                var existingSchedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => 
                        s.SectionId == model.SectionId && 
                        s.CourseCode == model.CourseCode);

                if (existingSchedule == null)
                {
                    _logger.LogInformation("Creating new schedule");

                    // Validate times
                    if (!TimeSpan.TryParse(model.StartTime, out var startTime) || 
                        !TimeSpan.TryParse(model.EndTime, out var endTime))
                    {
                        _logger.LogError($"Invalid time format. Start: {model.StartTime}, End: {model.EndTime}");
                        return Json(new { success = false, message = "Invalid time format. Use HH:mm" });
                    }

                    // Create new schedule
                    var schedule = new Schedule
                    {
                        CurriculumCode = model.CurriculumCode ?? string.Empty,
                        CourseCode = model.CourseCode,
                        SectionId = model.SectionId,
                        Room = model.Room ?? string.Empty,
                        Instructor = model.Instructor ?? "TBA"
                    };

                    _context.Schedules.Add(schedule);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new schedule with ID: {schedule.Id}");

                    // Add the session
                    var session = new ScheduleSession
                    {
                        ScheduleId = schedule.Id,
                        DayOfWeek = model.DayOfWeek,
                        StartTime = startTime,
                        EndTime = endTime
                    };

                    _context.ScheduleSessions.Add(session);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new session with ID: {session.Id}");

                    var course = await _context.Courses.FindAsync(schedule.CourseCode);
                    return Json(new { 
                        success = true, 
                        id = session.Id,
                        schedule = new {
                            courseCode = schedule.CourseCode,
                            courseTitle = course?.Crs_Title ?? "Unknown Course",
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
                // First find the session to be deleted
                var session = await _context.ScheduleSessions
                    .Include(s => s.Schedule) // Include the Schedule navigation property
                    .FirstOrDefaultAsync(s => s.Id == id);
                    
                if (session == null)
                {
                    return Json(new { success = false, message = "Schedule session not found" });
                }

                // Get the schedule ID before deleting the session
                var scheduleId = session.ScheduleId;
                
                // Remove the session
                _context.ScheduleSessions.Remove(session);
                
                // Check if this was the last session for the schedule
                var hasOtherSessions = await _context.ScheduleSessions
                    .AnyAsync(ss => ss.ScheduleId == scheduleId);
                
                // If no more sessions, delete the schedule as well
                if (!hasOtherSessions)
                {
                    var schedule = await _context.Schedules.FindAsync(scheduleId);
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
                        programCode = c.Program,  // The code
                        program = p.ProgramName,   // The full name
                        academicYear = c.AcademicYear
                    })
                .OrderBy(c => c.program)
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

     

        public class CourseDropdownItem
        {
            public string Crs_Code { get; set; }
            public string Crs_Title { get; set; }
            public string CategoryName { get; set; }
        }

        public class ScheduleCreateModel
        {
            [Required(ErrorMessage = "Curriculum code is required")]
            public string CurriculumCode { get; set; }
            
            [Required(ErrorMessage = "Course code is required")]
            public string CourseCode { get; set; }
            
            [Required(ErrorMessage = "Section ID is required")]
            public int SectionId { get; set; }
            
            [Required(ErrorMessage = "Room is required")]
            public string Room { get; set; }
            
            [Required(ErrorMessage = "Instructor is required")]
            public string Instructor { get; set; }
            
            [Required(ErrorMessage = "Day of week is required")]
            [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
            public int DayOfWeek { get; set; }
            
            [Required(ErrorMessage = "Start time is required")]
            [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format. Use HH:mm (24-hour format)")]
            public string StartTime { get; set; }
            
            [Required(ErrorMessage = "End time is required")]
            [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format. Use HH:mm (24-hour format)")]
            public string EndTime { get; set; }
        }

        public class SectionCreateModel
        {
            [Required]
            public string CurriculumCode { get; set; }
            
            [Required]
            [Display(Name = "Section")]
            public string SectionName { get; set; }
            
            [Required]
            public string Semester { get; set; }
            
            [Required]
            [Display(Name = "Year Level")]
            public string YearLevel { get; set; }
        }
        #endregion
    }
}