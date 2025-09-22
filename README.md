# Printer Mapper

## Deploy Printers

Within GroupPolicy you can create a user-based policy to deploy the following registry edits.
These will then be used by the PrinterMapper utility in the background.

```
[HKEY_CURRENT_USER\Software\PrinterMapper]
"ManagedPrintServer"="PRINT-SERVER-NAME"
"ITOffice"="\\PRINT-SERVER-NAME\ITOffice"
"MainOffice"="\\PRINT-SERVER-NAME\MainOffice"
"Reception"="\\PRINT-SERVER-NAME\Reception"
```

After deploying the utility to your devices you can then create a policy that will automatically start the application after user-logon.

This can be done by defining the following policy.

```
[HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run]
"StartPrinterMapper"="C:\Windows\System32\PrinterMapper.exe"
```

Optionally you can also allow non-administrators to install printer drivers without requiring elevation to allow a fully silent/automatic process of mapping printers.

```
[HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers\PointAndPrint]
"RestrictDriverInstallationToAdministrators"=dword:00000000
"Restricted"=dword:00000001
"TrustedServers"=dword:00000001
"ServerList"="PRINT-SERVER-NAME.domain.local"
"InForest"=dword:00000000
"NoWarningNoElevationOnInstall"=dword:00000001
"UpdatePromptSettings"=dword:00000002
```

## Debugging

Everytime the application runs it will create a log within the users appdata folder.
You can find this by going to the following path:

```
%appdata%/PrinterMapper/log.txt
```

## Building from source code.

If you need to modify/edit this application then you can build from source using the .NET
First make sure you have downloaded and installed the latest SDK from Microsoft. You can find this [here](https://dotnet.microsoft.com/en-us/download).

You can use the `dotnet run` command to quickly test and develop new feautes and once you are happy use the `dotnet publish` command to compile a single binary that can be distributed across your devices.

```
dotnet run

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
