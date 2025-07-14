using Carter;
using OrchidsShop.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServicesConfig(builder.Configuration);

// Register repositories and services by scutor
// builder.Services.Scan(scan => scan
//     .FromApplicationDependencies()
//     .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
//     .AsImplementedInterfaces()
//     .WithTransientLifetime()
// );

// builder.Services.Scan(scan => scan
//     .FromApplicationDependencies()
//     .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
//     .AsImplementedInterfaces()
//     .WithTransientLifetime()
// );

// Build the application without validating scopes to avoid startup issues
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Cors");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();
app.MapControllers();

app.Run();
