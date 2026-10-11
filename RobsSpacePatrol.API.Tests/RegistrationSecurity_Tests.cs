using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RobsSpacePatrol.API.DB;
using RobsSpacePatrol.DTO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class RegistrationSecurity_Tests
{
    [Fact]
    public async Task Register_IgnoresSubmittedAdminRole_AndDoesNotCreateAdmin()
    {
        await using var factory = new RegistrationTestFactory();
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        var email = $"registration-security-{Guid.NewGuid():N}@example.test";
        const string password = "TestPassword123$";

        using var registrationResponse = await client.PostAsJsonAsync("/api/login/register", new { Email = email, Password = password, Role = "Admin" }, cancellationToken);

        var registrationBody = await registrationResponse.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(registrationResponse.StatusCode == HttpStatusCode.OK, $"Expected registration to return 200 OK, but received {(int)registrationResponse.StatusCode}: {registrationBody}");

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);
            var roles = await userManager.GetRolesAsync(user);
            Assert.DoesNotContain("Admin", roles);
        }

        using var loginResponse = await client.PostAsJsonAsync("/api/login/login?useCookies=false", new LoginRequestDTO { Email = email, Password = password }, cancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(login?.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var adminResponse = await client.GetAsync("/api/debug/get/check", cancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsUsableAccessToken()
    {
        await using var factory = new RegistrationTestFactory();
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        var email = $"refresh-security-{Guid.NewGuid():N}@example.test";
        const string password = "TestPassword123$";

        using var registrationResponse = await client.PostAsJsonAsync(
            "/api/login/register",
            new { Email = email, Password = password },
            cancellationToken);
        registrationResponse.EnsureSuccessStatusCode();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/login/login?useCookies=false",
            new LoginRequestDTO { Email = email, Password = password },
            cancellationToken);
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>(
            cancellationToken: cancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(login?.RefreshToken));

        using var refreshResponse = await client.PostAsJsonAsync(
            "/api/login/refresh",
            new { login.RefreshToken },
            cancellationToken);
        refreshResponse.EnsureSuccessStatusCode();

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AccessTokenResponse>(
            cancellationToken: cancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(refreshed?.AccessToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);

        using var protectedResponse = await client.GetAsync("/api/login/manage/info", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        await using var factory = new RegistrationTestFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/login/refresh",
            new { RefreshToken = "not-a-valid-refresh-token" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class RegistrationTestFactory : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"registration-security-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        }
    }
}
