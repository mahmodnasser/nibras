# Image nibras/servicename-api. Build from the repository root:
#   docker build -f src/Services/ServiceName/Nibras.ServiceName.Api/Dockerfile -t nibras/servicename-api:<version> .
# Debian-based images keep ICU and tzdata, which Arabic culture, the Um Al Qura calendar and the
# regional time zones need (Appendix X); InvariantGlobalization is never enabled.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY global.json nuget.config Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet publish src/Services/ServiceName/Nibras.ServiceName.Api/Nibras.ServiceName.Api.csproj \
    --configuration Release --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=1
EXPOSE 8080
# Non-root, and the root filesystem can be mounted read-only.
USER $APP_UID
ENTRYPOINT ["dotnet", "Nibras.ServiceName.Api.dll"]
