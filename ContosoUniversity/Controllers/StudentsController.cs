using System.Diagnostics;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers;

public class StudentsController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notificationService;

    public StudentsController(SchoolContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

        if (searchString != null)
        {
            page = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        ViewBag.CurrentFilter = searchString;

        var students = _context.Students.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            students = students.Where(s => s.LastName.Contains(searchString)
                                   || s.FirstMidName.Contains(searchString));
        }

        students = sortOrder switch
        {
            "name_desc" => students.OrderByDescending(s => s.LastName),
            "Date" => students.OrderBy(s => s.EnrollmentDate),
            "date_desc" => students.OrderByDescending(s => s.EnrollmentDate),
            _ => students.OrderBy(s => s.LastName),
        };

        const int pageSize = 10;
        int pageNumber = (page ?? 1);
        return View(await PaginatedList<Student>.CreateAsync(students, pageNumber, pageSize));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.ID == id);
        
        if (student == null)
        {
            return NotFound();
        }
        
        return View(student);
    }

    public IActionResult Create()
    {
        var student = new Student
        {
            EnrollmentDate = DateTime.Today
        };
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)
    {
        try
        {
            if (student.EnrollmentDate == DateTime.MinValue || student.EnrollmentDate == default)
            {
                ModelState.AddModelError("EnrollmentDate", "Please enter a valid enrollment date.");
            }

            if (student.EnrollmentDate < new DateTime(1753, 1, 1) || student.EnrollmentDate > new DateTime(9999, 12, 31))
            {
                ModelState.AddModelError("EnrollmentDate", "Enrollment date must be between 1753 and 9999.");
            }

            if (ModelState.IsValid)
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                var studentName = $"{student.FirstMidName} {student.LastName}";
                await _notificationService.SendNotificationAsync("Student", student.ID.ToString(), studentName, EntityOperation.Created);

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error creating student: {ex.Message} | Student: {student?.FirstMidName} {student?.LastName} | EnrollmentDate: {student?.EnrollmentDate} | Stack: {ex.StackTrace}");
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
        }
        
        return View(student);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
    {
        if (id != student.ID)
        {
            return BadRequest();
        }

        try
        {
            if (student.EnrollmentDate == DateTime.MinValue || student.EnrollmentDate == default)
            {
                ModelState.AddModelError("EnrollmentDate", "Please enter a valid enrollment date.");
            }

            if (student.EnrollmentDate < new DateTime(1753, 1, 1) || student.EnrollmentDate > new DateTime(9999, 12, 31))
            {
                ModelState.AddModelError("EnrollmentDate", "Enrollment date must be between 1753 and 9999.");
            }

            if (ModelState.IsValid)
            {
                _context.Entry(student).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var studentName = $"{student.FirstMidName} {student.LastName}";
                await _notificationService.SendNotificationAsync("Student", student.ID.ToString(), studentName, EntityOperation.Updated);

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error editing student: {ex.Message} | Student ID: {student?.ID} | Student: {student?.FirstMidName} {student?.LastName} | EnrollmentDate: {student?.EnrollmentDate} | Stack: {ex.StackTrace}");
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
        }
        
        return View(student);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        
        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var studentName = $"{student.FirstMidName} {student.LastName}";
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                await _notificationService.SendNotificationAsync("Student", id.ToString(), studentName, EntityOperation.Deleted);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error deleting student: {ex.Message} | Student ID: {id} | Stack: {ex.StackTrace}");
            TempData["ErrorMessage"] = "Unable to delete the student. Try again, and if the problem persists see your system administrator.";
            return RedirectToAction(nameof(Index));
        }
    }
}
