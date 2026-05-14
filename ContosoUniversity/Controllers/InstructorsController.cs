using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : BaseController
    {
        public InstructorsController(SchoolContext context, NotificationService notificationService)
            : base(context, notificationService)
        {
        }

        // GET: Instructors - All roles can view
        public IActionResult Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(c => c.Course)
                        .ThenInclude(d => d.Department)
                .OrderBy(i => i.LastName);

            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single().CourseAssignments.Select(s => s.Course);
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;
                viewModel.Enrollments = viewModel.Courses.Where(
                    x => x.CourseID == courseID).Single().Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructors/Details/5 - All roles can view details
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("LastName,FirstMidName,HireDate,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                db.SaveChanges();
                SendEntityNotification("Instructor", instructor.ID.ToString(), EntityOperation.CREATE);
                return RedirectToAction("Index");
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor instructor = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(c => c.Course)
                .Where(i => i.ID == id)
                .Single();
            PopulateAssignedCourseData(instructor);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int? id, string[] selectedCourses, string LastName, string FirstMidName, DateTime HireDate, string OfficeLocation)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var instructorToUpdate = db.Instructors
               .Include(i => i.OfficeAssignment)
               .Include(i => i.CourseAssignments)
                   .ThenInclude(c => c.Course)
               .Where(i => i.ID == id)
               .Single();

            // Manually update instructor fields
            instructorToUpdate.LastName = LastName;
            instructorToUpdate.FirstMidName = FirstMidName;
            instructorToUpdate.HireDate = HireDate;

            if (String.IsNullOrWhiteSpace(OfficeLocation))
            {
                instructorToUpdate.OfficeAssignment = null;
            }
            else
            {
                if (instructorToUpdate.OfficeAssignment == null)
                {
                    instructorToUpdate.OfficeAssignment = new OfficeAssignment { InstructorID = instructorToUpdate.ID };
                }
                instructorToUpdate.OfficeAssignment.Location = OfficeLocation;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                    db.SaveChanges();
                    SendEntityNotification("Instructor", instructorToUpdate.ID.ToString(), EntityOperation.UPDATE);
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.CourseAssignments.Select(c => c.Course.CourseID));
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments.SingleOrDefault(i => i.CourseID == course.CourseID);
                        db.Entry(courseToRemove).State = EntityState.Deleted;
                    }
                }
            }
        }

        // GET: Instructors/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // POST: Instructors/Delete/5 - Only admins can delete instructors
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Instructor instructor = db.Instructors
              .Include(i => i.OfficeAssignment)
              .Where(i => i.ID == id)
              .Single();

            db.Instructors.Remove(instructor);

            var department = db.Departments
                .Where(d => d.InstructorID == id)
                .SingleOrDefault();
            if (department != null)
            {
                department.InstructorID = null;
            }

            db.SaveChanges();
            SendEntityNotification("Instructor", id.ToString(), EntityOperation.DELETE);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
