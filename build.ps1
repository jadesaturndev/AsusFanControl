# PowerShell build script for ASUS Fan Control
Write-Host "Building ASUS Fan Control for Windows 10 x64..." -ForegroundColor Green
Write-Host ""

# Clean previous builds
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

# Build and publish the GUI application
Write-Host "Publishing AsusFanControlGUI..." -ForegroundColor Yellow
dotnet publish AsusFanControlGUI/AsusFanControlGUI.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output publish `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:EnableCompressionInSingleFile=true

# Build the console application
Write-Host "Publishing AsusFanControl console..." -ForegroundColor Yellow
dotnet publish AsusFanControl/AsusFanControl.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output publish `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:EnableCompressionInSingleFile=true

Write-Host ""
Write-Host "Build complete! Files are in the 'publish' folder:" -ForegroundColor Green
Write-Host "- AsusFanControlGUI.exe (Main GUI application)" -ForegroundColor Cyan
Write-Host "- AsusFanControl.exe (Console version)" -ForegroundColor Cyan
Write-Host "- AsusWinIO64.dll (Required hardware library)" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run: Right-click AsusFanControlGUI.exe and select 'Run as administrator'" -ForegroundColor Yellow