# Building ASUS Fan Control

## Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "SDK" for your platform (Windows x64)

2. **Verify Installation**
   ```cmd
   dotnet --version
   ```
   Should show 8.0.x or higher

## Building the Application

### Option 1: Using Build Scripts (Recommended)

**Windows Command Prompt:**
```cmd
build.bat
```

**PowerShell:**
```powershell
.\build.ps1
```

### Option 2: Manual Build Commands

**GUI Application:**
```cmd
dotnet publish AsusFanControlGUI/AsusFanControlGUI.csproj --configuration Release --runtime win-x64 --self-contained true --output publish /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

**Console Application:**
```cmd
dotnet publish AsusFanControl/AsusFanControl.csproj --configuration Release --runtime win-x64 --self-contained true --output publish /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

## Output Files

After building, you'll find these files in the `publish` folder:

- **AsusFanControlGUI.exe** - Main GUI application (~100MB)
- **AsusFanControl.exe** - Console version (~80MB)
- **AsusWinIO64.dll** - Required hardware communication library

## Running the Application

### Important: Administrator Rights Required

The application needs administrator privileges to control hardware:

1. **Right-click** on `AsusFanControlGUI.exe`
2. Select **"Run as administrator"**
3. Click **"Yes"** when prompted by Windows UAC

### Alternative: Create Admin Shortcut

1. Right-click `AsusFanControlGUI.exe` → **"Create shortcut"**
2. Right-click the shortcut → **"Properties"**
3. Go to **"Compatibility"** tab
4. Check **"Run this program as an administrator"**
5. Click **"OK"**

Now you can double-click the shortcut to run with admin rights.

## Troubleshooting

### "ASUS System Analysis service not found"
- Install **MyASUS** app from Microsoft Store
- Ensure **ASUS System Control Interface** is installed
- Restart your computer

### "Access Denied" or "Permission Error"
- Make sure you're running as administrator
- Check Windows Defender isn't blocking the app
- Temporarily disable antivirus if needed

### "AsusWinIO64.dll not found"
- Ensure the DLL is in the same folder as the .exe
- Check if Windows blocked the DLL (right-click → Properties → Unblock)

## File Sizes

The single-file executables are large (~80-100MB) because they include:
- Complete .NET 8.0 runtime
- All required libraries
- Compressed application code

This ensures the app runs on any Windows 10/11 machine without requiring .NET installation.

## Distribution

To share your app:
1. Copy the entire `publish` folder
2. Or zip the contents of `publish` folder
3. Recipients need to run as administrator
4. Works on any Windows 10/11 x64 machine