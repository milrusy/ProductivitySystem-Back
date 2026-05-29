using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductivitySystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<Department> Departments => Set<Department>();
    public virtual DbSet<ExternalTask> Tasks => Set<ExternalTask>();
    public virtual DbSet<ExternalSource> Sources => Set<ExternalSource>();
    public virtual DbSet<Metric> Metrics => Set<Metric>();
    public virtual DbSet<Alert> Alerts => Set<Alert>();
    public virtual DbSet<ExternalUserMapping> ExternalUserMappings => Set<ExternalUserMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId);

        modelBuilder.Entity<ExternalTask>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.AssigneeId);
    }
}