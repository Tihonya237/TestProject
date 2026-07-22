using System.ComponentModel.DataAnnotations.Schema;

namespace TestProjects.DAL.Models
{
    public class Employee
    {
        public int Id { get; set; } = 0;
        public string FullName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;

        // Explicit mapping for Many-to-Many relation (An employee can be a participant in multiple projec
        [InverseProperty("Employees")]
        public List<Project> Projects { get; set; } = new();

        // Explicit mapping for One-to-Many relation (An employee can manage multiple projects)
        [InverseProperty("ProjectManager")]
        public List<Project> ManagedProjects { get; set; } = new();
    }
}
