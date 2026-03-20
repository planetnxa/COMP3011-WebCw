using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebAppComp3011.Data;
using WebAppComp3011.Models;
using Xunit;

namespace Tests.ApiTests;

public class UserProfileApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserProfileApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FragranceContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<FragranceContext>(options =>
                {
                    options.UseInMemoryDatabase("TestFragranceDb");
                });

                var userDesc = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FragranceContext>));
                if (userDesc != null) services.Remove(userDesc);
                services.AddDbContext<FragranceContext>(opt => opt.UseInMemoryDatabase("TestTodoDb"));
            });
        });
    }

    [Fact]
    public async Task Register_User_Returns_Created()
    {
        var client = _factory.CreateClient();

        var newUser = new UserProfile { Username = "testuser", Password = "password", Name = "Tester", FirstLogin = true };
        var post = await client.PostAsJsonAsync("/api/userprofile", newUser);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);

        var get = await client.GetAsync($"/api/userprofile/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }
}
