using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using OrchidsShop.PresentationLayer.Constants;
using OrchidsShop.PresentationLayer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    ;

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("Settings: SessionOutSecs", 60));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddHttpClient<ApiHelper>(client =>
{
    client.BaseAddress = new Uri(StringValue.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddHttpContextAccessor();

// Register API services explicitly
builder.Services.AddScoped<ApiHelper>();
builder.Services.AddScoped<CategoryApiService>();
builder.Services.AddScoped<OrchidApiService>();
builder.Services.AddScoped<AccountApiService>();
builder.Services.AddScoped<OrderApiService>();

builder.Services.Scan(scan => scan
    .FromEntryAssembly()
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
