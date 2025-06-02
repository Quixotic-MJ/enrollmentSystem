using Microsoft.AspNetCore.Mvc;

namespace enrollmentSystem.Controllers;

public class AdminController : Controller
{
    // GET
    public IActionResult AdminMenu()
    {
        return View();
    }
    
    public IActionResult ViewCurriculum()
    {
        return View();
    }
    
    public IActionResult AddCourse()
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
}