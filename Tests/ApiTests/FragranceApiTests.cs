using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebAppComp3011.Data;
using WebAppComp3011.Models;
using Xunit;

namespace Tests.ApiTests;

public class FragranceApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FragranceApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace FragranceContext with InMemory DB for tests
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FragranceContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<FragranceContext>(options =>
                {
                    options.UseInMemoryDatabase("TestFragranceDb");
                });
            });
        });
    }

    [Fact]
    public async Task Get_Fragrance_Returns_NotFound_For_Missing()
    {
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/fragrances/9999");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Create_And_Get_Fragrance()
    {
        var client = _factory.CreateClient();

        var newFrag = new Fragrance { FragName = "Test", Brand = "X" };
        var post = await client.PostAsJsonAsync("/api/frag", newFrag);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);

        var get = await client.GetAsync($"/api/frag/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }
}
