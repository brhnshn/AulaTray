# CONTEXT (Etki Alanı ve Terimler Sözlüğü)

Bu belge **AulaTray** projesinin etki alanı (domain) modelini, kullanılan terimleri ve kavramsal sınırlarını tanımlar.

---

## Terimler Sözlüğü (Ubiquitous Language)

### 1. Donanım & Model Kavramları
- **AulaTray:** Windows görev çubuğu sistem tepsisinde (System Tray) arka planda sessizce çalışan, minimal kaynak tüketen bağımsız klavye pil ve durum monitörü.
- **Klavye Modeli (KeyboardModel):** Belirli USB Vendor ID (VID), Product ID (PID) ve Bluetooth adı anahtar kelimelerine sahip klavye donanım tanımı. (Yerleşik: Aula F75, Aula F87, Aula F99, Aula F68; Özel: `models.json` üzerinden tanımlanan modeller).
- **Generic Fallback (Genel Geri Çekilme):** Takılı Compx/Aula dongle'ı (`VID_3554`) katalogda özel olarak eşleşmese bile pili okunabiliyorsa klavyeyi arayüzde *"Aula Wireless Keyboard"* olarak adlandırma politikası.

### 2. İletişim & Taşıyıcılar (Transports)
- **Taşıyıcı (Transport):** Klavyenin bilgisayarla fiziksel veya telsiz olarak veri alışverişi yaptığı protokol kanalı (`ITransport`).
  - **Dongle Taşıyıcısı (Wireless24G):** Compx 2.4 GHz USB RF alıcısı üzerinden 20 baytlık tescilli sorgu paketi (`0x13, 0x4A`) ile iletişim.
  - **Kablolu Taşıyıcı (Wired):** Sinowealth USB HID Feature Report (`0x06, 0x84`) üzerinden şarj ve pil seviyesi okuma.
  - **Bluetooth Taşıyıcısı (Bluetooth):** Standart Windows Bluetooth Low Energy GATT Battery Service (`0x180F` / `0x2A19`) üzerinden okuma.

### 3. Durum ve Güç Kavramları
- **Bağlantı Durumu (ConnectionState):**
  - **Bağlı (Connected):** Klavye açık, menzil içinde ve donanım paketlerine anında yanıt veriyor.
  - **Uykuda (Sleeping):** Dongle takılı ancak klavye güç tasarrufu için telsiz yayını kapatmış durumda; ilk tuşa basıldığında uyanır.
  - **Çevrimdışı (Disconnected):** Ne kablo, ne dongle ne de aktif bir Bluetooth bağlantısı mevcut değil.
- **Güç Durumu (PowerState):**
  - **Pilde çalışıyor (Discharging):** Dahili lityum bataryadan güç tüketiliyor.
  - **Şarj oluyor (Charging):** Kablo üzerinden harici güç beslemesi alınıyor.
- **Voltaj Toparlanması (Voltage Sag Rebound):** Lityum pillerde tuş vuruşları veya RGB aydınlatma yükü kalktığında voltajın geçici olarak yükselmesiyle okunan %1-%7'lik sahte pil sıçraması.
- **Pil Filtresi (BatteryFilter):** Voltaj toparlanmalarını en az 5 periyot (yaklaşık 18s) boyunca filtreleyip sahte artışları engelleyen, ancak gerçek şarj durumunda veya $\ge \%8$ artışlarda anında yeni değeri kabul eden durum makinesi.

### 4. Görsel Kimlik ve Tasarım Sistemi
- **Marka Logosu (Brand Logo):** Evrensel marka kimliğini temsil eden ve hem tepside hem açılır durum kartında kullanılan parlak metalik cam "A" logosu.
- **Tasarım Sistemi (ThemeTokens):**
  - **İlkel Katman (Primitives):** Ham renk hex değerleri ve font aileleri.
  - **Anlamsal Katman (Semantics):** Amaca yönelik yüzey ve durum adlandırmaları (`SurfaceBase`, `StateActive`, `StateCharging`).
  - **Bileşen Katmanı (Components):** Durum halkası, mod kapsülü ve kart öğelerine özel ölçü ve fırçalar.
