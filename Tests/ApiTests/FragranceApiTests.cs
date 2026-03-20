using System.Net;
using System.Net.Http.Json;
using WebAppComp3011.Models;
using Xunit;

namespace Tests.ApiTests;

public class FragranceApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FragranceApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Fragrance_Returns_NotFound_For_Missing()
    {
        var res = await _client.GetAsync("/api/frag/9999");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Create_And_Get_Fragrance()
    {
        var newFrag = new Fragrance { FragName = "Test", Brand = "X" };
        var post = await _client.PostAsJsonAsync("/api/frag", newFrag);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);

        var get = await _client.GetAsync($"/api/frag/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }
}
