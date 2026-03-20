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
builder.Services.AddScoped<UserProfileService>();

// Register HttpClient with base address for API calls
string baseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5000";
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(10);
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

    // to add context to our web api
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
} );

// something something dependency injections

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //and use swashbuckly nd ting
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "D'Arome API v1");
    }); // baby what does this do????
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(name:"default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for frontend
app.MapRazorPages();

app.Run();
