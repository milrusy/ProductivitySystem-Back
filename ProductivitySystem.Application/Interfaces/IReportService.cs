namespace ProductivitySystem.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateMetricsCsv();

    Task<byte[]> GenerateMetricsPdf();
}
