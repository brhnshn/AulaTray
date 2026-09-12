# CONTEXT (Etki Alanı Modeli ve Terimler Sözlüğü)

Bu belge Aula F75 klavye izleme ajanı projesinin etki alanı kavramlarını ve resmi terimlerini tanımlar.

## Terimler Sözlüğü

### Donanım ve Taşıma Katmanı
- **Compx Alıcı (Receiver / Dongle):** Klavyenin 2.4 GHz kablosuz iletişimini sağlayan USB aygıtı (`VID: 0x3554`, `PID: 0xFA09`).
- **Özel İletişim Kanalı (Vendor Channel):** Dongle üzerinde `UsagePage: 0xFF02` ve `Usage: 0x0002` ile tanımlı 20 baytlık iki yönlü raporlama arayüzü (Col01).
- **Durum Sorgusu (Status Query):** Dongle'a gönderilen 20 baytlık `0x13 0x4A` komut çerçevesi.

### Durum ve Ölçüm Katmanı
- **Pil Düzeyi (Battery Level):** Klavyenin bildirdiği yüzde cinsinden (`%0 - %100`) tam sayı değeri (Yanıt çerçevesinin 5. baytı).
- **Güç Durumu (Power State):** Klavyenin güç kaynağı davranışı:
  - `Pilde (Discharging)`: Klavye kendi bataryasından çalışıyor (Durum baytı: `0x01`).
  - `Şarj Oluyor (Charging)`: Klavye harici güç kaynağına bağlı ve şarj ediliyor (Durum baytı: `0x10`).
- **Bağlantı Durumu (Connection State):**
  - `Bağlı (Connected)`: Dongle takılı ve klavyeden geçerli durum çerçevesi alınıyor.
  - `Klavye Uykuda / Kapalı (Sleeping / Offline)`: Dongle takılı ancak klavye yanıt vermiyor.
  - `Alıcı Yok (Disconnected)`: Dongle bilgisayara takılı değil.

### Kullanıcı Arayüzü Katmanı
- **Sistem Tepsisi Simgesi (Tray Icon):** Windows 11 tasarım diliyle birebir uyumlu, yatay yuvarlak hatlı pil gövdesi + şimşek glifi (şarjdaysa) + yanında sayısal yüzde metni barındıran dinamik simge.
- **Durum İpucu (Tooltip):** Fare imleci simge üzerine geldiğinde görünen `Aula F75: %XX (Durum)` formatında sade metin.
- **Yoklama Aralığı (Polling Interval):** 60 saniye.
- **Kritik Pil Bildirimi:** Pil seviyesi %20 altına düştüğünde gönderilen tek seferlik yerel bildirim.
- **Otomatik Başlatma (Auto-Start):** Windows açılışında `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` anahtarı üzerinden otomatik başlama.
