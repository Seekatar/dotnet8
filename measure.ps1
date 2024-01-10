# don't see much difference in chiseled vs aspnet. Pretty tiny app.
param(
    [Parameter(Mandatory)]
    [ValidateSet('chiseled', 'aspnet')]
    [string] $DockerTag
)
Measure-Command {
    "Starting $DockerTag"
    ./run.ps1 runDocker -DockerTag $DockerTag
    while ($true) {
        try {
            irm http://localhost:5000/time
            break
        } catch {
        }
    }
}
docker stop dotnet8