using enrollmentSystem.Data;
using enrollmentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace enrollmentSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        
        public AuthController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> LoginStudent(string IDNumber, string Password)
        {
            try
            {
                var student = await _context.Student
                    .FirstOrDefaultAsync(s => s.Stud_Id == IDNumber);

                if (student == null)
                {
                    ViewBag.ErrorMessage = "Student ID not found. Please register first.";
                    ViewBag.PreviousIDNumber = IDNumber;
                    return View("Login");
                }

                if (student.Stud_Password == Password)
                {
                    HttpContext.Session.SetString("StudentID", student.Stud_Id);
                    HttpContext.Session.SetString("StudentName", $"{student.Stud_Fname} {student.Stud_Lname}");
            
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("StudentMenu", "Student");
                }

                ViewBag.ErrorMessage = "Invalid password. Please try again.";
                ViewBag.PreviousIDNumber = IDNumber;
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
                return View("Login");
            }
        }


        [HttpGet]
        public IActionResult LoginAdmin()
        {
            return View();
        }
        
        

        [HttpPost]
        public async Task<IActionResult> LoginAdmin(string username, string Password)
        {
            try
            {
                var admin = await _context.Admin
                    .FirstOrDefaultAsync(a => a.Admin_Id == username);

                if (admin == null)
                {
                    ViewBag.ErrorMessage = "Admin ID not found. Please register first.";
                    return View("LoginAdmin"); 
                }

                if (admin.Admin_Password == Password)
                {
                    HttpContext.Session.SetString("AdminID", admin.Admin_Id);
                    HttpContext.Session.SetString("AdminName", $"{admin.Admin_Fname} {admin.Admin_Lname}");
            
                    TempData["AdminLoginSuccess"] = "Admin login successful!";
                    return RedirectToAction("AdminMenu", "Admin");
                }

                ViewBag.ErrorMessage = "Invalid password. Please try again.";
                return View("LoginAdmin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
                return View("LoginAdmin");
            }
        }

        
        [HttpGet]
        public IActionResult StudSignUp()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> StudSignUp(Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _context.Student.AnyAsync(s => s.Stud_Id == student.Stud_Id))
                    {
                        ModelState.AddModelError("Stud_Id", "Student ID already exists");
                        return View(student);
                    }

                    _context.Student.Add(student);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Student created successfully!";
                    return RedirectToAction("SignUpSuccess");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving student: {ex.Message}");
                Console.WriteLine($"Error: {ex}");
            }

            return View(student);
        }
        
        
        [HttpGet]
        public IActionResult AdminSignUp()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminSignUp(Admin admin)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingAdmin = await _context.Admin.FindAsync(admin.Admin_Id);
                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError("Admin_Id", "An admin with this ID already exists.");
                        return View(admin);
                    }

                    _context.Admin.Add(admin); 
                    await _context.SaveChangesAsync();
            
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("SignUpSuccess");
                }
            }
            catch (Exception ex)
            {
                // More detailed error logging
                Console.WriteLine($"Error saving admin: {ex}");
                ModelState.AddModelError("", $"Error saving to database: {ex.Message}");
            }

            return View(admin);
        }

        public IActionResult SignUpSuccess()
        {
            return View();
        }
    }
}