using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Permissions
            await SeedPermissionsAsync(context, logger);

            // Seed Roles
            await SeedRolesAsync(context, logger);

            // Seed Admin User
            await SeedAdminUserAsync(context, passwordHasher, logger);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedPermissionsAsync(NotificationDbContext context, ILogger logger)
    {
        if (await context.Permissions.AnyAsync())
        {
            logger.LogInformation("Permissions already exist, skipping seed");
            return;
        }

        var permissions = new List<Permission>
        {
            // Notification permissions
            new() { Id = Guid.NewGuid(), Code = "notification.send", Name = "Enviar Notificações", Description = "Permite enviar notificações", Category = "Notification", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "notification.view", Name = "Visualizar Notificações", Description = "Permite visualizar notificações", Category = "Notification", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "notification.delete", Name = "Excluir Notificações", Description = "Permite excluir notificações", Category = "Notification", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "notification.stats", Name = "Estatísticas de Notificações", Description = "Permite visualizar estatísticas de notificações", Category = "Notification", CreatedAt = DateTime.UtcNow },

            // Channel permissions
            new() { Id = Guid.NewGuid(), Code = "channel.create", Name = "Criar Canais", Description = "Permite criar canais de notificação", Category = "Channel", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "channel.update", Name = "Atualizar Canais", Description = "Permite atualizar canais de notificação", Category = "Channel", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "channel.delete", Name = "Excluir Canais", Description = "Permite excluir canais de notificação", Category = "Channel", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "channel.view", Name = "Visualizar Canais", Description = "Permite visualizar canais de notificação", Category = "Channel", CreatedAt = DateTime.UtcNow },

            // User permissions
            new() { Id = Guid.NewGuid(), Code = "user.create", Name = "Criar Usuários", Description = "Permite criar usuários", Category = "User", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "user.update", Name = "Atualizar Usuários", Description = "Permite atualizar usuários", Category = "User", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "user.delete", Name = "Excluir Usuários", Description = "Permite excluir usuários", Category = "User", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "user.view", Name = "Visualizar Usuários", Description = "Permite visualizar usuários", Category = "User", CreatedAt = DateTime.UtcNow },

            // Role permissions
            new() { Id = Guid.NewGuid(), Code = "role.create", Name = "Criar Perfis", Description = "Permite criar perfis", Category = "Role", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "role.update", Name = "Atualizar Perfis", Description = "Permite atualizar perfis", Category = "Role", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "role.delete", Name = "Excluir Perfis", Description = "Permite excluir perfis", Category = "Role", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "role.view", Name = "Visualizar Perfis", Description = "Permite visualizar perfis", Category = "Role", CreatedAt = DateTime.UtcNow },

            // Settings permissions
            new() { Id = Guid.NewGuid(), Code = "settings.manage", Name = "Gerenciar Configurações", Description = "Permite gerenciar configurações do sistema", Category = "Settings", CreatedAt = DateTime.UtcNow }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} permissions", permissions.Count);
    }

    private static async Task SeedRolesAsync(NotificationDbContext context, ILogger logger)
    {
        if (await context.Roles.AnyAsync())
        {
            logger.LogInformation("Roles already exist, skipping seed");
            return;
        }

        var allPermissions = await context.Permissions.ToListAsync();

        // Admin role - all permissions
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "Administrador do sistema com acesso total",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        adminRole.RolePermissions = allPermissions.Select(p => new RolePermission
        {
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            GrantedAt = DateTime.UtcNow
        }).ToList();

        // Manager role
        var managerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Description = "Gerente com permissões de gerenciamento",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var managerPermissionCodes = new[]
        {
            "notification.send", "notification.view", "notification.delete", "notification.stats",
            "channel.view", "channel.update",
            "user.view"
        };

        managerRole.RolePermissions = allPermissions
            .Where(p => managerPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = managerRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            }).ToList();

        // Operator role
        var operatorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            Description = "Operador com permissões básicas de operação",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var operatorPermissionCodes = new[]
        {
            "notification.send", "notification.view",
            "channel.view"
        };

        operatorRole.RolePermissions = allPermissions
            .Where(p => operatorPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = operatorRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            }).ToList();

        // Viewer role
        var viewerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Viewer",
            Description = "Visualizador apenas com permissões de leitura",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var viewerPermissionCodes = new[]
        {
            "notification.view", "notification.stats",
            "channel.view"
        };

        viewerRole.RolePermissions = allPermissions
            .Where(p => viewerPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = viewerRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            }).ToList();

        await context.Roles.AddRangeAsync(new[] { adminRole, managerRole, operatorRole, viewerRole });
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} roles", 4);
    }

    private static async Task SeedAdminUserAsync(NotificationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        var adminEmail = "admin@example.com";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            logger.LogInformation("Admin user already exists, skipping seed");
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            FullName = "Administrador do Sistema",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        adminUser.UserRoles = new List<UserRole>
        {
            new()
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        logger.LogWarning("DEFAULT ADMIN PASSWORD: Admin123! - CHANGE THIS IMMEDIATELY IN PRODUCTION!");
    }
}
