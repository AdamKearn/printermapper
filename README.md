# Printer Mapper

## Overview

Printer Mapper helps automate printer deployment via Group Policy and registry settings. It logs activity for troubleshooting and can be built from source using .NET.

When running the utility it will silently map the printers to the users device with zero human interaction.

---

## 1. Deploying Printers

**Step 1:** Create a user-based Group Policy to set these registry values:

```reg
[HKEY_CURRENT_USER\Software\PrinterMapper]
"ManagedPrintServer"="PRINT-SERVER-NAME"
"ITOffice"="\\PRINT-SERVER-NAME\ITOffice"
"MainOffice"="\\PRINT-SERVER-NAME\MainOffice"
"Reception"="\\PRINT-SERVER-NAME\Reception"
```

**Step 2:** Automatically start PrinterMapper after user logon by adding this to Group Policy:

```reg
[HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run]
"StartPrinterMapper"="C:\Windows\System32\PrinterMapper.exe"
```

**Optional:** Allow non-admins to install printer drivers silently:

```reg
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers\PointAndPrint]
"RestrictDriverInstallationToAdministrators"=dword:00000000
"Restricted"=dword:00000001
"TrustedServers"=dword:00000001
"ServerList"="PRINT-SERVER-NAME.domain.local"
"InForest"=dword:00000000
"NoWarningNoElevationOnInstall"=dword:00000001
"UpdatePromptSettings"=dword:00000002
```

---

## 2. Debugging

PrinterMapper creates a log file every run:

```
%appdata%\PrinterMapper\log.txt
```

---

## 3. Building from Source

1. Install the latest [.NET SDK](https://dotnet.microsoft.com/en-us/download).
2. To run the code locally on your computer for quicker development:

   ```
   dotnet run
   ```

3. To create a single distributable binary:

   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
