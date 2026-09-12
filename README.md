<p align="center">
  <a href="https://github.com/brhnshn/AulaTray">
    <img src="assets/banner.png" width="100%" alt="AulaTray Banner" />
  </a>
</p>

<p align="center">
  <strong>Lightweight Windows system tray battery monitor for the AULA F75 wireless mechanical keyboard.</strong>
</p>

<p align="center">
  <a href="#english">English</a> •
  <a href="#türkçe">Türkçe</a>
</p>

<p align="center">
  <a href="https://github.com/brhnshn/AulaTray/releases"><img src="https://img.shields.io/github/v/release/brhnshn/AulaTray?style=flat-square&color=00d26a&label=Release" alt="Latest Release" /></a>
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?style=flat-square&logo=windows&logoColor=white" alt="Platform" />
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 9.0" />
  <img src="https://img.shields.io/badge/Hardware-AULA%20F75-ff6600?style=flat-square" alt="AULA F75" />
  <img src="https://img.shields.io/badge/RAM-~15%20MB-brightgreen?style=flat-square" alt="RAM Usage" />
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-blue?style=flat-square" alt="License: MIT" /></a>
</p>

---

<a name="english"></a>
## English

### Overview

**AulaTray** is an independent, lightweight Windows system tray utility for the **AULA F75** wireless mechanical keyboard. It communicates directly with the keyboard hardware over native USB HID and Bluetooth Low Energy, displaying real-time battery percentage and charging status with a minimal ~15 MB RAM footprint.

---

### Key Features

- **Live System Tray Battery Icon**: Real-time percentage with color-coded status (Green for high, Yellow for medium, Red for low, Cyan for charging).
- **Tri-Mode Auto-Detection**: Seamlessly detects 2.4 GHz Wireless Dongle, Bluetooth 5.0, and Wired USB modes.
- **Windows 11 Fluent Card**: Click the tray icon to view connection details and battery status. Automatically closes when clicking outside or pressing ESC.
- **Low Battery Notifications**: Native Windows alerts when battery drops below 15%.
- **Start with Windows**: Toggle automatic startup with a single click from the tray menu.
- **Privacy & Performance**: 100% offline, zero telemetry, no background network traffic.

---

### Installation

1. Go to the **[Releases](https://github.com/brhnshn/AulaTray/releases)** page.
2. Download **`AulaTray-v1.0.0-win-x64.zip`** (or `AulaTray.exe`).
3. Extract and run `AulaTray.exe`. No installation required.
4. (Optional) Right-click the tray icon and check **Start with Windows**.

> **Requirements**: Windows 10 (version 1809+) or Windows 11. [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0).

<details>
<summary>Building from Source</summary>

```bash
git clone https://github.com/brhnshn/AulaTray.git
cd AulaTray/AulaTray
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ../dist
```
</details>

---

<a name="türkçe"></a>
## Türkçe

### Genel Bakış

**AulaTray**, **AULA F75** kablosuz mekanik klavye için geliştirilmiş bağımsız ve hafif bir Windows sistem tepsisi pil monitörüdür. Klavye donanımıyla doğrudan USB HID ve Bluetooth Low Energy üzerinden haberleşerek gerçek zamanlı pil yüzdesini ve şarj durumunu yaklaşık 15 MB RAM kullanımıyla arka planda sessizce gösterir.

### Öne Çıkan Özellikler

- **Canlı Tepsi Göstergesi**: Görev çubuğunda anlık pil yüzdesi ve şarj durumu renk göstergeleri.
- **3 Mod Otomatik Algılama**: 2.4 GHz Dongle, Bluetooth 5.0 ve Kablolu (USB) modları arasında geçiş yapıldığında anında tespit.
- **Windows 11 Açılır Durum Kartı**: Tepsi simgesine tıklandığında açılan şık koyu temalı kart; kart dışına tıklandığında veya ESC tuşuyla otomatik kapanır.
- **Düşük Pil Uyarısı**: Şarj %15 altına düştüğünde Windows bildirimi ile bilgilendirme.
- **Windows ile Başlatma**: Tepsi menüsünden tek tıkla otomatik başlatma ayarı.

### Kurulum

1. **[Releases](https://github.com/brhnshn/AulaTray/releases)** sayfasından **`AulaTray-v1.0.0-win-x64.zip`** dosyasını indirin.
2. Arşivi bir klasöre çıkartın ve `AulaTray.exe` dosyasını çalıştırın. Kurulum gerektirmez.

---

## Disclaimer

This project is an independent community tool and is not affiliated with, authorized, or endorsed by AULA, Shenzhen Dexin Electronics, or Epomaker. "AULA" and "AULA F75" are trademarks of their respective owners and are used strictly for hardware identification.

## License

This project is licensed under the [MIT License](LICENSE).
