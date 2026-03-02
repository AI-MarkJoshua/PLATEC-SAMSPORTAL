using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Controllers
{
    public class SectionsController : Controller
    {
        private readonly AdminWebPageContext _context;

        public SectionsController(AdminWebPageContext context)
        {
            _context = context;
        }

        // GET: Sections
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            var sections = await _context.Sections
                .Include(s => s.TeacherSections)
                .ThenInclude(ts => ts.Teacher)
                .OrderBy(s => s.SectionName)
                .ToListAsync();
            
            return View(sections);
        }

        // GET: Sections/Create
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: Sections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SectionID,SectionName")] Section section)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                // Check if section name already exists
                var existingSection = await _context.Sections
                    .FirstOrDefaultAsync(s => s.SectionName.ToLower() == section.SectionName.ToLower());
                
                if (existingSection != null)
                {
                    ModelState.AddModelError("SectionName", "A section with this name already exists.");
                    return View(section);
                }

                section.CreatedAt = DateTime.Now;
                _context.Add(section);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Section created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(section);
        }

        // GET: Sections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            
            return View(section);
        }

        // POST: Sections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SectionID,SectionName")] Section section)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id != section.SectionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if section name already exists (excluding this section)
                    var existingSection = await _context.Sections
                        .FirstOrDefaultAsync(s => s.SectionName.ToLower() == section.SectionName.ToLower() && s.SectionID != id);
                    
                    if (existingSection != null)
                    {
                        ModelState.AddModelError("SectionName", "A section with this name already exists.");
                        return View(section);
                    }

                    _context.Update(section);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Section updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.SectionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(section);
        }

        // GET: Sections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.TeacherSections)
                .ThenInclude(ts => ts.Teacher)
                .FirstOrDefaultAsync(m => m.SectionID == id);
                
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // POST: Sections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Only Admin can access sections management
            if (userRole != "Admin")
            {
                TempData["Error"] = "Access Denied. Only Admins can manage sections.";
                return RedirectToAction("Index", "Dashboard");
            }

            var section = await _context.Sections
                .Include(s => s.TeacherSections)
                .FirstOrDefaultAsync(s => s.SectionID == id);
                
            if (section != null)
            {
                // Check if any teachers are assigned to this section
                if (section.TeacherSections.Any())
                {
                    TempData["Error"] = "Cannot delete section. Teachers are still assigned to this section.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Section deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.SectionID == id);
        }
    }
}
