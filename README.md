# Rob's Space Patrol — API

An ASP.NET Core API for Rob's Space Patrol, a command-line space strategy game
and personal learning project. The API exposes the game engine through
authenticated HTTP endpoints, stores each player's games in SQL Server, and
includes a client library that translates command-line actions into API calls.

The shared game engine and DTOs are maintained in the
[CoreLogic repository](https://github.com/robm675/RobsSpacePatrol.CoreLogic).
The directly playable, non-API command-line version is available in the
[CSharp repository](https://github.com/robm675/RobsSpacePatrol.CSharp).

I independently wrote the original C# implementation, inspired by classic
text-based space games, and later ported my own implementation to other
languages.

## Highlights

- ASP.NET Core controllers expose navigation, scans, combat, shields, status,
  and saved-game operations.
- ASP.NET Core Identity provides bearer and refresh tokens.
- Saved games are associated with the authenticated user rather than trusting
  a user identifier supplied by the client.
- Entity Framework Core persists game state in SQL Server and uses optimistic
  concurrency protection for competing updates.
- OpenAPI and Scalar provide an interactive development-time API reference.
- Unit and integration tests cover authorization, game ownership, persistence,
  API behavior, token refresh, and the API-backed command layer.

## Requirements

- Windows with the .NET 10 SDK. The checked-in `global.json` selects SDK
  10.0.401 and allows later 10.0.4xx patches.
- SQL Server LocalDB, SQL Server Express, or another accessible SQL Server
  instance.
- PowerShell for the commands below.
- Internet access on the first restore for public NuGet dependencies.
- The `dotnet-ef` tool when creating or applying database migrations.

Visual Studio is optional. Private `RobsSpacePatrol.DTO` and
`RobsSpacePatrol.Library` packages are committed under `local-packages`, so a
separate CoreLogic checkout or private package feed is not required.

## Configure the database

Development configuration defaults to this LocalDB database:

```text
Server=(localdb)\MSSQLLocalDB;Database=RobsSpacePatrol;Trusted_Connection=True;TrustServerCertificate=True
```

To use another SQL Server without committing its connection string, set it
with .NET User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" `
    "Server=YOUR_SERVER;Database=RobsSpacePatrol;Trusted_Connection=True;TrustServerCertificate=True" `
    --project .\RobsSpacePatrol.API\RobsSpacePatrol.API.csproj
```

SQL authentication can also be used, but its credentials should remain in
User Secrets or the `ConnectionStrings__DefaultConnection` environment
variable. Never add them to `appsettings.json`.

Install the matching Entity Framework command-line tool if necessary, then
apply the checked-in migrations:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12

dotnet ef database update `
    --project .\RobsSpacePatrol.API\RobsSpacePatrol.API.csproj `
    --startup-project .\RobsSpacePatrol.API\RobsSpacePatrol.API.csproj
```

If `dotnet-ef` is already installed, update it instead of installing it again.

## Build and run

From the repository directory:

```powershell
dotnet restore .\RobsSpacePatrol.API.sln
dotnet build .\RobsSpacePatrol.API.sln --configuration Release --no-restore
dotnet run --project .\RobsSpacePatrol.API\RobsSpacePatrol.API.csproj `
    --launch-profile https
```

The HTTPS profile listens at `https://localhost:5000`. In Development, the
Scalar interface is available at `https://localhost:5000/scalar` and the
OpenAPI document at `https://localhost:5000/openapi/v1.json`.

The Identity endpoints under `/api/login` handle registration, login, and
token refresh. Game endpoints under `/api/game` require authentication. Debug
endpoints under `/api/debug` additionally require the `Admin` role. The
repository intentionally provides no public endpoint that can promote a user
to administrator.

## Tests

The API test project is self-contained: it hosts the application in memory and
replaces authentication and persistence dependencies with controlled test
implementations.

```powershell
dotnet test .\RobsSpacePatrol.API.Tests\RobsSpacePatrol.API.Tests.csproj `
    --configuration Release
```

The CLI API library test project contains both isolated HTTP-client tests and
live integration tests. Before running the full project:

1. Apply the database migrations and start the API with its HTTPS profile.
2. Create a normal test account and an administrator test account in that
   database. The administrator account must be assigned the `Admin` role.
3. Set the following environment variables in the test process:

```powershell
$env:ROBSSPACEPATROL_API_TEST_URL = "https://localhost:5000"
$env:ROBSSPACEPATROL_API_TEST_USER = "player@example.test"
$env:ROBSSPACEPATROL_API_TEST_ADMIN = "admin@example.test"
$env:ROBSSPACEPATROL_API_TEST_PASSWORD = "your-test-password"

dotnet test `
    .\RobsSpacePatrol.CLI.API.Library.Tests\RobsSpacePatrol.CLI.API.Library.Tests.csproj `
    --configuration Release
```

`ROBSSPACEPATROL_API_TEST_URL` is optional and defaults to
`https://localhost:5000`. The account variables have no committed defaults so
credentials cannot be published accidentally. Use dedicated development data,
not production accounts or databases.

## Project layout

- `RobsSpacePatrol.API/`: ASP.NET Core host, controllers, Identity setup,
  Entity Framework Core context, migrations, and saved-game service.
- `RobsSpacePatrol.API.Tests/`: self-contained API tests using the ASP.NET Core
  test host.
- `RobsSpacePatrol.CLI.API.Library/`: authentication, token refresh, game
  session state, and reusable HTTP client behavior.
- `RobsSpacePatrol.CLI.API/`: command parsing and presentation for the planned
  API-backed console client.
- `RobsSpacePatrol.CLI.API.Library.Tests/`: isolated client tests and live API
  integration tests.
- `local-packages/`: private CoreLogic NuGet packages required by the solution.
- `NuGet.config`: restores private packages locally and public dependencies
  from NuGet.org.
- `global.json`: pins the .NET SDK feature band and test runner.

## Current limitations

- `RobsSpacePatrol.CLI.API` contains the API-backed command implementation, but
  its interactive executable entry point is not wired up yet. Use the CSharp
  repository for a directly playable version.
- The default development database is Windows LocalDB. Other environments must
  supply their own SQL Server connection string.
- Live integration tests require separately configured normal and
  administrator accounts and are intentionally not self-contained.
- Debug endpoints exist to support testing and are restricted to the `Admin`
  role.
- This is a portfolio and learning project rather than a production-hosting
  template. Deployment automation and production secret management are out of
  scope.

## Authorship, inspiration, and license

The original C# game and this API adaptation are my own implementation. The
game was inspired by classic text-based space games, including the historical
Star Trek game associated with Mike Mayfield; code was not copied from that
project. This acknowledgment describes inspiration, not affiliation or
endorsement.

My code is distributed under the [MIT license](LICENSE), copyright 2026 Rob.
NuGet dependencies remain subject to their respective licenses.
