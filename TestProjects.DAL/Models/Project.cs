namespace TestProjects.DAL.Models
{
    public class Project
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = String.Empty;
        public string CustomerCompany { get; set; } = String.Empty;
        public string CExecutorCompany { get; set; } = String.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Priority { get; set; }

        // Foreign key and navigation property for the Project Manager (One-to-Many relation)
        public int? ProjectManagerId { get; set; }
        public Employee? ProjectManager { get; set; }

        // Collection of project participants (Many-to-Many relation)
        public List<Employee> Employees { get; set; } = new();
        public string DocumentPaths { get; set; } = String.Empty;
    }
}
