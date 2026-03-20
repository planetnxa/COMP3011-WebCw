using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebAppComp3011.Models;
using Microsoft.Extensions.DependencyInjection;
using WebAppComp3011.Data;
using WebAppComp3011.Services;
using Microsoft.OpenApi;
using System.Reflection;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FragranceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FragranceContext") ?? throw new InvalidOperationException("Connection string 'FragranceContext' not found.")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Enable Razor Pages for frontend views
builder.Services.AddRazorPages();

// Register services for dependency injection
builder.Services.AddScoped<UserProfileController>();

// Register HttpClient with base address for API calls
string baseAddress = builder.Configuration["ApiBaseAddress"] ?? "";
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "D'Arome API",
        Description = "An ASP.NET Core Web API for managing fragrances and user profiles."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "D'Arome API v1");
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.Run();

public partial class Program { }
