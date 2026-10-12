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

# No curl/wget in the runtime image: probe /api/health (includes the database) over bash /dev/tcp.
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080 && printf "GET /api/health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && head -1 <&3 | grep -q " 200 "'
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BoaSorte.Api.dll"]
