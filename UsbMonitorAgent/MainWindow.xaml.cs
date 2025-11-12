using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Threading;
using System.Linq;

namespace UsbMonitorAgent
{
    public partial class MainWindow : Window
    {
        private string _dbPath = @"C:\ProgramData\UsbMonitor\usb_logs.db";
        public ObservableCollection<UsbLogModel> Logs { get; set; } = new();
        private DispatcherTimer _refreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadLogs();
                StartAutoRefresh();
            };
        }

        private void LoadLogs()
        {
            if (!File.Exists(_dbPath)) return;
            Logs.Clear();

            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Username,FileName,SourcePath,DestPath,DriveLabel,FileSize,TimestampUtc FROM UsbLogs ORDER BY Id DESC LIMIT 200";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                Logs.Add(new UsbLogModel
                {
                    Username = r.GetString(0),
                    FileName = r.GetString(1),
                    SourcePath = r.GetString(2),
                    DestPath = r.GetString(3),
                    DriveLabel = r.GetString(4),
                    FileSize = r.GetInt64(5),
                    TimestampUtc = r.GetString(6)
                });
            }

            // 🔹 En yeni en başta göster
            LogsGrid.ItemsSource = Logs.OrderByDescending(x => x.TimestampUtc).ToList();
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += (s, e) => LoadLogs();
            _refreshTimer.Start();
        }
    }
}
