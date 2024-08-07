using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class TeacherGroupsController : Controller
    {
        private readonly StudentManagementContext _context;

        public TeacherGroupsController(StudentManagementContext context)
        {
            _context = context;
        }

        // GET: TeacherGroups/Index/5
        public async Task<IActionResult> Index(int? teacherId)
        {
            if (teacherId == null)
            {
                return NotFound();
            }

            var groups = await _context.Groups
                .Include(g => g.Departments)
                .Include(g => g.Terms)
                .Include(g => g.Accounts)
                    .ThenInclude(a => a.Users)
                .Where(g => g.Teacher_id == teacherId)
                .ToListAsync();

            if (groups == null || groups.Count == 0)
            {
                return NotFound();
            }

            ViewBag.TeacherId = teacherId;
            return View(groups);
        }

        // GET: TeacherGroups/GroupDetails/5
        public async Task<IActionResult> GroupDetails(int? groupId)
        {
            if (groupId == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Departments)
                .Include(g => g.Terms)
                .Include(g => g.Accounts)
                    .ThenInclude(a => a.Users)
                .FirstOrDefaultAsync(m => m.Id == groupId);

            if (group == null)
            {
                return NotFound();
            }

            var students = await _context.Group_student
                .Include(gs => gs.Accounts)
                    .ThenInclude(a => a.Users)
                .Where(gs => gs.Group_id == groupId)
                .ToListAsync();

            ViewBag.Students = students;
            ViewBag.TeacherId = group.Teacher_id; // Ensure the TeacherId is set correctly

            return View(group);
        }

        // POST: TeacherGroups/Attend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attend(int groupId, IFormCollection formCollection)
        {
            var studentIds = _context.Group_student
                .Where(gs => gs.Group_id == groupId)
                .Select(gs => gs.Student_id)
                .ToList();

            foreach (var studentId in studentIds)
            {
                var attendanceStatus = formCollection[$"attendanceStatus_{studentId}"];
                if (!string.IsNullOrEmpty(attendanceStatus))
                {
                    var groupStudent = await _context.Group_student
                        .FirstOrDefaultAsync(gs => gs.Group_id == groupId && gs.Student_id == studentId);

                    if (groupStudent != null)
                    {
                        groupStudent.Absent = attendanceStatus == "0" ? (byte)1 : (byte)0;
                        groupStudent.Present = attendanceStatus == "1" ? (byte)1 : (byte)0;

                        _context.Update(groupStudent);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("GroupDetails", new { groupId = groupId });
        }
    }
}
