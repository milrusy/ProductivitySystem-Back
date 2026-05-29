using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateMetricsCsv();

    Task<byte[]> GenerateMetricsPdf(ExportPdfDto dto);
}
