#!/bin/bash
# .railway/build.sh
set -e

echo "=== Railway Build Script ==="

# Remove Windows configs
find . -name "NuGet.Config" -o -name "nuget.config" -delete 2>/dev/null || true

# Create Linux config
cat > NuGet.Config << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <fallbackPackageFolders>
    <clear />
  </fallbackPackageFolders>
</configuration>
EOF

# Build
dotnet nuget locals all --clear
dotnet restore
dotnet publish -c Release -o out

echo "=== Build Complete ==="