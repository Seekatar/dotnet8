# .NET 8 has chiseld containers
ARG aspNetVersion=8.0.0-jammy-chiseled
ARG sdkVersion=8.0.100

FROM mcr.microsoft.com/dotnet/aspnet:${aspNetVersion} AS runtime

FROM mcr.microsoft.com/dotnet/sdk:${sdkVersion} AS build
ARG build_configuration=Debug
ARG BUILD_VERSION=1.0.0.0
ARG RUN_UNIT_TEST=true


WORKDIR /app

COPY ["dotnet8.csproj", "."]

RUN dotnet restore "dotnet8.csproj"

COPY . .

# we don't want the dev config, which will get used if the ASPNETCORE_ENVIRONMENT is set to Development
RUN dotnet build "dotnet8.csproj" -c $build_configuration \
                                                /warnaserror \
                                                /p:Version=$BUILD_VERSION \
                                                --no-restore \
                                                --no-self-contained \
                                                /flp:logfile=/logs/Build.log

# ===== Publishing =====
WORKDIR /app
RUN find / -name "appsettings.Development.json" -exec rm {} \;
RUN dotnet publish "dotnet8.csproj" -c $build_configuration -o /app/publish --no-build --no-restore

# ===== Output Stage =====
FROM scratch as build-test-output
COPY --from=build /logs /logs

# ===== Final Stage =====
FROM runtime AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=5000

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "dotnet8.dll"]