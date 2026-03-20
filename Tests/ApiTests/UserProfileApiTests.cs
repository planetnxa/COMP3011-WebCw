using System.Net;
using System.Net.Http.Json;
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
}
