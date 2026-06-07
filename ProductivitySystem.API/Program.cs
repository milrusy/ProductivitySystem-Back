using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(
            CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(
              builder.Configuration
                  .GetConnectionString("DefaultConnection")
          ));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserManagementService,
    UserManagementService>();
builder.Services.AddScoped<IUserAnalyticsService,
    UserAnalyticsService>();
builder.Services.AddScoped<IReportService,
    ReportService>();
builder.Services.AddScoped<
    IMetricsCalculationService,
    MetricsCalculationService>();
builder.Services.AddScoped<IAlertService,
    AlertService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMappingService, MappingService>();

builder.Services.AddHttpClient<IGitHubService, GitHubService>();

builder.Services.AddScoped<GitHubSyncService>();
builder.Services.AddHostedService<GitHubSyncBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!
            )
        )
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //DbSeeder.Seed(db);
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireAuthorizationFilter()
    }
});

RecurringJob.AddOrUpdate<IAlertService>(
    "generate-alerts",

    service => service.GenerateAlerts(),

    Cron.Minutely
);

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

RecurringJob.AddOrUpdate<IMetricsCalculationService>(
    "calculate-metrics",

    service => service.CalculateMetrics(),

    Cron.Hourly
);

app.Run();