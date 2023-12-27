#! pwsh
<#
.SYNOPSIS
Helper for running various tasks

.PARAMETER Tasks
One or more tasks to run. Use tab completion to see available tasks.

.PARAMETER DockerTag
For building docker images, the tag to use. Defaults to a timestamp

.PARAMETER Plain
For building docker images, use plain output since default collapses output after each step.

.PARAMETER NoCache
For building docker images, add --no-cache

.PARAMETER DeleteDocker
Add --rm to docker build

.EXAMPLE
./run.ps1 buildDocker -Plain -NoCache

Build the docker image with plain output

.EXAMPLE
./run.ps1 buildDocker -AspNetVersion aspNetVersion=8.0.0-jammy

Build the docker image for the non-chiseled version

#>
[CmdletBinding()]
param (
    [ArgumentCompleter({
        param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
        $runFile = (Join-Path (Split-Path $commandAst -Parent) run.ps1)
        if (Test-Path $runFile) {
            Get-Content $runFile |
                    Where-Object { $_ -match "^\s+'([\w+-]+)'\s*{" } |
                    ForEach-Object {
                        if ( !($fakeBoundParameters[$parameterName]) -or
                            (($matches[1] -notin $fakeBoundParameters.$parameterName) -and
                             ($matches[1] -like "$wordToComplete*"))
                            )
                        {
                            $matches[1]
                        }
                    }
        }
     })]
    [string[]] $Tasks,
    [string] $DockerTag = [DateTime]::Now.ToString("MMdd-HHmmss"),
    [switch] $Plain,
    [switch] $NoCache,
    [bool] $DeleteDocker = $True,
    [int] $TestPort = 5000,
    [string] $Version,
    [switch] $Prerelease,
    [string] $BuildLogFolder,
    [string] $AspNetVersion
)

$appName = 'dotnet8' # image, folder

$currentTask = ""

function Get-DockerEnvFile() {
    $folder = $PSScriptRoot
    while ($folder -and !(Test-Path (Join-Path $folder shared.appsettings.Development.json))) {
        $folder = Split-Path $folder -Parent
    }
    if (Test-Path (Join-Path $folder shared.appsettings.Development.json)) {
        $env = Get-Content (Join-Path $folder shared.appsettings.Development.json) -raw | ConvertFrom-json
        Get-Member -input $env -MemberType NoteProperty | ForEach-Object {
            $name = $_.name
            "$($name -replace ':','__')=$(($env.$name -replace "`n","\n") -replace "localhost","host.docker.internal")"
        } | Out-File (Join-Path $PSScriptRoot ".env") -Encoding ascii
            "Wrote $(Join-Path $PSScriptRoot ".env")"
        } else {
            "No shared appsettings found."
        }
}

# execute a script, checking lastexit code
function executeSB
{
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [scriptblock] $ScriptBlock,
    [string] $RelativeDir,
    [string] $Name = $currentTask
)
    if ($RelativeDir) {
        Push-Location (Join-Path $PSScriptRoot $RelativeDir)
    } else {
        Push-Location $PSScriptRoot
    }
    try {
        $global:LASTEXITCODE = 0

        Invoke-Command -ScriptBlock $ScriptBlock

        if ($LASTEXITCODE -ne 0) {
            throw "Error executing command '$Name', last exit $LASTEXITCODE"
        }
    } finally {
        Pop-Location
    }
}

if (!$BuildLogFolder) {
    $BuildLogFolder = [System.IO.Path]::GetTempPath()
}

foreach ($currentTask in $Tasks) {

    try {
        $prevPref = $ErrorActionPreference
        $ErrorActionPreference = "Stop"

        "-------------------------------"
        "Starting $currentTask"
        "-------------------------------"

        switch ($currentTask) {
            'build' {
                executeSB -RelativeDir 'src' {
                    dotnet build
                }
            }
            'run' {
                executeSB -RelativeDir "src" {
                    dotnet run
                }
            }
            'runDocker' {
                Get-DockerEnvFile
                executeSB {
                    $DockerTag = "latest"
                    docker run --rm `
                               --env-file .env `
                               --interactive `
                               -e aspnetcore_hostBuilder__reloadConfigOnChange=false `
                               --publish 5000:5000 `
                               --tty `
                               --name $appName `
                               "$($appName.ToLowerInvariant()):$DockerTag"
                }
            }
            'buildDocker' {
                executeSB -RelativeDir 'src' {
                    $extra = @()
                    if ($Plain) {
                        $extra += "--progress","plain"
                    }
                    if ($DeleteDocker) {
                        $extra += "--rm"
                    }
                    if (Test-Path '../DevOps/Docker/.dockerignore') {
                        Copy-Item '../DevOps/Docker/.dockerignore' '.'
                    }
                    $lowerAppName = $appName.ToLowerInvariant()

                    Write-Verbose "Extra is $($extra -join ' ')"

                    if ($AspNetVersion) {
                        $extra += "--build-arg", $AspNetVersion
                    }
                    $file = '../DevOps/Docker/Dockerfile'
                    $buildExtra = $extra
                    if ($NoCache){
                        $buildExtra += "--no-cache"
                    }

                    "-----------------------------------"
                    "  docker build for build and test"
                    "-----------------------------------"
                    docker build  `
                            --build-arg RUN_UNIT_TEST=true `
                            --target build-test-output `
                            --output ../docker-build-output `
                            --file $file `
                            @buildExtra `
                            .
                    if ($LASTEXITCODE -eq 0) {
                        "-----------------------------------"
                        "  docker build for publish & final"
                        "-----------------------------------"
                        docker build  `
                                --tag ${lowerAppName}:$DockerTag `
                                --file $file `
                                --no-cache `
                                @extra `
                                .
                        if ($LASTEXITCODE -eq 0) {
                            docker tag ${lowerAppName}:$DockerTag ${lowerAppName}:latest
                        }
                    }
                    Remove-Item (Join-Path $PSScriptRoot .dockerignore) -ErrorAction SilentlyContinue
                }
            }
            Default {
                Write-Warning "Unknown task $currentTask"
            }
        }

    } finally {
        $ErrorActionPreference = $prevPref
    }
}
