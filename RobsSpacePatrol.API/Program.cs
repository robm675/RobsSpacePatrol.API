using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using RobsSpacePatrol.API.DB;
using RobsSpacePatrol.API.Services;
using RobsSpacePatrol.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace RobsSpacePatrol.API;

//this is a test comment
//this is a test comment
//this is a test comment

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddIdentityApiEndpoints<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.Configure<BearerTokenOptions>(
            IdentityConstants.BearerScheme,
            options =>
            {
                // These preserve your current lifetimes initially.
                options.BearerTokenExpiration = TimeSpan.FromHours(1);
                options.RefreshTokenExpiration = TimeSpan.FromDays(7);
            });


        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set it with User Secrets or the ConnectionStrings__DefaultConnection environment variable.");

        builder.Services.AddDbContext<ApplicationDbContext>(option =>
        {
            option.UseSqlServer(connectionString);
        });

        builder.Services.AddControllers();

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, CancellationToken) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        Description = "Enter the access token returned by the login endpoint"
                    }
                };
                document.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document, null),
                        new List<string>()
                    }
                }
            };
                return Task.CompletedTask;
            });
        });




        builder.Services.AddScoped<IGameDBService, GameDBService>();


        var app = builder.Build();

        app.MapGroup("/api/login").MapIdentityApi<ApplicationUser>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", (ILogger<Program> logger) =>
        {
            logger.LogInformation("API hit");
            return "ok";
        });

        app.MapControllers();

        app.Run();
    }
}