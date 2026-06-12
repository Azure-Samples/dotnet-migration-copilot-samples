using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers;

public class InstructorsController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notificationService;

    public InstructorsController(SchoolContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var instructors = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
            .ThenInclude(ca => ca.Course)
            .ToListAsync();
        return View(instructors);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var instructor = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
            .ThenInclude(ca => ca.Course)
            .FirstOrDefaultAsync(i => i.ID == id);

        if (instructor == null)
            return NotFound();

        return View(instructor);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LastName,FirstMidName,HireDate")] Instructor instructor)
    {
        if (ModelState.IsValid)
        {
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Instructor", instructor.ID.ToString(), $"{instructor.FirstMidName} {instructor.LastName}", EntityOperation.Created);
            return RedirectToAction(nameof(Index));
        }
        return View(instructor);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var instructor = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .FirstOrDefaultAsync(i => i.ID == id);

        if (instructor == null)
            return NotFound();

        return View(instructor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,HireDate")] Instructor instructor)
    {
        if (id != instructor.ID)
            return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Entry(instructor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync("Instructor", instructor.ID.ToString(), $"{instructor.FirstMidName} {instructor.LastName}", EntityOperation.Updated);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Instructors.AnyAsync(i => i.ID == id))
                    return NotFound();
                throw;
            }
        }
        return View(instructor);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return BadRequest();

        var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.ID == id);
        if (instructor == null)
            return NotFound();

        return View(instructor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor != null)
        {
            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Instructor", id.ToString(), $"{instructor.FirstMidName} {instructor.LastName}", EntityOperation.Deleted);
        }
        return RedirectToAction(nameof(Index));
    }
}
