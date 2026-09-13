# ADR 0004: Çoklu Model Desteği ve Hibrit Model Kayıt Mimarisi

- **Durum:** Kabul Edildi
- **Tarih:** 2026-09-13

## Bağlam
Uygulama ilk geliştirildiğinde yalnızca Aula F75 modeli için tasarlanmıştı. Ancak Aula klavye ailesi F87, F99, F68 gibi benzer Compx ve Sinowealth yonga setlerini kullanan birçok popüler modele sahiptir. Kullanıcıların uygulamanın kaynak kodunu derlemek zorunda kalmadan yeni modeller veya özel donanım kimlikleri ekleyebilmesi gerekmektedir.

## Karar
1. **Hibrit Katalog:**
   - **C# İçi Yerleşik Katalog:** Aula F75, F87, F99 ve F68 için fabrika çıkışı VID/PID ve Bluetooth eşleşmeleri kod içine gömüldü.
   - **Harici Yapılandırma (`models.json`):** Uygulamanın çalıştığı dizinde bir `models.json` dosyası varsa, açılışta okunarak yerleşik katalogla birleştirilir veya mevcut modelleri geçersiz kılar (override).
2. **Generic Fallback:**
   - Eğer takılan dongle Compx/Aula ailesine aitse (`VID_3554`) ancak özel PID tanımlanmamışsa, uygulama klavyeyi sorgulamaktan vazgeçmez; arayüzde *"Aula Wireless Keyboard"* olarak adlandırıp pili okumaya devam eder.
3. **Dinamik Durum:**
   - Algılanan model adı (`KeyboardStatus.ModelName`), açılır kartın başlığında, tepsi bildirimlerinde ve menüde dinamik olarak gösterilir.

## Sonuçlar
- **Artılar:** Uygulama tek bir klavyeye bağımlı olmaktan çıkıp tüm Aula ekosistemini destekleyen genişletilebilir bir platforma dönüştü.
- **Eksiler:** `models.json` hatalı JSON formatında düzenlenirse sessizce yerleşik kataloga geri düşülür (fallback).
