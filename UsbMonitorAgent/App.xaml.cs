using System;
using System.Windows;
using UsbMonitorAgent;
using WinForms = System.Windows.Forms;          // 🔹 Alias
using System.Drawing;                           // 🔹 Icon için

namespace UsbMonitorAgent
{
    public partial class App : Application
    {
        private WinForms.NotifyIcon _trayIcon;
        private MainWindow _window;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _window = new MainWindow();

            _trayIcon = new WinForms.NotifyIcon
            {
                Icon = SystemIcons.WinLogo,      // 🔹 Hazır disket ikonu (modern değil ama iş görür)
                Visible = true,
                Text = "USB Monitor Agent"
            };

            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("Logları Göster", null, (s, a) =>
            {
                if (!_window.IsVisible)
                {
                    _window.Show();
                    _window.Activate();
                }
                else
                {
                    _window.Activate();
                }
            });

            menu.Items.Add("Servis Durumu", null, async (s, a) =>
            {
                bool ok = await PipeClientService.CheckServiceAsync();
                MessageBox.Show(ok ? "Servis çalışıyor." : "Servise ulaşılamadı.", "Durum");
            });

            menu.Items.Add("Çıkış", null, (s, a) =>
            {
                _trayIcon.Visible = false;
                Shutdown();
            });

            _trayIcon.ContextMenuStrip = menu;

            // USB watcher başlat
            var watcher = new FileWatcherService();
            watcher.StartWatching();
        }
    }
}
