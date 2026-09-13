<p align="center">
  <a href="https://github.com/brhnshn/AulaTray">
    <img src="assets/banner.png" width="100%" alt="AulaTray Banner" />
  </a>
</p>

<p align="center">
  <strong>Ultra-lightweight Windows system tray battery monitor and hardware companion for AULA wireless mechanical keyboards (F75, F87, F99, F68 & Custom Models).</strong>
</p>

<p align="center">
  <a href="#english">English</a> •
  <a href="#türkçe">Türkçe</a> •
  <a href="#models-json">models.json Configuration</a> •
  <a href="#architecture">Architecture & ADRs</a>
</p>

<p align="center">
  <a href="https://github.com/brhnshn/AulaTray/releases"><img src="https://img.shields.io/github/v/release/brhnshn/AulaTray?style=flat-square&color=3EBE8E&label=Release" alt="Latest Release" /></a>
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?style=flat-square&logo=windows&logoColor=white" alt="Platform" />
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 9.0" />
  <img src="https://img.shields.io/badge/Hardware-AULA%20Series%20(F75%2C%20F87%2C%20F99%2C%20F68)-ff6600?style=flat-square" alt="Hardware Support" />
  <img src="https://img.shields.io/badge/RAM-~12.8%20MB-brightgreen?style=flat-square" alt="RAM Usage" />
  <img src="https://img.shields.io/badge/Tests-12%20Passing-success?style=flat-square" alt="Tests" />
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-blue?style=flat-square" alt="License: MIT" /></a>
</p>

---

<a name="english"></a>
## English

### Overview

**AulaTray** is an independent, ultra-lightweight Windows system tray utility for **AULA wireless mechanical keyboards**. While the official vendor software lacks a reliable battery indicator and requires high background resources, AulaTray communicates directly with the keyboard over native USB HID, 2.4 GHz RF dongles, and Bluetooth Low Energy (BLE), consuming just **~12.8 MB of RAM** with **0% CPU at idle**.

---

### Key Features

- **Metallic Glass "A" Tray Icon**: High-definition, anti-aliased glass logo in the system tray. Stays vibrant and bright when active; turns subtle and semi-transparent grayscale when sleeping or disconnected. Zero memory allocation on periodic ticks.
- **Modern Dark Flyout Card**: Left-click the tray icon to reveal a sleek 300x190 dark card featuring a circular battery ring indicator (`#3EBE8E` emerald for active, `#00D2FF` cyan for charging, `#EB5555` red for low battery), monospace percentage, and natural status labels.
- **Multi-Model Support**: Out-of-the-box native catalog for **AULA F75, F87, F99, and F68**. Includes an intelligent fallback to *"Aula Wireless Keyboard"* for any Compx/Aula dongle (`VID_3554`).
- **Extensible via `models.json`**: Easily add new keyboard models or override hardware IDs by simply editing `models.json` in the application directory—no code recompilation required.
- **Asynchronous Event-Driven Monitoring**: Uses a decoupled background `PeriodicTimer` worker. Timeouts and device reconnections never block or freeze the Windows taskbar or tray menu.
- **Lithium Voltage Sag Rebound Filter**: Suppresses false +1% to +7% battery jumps caused by lithium voltage recovery when keypresses/RGB loads stop, while immediately recognizing authentic charging.
- **Ultra-Low Memory Footprint**: Automatically trims unneeded startup CLR and JIT pages, maintaining a lean **~12.8 MB Working Set** and 0% CPU.
- **100% Offline & Private**: Zero telemetry, zero external network calls, zero third-party drivers.

---

### Installation & Quick Start

1. Download the latest release from the **[Releases](https://github.com/brhnshn/AulaTray/releases)** page (`AulaTray.exe` or `AulaTray-v1.x.x-win-x64.zip`).
2. Place `AulaTray.exe` (and optionally `models.json`) in your preferred folder.
3. Run `AulaTray.exe`. No installer or admin privileges required.
4. *(Optional)* Right-click the tray icon and check **Start with Windows** (`Windows ile Başlat`) to run on boot.

> **System Requirements**: Windows 10 (version 1809+) or Windows 11 (x64). [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0).

---

<a name="models-json"></a>
### Custom Model Configuration (`models.json`)

To add support for a new keyboard or customize hardware IDs, place a `models.json` file next to `AulaTray.exe`:

```json
[
  {
    "modelName": "Aula F75",
    "dongleVid": "0x3554",
    "donglePids": ["0xFA09"],
    "wiredVid": "0x258A",
    "wiredPids": ["0x010C"],
    "btKeywords": ["F75", "AULA F75", "AULA-F75"]
  },
  {
    "modelName": "Aula F87",
    "dongleVid": "0x3554",
    "donglePids": ["0xFA0A", "0xFA10"],
    "wiredVid": "0x258A",
    "wiredPids": ["0x010D"],
    "btKeywords": ["F87", "AULA F87"]
  },
  {
    "modelName": "Aula F99",
    "dongleVid": "0x3554",
    "donglePids": ["0xFA11"],
    "wiredVid": "0x258A",
    "wiredPids": ["0x010E"],
    "btKeywords": ["F99", "AULA F99"]
  }
]
```

---

<a name="türkçe"></a>
## Türkçe

### Genel Bakış

**AulaTray**, **AULA mekanik klavye ailesi** (F75, F87, F99, F68 ve özel modeller) için geliştirilmiş bağımsız, hafif ve modern bir Windows sistem tepsisi pil ve durum monitörüdür. 

Resmi yazılımın arayüzünde pil takibinin yetersiz olması ve yüksek arka plan kaynak tüketimi sorununa çözüm olarak geliştirilen AulaTray; doğrudan yerel USB HID, 2.4 GHz RF dongle ve Bluetooth LE (BLE) üzerinden haberleşir. Yalnızca **~12.8 MB RAM** tüketir ve bekleme anında **%0 CPU** yüküyle sessizce çalışır.

---

### Öne Çıkan Özellikler

- **Metalik Cam "A" Logo Simgesi**: Görev çubuğunda estetik cam marka logosu. Klavye bağlıyken canlı ve parlak; uykuya geçtiğinde veya bağlantı koptuğunda loş ve yarı saydam gri tonlamalı görünür. Periyodik döngülerde sıfır bellek tahsisi (zero-allocation) yapar.
- **Kompakt Koyu Temalı Durum Kartı**: Tepsi simgesine tıklandığında açılan 300x190 ölçülerinde modern kart. Dairesel ilerleme halkası (`#3EBE8E` yeşil / `#00D2FF` şarj mavisi / `#EB5555` kırmızı), konsol fontunda net pil yüzdesi ve sade Türkçe durum metinleri. Kart dışına tıklandığında veya ESC ile anında kapanır.
- **Çoklu Model Desteği (Multi-Model)**: Aula F75, F87, F99 ve F68 için hazır yerleşik katalog. Bilinmeyen bir Compx/Aula dongle takıldığında ise arayüzde *"Aula Wireless Keyboard"* olarak adlandırıp pili izlemeye devam eden akıllı geri çekilme (fallback) mimarisi.
- **`models.json` ile Genişletilebilirlik**: Uygulama klasöründeki `models.json` dosyasını düzenleyerek kod derlemeden yeni modeller ve donanım kimlikleri ekleyebilme.
- **Asenkron Olay Güdümlü İzleme**: UI iş parçacığından bağımsız çalışan arka plan görevi (`PeriodicTimer`). Olası donanım zaman aşımlarında sistem tepsisi ve sağ tık menüsü asla kilitlenmez.
- **Lityum Voltaj Toparlanma (Sag Rebound) Filtresi**: RGB veya tuş yükü kalktığında pilde oluşan sahte %1-%7'lik voltaj sıçramalarını filtreler; gerçek şarj durumlarını ise anında tanır.
- **Ultra Düşük Bellek Kullanımı**: Otomatik çalışma kümesi daraltması (Working Set trimming) ile **~12.8 MB RAM** ve %0 CPU kullanımı.
- **%100 Çevrimdışı ve Güvenli**: Telemetri yok, harici ağ çağrısı yok, üçüncü taraf sürücü gereksinimi yok.

---

<a name="architecture"></a>
## Architecture & Documentation

The codebase is designed around deep module boundaries, a 3-layer design token system, and decoupled hardware transports:

- [**CONTEXT.md**](CONTEXT.md): Ubiquitous domain language, glossary, and conceptual models.
- [**ADR 0001**](docs/adr/0001-dongle-hid-ve-tray-mimarisi.md): 2.4 GHz Dongle HID Protocol & Native Win32 Architecture.
- [**ADR 0002**](docs/adr/0002-windows-11-pil-gorsel-tasarimi.md): Windows 11 Battery Icon Design (Superceded by ADR 0003).
- [**ADR 0003**](docs/adr/0003-cam-a-logosu-ve-status-kart-tasarimi.md): Metallic Glass "A" Brand Icon & Circular Ring Status Card.
- [**ADR 0004**](docs/adr/0004-coklu-model-ve-hibrit-model-kayit-mimarisi.md): Multi-Model Hybrid Registry & Generic Fallback.
- [**ADR 0005**](docs/adr/0005-3-katmanli-tasarim-sistemi-ve-performans-optimizasyonu.md): 3-Layer Design Token System, Zero-Allocation Icons & Async Monitoring.

### Building from Source

```powershell
# Clone repository
git clone https://github.com/brhnshn/AulaTray.git
cd AulaTray

# Run unit tests
dotnet test tests/AulaTray.Tests/AulaTray.Tests.csproj

# Publish single-file executable to dist
dotnet publish AulaTray/AulaTray.csproj -c Release -r win-x64 -o dist --self-contained false
```

---

## Disclaimer

This project is an independent community tool and is not affiliated with, authorized, or endorsed by AULA, Shenzhen Dexin Electronics, or Epomaker. "AULA", "F75", "F87", "F99", and other model designations are trademarks of their respective owners and are used strictly for hardware identification.

## License

This project is licensed under the [MIT License](LICENSE).
