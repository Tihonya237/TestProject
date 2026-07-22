using Microsoft.AspNetCore.Mvc;
using TestProjects.BL.Services;
using TestProjects.DAL.Models;

namespace TestProjects.Web.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeService _employeeService;

        // Dependency Injection: Injecting the logic layer service to decouple the controller from EF Core
        public EmployeesController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> List(string searchString)
        {
            // Retain the search query value in ViewBag to populate the UI input field after reload
            ViewBag.CurrentFilter = searchString;

            var employees = await _employeeService.GetEmployeesListAsync(searchString);

            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            // Server-side validation: check if the model strictly complies with data annotations
            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(employee);

                return RedirectToAction("List");
            }

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) 
                return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);

            if (employee == null) 
                return NotFound();

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
                return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);

            if (employee == null) 
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                return RedirectToAction("List");
            }

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction("List");
        }
    }
}
