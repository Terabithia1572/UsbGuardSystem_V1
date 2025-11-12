using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows; // 🔹 Clipboard erişimi için

namespace UsbMonitorAgent
{
    public class FileWatcherService
    {
        public void StartWatching()
        {
            Console.WriteLine("WMI dosya izleme başlatıldı...");

            try
            {
                string query = "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
                               "WHERE TargetInstance ISA 'CIM_DataFile' AND " +
                               "(TargetInstance.Drive='E:' OR TargetInstance.Drive='F:' OR TargetInstance.Drive='G:' OR TargetInstance.Drive='H:')";

                var watcher = new ManagementEventWatcher(new WqlEventQuery(query));
                watcher.EventArrived += async (s, e) =>
                {
                    try
                    {
                        var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                        string fullPath = instance["Name"]?.ToString();
                        if (string.IsNullOrEmpty(fullPath)) return;

                        var fi = new FileInfo(fullPath);
                        if (!fi.Exists) return;

                        Console.WriteLine($"Yeni dosya tespit edildi (WMI): {fullPath}");

                        var drive = new DriveInfo(fi.Directory.Root.FullName);
                        var identity = UsbInfoHelper.GetUsbIdentity(drive.Name.Replace("\\", ""));

                        // 🔹 Clipboard'dan kaynak dosya yolunu dene
                        string sourcePath = "";
                        try
                        {
                            if (Clipboard.ContainsFileDropList())
                            {
                                var files = Clipboard.GetFileDropList();
                                if (files.Count > 0)
                                    sourcePath = files[0];
                            }
                        }
                        catch (Exception clipEx)
                        {
                            Console.WriteLine("Clipboard erişim hatası: " + clipEx.Message);
                        }

                        // 🔹 Log oluştur
                        var log = new UsbLogModel
                        {
                            Username = Environment.UserName,
                            FileName = fi.Name,
                            SourcePath = sourcePath, // 🔹 artık clipboard’tan gelen kaynak yolu dolacak
                            DestPath = fullPath,
                            DriveLabel = drive.VolumeLabel,
                            DeviceIdentity = identity, // 🔹 eklendi
                            FileSize = fi.Length,
                            TimestampUtc = DateTime.UtcNow.ToString("o")
                        };

                        await PipeClientService.SendLogAsync(log);
                        Console.WriteLine($"Log servise gönderildi: {fi.Name} (Kaynak: {sourcePath})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WMI Event error: " + ex.Message);
                    }
                };

                watcher.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WMI watcher başlatılamadı: " + ex.Message);
            }
        }
    }
}
