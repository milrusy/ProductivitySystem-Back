using ProductivitySystem.Domain.Entities;

namespace ProductivitySystem.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        var random = new Random();

        var departments = new List<Department>
        {
            new Department { Name = "Development", ExternalId = "GitHub"},
            new Department { Name = "Marketing", ExternalId = "GitHub" },
            new Department { Name = "HR", ExternalId = "GitHub" }
        };

        context.Departments.AddRange(departments);
        context.SaveChanges();

        var users = new List<User>();

        for (int i = 1; i <= 15; i++)
        {
            users.Add(new User
            {
                ExternalId = $"user-{i}",
                Role = "Employee",
                PasswordHash = "123456",
                Name = $"User {i}",
                Email = $"user{i}@test.com",
                DepartmentId = departments[random.Next(departments.Count)].Id
            });
        }

        context.Users.AddRange(users);
        context.SaveChanges();

        var source = new ExternalSource
        {
            Name = "Jira",
            Type = "API"
        };

        context.Set<ExternalSource>().Add(source);
        context.SaveChanges();

        var tasks = new List<ExternalTask>();

        for (int i = 1; i <= 120; i++)
        {
            var user = users[random.Next(users.Count)];

            var created = DateTime.UtcNow.AddDays(-random.Next(30));
            var deadline = created.AddDays(random.Next(1, 10));

            var isDone = random.NextDouble() > 0.3;

            DateTime? completed = null;

            if (isDone)
            {
                completed = created.AddDays(random.Next(1, 12));
            }

            tasks.Add(new ExternalTask
            {
                ExternalId = $"task-{i}",
                Title = $"Task {i}",
                Status = isDone ? "Done" : "InProgress",
                Priority = random.Next(3) switch
                {
                    0 => "Low",
                    1 => "Medium",
                    _ => "High"
                },
                AssigneeId = user.Id,
                SourceId = source.Id,
                CreatedAt = created,
                Deadline = deadline,
                CompletedAt = completed,
                EstimatedTime = random.Next(2, 10)
            });
        }

        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        var logs = new List<TimeLog>();

        foreach (var task in tasks)
        {
            if (random.NextDouble() > 0.3)
            {
                logs.Add(new TimeLog
                {
                    TaskId = task.Id,
                    UserId = task.AssigneeId,
                    TimeSpent = random.Next(1, 12),
                    LogDate = task.CreatedAt.AddDays(random.Next(1, 5))
                });
            }
        }

        context.TimeLogs.AddRange(logs);
        context.SaveChanges();
    }
}
