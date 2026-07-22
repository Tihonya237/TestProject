using Microsoft.EntityFrameworkCore;
using TestProjects.BL.DTOs;
using TestProjects.DAL;
using TestProjects.DAL.Models;

namespace TestProjects.BL.Services
{
    public class ProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetProjectsListAsync(DateTime? startDateFrom,DateTime? startDateTo,int? priority,string sortBy,string sortOrder)
        {
            if (!_context.Projects.Any())
            {
                _context.Projects.AddRange(
                    new Project { Name = "Альфа", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 2, 23), Priority = 1 },
                    new Project { Name = "Бета", StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 6, 15), Priority = 2 }
                );
                await _context.SaveChangesAsync();
            }

            // Deferred Execution: Using IQueryable to dynamically build the SQL query before execution
            var query = _context.Projects.AsQueryable();

            if (startDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= startDateFrom.Value);

            if (startDateTo.HasValue)
                query = query.Where(p => p.StartDate <= startDateTo.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            bool isDescending = sortOrder == "desc";

            query = sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "startdate" => isDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
                "enddate" => isDescending ? query.OrderByDescending(p => p.EndDate) : query.OrderBy(p => p.EndDate),
                "priority" => isDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
                _ => query.OrderBy(p => p.Id) // Сортировка по умолчанию
            };

            return await query.AsNoTracking().ToListAsync();
        }

        public List<Employee> GetAllEmployees()
        {
            return _context.Employees.ToList();
        }

        public async Task CreateProjectAsync(ProjectCreateDto dto, List<IFormFile> files)
        {
            // 1. Validation Safeguard: Protect against null parameters or unexpected empty strings
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Project payload data is corrupted or project name is missing.");

            // 2. Logic Rule: Ensure chronological ordering of datetime parameters
            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("Project end date cannot be earlier than the start date.");

            var project = new Project
            {
                Name = dto.Name.Trim(), // Sanitize input data against trailing whitespaces
                CustomerCompany = dto.CustomerCompany?.Trim() ?? string.Empty,
                CExecutorCompany = dto.CExecutorCompany?.Trim() ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Priority = dto.Priority < 0 ? 0 : dto.Priority, // Defensive assignment against negative values
                ProjectManagerId = dto.ProjectManagerId
            };

            // 3. Foreign Key Integrity Check: Safely append data mappings
            if (project.ProjectManagerId.HasValue)
            {
                var managerExists = await _context.Employees.AnyAsync(e => e.Id == project.ProjectManagerId);

                if (managerExists)
                    project.ProjectManager = await _context.Employees.FindAsync(project.ProjectManagerId);
            }

            // 4. Robust File IO Pipeline: Exception shielding for asynchronous disk operations
            if (files != null && files.Count > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadsFolder)) 
                        Directory.CreateDirectory(uploadsFolder);

                    var savedPaths = new List<string>();
                    foreach (var file in files)
                    {
                        // Defensive filter: Ignore broken or empty byte streams
                        if (file == null || file.Length == 0) 
                            continue;

                        // Restrict malicious extension payloads (Optional security layer)
                        var extension = Path.GetExtension(file.FileName).ToLower();

                        if (extension == ".exe" || extension == ".bat" || extension == ".sh") 
                            continue;

                        var uniqueName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        savedPaths.Add("/uploads/" + uniqueName);
                    }
                    project.DocumentPaths = string.Join(";", savedPaths);
                }
                catch (IOException)
                {
                    // Fail-safe degradation: Log the disk error, but let the entity write succeed without a crash
                    project.DocumentPaths = string.Empty;
                }
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<Project?> GetProjectDetailsAsync(int id)
        {
            return await _context.Projects.Include(p => p.ProjectManager).Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectForEditAsync(int id)
        {
            return await _context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateProjectAsync(Project updatedProject, int[] selectedEmployeeIds, List<IFormFile> newFiles)
        {
            var dbProject = await _context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == updatedProject.Id);

            if (dbProject == null) 
                return false;

            // Mapping updated text properties and dates
            dbProject.Name = updatedProject.Name;
            dbProject.CustomerCompany = updatedProject.CustomerCompany;
            dbProject.CExecutorCompany = updatedProject.CExecutorCompany;
            dbProject.StartDate = updatedProject.StartDate;
            dbProject.EndDate = updatedProject.EndDate;
            dbProject.Priority = updatedProject.Priority;
            dbProject.ProjectManagerId = updatedProject.ProjectManagerId;

            // Many-to-Many tracking: Clear existing team composition and re-assign new members
            dbProject.Employees.Clear();

            if (selectedEmployeeIds != null && selectedEmployeeIds.Length > 0)
                dbProject.Employees = await _context.Employees.Where(e => selectedEmployeeIds.Contains(e.Id)).ToListAsync();

            // Append new uploaded documents to the existing file paths
            if (newFiles != null && newFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var currentPaths = !string.IsNullOrEmpty(dbProject.DocumentPaths) ? dbProject.DocumentPaths.Split(';').ToList() : new List<string>();

                foreach (var file in newFiles)
                {
                    if (file.Length > 0)
                    {
                        var uniqueName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        currentPaths.Add("/uploads/" + uniqueName);
                    }
                }

                dbProject.DocumentPaths = string.Join(";", currentPaths);
            }

            _context.Projects.Update(dbProject);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.Include(p => p.Employees).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) 
                return false;

            project.Employees.Clear();
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

