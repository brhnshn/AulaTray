# ADR 0001: 2.4 GHz Dongle HID Protokolü ve NativeAOT / Win32 Tray Mimarisi

- **Durum:** Kabul Edildi
- **Tarih:** 2026-09-12

## Bağlam
Aula F75 klavyesinin şarj ve güç durumunun Windows ortamında sürekli, düşük bellek (RAM < 10MB) ve sıfır işlemci (%0 CPU) yüküyle izlenmesi gerekmektedir. Resmi üretici yazılımı pil göstergesini arayüzde devre dışı bırakmıştır (`ShowPower=0`). Tersine mühendislikle Compx 2.4G dongle (`0x3554:0xFA09`) üzerinde `UsagePage 0xFF02` kanalında `0x13 0x4A` komutunun gerçek pil yüzdesini ve şarj durumunu (`0x10` = şarj, `0x01` = pilde) verdiği doğrulanmıştır.

## Karar
1. **İletişim Katmanı:** Harici ağır HID kütüphaneleri (HidSharp, libusb vb.) yerine doğrudan Windows `hid.dll` ve `kernel32.dll` Win32 API çağrıları (P/Invoke) kullanılacaktır.
2. **Uygulama Mimarisi:** Sistemde kurulu .NET 9.0 altyapısı üzerinde Windows Forms NotifyIcon ve ApplicationContext tabanlı, penceresiz (windowless) saf sistem tepsisi mimarisi kurulacaktır.
3. **Optimizasyon:** Bellek tüketimini en aza indirmek için gereksiz arka plan iş parçacıkları açılmayacak, tek bir timer tabanlı olay döngüsü işletilecektir. Simge bellekte dinamik ve sade olarak çizilecektir.
4. **Tasarım Dili:** Emojisiz, nötr, sade ve profesyonel bir Windows yerel arayüz dili benimsenecektir.

## Sonuçlar
- **Artılar:** Bellek tüketimi ~5-8 MB aralığında kalır. CPU tüketimi yoklama aralıkları dışında %0.0'dır. Dış kütüphane bağımlılığı yoktur.
- **Eksiler:** Dongle çıkarıldığında veya klavye derin uykuya girdiğinde durum 'Yanıt Yok / Uykuda' olarak ele alınmalıdır.
