[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateNotNullOrEmpty()]
    [string]$CommitMessage = "Initial public release",

    [switch]$SkipLiveIntegrationTests,

    [switch]$NoPause
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Native programs report their own exit codes, which are checked below.
if (Get-Variable -Name PSNativeCommandUseErrorActionPreference -ErrorAction SilentlyContinue) {
    $PSNativeCommandUseErrorActionPreference = $false
}

$repositoryRoot = $PSScriptRoot
$solution = Join-Path $repositoryRoot "RobsSpacePatrol.API.sln"
$apiTests = Join-Path $repositoryRoot "RobsSpacePatrol.API.Tests\RobsSpacePatrol.API.Tests.csproj"
$clientTests = Join-Path $repositoryRoot "RobsSpacePatrol.CLI.API.Library.Tests\RobsSpacePatrol.CLI.API.Library.Tests.csproj"
$expectedRemote = "https://github.com/robm675/RobsSpacePatrol.API.git"
$requiredFiles = @(
    $solution,
    (Join-Path $repositoryRoot "README.md"),
    (Join-Path $repositoryRoot "LICENSE"),
    (Join-Path $repositoryRoot "local-packages\RobsSpacePatrol.DTO.1.0.5.nupkg"),
    (Join-Path $repositoryRoot "local-packages\RobsSpacePatrol.Library.1.0.12.nupkg")
)

$isolatedClientTestFilter = @(
    "FullyQualifiedName~GameDataApiResponse_Tests",
    "FullyQualifiedName~GameDataDelete_Tests",
    "FullyQualifiedName~GameDataRefresh_Tests",
    "FullyQualifiedName~GameDataStart_Tests"
) -join "|"

function Invoke-DotNet {
    param([Parameter(Mandatory)][string[]]$Arguments)

    Write-Host "`n> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Git {
    param([Parameter(Mandatory)][string[]]$Arguments)

    Write-Host "`n> git $($Arguments -join ' ')" -ForegroundColor Cyan
    & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git failed with exit code $LASTEXITCODE."
    }
}

function Read-GitValue {
    param([Parameter(Mandatory)][string[]]$Arguments)

    $value = & git @Arguments 2>$null
    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    return ($value | Out-String).Trim()
}

function Wait-ForExit {
    if (-not $NoPause) {
        [void](Read-Host "`nPress Enter to finish")
    }
}

try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "dotnet was not found on PATH."
    }
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        throw "git was not found on PATH."
    }
    foreach ($requiredFile in $requiredFiles) {
        if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
            throw "Required publication file not found: $requiredFile"
        }
    }

    Push-Location $repositoryRoot
    try {
        Invoke-DotNet @("restore", $solution, "--force-evaluate")
        Invoke-DotNet @("build", $solution, "--configuration", "Release", "--no-restore")
        Invoke-DotNet @(
            "test", $apiTests,
            "--configuration", "Release",
            "--no-build",
            "--no-restore"
        )

        if ($SkipLiveIntegrationTests) {
            Write-Host "`nLive API integration tests were explicitly skipped." -ForegroundColor Yellow
            Invoke-DotNet @(
                "test", $clientTests,
                "--configuration", "Release",
                "--no-build",
                "--no-restore",
                "--filter", $isolatedClientTestFilter
            )
        }
        else {
            $requiredTestVariables = @(
                "ROBSSPACEPATROL_API_TEST_USER",
                "ROBSSPACEPATROL_API_TEST_ADMIN",
                "ROBSSPACEPATROL_API_TEST_PASSWORD"
            )

            foreach ($variableName in $requiredTestVariables) {
                $variableValue = [Environment]::GetEnvironmentVariable($variableName)
                if ([string]::IsNullOrWhiteSpace($variableValue)) {
                    throw "Set $variableName and start the API before publishing, or explicitly use -SkipLiveIntegrationTests."
                }
            }

            Write-Host "`nRunning the live API integration tests." -ForegroundColor Yellow
            Write-Host "The API must already be running at the configured test URL." -ForegroundColor Yellow
            Invoke-DotNet @(
                "test", $clientTests,
                "--configuration", "Release",
                "--no-build",
                "--no-restore"
            )
        }

        if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot ".git"))) {
            Invoke-Git @("init", "--initial-branch=main")
        }

        $gitRoot = Read-GitValue @("rev-parse", "--show-toplevel")
        if (-not $gitRoot) {
            throw "This directory could not be initialized as a Git repository."
        }
        if ([IO.Path]::GetFullPath($gitRoot) -ne [IO.Path]::GetFullPath($repositoryRoot)) {
            throw "The Git repository root is $gitRoot, not $repositoryRoot."
        }

        $branch = Read-GitValue @("branch", "--show-current")
        if ($branch -ne "main") {
            throw "The current branch must be main. Current branch: $branch"
        }

        $origin = Read-GitValue @("remote", "get-url", "origin")
        if (-not $origin) {
            Invoke-Git @("remote", "add", "origin", $expectedRemote)
            $origin = $expectedRemote
        }
        if ($origin -ne $expectedRemote) {
            throw "The origin remote must be $expectedRemote. Current value: $origin"
        }

        $gitName = Read-GitValue @("config", "user.name")
        $gitEmail = Read-GitValue @("config", "user.email")
        if (-not $gitName -or -not $gitEmail) {
            throw "Configure git user.name and git user.email before publishing."
        }

        $remoteMain = & git ls-remote --heads origin refs/heads/main
        if ($LASTEXITCODE -ne 0) {
            throw "GitHub could not be reached or authenticated."
        }

        & git rev-parse --verify --quiet HEAD 2>$null
        $hasLocalCommit = $LASTEXITCODE -eq 0

        if ($remoteMain) {
            if (-not $hasLocalCommit) {
                throw "The remote main branch already has history, but this local repository does not. Resolve that history before publishing."
            }

            Invoke-Git @("fetch", "origin", "main")
            & git merge-base --is-ancestor origin/main HEAD
            if ($LASTEXITCODE -ne 0) {
                throw "The local main branch does not contain origin/main. Pull and resolve the histories before publishing."
            }
        }

        Write-Host "`nFiles currently eligible for the commit:" -ForegroundColor Yellow
        & git status --short --untracked-files=all
        if ($LASTEXITCODE -ne 0) {
            throw "git status failed."
        }

        if (-not $PSCmdlet.ShouldProcess($expectedRemote, "commit the current repository contents and push main")) {
            Write-Host "`nPublication preview completed; nothing was staged or pushed." -ForegroundColor Yellow
            return
        }

        $confirmation = Read-Host "`nType PUBLISH to commit and push these files"
        if ($confirmation -cne "PUBLISH") {
            throw "Publishing cancelled; no files were staged, committed, or pushed."
        }

        Invoke-Git @("add", "--all")
        Invoke-Git @("diff", "--cached", "--check")

        & git diff --cached --quiet
        $hasStagedChanges = $LASTEXITCODE -ne 0
        if ($hasStagedChanges) {
            Invoke-Git @("commit", "-m", $CommitMessage)
        }
        elseif (-not $hasLocalCommit) {
            throw "There are no changes to commit and no existing local commit to push."
        }
        else {
            Write-Host "No new changes to commit; the existing local commits will be pushed." -ForegroundColor Yellow
        }

        Invoke-Git @("push", "--set-upstream", "origin", "main")
        Write-Host "`nPublished successfully to $expectedRemote" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
catch {
    Write-Host "`nPublishing failed: $($_.Exception.Message)" -ForegroundColor Red
    Wait-ForExit
    throw
}

Wait-ForExit
