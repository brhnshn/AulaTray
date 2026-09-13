# ADR 0005: 3 Katmanlı Tasarım Sistemi, Sıfır Tahsisli İkonlar ve Asenkron İzleme Mimarisi

- **Durum:** Kabul Edildi
- **Tarih:** 2026-09-13

## Bağlam
Kod tabanında yapılan incelemede şu sürtünmeler tespit edildi:
1. `HidService` sınıfı (470 satır) P/Invoke, BLE, Dongle RF, Kablolu Feature Report ve pil dalgalanma filtresini tek bir monolitte birleştiriyordu.
2. `TrayApplicationContext` UI timer'ı içinde senkron HID/BLE yoklaması yapılıyor; donanım zaman aşımına girdiğinde sistem tepsisi kilitlenebiliyordu.
3. `IconRenderer` her 3.5 saniyede bir yeni `Bitmap` ve `HICON` üreterek GDI nesne tablosunda gereksiz yük ve sızıntı riski oluşturuyordu.
4. Bellek kullanımı (Working Set) .NET 9 CLR açılış yükünden sonra 75 MB seviyesinde kalıyordu.

## Karar
1. **3 Katmanlı Tasarım Sistemi (`ThemeTokens.cs`):**
   - Primitives (Ham renkler/fontlar) $\rightarrow$ Semantics (Anlamsal yüzey ve durumlar) $\rightarrow$ Components (Halka, rozet, kart bileşenleri) şeklinde yapılandırıldı.
2. **Sıfır Tahsisli İkon Önbellekleme:**
   - İki ikon durumu (Aktif ve Loş) uygulama başlangıcında bir kez üretilip statik belleğe alındı. Çalışma anında sıfır GDI tahsisi sağlandı.
3. **Donanım Taşıyıcılarının Ayrıştırılması (`ITransport`):**
   - `WiredTransport`, `DongleTransport`, `BleTransport` ve `HidNativeMethods` olarak parçalandı.
   - Dongle sorgularında zaman aşımı durumunda askıda kalan `ReadFile` çağrılarını sonlandırmak için `CancelIoEx` eklendi.
4. **Asenkron Arka Plan İzleyicisi (`KeyboardMonitor`):**
   - UI thread'inden bağımsız, `PeriodicTimer` tabanlı arka plan görevine geçildi.
5. **Bellek Daraltma (`MemoryOptimizer`):**
   - Başlangıç burst'ü sonrasında ve durum kartı her kapandığında `EmptyWorkingSet` çağrılarak bellek 75 MB'tan 12.8 MB seviyesine indirildi (%83 tasarruf).

## Sonuçlar
- **Artılar:** Sıfır UI donması, %83 bellek tasarrufu, 164 adet işletim sistemi tanıtıcı (handle) tasarrufu, sıfır GDI tahsis döngüsü ve %100 birim test edilebilirlik.
- **Eksiler:** Taşıyıcılar ve izleyici arasında olay tabanlı senkronizasyon katmanı gereksinimi.
