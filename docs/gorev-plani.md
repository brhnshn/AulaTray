# Aula F75 Pil Monitörü - Görev Planı ve Mimari

Bu belge Aula F75 klavyesi için geliştirilecek sade, Windows 11 uyumlu, ultra düşük kaynak tüketimli sistem tepsisi uygulamasının görev planını içerir.

## Görev Dağılımı

### [Görev 1] Proje İskeleti (Altyapı)
- .NET 9 WinForms tabanlı penceresiz (windowless) proje oluşturma (`AulaTray`).
- Dış bağımlılık olmadan, tek dosya (`PublishSingleFile=true`) konfigürasyonu.

### [Görev 2] Win32 HID İletişim Servisi (`HidService.cs`)
- `hid.dll` ve `kernel32.dll` P/Invoke bildirimleri.
- `VID: 0x3554, PID: 0xFA09, UsagePage: 0xFF02` aygıtını algılama.
- `0x13 0x4A` komutu ile 20 baytlık çerçeve alışverişi.
- `KeyboardStatus` tipine dönüştürme: Pil yüzdesi, güç durumu (Pilde / Şarj), bağlantı durumu.

### [Görev 3] Windows 11 Pil Simgesi Çizicisi (`IconRenderer.cs`)
- 32x32 bellek içi bitmap çizimi.
- İnce beyaz yuvarlak pil gövdesi + sağ kutup.
- Yüzdeye göre yeşil (#5AD469), sarı (#E8B83A), kırmızı (#E04848) iç dolum.
- Şarjdayken pil içinde beyaz şimşek glifi.
- Pilin yanında keskin Segoe UI yazı tipiyle sayısal yüzde metni.

### [Görev 4] Sistem Tepsisi ve Menü Yönetimi (`TrayApplicationContext.cs`)
- 60 saniyelik düşük CPU'lu timer yoklaması.
- Fare üzerine gelindiğinde sade Tooltip: `Aula F75: %XX (Pilde)`.
- %20 altına indiğinde tek seferlik sade Windows bildirimi.
- Sağ tık menüsü:
  - Durum satırı (tıklanamaz bilgi)
  - Şimdi Yenile
  - Windows ile Başlat (Onay kutulu)
  - Çıkış
- `AutoStartService.cs`: Registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` yönetimi.

### [Görev 5] Derleme, Doğrulama ve Teslim
- `dotnet publish` ile tek dosya derleme (`bin\Release\net9.0-windows\publish\AulaTray.exe`).
- RAM ve CPU tüketiminin izlenmesi (Hedef: < 10 MB RAM, %0.0 CPU).
- Kullanıcıya teslim.
