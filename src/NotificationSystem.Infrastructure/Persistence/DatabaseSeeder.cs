using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Authorization;
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
        // Mapa de permissões sincronizado com Permissions.cs
        // (código da permissão, nome amigável, categoria)
        var permissionMap = new Dictionary<string, (string Name, string Category)>
        {
            // Roles
            { Permissions.RoleCreate, ("Criar Perfis", "Role") },
            { Permissions.RoleView, ("Visualizar Perfis", "Role") },
            { Permissions.RoleUpdate, ("Atualizar Perfis", "Role") },
            { Permissions.RoleDelete, ("Excluir Perfis", "Role") },

            // Users
            { Permissions.UserCreate, ("Criar Usuários", "User") },
            { Permissions.UserView, ("Visualizar Usuários", "User") },
            { Permissions.UserUpdate, ("Atualizar Usuários", "User") },
            { Permissions.UserDelete, ("Excluir Usuários", "User") },
            { Permissions.UserChangePassword, ("Alterar Senha", "User") },
            { Permissions.UserAssignRoles, ("Atribuir Funções", "User") },

            // Notifications
            { Permissions.NotificationCreate, ("Criar Notificações", "Notification") },
            { Permissions.NotificationView, ("Visualizar Notificações", "Notification") },
            { Permissions.NotificationStats, ("Estatísticas de Notificações", "Notification") },
            { Permissions.NotificationDelete, ("Excluir Notificações", "Notification") },

            // Bulk Notifications
            { Permissions.BulkNotificationCreate, ("Criar Notificações em Massa", "BulkNotification") },
            { Permissions.BulkNotificationView, ("Visualizar Notificações em Massa", "BulkNotification") },
            { Permissions.BulkNotificationCancel, ("Cancelar Notificações em Massa", "BulkNotification") },

            // Providers (Admin-only)
            { Permissions.ProviderCreate, ("Criar Provedores", "Provider") },
            { Permissions.ProviderView, ("Visualizar Provedores", "Provider") },
            { Permissions.ProviderUpdate, ("Atualizar Provedores", "Provider") },
            { Permissions.ProviderDelete, ("Excluir Provedores", "Provider") },
            { Permissions.ProviderUpload, ("Upload de Credenciais", "Provider") },
            { Permissions.ProviderToggle, ("Ativar/Desativar Provedores", "Provider") },
            { Permissions.ProviderSetPrimary, ("Definir Provedor Primário", "Provider") },

            // Dead Letter Queue (Crítico)
            { Permissions.DlqView, ("Visualizar DLQ", "DeadLetterQueue") },
            { Permissions.DlqReprocess, ("Reprocessar DLQ", "DeadLetterQueue") },
            { Permissions.DlqPurge, ("Limpar DLQ", "DeadLetterQueue") },

            // Permissions
            { Permissions.PermissionView, ("Visualizar Permissões", "Permission") },

            // Audit Logs
            { Permissions.AuditView, ("Visualizar Logs de Auditoria", "AuditLog") },
            { Permissions.AuditViewAll, ("Visualizar Todos os Logs", "AuditLog") },
            { Permissions.AuditExport, ("Exportar Logs de Auditoria", "AuditLog") }
        };

        var existingPermissions = await context.Permissions.ToListAsync();
        var existingCodes = existingPermissions.Select(p => p.Code).ToHashSet();

        // Detecta permissões novas (faltando no banco)
        var newPermissionCodes = permissionMap.Keys.Except(existingCodes).ToList();

        if (newPermissionCodes.Count == 0)
        {
            logger.LogInformation("All {Count} permissions already exist", permissionMap.Count);
            return;
        }

        // Cria apenas as permissões faltantes
        var newPermissions = newPermissionCodes.Select(code =>
        {
            var (name, category) = permissionMap[code];
            return new Permission
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Description = $"Permite {name.ToLower()}",
                Category = category,
                CreatedAt = DateTime.UtcNow
            };
        }).ToList();

        await context.Permissions.AddRangeAsync(newPermissions);
        await context.SaveChangesAsync();

        logger.LogInformation("Synced permissions: {Total} total, {New} added, {Existing} already exist",
            permissionMap.Count, newPermissions.Count, existingCodes.Count);
    }

    private static async Task SeedRolesAsync(NotificationDbContext context, ILogger logger)
    {
        var allPermissions = await context.Permissions.ToListAsync();

        // ========== ADMIN ROLE - FULL ACCESS (Sincroniza automaticamente) ==========
        var adminRole = await context.Roles.Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "Administrator");

        if (adminRole == null)
        {
            // Primeira execução: cria role Admin com todas as permissões
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Description = "Administrador do sistema com acesso total a todos os recursos",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            };

            adminRole.RolePermissions = [.. allPermissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })];

            await context.Roles.AddAsync(adminRole);
            logger.LogInformation("Created Administrator role with {Count} permissions", allPermissions.Count);
        }
        else
        {
            // Sincroniza: adiciona novas permissões ao admin sem remover as antigas
            var adminPermissionIds = adminRole.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var missingPermissions = allPermissions.Where(p => !adminPermissionIds.Contains(p.Id)).ToList();

            if (missingPermissions.Count > 0)
            {
                foreach (var permission in missingPermissions)
                {
                    adminRole.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow
                    });
                }

                context.Roles.Update(adminRole);
                logger.LogInformation("Updated Administrator role: added {Count} new permissions",
                    missingPermissions.Count);
            }
        }

        await context.SaveChangesAsync();

        // Verifica se as outras roles já foram criadas
        var otherRolesExist = await context.Roles.AnyAsync(r => r.Name != "Administrator");
        if (otherRolesExist)
        {
            logger.LogInformation("Other roles already exist, skipping seed");
            return;
        }

        // ========== MANAGER ROLE - GERENCIAMENTO DE USUÁRIOS E NOTIFICAÇÕES ==========
        var managerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Description = "Gerente com permissões de gerenciamento de usuários e notificações",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var managerPermissionCodes = new[]
        {
            // User management
            "user.create", "user.view", "user.update", "user.delete", "user.assign-roles",
            // Notification management
            "notification.create", "notification.view", "notification.stats", "notification.delete",
            // Bulk Notification management
            "bulk-notification.create", "bulk-notification.view", "bulk-notification.cancel",
            // Roles
            "role.view",
            // Audit Logs
            "audit.view",
            // Permission view
            "permission.view"
        };

        managerRole.RolePermissions = [.. allPermissions
            .Where(p => managerPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = managerRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })];

        // ========== DEVELOPER ROLE - GERENCIAMENTO DE PROVEDORES ==========
        var developerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Developer",
            Description = "Desenvolvedor com permissões de configuração de provedores",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var developerPermissionCodes = new[]
        {
            // Provider management
            "provider.create", "provider.view", "provider.update", "provider.delete",
            "provider.upload", "provider.toggle", "provider.set-primary",
            // Notification view
            "notification.view", "notification.stats",
            // Permission view
            "permission.view"
        };

        developerRole.RolePermissions = [.. allPermissions
            .Where(p => developerPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = developerRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })];

        // ========== OPERATOR ROLE - MONITORAMENTO DE DLQ ==========
        var operatorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            Description = "Operador com permissões de monitoramento e gerenciamento de filas",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var operatorPermissionCodes = new[]
        {
            // DLQ management
            "dlq.view", "dlq.reprocess",
            // Notification view
            "notification.view", "notification.stats",
            // Audit Logs
            "audit.view",
            // Permission view
            "permission.view"
        };

        operatorRole.RolePermissions = [.. allPermissions
            .Where(p => operatorPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = operatorRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })];

        // ========== VIEWER ROLE - APENAS LEITURA ==========
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
            // Read-only
            "notification.view", "notification.stats",
            "user.view",
            "role.view",
            "provider.view",
            "dlq.view",
            "permission.view"
        };

        viewerRole.RolePermissions = [.. allPermissions
            .Where(p => viewerPermissionCodes.Contains(p.Code))
            .Select(p => new RolePermission
            {
                RoleId = viewerRole.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })];

        await context.Roles.AddRangeAsync([managerRole, developerRole, operatorRole, viewerRole]);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} roles", 5);
    }

    private static async Task SeedAdminUserAsync(NotificationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        var adminEmail = "admin@example.com";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            logger.LogInformation("Admin user already exists, skipping seed");
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            FullName = "Administrador do Sistema",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        adminUser.UserRoles =
        [
            new()
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            }
        ];

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        logger.LogWarning("DEFAULT ADMIN PASSWORD: Admin123! - CHANGE THIS IMMEDIATELY IN PRODUCTION!");
    }
}
