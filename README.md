# 🧠 UsbMonitorSuite  
**Advanced USB Activity Monitoring System (Agent + Windows Service)**  

Bu proje, takılan USB aygıtlarındaki dosya aktivitelerini (kopyalama, oluşturma vb.) gerçek zamanlı olarak izleyen iki bileşenden oluşur:  
- **UsbMonitorAgent** → Kullanıcı tarafında çalışır, USB üzerinde gerçekleşen olayları tespit eder.  
- **UsbMonitorService** → Windows servisi olarak çalışır, gelen logları SQLite veritabanına kaydeder.

---

## ⚙️ Özellikler
✅ USB belleğe kopyalanan veya yazılan her dosyayı algılar  
✅ Kullanıcı adı, dosya adı, kaynak/destek yolu, dosya boyutu, tarih/saat kaydı  
✅ VID / PID / Seri Numarası (SN) kimlik bilgilerini toplar  
✅ Gerçek zamanlı log kaydı (Named Pipe ile Agent → Service iletişimi)  
✅ SQLite veritabanında (`C:\ProgramData\UsbMonitor\usb_logs.db`) saklama  
✅ Tray ikonundan kontrol: Logları Göster, Servis Durumu, Yeniden Başlat, Çıkış  

---

## 🧩 Proje Yapısı
UsbMonitorSuite/
│
├── UsbMonitorAgent/
│ ├── FileWatcherService.cs
│ ├── PipeClientService.cs
│ ├── UsbInfoHelper.cs
│ ├── UsbLogModel.cs
│ ├── App.xaml / App.xaml.cs
│ ├── MainWindow.xaml / MainWindow.xaml.cs
│ └── UsbMonitorAgent.csproj
│
├── UsbMonitorService/
│ ├── Program.cs
│ ├── ServiceWorker.cs
│ ├── UsbLogRepository.cs
│ ├── appsettings.json
│ └── UsbMonitorService.csproj
│
└── README.md




---

## 🪟 Kurulum Adımları (Windows)
### 1️⃣ Servisi derle
Visual Studio’da **UsbMonitorService** projesini `Release` veya `Debug` olarak derle.

### 2️⃣ Servisi kaydet
PowerShell’i **Yönetici** olarak aç:

```powershell
sc.exe stop UsbMonitorService
sc.exe delete UsbMonitorService

C:\ProgramData\UsbMonitor\service.log

💻 Agent (Kullanıcı Arayüzü)

UsbMonitorAgent.exe’yi çalıştır.

Tepsi (tray) simgesine sağ tıkla:

Logları Göster → Veritabanındaki son kayıtları görüntüler

Servis Durumu → Servisin aktif olup olmadığını kontrol eder

Yeniden Başlat → Dosya izleme sistemini yeniden başlatır

Çıkış → Agent’ı kapatır
🗃️ Veritabanı Şeması

SQLite tablo: UsbLogs

Sütun	Açıklama
Id	Otomatik artan kayıt numarası
Username	Dosyayı aktaran kullanıcı
FileName	Dosya adı
SourcePath	Dosyanın geldiği yol
DestPath	Dosyanın USB'deki konumu
DriveLabel	USB sürücü etiketi
DriveSerial	USB seri numarası
DeviceIdentity	VID / PID / SN bilgileri
FileSize	Dosya boyutu (byte)
TimestampUtc	Tarih/saat (UTC)
FileHash	(opsiyonel) dosya hash alanı
🧠 Teknik Notlar

İletişim: Agent ve Service arası Named Pipe (UsbMonitorPipe) ile sağlanır.

Depolama: C:\ProgramData\UsbMonitor\usb_logs.db

Loglar: C:\ProgramData\UsbMonitor\service.log

Framework: .NET 6.0

Veritabanı: SQLite (Microsoft.Data.Sqlite)

🧰 Gelecek Geliştirmeler

🔹 Dosya hash hesaplama (SHA256)

🔹 Yetkisiz USB engelleme modu

🔹 Admin konsolu (uzaktan log görüntüleme)

🔹 Otomatik e-posta / webhook bildirimleri

👤 Geliştirici

Yunus İNAN
@yunusiinan

.NET Developer | Security & Monitoring Enthusiast

📜 Lisans

MIT License © 2025
Bu proje, güvenlik ve denetim amaçlı kullanım için geliştirilmiştir.
sc.exe create UsbMonitorService binPath= "\"C:\Users\Administrator\Desktop\UsbMonitorSolution\UsbMonitorService\bin\Debug\net6.0\UsbMonitorService.exe\"" start= auto
sc.exe start UsbMonitorService
