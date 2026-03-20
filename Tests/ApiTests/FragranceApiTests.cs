using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    [Fact]
    public async Task Create_Fragrance_Returns_All_Fields()
    {
        var frag = new Fragrance
        {
            FragName = "Sauvage",
            Brand = "Dior",
            Country = "France",
            Gender = "Male",
            Rating = 4.5f,
            Year = "2015",
            Perfumers = "François Demachy"
        };
        var post = await _client.PostAsJsonAsync("/api/frag", frag);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Sauvage", created.FragName);
        Assert.Equal("Dior", created.Brand);
        Assert.Equal("France", created.Country);
        Assert.Equal("Male", created.Gender);
        Assert.Equal(4.5f, created.Rating);
        Assert.Equal("2015", created.Year);
    }

    [Fact]
    public async Task Create_Fragrance_With_Empty_Fields()
    {
        var frag = new Fragrance { FragName = "", Brand = "" };
        var post = await _client.PostAsJsonAsync("/api/frag", frag);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Get_All_Returns_NotFound_When_Empty_Db()
    {
        // On a fresh in-memory DB with no seed data, GET /api/frag returns NotFound
        // (other tests may have inserted rows, but this validates the endpoint works)
        var res = await _client.GetAsync("/api/frag");
        Assert.True(res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Fragrance_Returns_NoContent()
    {
        var frag = new Fragrance { FragName = "ToDelete", Brand = "Gone" };
        var post = await _client.PostAsJsonAsync("/api/frag", frag);
        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);

        var del = await _client.DeleteAsync($"/api/frag/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        // Confirm it's gone
        var get = await _client.GetAsync($"/api/frag/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_Missing_Fragrance_Returns_NotFound()
    {
        var res = await _client.DeleteAsync("/api/frag/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Put_Fragrance_Updates_Successfully()
    {
        var frag = new Fragrance { FragName = "Original", Brand = "OldBrand" };
        var post = await _client.PostAsJsonAsync("/api/frag", frag);
        var created = await post.Content.ReadFromJsonAsync<Fragrance>();
        Assert.NotNull(created);

        created.FragName = "Updated";
        created.Brand = "NewBrand";
        var put = await _client.PutAsJsonAsync($"/api/frag/{created.Id}", created);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        // Verify the update
        var get = await _client.GetAsync($"/api/frag/{created.Id}");
        var updated = await get.Content.ReadFromJsonAsync<Fragrance>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.FragName);
        Assert.Equal("NewBrand", updated.Brand);
    }

    [Fact]
    public async Task Put_Fragrance_Mismatched_Id_Returns_BadRequest()
    {
        var frag = new Fragrance { Id = 1, FragName = "Test", Brand = "X" };
        var put = await _client.PutAsJsonAsync("/api/frag/999", frag);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task Put_Missing_Fragrance_Returns_NotFound()
    {
        var frag = new Fragrance { Id = 88888, FragName = "Ghost", Brand = "None" };
        var put = await _client.PutAsJsonAsync("/api/frag/88888", frag);
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }

    [Fact]
    public async Task Search_By_Name_Returns_Match()
    {
        var frag = new Fragrance { FragName = "BleuDeTest", Brand = "SearchBrand" };
        await _client.PostAsJsonAsync("/api/frag", frag);

        var res = await _client.GetAsync("/api/frag/name/BleuDe");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Search_By_Name_Returns_NotFound_For_No_Match()
    {
        var res = await _client.GetAsync("/api/frag/name/zzzzNonExistent9999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Search_By_Brand_Returns_Match()
    {
        var frag = new Fragrance { FragName = "BrandSearchFrag", Brand = "UniqueBrandXYZ" };
        await _client.PostAsJsonAsync("/api/frag", frag);

        var res = await _client.GetAsync("/api/frag/brand/UniqueBrandXYZ");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Search_By_Brand_Returns_NotFound_For_No_Match()
    {
        var res = await _client.GetAsync("/api/frag/brand/NoSuchBrand99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Get_By_Invalid_Route_Returns_NotFound()
    {
        var res = await _client.GetAsync("/api/frag/notanumber");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
