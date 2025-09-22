using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing.Printing;

namespace PrinterMapper
{
    class Program
    {
        private static string? logFilePath;

        static void Main(string[] args)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirectory = Path.Combine(appDataPath, "PrinterMapper");
            Directory.CreateDirectory(logDirectory);
            logFilePath = Path.Combine(logDirectory, "log.txt");

            if (File.Exists(logFilePath))
            {
                File.WriteAllText(logFilePath, string.Empty);
            }

            LogMessage("PrinterMapper application started");

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\PrinterMapper"))
                {
                    if (key == null)
                    {
                        LogMessage("ERROR: Registry key not found");
                        return;
                    }

                    object? managedPrintServerObj = key.GetValue("ManagedPrintServer");
                    string? managedPrintServer = managedPrintServerObj?.ToString();
                    if (string.IsNullOrEmpty(managedPrintServer))
                    {
                        LogMessage("ERROR: ManagedPrintServer value not found or empty");
                        return;
                    }

                    LogMessage($"Found ManagedPrintServer: {managedPrintServer}");

                    if (!TestServerConnectivity(managedPrintServer))
                    {
                        LogMessage($"ERROR: Print server '{managedPrintServer}' is not accessible");
                        return;
                    }

                    LogMessage($"Print server '{managedPrintServer}' is accessible");

                    HashSet<string> desiredPrinters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (valueName.Equals("ManagedPrintServer", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string? printerPath = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(printerPath))
                        {
                            desiredPrinters.Add(printerPath);
                        }
                    }

                    List<string> installedPrinters = GetInstalledPrintersFromServer(managedPrintServer);
                    foreach (string printer in installedPrinters)
                    {
                        if (!desiredPrinters.Contains(printer))
                        {
                            LogMessage($"Removing old printer: {printer}");
                            bool removed = RemovePrinter(printer);
                            LogMessage(removed ? $"Successfully removed {printer}" : $"Failed to remove {printer}");
                        }
                    }

                    int successCount = 0;
                    foreach (string printerPath in desiredPrinters)
                    {
                        LogMessage($"Processing printer: {printerPath}");
                        if (MapPrinter(printerPath))
                        {
                            successCount++;
                            LogMessage($"Successfully mapped printer: {printerPath}");
                        }
                        else
                        {
                            LogMessage($"Failed to map printer: {printerPath}");
                        }
                    }

                    LogMessage($"Printer mapping completed. {successCount}/{desiredPrinters.Count} printers mapped successfully");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex.Message}");
            }

            LogMessage("PrinterMapper application finished");
        }

        #region Native Printer API (Winspool)

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool AddPrinterConnection(string pName);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool DeletePrinterConnection(string pName);

        #endregion

        #region Printer Listing

        private static List<string> GetInstalledPrintersFromServer(string serverName)
        {
            List<string> printers = new List<string>();

            try
            {
                LogMessage($"Querying installed printers from server: {serverName}");
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    if (printerName.StartsWith($"\\\\{serverName}\\", StringComparison.OrdinalIgnoreCase))
                    {
                        printers.Add(printerName);
                        LogMessage($"Found installed printer: {printerName}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error enumerating printers: {ex.Message}");
            }

            return printers;
        }

        #endregion

        #region Printer Add/Remove

        private static bool MapPrinter(string printerPath)
        {
            try
            {
                bool result = AddPrinterConnection(printerPath);
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    LogMessage($"Failed to map printer {printerPath}, error code: {error}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogMessage($"Exception mapping printer {printerPath}: {ex.Message}");
                return false;
            }
        }

        private static bool RemovePrinter(string printerPath)
        {
            try
            {
                bool result = DeletePrinterConnection(printerPath);
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    LogMessage($"Failed to remove printer {printerPath}, error code: {error}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogMessage($"Exception removing printer {printerPath}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Connectivity + Logging

        private static bool TestServerConnectivity(string serverName)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(serverName, 3000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }

        private static void LogMessage(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            try
            {
                if (!string.IsNullOrEmpty(logFilePath))
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch { }
        }

        #endregion
    }
}
