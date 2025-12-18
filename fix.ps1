# fix.ps1 - Complete .NET 8 build fix
Write-Host "=== Fixing .NET Build Issues ===" -ForegroundColor Cyan

# 1. Remove old files
Write-Host "1. Cleaning up old files..." -ForegroundColor Yellow
$filesToDelete = @("NuGet.Config", "nuget.config")
foreach ($file in $filesToDelete) {
    if (Test-Path $file) {
        Remove-Item $file -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $file" -ForegroundColor Gray
    }
}

# 2. Remove directories
$dirsToDelete = @("bin", "obj", "out", "packages")
foreach ($dir in $dirsToDelete) {
    if (Test-Path $dir) {
        Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $dir\" -ForegroundColor Gray
    }
}

# 3. Create clean NuGet.Config
Write-Host "2. Creating clean NuGet.Config..." -ForegroundColor Yellow
$xmlContent = '<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <fallbackPackageFolders>
    <clear />
  </fallbackPackageFolders>
</configuration>'

Set-Content -Path "NuGet.Config" -Value $xmlContent -Encoding UTF8
Write-Host "  Created: NuGet.Config" -ForegroundColor Gray

# 4. Clear NuGet cache
Write-Host "3. Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear

# 5. Set environment
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "true"

# 6. Restore packages
Write-Host "4. Restoring packages..." -ForegroundColor Yellow
dotnet restore --configfile "NuGet.Config"

# 7. Build and publish
Write-Host "5. Building and publishing..." -ForegroundColor Yellow
dotnet publish SIOMS.csproj -c Release -o out

Write-Host "`n=== SUCCESS! Build completed. ===" -ForegroundColor Green
Write-Host "Output is in: out\" -ForegroundColor Cyan