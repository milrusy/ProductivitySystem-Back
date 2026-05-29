namespace ProductivitySystem.Application.DTOs; 

public class ExportPdfDto
{
    public string ChartImage { get; set; }
    public ReportFiltersDto Filters { get; set; }
}
