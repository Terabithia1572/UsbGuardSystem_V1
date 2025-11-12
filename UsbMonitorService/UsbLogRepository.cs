using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace UsbMonitorService
{
    public class UsbLogRepository
    {
        private readonly string _dbPath = @"C:\ProgramData\UsbMonitor\usb_logs.db";

        public UsbLogRepository()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UsbLogs(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT, FileName TEXT,
                    SourcePath TEXT, DestPath TEXT,
                    DriveLabel TEXT, DriveSerial TEXT,
                    FileSize INTEGER, TimestampUtc TEXT, FileHash TEXT)";
            cmd.ExecuteNonQuery();
        }

        public void InsertLog(UsbLogEntry log)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO UsbLogs
                (Username,FileName,SourcePath,DestPath,DriveLabel,DriveSerial,FileSize,TimestampUtc,FileHash)
                VALUES($u,$f,$src,$dst,$lab,$ser,$sz,$t,$h)";
            cmd.Parameters.AddWithValue("$u", log.Username);
            cmd.Parameters.AddWithValue("$f", log.FileName);
            cmd.Parameters.AddWithValue("$src", log.SourcePath ?? "");
            cmd.Parameters.AddWithValue("$dst", log.DestPath ?? "");
            cmd.Parameters.AddWithValue("$lab", log.DriveLabel ?? "");
            cmd.Parameters.AddWithValue("$ser", log.DriveSerial ?? "");
            cmd.Parameters.AddWithValue("$sz", log.FileSize);
            cmd.Parameters.AddWithValue("$t", log.TimestampUtc);
            cmd.Parameters.AddWithValue("$h", log.FileHash ?? "");
            cmd.ExecuteNonQuery();
        }
    }

    public class UsbLogEntry
    {
        public string Username { get; set; } = "";
        public string FileName { get; set; } = "";
        public string? SourcePath { get; set; }
        public string? DestPath { get; set; }
        public string? DriveLabel { get; set; }
        public string? DriveSerial { get; set; }
        public long FileSize { get; set; }
        public string TimestampUtc { get; set; } = "";
        public string? FileHash { get; set; }
    }
}
