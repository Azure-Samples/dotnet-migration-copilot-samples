using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoUniversity.Controllers;

public class CoursesController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notificationService;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public CoursesController(SchoolContext context, INotificationService notificationService, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _notificationService = notificationService;
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.Include(c => c.Department).ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseID == id);
        if (course == null)
            return NotFound();

        return View(course);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.DepartmentID = await GetDepartmentSelectListAsync();
        return View(new Course());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID")] Course course, IFormFile? teachingMaterialImage)
    {
        if (ModelState.IsValid)
        {
            if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
            {
                try
                {
                    course.TeachingMaterialImagePath = await SaveUploadedFileAsync(teachingMaterialImage, course.CourseID.ToString());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("teachingMaterialImage", $"Error uploading file: {ex.Message}");
                    ViewBag.DepartmentID = await GetDepartmentSelectListAsync();
                    return View(course);
                }
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Course", course.CourseID.ToString(), course.Title, EntityOperation.Created);

            return RedirectToAction(nameof(Index));
        }

        ViewBag.DepartmentID = await GetDepartmentSelectListAsync(course.DepartmentID);
        return View(course);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound();

        ViewBag.DepartmentID = await GetDepartmentSelectListAsync(course.DepartmentID);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile? teachingMaterialImage)
    {
        if (id != course.CourseID)
            return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    course.TeachingMaterialImagePath = await SaveUploadedFileAsync(teachingMaterialImage, id.ToString());
                }

                _context.Entry(course).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync("Course", course.CourseID.ToString(), course.Title, EntityOperation.Updated);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Courses.AnyAsync(c => c.CourseID == id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.DepartmentID = await GetDepartmentSelectListAsync(course.DepartmentID);
        return View(course);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseID == id);
        if (course == null)
            return NotFound();

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            await _notificationService.SendNotificationAsync("Course", id.ToString(), course.Title, EntityOperation.Deleted);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetDepartmentSelectListAsync(int? selectedId = null)
    {
        var departments = await _context.Departments.ToListAsync();
        return new SelectList(departments, "DepartmentID", "Name", selectedId);
    }

    private async Task<string> SaveUploadedFileAsync(IFormFile file, string courseId)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file type");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("File size must be less than 5MB");

        var uploadsPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "teaching-materials");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"course_{courseId}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/teaching-materials/{fileName}";
    }
}
