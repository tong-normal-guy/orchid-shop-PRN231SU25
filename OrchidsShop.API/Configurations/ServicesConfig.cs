using System;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrchidsShop.BLL.Commons;
using OrchidsShop.BLL.Services;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Repos;

namespace OrchidsShop.API.Configurations;

public static class ServicesConfig
{
    public static IServiceCollection AddServicesConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AutoMapper
        services.AddAutoMapper(typeof(MapperHelper));

        services.AddRoutings();
        services.AddControllersAndCors();
        services.AddSwaggers();

        services.AddContexts(configuration);

        // Register repositories and services
        services.AddServices();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IOrchidService, OrchidService>();
        services.AddTransient<IOrchidRepository, OrchidRepository>();

        return services;
    }

    public static IServiceCollection AddControllersAndCors(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use original property names
                options.JsonSerializerOptions.DictionaryKeyPolicy = null; // Use original dictionary keys
            });
        services.AddCors(options =>
        {
            options.AddPolicy("Cors", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
    public static IServiceCollection AddRoutings(this IServiceCollection services)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        return services;
    }

    public static IServiceCollection AddSwaggers(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.CustomSchemaIds(type => type.FullName);

            opt.EnableAnnotations();

            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });

        return services;
    }

    public static IServiceCollection AddContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrchidShopDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DBDefault");
            options.UseSqlServer(connectionString);
        }, ServiceLifetime.Scoped);

        // Register DbContext as the base type for UnitOfWork
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<OrchidShopDbContext>());

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
