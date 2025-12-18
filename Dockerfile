FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy ALL files from current directory (including .csproj and .sln)
COPY . .

# Remove any Windows NuGet configs
RUN find . -name "NuGet.Config" -o -name "nuget.config" -delete 2>/dev/null || true

# Create clean Linux NuGet config
RUN echo '<?xml version="1.0" encoding="utf-8"?> \
<configuration> \
  <packageSources> \
    <clear /> \
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" /> \
  </packageSources> \
  <fallbackPackageFolders> \
    <clear /> \
  </fallbackPackageFolders> \
</configuration>' > NuGet.Config

# Restore and build
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SIOMS.dll"]