using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CoursesController(SchoolContext context, NotificationService notificationService, IWebHostEnvironment webHostEnvironment)
            : base(context, notificationService)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Courses
        public IActionResult Index()
        {
            var courses = db.Courses.Include(c => c.Department);
            return View(courses.ToList());
        }

        // GET: Courses/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).Single();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name");
            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if an image is provided
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "TeachingMaterials");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            teachingMaterialImage.CopyTo(stream);
                        }
                        course.TeachingMaterialImagePath = $"/Uploads/TeachingMaterials/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Courses.Add(course);
                db.SaveChanges();
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.CREATE);
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(teachingMaterialImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Please upload a valid image file (jpg, jpeg, png, gif, bmp).");
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    if (teachingMaterialImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "File size must be less than 5MB.");
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }

                    try
                    {
                        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "TeachingMaterials");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, course.TeachingMaterialImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            teachingMaterialImage.CopyTo(stream);
                        }
                        course.TeachingMaterialImagePath = $"/Uploads/TeachingMaterials/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("teachingMaterialImage", "Error uploading file: " + ex.Message);
                        ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                }

                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.UPDATE);
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentID = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Course course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).Single();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Find(id);
            var courseTitle = course.Title;

            if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, course.TeachingMaterialImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                    }
                }
            }

            db.Courses.Remove(course);
            db.SaveChanges();
            SendEntityNotification("Course", id.ToString(), courseTitle, EntityOperation.DELETE);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Base class will dispose db
            }
            base.Dispose(disposing);
        }
    }
}
