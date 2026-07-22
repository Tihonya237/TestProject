namespace TestProjects.BL.DTOs
{
    public class ProjectCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string CustomerCompany { get; set; } = string.Empty;
        public string CExecutorCompany { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);
        public int Priority { get; set; }
        public int? ProjectManagerId { get; set; }
        public int[] SelectedEmployeeIds { get; set; } = Array.Empty<int>();
        public List<IFormFile> Files { get; set; } = new();
    }
}
