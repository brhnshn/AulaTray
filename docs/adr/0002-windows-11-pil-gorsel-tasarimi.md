# ADR 0002: Windows 11 Pil ve Yüzde Görsel Tasarımı

- **Durum:** Kabul Edildi
- **Tarih:** 2026-09-12

## Bağlam
Sistem tepsisi simgesi için Windows 11 yerel tasarım dili tercih edilmiştir. Kullanıcının sağladığı referans görsel doğrultusunda simge; ince beyaz hatlı yuvarlatılmış pil gövdesi, iç doluluk, şarj anında şimşek simgesi ve yanında okunaklı yüzde metnini içermelidir.

## Karar
1. **Görsel Çizim:** Sistem tepsisi simgesi (Icon) `System.Drawing` ile bellek içinde (in-memory bitmap) 32x32 piksel formatında dinamik üretilecektir.
2. **Düzen:**
   - Sol tarafta yatay pil gövdesi (beyaz sınır, yeşil/sarı/kırmızı iç doluluk, şarj halinde beyaz şimşek ikonu).
   - Sağ tarafta kompakt ve keskin 'Segoe UI' yazı tipiyle sayısal yüzde metni (örn. `83%`).
   - Pil doluluk rengi: %50-%100 yeşil (#5AD469), %20-%49 sarı (#E8B83A), %0-%19 kırmızı (#E04848).
3. **Tooltip:** Emojisiz, standart `Aula F75: %XX (Pilde)` veya `Aula F75: %XX (Şarj Ediliyor)`.
