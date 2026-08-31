# Yol Haritası

## M0 — Repo ve Proje Kurulumu ✅

- [x] Oyun tasarım dokümanı, kurulum ve asset rehberleri
- [x] Unity 6.3 LTS proje iskeleti (`Packages/manifest.json`, `ProjectSettings`)
- [x] İlk açılışta otomatik kurulum: URP ayarları, sahneler, build ayarları (`Assets/Editor/ProjectSetup.cs`)
- [x] Unity `.gitignore`

## M1 — Sürülebilir Prototip ✅

- [x] Prosedürel şehir: yollar, şerit çizgileri, kaldırımlar, binalar, parklar, çevre çiti
- [x] Arcade araç kontrolcüsü: otomatik gaz, iki butonlu direksiyon, geri vites, yumuşak çarpışma
- [x] Primitive'lerden araç gövdesi (kasa, kabin, tekerlekler, farlar)
- [x] Kuzeyi sabit, eğimli takip kamerası (look-ahead ile)
- [x] Dokunmatik butonlar + editörde klavye

## M2 — Görev Sistemi ✅

- [x] Görev üretici: 5 görev türü, günlük tohum, mesafeye göre ödül
- [x] Görev akışı: teklif → alış noktası → bırakış noktası → kutlama → yeni görev
- [x] HUD: altın/yıldız sayaçları, görev metni, hedefe dönen ok + mesafe, günlük ilerleme
- [x] Hedef işaretleri: ışık halkası + zıplayan ikon + araca binen kargo görseli
- [x] Kayıt sistemi: JSON (altın, yıldız, günlük sayaç), arka plana geçişte otomatik kayıt
- [x] Boot sahnesi: ana menü (OYNA butonu, altın/yıldız göstergesi)

## M3 — Şehir Hayatı ✅

- [x] Yaya geçitleri ve kaldırımda park halindeki arabalar
- [x] Kavşak trafik lambaları (senkron faz, NPC uyar, oyuncu cezalandırılmaz)
- [x] NPC araçlar: sağ şerit, ışık, mesafe, kavşak dönüşü, yumuşak çarpışma
- [x] Yayalar: kaldırım döngüsü, yeşilde karşıya geçiş, oyuncudan kaçma
- [x] Korna (BİP / H) — yayalar zıplar, prosedürel ses
- [x] Kırmızıda durunca günde bir kez nezaket yıldızı
- [x] Görev teklifi açıkken oyuncu durur; şehir yaşamaya devam eder
- [x] Gün değişince (oturum ortası dahil) günlük sayaç yenilenir

## M4 — Garaj 🔜

- [ ] Araç tanımları (katalog: 7 araç, fiyat/hız/boyut)
- [ ] Garaj sahnesi: podyum, araç seçimi, satın alma
- [ ] Renk özelleştirme (8 renk paleti)
- [ ] Kayıt sistemine araç/renk alanlarının bağlanması
- [ ] Ana menüden ve şehirden garaja geçiş

## M5 — İçerik ve Cila 🔜

- [ ] Kenney/Meshy modelleriyle görsel yükseltme (araçlar, binalar, dekorlar)
- [ ] Ses: müzik, motor sesi, kutlama jingle'ı
- [ ] Konfeti/partikül kutlamaları
- [ ] Performans: draw call azaltma (static batching / mesh birleştirme)

## M6 — iOS Yayın 🔜

- [ ] Xcode build doğrulaması, cihaz testleri (iPhone + iPad)
- [ ] Uygulama ikonu ve açılış ekranı
- [ ] TestFlight dağıtımı
- [ ] App Store "Made for Kids" başvuru hazırlığı
