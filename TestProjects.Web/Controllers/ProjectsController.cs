using Microsoft.AspNetCore.Mvc;
using TestProjects.BL.DTOs;
using TestProjects.BL.Services;
using TestProjects.DAL.Models;

namespace TestProjects.Web.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectService _projectService;

        // Dependency Injection: Injecting the service layer to decouple the controller from the database context
        public ProjectsController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> List(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy = "name", string sortOrder = "asc")
        {
            // State Retention: Persist filter and sorting parameters in ViewBag to re-populate UI components
            ViewBag.StartDateFrom = startDateFrom?.ToString("yyyy-MM-dd");
            ViewBag.StartDateTo = startDateTo?.ToString("yyyy-MM-dd");
            ViewBag.Priority = priority;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            // Delegate query compilation and data retrieval to the business logic layer
            var projects = await _projectService.GetProjectsListAsync(startDateFrom, startDateTo, priority, sortBy, sortOrder);

            return View(projects);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.AllEmployees = _projectService.GetAllEmployees();
            return View(new ProjectCreateDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProjectCreateDto dto, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllEmployees = _projectService.GetAllEmployees();
                return View(dto);
            }

            try
            {
                await _projectService.CreateProjectAsync(dto, files);
                return RedirectToAction(nameof(List));
            }
            catch (ArgumentException ex)
            {
                // Intercept validation violations and push feedback state to the UI view model
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Handle logical business boundary errors gracefully
                ModelState.AddModelError("EndDate", ex.Message);
            }
            catch (Exception)
            {
                // General fallback handler for generic runtime structural crashes
                ModelState.AddModelError(string.Empty, "An unexpected system anomaly occurred. Please try again.");
            }

            // Retain state and return form context with localized alerts
            ViewBag.AllEmployees = _projectService.GetAllEmployees();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) 
                return NotFound();

            var project = await _projectService.GetProjectDetailsAsync(id.Value);

            if (project == null) 
                return NotFound();

            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
                return NotFound();

            var project = await _projectService.GetProjectForEditAsync(id.Value);

            if (project == null) 
                return NotFound();

            // Populate view bag with employee dictionaries for lookups and multi-select tracking
            ViewBag.AllEmployees = _projectService.GetAllEmployees();
            ViewBag.CurrentEmployeeIds = project.Employees.Select(e => e.Id).ToArray();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Project updatedProject, int[] selectedEmployeeIds, List<IFormFile> newFiles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllEmployees = _projectService.GetAllEmployees();
                ViewBag.CurrentEmployeeIds = selectedEmployeeIds ?? Array.Empty<int>();
                return View(updatedProject);
            }

            var result = await _projectService.UpdateProjectAsync(updatedProject, selectedEmployeeIds, newFiles);

            if (!result) 
                return NotFound();

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _projectService.DeleteProjectAsync(id);
            return RedirectToAction("List");
        }
    }
}

