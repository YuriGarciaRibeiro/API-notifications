using System.Reflection;
using Microsoft.OpenApi;

namespace NotificationSystem.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notification System API",
                Version = "v1",
                Description = "API REST para gerenciamento e envio de notificações por Email, SMS e Push Notifications",
                Contact = new OpenApiContact
                {
                    Name = "Yuri Garcia Ribeiro",
                    Url = new Uri("https://github.com/YuriGarciaRibeiro/API-notifications")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Suporte a XML comments (se existir)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Suporte a enums como strings
            options.UseAllOfToExtendReferenceSchemas();

            // Configurar polimorfismo para Channel DTOs
            options.UseOneOfForPolymorphism();
            options.SelectDiscriminatorNameUsing(_ => "type");
            options.SelectDiscriminatorValueUsing(subType => subType.Name switch
            {
                "EmailChannelDto" => "Email",
                "SmsChannelDto" => "Sms",
                "PushChannelDto" => "Push",
                _ => subType.Name
            });

            // Customizar schemas
            options.CustomSchemaIds(type => type.Name.Replace("Dto", "").Replace("Response", ""));
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification System API v1");
            options.RoutePrefix = "swagger";

            // Configurações de UI
            options.DocumentTitle = "Notification System API - Documentation";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(2);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();

            // Tema e customização
            if (env.IsDevelopment())
            {
                options.EnableTryItOutByDefault();
            }
        });

        return app;
    }
}
