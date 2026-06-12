using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoUniversity.Controllers;

public class DepartmentsController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notificationService;

    public DepartmentsController(SchoolContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments.Include(d => d.Administrator).ToListAsync();
        return View(departments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var department = await _context.Departments
            .Include(d => d.Administrator)
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.DepartmentID == id);

        if (department == null)
            return NotFound();

        return View(department);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.InstructorID = new SelectList(await _context.Instructors.ToListAsync(), "ID", "FullName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Budget,StartDate,InstructorID")] Department department)
    {
        if (ModelState.IsValid)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Department", department.DepartmentID.ToString(), department.Name, EntityOperation.Created);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.InstructorID = new SelectList(await _context.Instructors.ToListAsync(), "ID", "FullName", department.InstructorID);
        return View(department);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        ViewBag.InstructorID = new SelectList(await _context.Instructors.ToListAsync(), "ID", "FullName", department.InstructorID);
        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentID,Name,Budget,StartDate,InstructorID,RowVersion")] Department department)
    {
        if (id != department.DepartmentID)
            return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Entry(department).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync("Department", department.DepartmentID.ToString(), department.Name, EntityOperation.Updated);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Departments.AnyAsync(d => d.DepartmentID == id))
                    return NotFound();
                throw;
            }
        }
        ViewBag.InstructorID = new SelectList(await _context.Instructors.ToListAsync(), "ID", "FullName", department.InstructorID);
        return View(department);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return BadRequest();

        var department = await _context.Departments.Include(d => d.Administrator).FirstOrDefaultAsync(d => d.DepartmentID == id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Department", id.ToString(), department.Name, EntityOperation.Deleted);
        }
        return RedirectToAction(nameof(Index));
    }
}
