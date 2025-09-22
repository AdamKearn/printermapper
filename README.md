# Printer Mapper

## Overview

Printer Mapper helps automate printer deployment via Group Policy and registry settings. It logs activity for troubleshooting and can be built from source using .NET.
When running the utility it will silently map the printers to the users device with zero human interaction.  This will work from any Windows Print Server or Samba printer share.

Download the utility from [here](https://github.com/AdamKearn/printermapper/releases/latest) or compile the source-code yourself using the notes below.
This can then be deployed using an RMM or Intune.

---

## Deploying Printers

**Step 1:** Create a user-based Group Policy to set these registry values:
I recommend when creating these registrys changes you select replace and also enable the checkbox labled "Remove this item when no longer applied"

```reg
[HKEY_CURRENT_USER\Software\PrinterMapper]
"ManagedPrintServer"="PRINT-SERVER-NAME"
"ITOffice"="\\PRINT-SERVER-NAME\ITOffice"
"MainOffice"="\\PRINT-SERVER-NAME\MainOffice"
"Reception"="\\PRINT-SERVER-NAME\Reception"
```

<img width="1048" height="640" alt="image" src="https://github.com/user-attachments/assets/4f0264d3-193f-4451-a698-3b3d558174d7" />
<img width="1048" height="542" alt="image" src="https://github.com/user-attachments/assets/c7a12ab0-749c-4fe2-8f1e-99d6e5b1c1d0" />

**Step 2:** Automatically start PrinterMapper after user logon by adding this to Group Policy:
Make sure you have already downloaded and deployed the utility out to your devices before hand.  You can download the latest version [here](https://github.com/AdamKearn/printermapper/releases/latest) or compile the application yourself.

```reg
[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run]
"StartPrinterMapper"="C:\Windows\System32\PrinterMapper.exe"
```

**Optional:** Allow non-admins to install printer drivers silently:
This can be defined using the following GPO:
```
Computer Configuration > Policies > Administrative Templates > Printers > Point and Print Restrictions
```

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

<img width="957" height="487" alt="image" src="https://github.com/user-attachments/assets/d0742180-0d2b-433a-9a14-c5e6d55349fa" />


---

## Debugging

PrinterMapper creates a log file every run.  You can view/access this log from the following location:

```
%appdata%\PrinterMapper\log.txt
```

---

## Building from Source

1. Install the latest [.NET SDK](https://dotnet.microsoft.com/en-us/download).
2. To run the code locally on your computer for quicker development:

   ```
   dotnet run
   ```

3. To create a single distributable binary:

   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
