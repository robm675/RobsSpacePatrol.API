using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RobsSpacePatrol.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace RobsSpacePatrol.API.Tests;

public class TestFactory(IGameDBService fakeGameService) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IGameDBService>();
            services.AddSingleton<IGameDBService>(fakeGameService);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultChallengeScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultForbidScheme =
                        TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        _ => { });
        });
    }
}
