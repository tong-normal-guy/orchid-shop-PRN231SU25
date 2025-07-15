using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrchidsShop.BLL.Commons;
using OrchidsShop.BLL.Services;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Enums;
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

        // Add authentication and authorization
        services.AddAuths(configuration);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IOrchidService, OrchidService>();
        services.AddScoped<OrchidCategoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<AccountService>();
        
        services.AddTransient<IOrchidRepository, OrchidRepository>();

        return services;
    }

    public static IServiceCollection AddControllersAndCors(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        services.AddCarter();

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
            /*opt.SchemaGeneratorOptions*/

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

    public static IServiceCollection AddAuths(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
            };
            opt.RequireHttpsMetadata = false;
            opt.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(option =>
        {
            option.AddPolicy(EnumAccountRole.ADMIN.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, EnumAccountRole.ADMIN.ToString(), "true"));
            option.AddPolicy(EnumAccountRole.CUSTOMER.ToString(), policy => policy.RequireClaim(ClaimTypes.Role, EnumAccountRole.CUSTOMER.ToString(), "true"));
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("Cors",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        return services;
    }
}
