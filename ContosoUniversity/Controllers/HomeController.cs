using ContosoUniversity.Data;
using ContosoUniversity.Models.SchoolViewModels;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers;

public class HomeController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notificationService;

    public HomeController(SchoolContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> About()
    {
        var data = await (from student in _context.Students
            group student by student.EnrollmentDate into dateGroup
            select new EnrollmentDateGroup
            {
                EnrollmentDate = dateGroup.Key,
                StudentCount = dateGroup.Count()
            }).ToListAsync();

        return View(data);
    }

    public IActionResult Contact()
    {
        ViewBag.Message = "Your contact page.";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }

    public IActionResult UnauthorizedAction()
    {
        ViewBag.Message = "You don't have permission to access this resource.";
        return View();
    }
}
