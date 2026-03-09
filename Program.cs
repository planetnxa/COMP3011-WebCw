using Microsoft.EntityFrameworkCore;
using WebAppComp3011.Models;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));

// something something dependency injections
/* data to read

 https://blog.intertoons.com/simplified-guide-to-documenting-asp-net-web-api-with-swagger-f4c40731f90c
https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0
https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-10.0&tabs=visual-studio#test-the-get-endpoints
 https://fragdb.net/ 50 sample, you might have too many in the og db

allegedly the swagger should only be enabled in development. that's somewhere here

 */
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
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    }); // baby what does this do????
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
