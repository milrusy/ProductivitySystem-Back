namespace ProductivitySystem.Application.DTOs; 

public class ReportFiltersDto
{
    public string SelectedUser { get; set; }
    public string SelectedDepartment { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
