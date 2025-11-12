using System;
using System.Management;

namespace UsbMonitorAgent
{
    public static class UsbInfoHelper
    {
        public static string GetUsbIdentity(string driveLetter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(driveLetter))
                    return "Unknown";

                string volumeQuery = $"SELECT DeviceID FROM Win32_LogicalDisk WHERE DeviceID = '{driveLetter}'";
                using var searcher = new ManagementObjectSearcher(volumeQuery);
                foreach (ManagementObject logicalDisk in searcher.Get())
                {
                    string deviceId = logicalDisk["DeviceID"]?.ToString();
                    if (string.IsNullOrEmpty(deviceId)) continue;

                    string associatorsQuery =
                        $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{deviceId}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
                    using var assocSearcher = new ManagementObjectSearcher(associatorsQuery);
                    foreach (ManagementObject partition in assocSearcher.Get())
                    {
                        string driveQuery =
                            $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                        using var driveSearcher = new ManagementObjectSearcher(driveQuery);
                        foreach (ManagementObject drive in driveSearcher.Get())
                        {
                            string pnpId = drive["PNPDeviceID"]?.ToString() ?? "";
                            string serial = drive["SerialNumber"]?.ToString() ?? "N/A";
                            string vid = ExtractValue(pnpId, "VID_");
                            string pid = ExtractValue(pnpId, "PID_");

                            return $"VID:{vid} PID:{pid} SN:{serial}";
                        }
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private static string ExtractValue(string input, string key)
        {
            try
            {
                int index = input.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (index == -1) return "N/A";
                return input.Substring(index + key.Length, 4);
            }
            catch
            {
                return "N/A";
            }
        }
    }
}
