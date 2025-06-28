@echo off
echo Building ASUS Fan Control for Windows 10 x64...
echo.

REM Clean previous builds
if exist "publish" rmdir /s /q "publish"

REM Build and publish the GUI application
echo Publishing AsusFanControlGUI...
dotnet publish AsusFanControlGUI/AsusFanControlGUI.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output publish ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true

REM Build the console application
echo Publishing AsusFanControl console...
dotnet publish AsusFanControl/AsusFanControl.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output publish ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true

echo.
echo Build complete! Files are in the 'publish' folder:
echo - AsusFanControlGUI.exe (Main GUI application)
echo - AsusFanControl.exe (Console version)
echo - AsusWinIO64.dll (Required hardware library)
echo.
echo To run: Right-click AsusFanControlGUI.exe and select "Run as administrator"
pause