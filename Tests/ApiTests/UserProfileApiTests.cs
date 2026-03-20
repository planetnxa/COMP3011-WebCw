using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebAppComp3011.Models;
using Xunit;

namespace Tests.ApiTests;

public class UserProfileApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserProfileApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_User_Returns_Created()
    {
        var newUser = new UserProfile { Username = "testuser", Password = "password", Name = "Tester", FirstLogin = true };
        var post = await _client.PostAsJsonAsync("/api/userprofile", newUser);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);

        var get = await _client.GetAsync($"/api/userprofile/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Register_User_Returns_All_Fields()
    {
        var user = new UserProfile { Username = "fieldcheck", Password = "secret123", Name = "Field Checker", FirstLogin = false };
        var post = await _client.PostAsJsonAsync("/api/userprofile", user);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("fieldcheck", created.Username);
        Assert.Equal("Field Checker", created.Name);
    }

    [Fact]
    public async Task Get_User_By_Id_Returns_NotFound_For_Missing()
    {
        var res = await _client.GetAsync("/api/userprofile/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Get_User_By_Username()
    {
        var user = new UserProfile { Username = "lookupuser", Password = "pass123", Name = "Lookup", FirstLogin = true };
        await _client.PostAsJsonAsync("/api/userprofile", user);

        var res = await _client.GetAsync("/api/userprofile/username/lookupuser");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var found = await res.Content.ReadFromJsonAsync<UserProfile>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(found);
        Assert.Equal("lookupuser", found.Username);
    }

    [Fact]
    public async Task Get_User_By_Username_Returns_NotFound_For_Missing()
    {
        var res = await _client.GetAsync("/api/userprofile/username/nonexistentuser99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Delete_User_Returns_NoContent()
    {
        var user = new UserProfile { Username = "todelete", Password = "pass", Name = "Delete Me", FirstLogin = false };
        var post = await _client.PostAsJsonAsync("/api/userprofile", user);
        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);

        var del = await _client.DeleteAsync($"/api/userprofile/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        // Confirm it's gone
        var get = await _client.GetAsync($"/api/userprofile/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_Missing_User_Returns_NotFound()
    {
        var res = await _client.DeleteAsync("/api/userprofile/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Put_User_Updates_Successfully()
    {
        var user = new UserProfile { Username = "updateme", Password = "pass123456", Name = "Old Name", FirstLogin = true };
        var post = await _client.PostAsJsonAsync("/api/userprofile", user);
        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);

        created.Name = "New Name";
        created.Password = "newpassword12345";
        var put = await _client.PutAsJsonAsync($"/api/userprofile/{created.Id}", created);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        // Verify the update
        var get = await _client.GetAsync($"/api/userprofile/{created.Id}");
        var updated = await get.Content.ReadFromJsonAsync<UserProfile>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
    }

    [Fact]
    public async Task Put_User_Mismatched_Id_Returns_BadRequest()
    {
        var user = new UserProfile { Id = 1, Username = "mismatch", Password = "pass", Name = "X", FirstLogin = false };
        var put = await _client.PutAsJsonAsync("/api/userprofile/999", user);
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task Put_Missing_User_Returns_NotFound()
    {
        var user = new UserProfile { Id = 88888, Username = "ghost", Password = "pass", Name = "Ghost", FirstLogin = false };
        var put = await _client.PutAsJsonAsync("/api/userprofile/88888", user);
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }

    [Fact]
    public async Task Register_User_With_Empty_Fields()
    {
        var user = new UserProfile { Username = "", Password = "", Name = "", FirstLogin = false };
        var post = await _client.PostAsJsonAsync("/api/userprofile", user);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<UserProfile>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Get_By_Invalid_Route_Returns_NotFound()
    {
        var res = await _client.GetAsync("/api/userprofile/notanumber");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
