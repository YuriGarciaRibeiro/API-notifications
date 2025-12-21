using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationSystem.Infrastructure.Persistence;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        // Connection string para desenvolvimento
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notifications;Username=postgres;Password=postgres");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
