using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductivitySystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ExternalTask> Tasks => Set<ExternalTask>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<Metric> Metrics => Set<Metric>();

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

        modelBuilder.Entity<TimeLog>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeLog>()
            .HasOne(t => t.Task)
            .WithMany(t => t.TimeLogs)
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}