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
        public IActionResult LoginStudent(string IDNumber, string Password)
        {
            if (IDNumber == "123456" && Password == "123456")
            {
                HttpContext.Session.SetString("StudentID", IDNumber);
                TempData["message"] = "Login successful!";
                return RedirectToAction("StudentMenu", "Student");
            }

            ViewBag.ErrorMessage = "Invalid ID number or password.";
            ViewBag.PreviousIDNumber = IDNumber; // Remember the ID on failed attempt
            return View("Login");
        }


        [HttpGet]
        public IActionResult LoginAdmin()
        {
            return View();
        }
        
        

        [HttpPost]
        public IActionResult LoginAdmin(string username, string Password)
        {
            if (username == "admin" && Password == "123456")
            {
                // Optional: Store something in session
                TempData["message"] = "Login successful!";
                return RedirectToAction("AdminMenu", "Admin");
            }

            ViewBag.ErrorMessage = "Invalid ID number or password.";
            return View("Login");
        }
        
        public IActionResult StudSignUp()
        {
            return View();
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
                    // Check if admin exists - using Admin (singular) property
                    var existingAdmin = await _context.Admin.FindAsync(admin.Admin_Id);
                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError("Admin_Id", "An admin with this ID already exists.");
                        return View(admin);
                    }

                    _context.Admin.Add(admin); // Using singular Admin
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