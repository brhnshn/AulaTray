# ADR 0003: Metalik Cam "A" Logosu ve Kompakt Dairesel Durum Kartı

- **Durum:** Kabul Edildi (ADR 0002'nin yerine geçti)
- **Tarih:** 2026-09-13

## Bağlam
ADR 0002'de tasarlanan yatay pil gövdesi ve yanındaki sayısal metin simgesi, Windows görev çubuğunda küçük çözünürlüklerde karmaşık ve dağınık görünmekteydi. Ayrıca açılır durum penceresinde gereksiz teknik çip ve dongle detaylarının (`2.4G`, ham donanım ID'leri) tekrarlanması arayüz kalitesini düşürüyordu.

## Karar
1. **Sistem Tepsisi İkonu:**
   - Sayısal pil ikonunun yerine, yüksek çözünürlüklü metalik cam **"A" logosu** ortak marka simgesi olarak konumlandırıldı.
   - Bağlantı aktifken tam parlak, uyku modunda veya bağlantı yokken ise loş/yarı saydam gri tonlamalı render edilir.
2. **Açılır Durum Kartı (Flyout Status Card):**
   - 300x190 piksel ölçülerinde kompakt, modern ve koyu temalı kart mimarisi kuruldu.
   - Sol tarafta 72x72 SVG esintili dairesel batarya ilerleme halkası (`#3EBE8E` yeşil / `#00D2FF` şarj mavisi / `#EB5555` kritik kırmızı) ve ortasında okunaklı konsol fontuyla sayısal yüzde gösterilir.
   - Sağ tarafta doğal Türkçe durum ifadeleri (`Pilde çalışıyor`, `Şarj oluyor`, `Uykuda`, `Çevrimdışı`) ve bağlantı tipi rozeti (`USB`, `2.4G`, `BT`) yer alır.
3. **Pencere Kapanma Davranışı:**
   - Düşük seviyeli global fare kancası yerine WinForms yerel `WM_ACTIVATE (WA_INACTIVE)` pencere mesajı ve `Deactivate` olayı ile güvenli kapanma sağlandı.

## Sonuçlar
- **Artılar:** Sistem tepsisinde profesyonel, temiz ve şık bir görünüm. Durum kartında gereksiz metin kalabalığından arındırılmış yüksek okunabilirlik.
- **Eksiler:** Pil yüzdesini anlık görmek için tepsiye bir kez sol tıklamak gerekir (ancak tooltip üzerinde yüzde anlık gösterilmeye devam eder).
