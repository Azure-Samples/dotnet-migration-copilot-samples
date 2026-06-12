using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class CourseAssignment
{
    public int InstructorID { get; set; }
    public int CourseID { get; set; }
    public virtual Instructor Instructor { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}
