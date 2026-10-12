# Build context: repository root.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer): only build props and project files.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/BoaSorte.Domain/BoaSorte.Domain.csproj src/BoaSorte.Domain/
COPY src/BoaSorte.Application/BoaSorte.Application.csproj src/BoaSorte.Application/
COPY src/BoaSorte.Infrastructure/BoaSorte.Infrastructure.csproj src/BoaSorte.Infrastructure/
COPY src/BoaSorte.Api/BoaSorte.Api.csproj src/BoaSorte.Api/
RUN dotnet restore src/BoaSorte.Api/BoaSorte.Api.csproj

COPY src/ src/
RUN dotnet publish src/BoaSorte.Api/BoaSorte.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BoaSorte.Api.dll"]
