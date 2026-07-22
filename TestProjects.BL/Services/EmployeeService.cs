using Microsoft.EntityFrameworkCore;
using TestProjects.DAL;
using TestProjects.DAL.Models;

namespace TestProjects.BL.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        //Database seeding: automatically populate the database with demo data if it is empty
        public async Task<List<Employee>> GetEmployeesListAsync(string searchString)
        {
            if (!_context.Employees.Any())
            {
                _context.Employees.AddRange(
                    new Employee { FullName = "Ivan Ivanov", Email = "ivanov@company" },
                    new Employee { FullName = "Alex Tihonov ", Email = "tihonov@company" },
                    new Employee { FullName = "Tom Sawyer", Email = "sawyer@company" }
                );
                await _context.SaveChangesAsync();
            }

            // Deferred Execution: using IQueryable to dynamically build the SQL query before execution
            var employeesQuery = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                employeesQuery = employeesQuery.Where(e => e.FullName.Contains(searchString) || e.Email.Contains(searchString));

            // AsNoTracking is used here to optimize read-only query performance
            return await employeesQuery.AsNoTracking().ToListAsync();
        }

        public async Task CreateEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) 
                return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
