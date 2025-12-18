FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy project files
COPY *.sln .
COPY SIOMS/*.csproj ./SIOMS/

# Create clean NuGet config for Linux
RUN echo '<?xml version="1.0" encoding="utf-8"?> \
<configuration> \
  <config> \
    <add key="globalPackagesFolder" value="/app/packages" /> \
  </config> \
  <packageSources> \
    <clear /> \
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" /> \
  </packageSources> \
  <fallbackPackageFolders> \
    <clear /> \
  </fallbackPackageFolders> \
</configuration>' > /app/NuGet.Config

# Restore packages
RUN dotnet restore --configfile /app/NuGet.Config

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish SIOMS/SIOMS.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SIOMS.dll"]