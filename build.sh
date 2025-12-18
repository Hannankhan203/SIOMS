#!/bin/bash
set -e

echo "=== Cleaning up ==="
rm -rf bin obj out packages

echo "=== Clearing NuGet cache ==="
dotnet nuget locals all --clear

echo "=== Removing any existing NuGet config files ==="
find . -name "NuGet.Config" -o -name "nuget.config" -delete 2>/dev/null || true

echo "=== Creating clean NuGet config for Linux ==="
cat > NuGet.Config << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="packages" />
  </config>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <fallbackPackageFolders>
    <clear />
  </fallbackPackageFolders>
</configuration>
EOF

echo "=== Restoring packages ==="
dotnet restore --configfile NuGet.Config

echo "=== Building project ==="
dotnet build -c Release --no-restore

echo "=== Publishing ==="
dotnet publish SIOMS.csproj -c Release -o out

echo "=== Build successful! ==="