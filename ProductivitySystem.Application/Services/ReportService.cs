using System.Text;
using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ProductivitySystem.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateMetricsCsv()
    {
        var metrics = await _context.Metrics
            .Include(m => m.User)
            .ToListAsync();

        var sb = new StringBuilder();

        sb.AppendLine(
            "Employee,CompletedTasks,OverdueTasks,AvgCompletionTime,ProductivityScore"
        );

        foreach (var metric in metrics)
        {
            sb.AppendLine(
                $"{metric.User.Name}," +
                $"{metric.CompletedTasks}," +
                $"{metric.OverdueTasks}," +
                $"{metric.AvgCompletionTime}," +
                $"{metric.ProductivityScore}"
            );
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateMetricsPdf(ExportPdfDto dto)
    {
        var metrics = await _context.Metrics
            .Include(m => m.User)
            .ToListAsync();

        QuestPDF.Settings.License =
            LicenseType.Community;

        var base64 = dto.ChartImage
            .Split(",")[1];

        var imageBytes =
            Convert.FromBase64String(base64);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text("Employee Productivity Report")
                    .FontSize(24)
                    .Bold();

                page.Content()
                    .Column(column =>
                    {
                        column.Item()
                            .PaddingBottom(20)
                            .Image(imageBytes)
                            .FitWidth();

                        column.Item()
                            .Text("Productivity Metrics")
                            .FontSize(18)
                            .Bold();

                        foreach (var metric in metrics)
                        {
                            column.Item()
                                .PaddingVertical(10)
                                .BorderBottom(1)
                                .Column(item =>
                                {
                                    item.Item().Text(
                                        $"Employee: {metric.User.Name}")
                                        .Bold();

                                    item.Item().Text(
                                        $"Completed Tasks: {metric.CompletedTasks}");

                                    item.Item().Text(
                                        $"Overdue Tasks: {metric.OverdueTasks}");

                                    item.Item().Text(
                                        $"Average Completion Time: {metric.AvgCompletionTime:F2} h");

                                    item.Item().Text(
                                        $"Productivity Score: {metric.ProductivityScore:F2}");
                                });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated: ");

                        x.Span(DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm"));
                    });
            });
        });

        return document.GeneratePdf();
    }
}
