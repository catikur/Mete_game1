# Yol Haritası

Bağlam özeti: [progress.md](progress.md). Tasarım: [game-design.md](game-design.md).

## M0 — Repo ve Proje Kurulumu ✅

- [x] Oyun tasarım dokümanı, kurulum ve asset rehberleri
- [x] Unity 6.3 LTS proje iskeleti (`Packages/manifest.json`, `ProjectSettings`)
- [x] İlk açılışta otomatik kurulum: URP ayarları, sahneler, build ayarları (`Assets/Editor/ProjectSetup.cs`)
- [x] Unity `.gitignore`

## M1 — Sürülebilir Prototip ✅

- [x] Prosedürel şehir: yollar, şerit çizgileri, kaldırımlar, binalar, parklar, çevre çiti
- [x] Arcade araç: **tek joystick** itince o yöne gider, bırakınca durur; yumuşak çarpışma
- [x] Primitive'lerden araç gövdesi (kasa, kabin, tekerlekler, farlar)
- [x] Kuzeyi sabit, eğimli takip kamerası (look-ahead + hızda FOV)
- [x] Sağ alt sürüş joystick'i + sol **BİP**; editörde WASD

## M2 — Görev Sistemi ✅

- [x] Görev üretici: 5 görev türü, günlük tohum, mesafeye göre ödül
- [x] Görev akışı: teklif → alış noktası → bırakış noktası → kutlama → yeni görev
- [x] HUD: ALTIN/YILDIZ etiketli sayaçlar, görev metni, büyük sarı ok + mesafe, günlük ilerleme
- [x] Hedef işaretleri: ışık halkası + ışık sütunu + zıplayan ikon + çatıda kargo (araç üstü 3D ok yok)
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

## M3b — Görev süreleri ve tempo ✅

- [x] Mesafe + tür + Kolay/Orta/Zor ile bacak süreleri (`MissionClock`)
- [x] İki geri sayım: AL (teklif → ilk adres), TESLİM (alış → bırakış)
- [x] Süre bitince görev batmaz; GEÇ + zamanında yıldız/altın bonusu
- [x] Teklif kartında zorluk ve her iki süre
- [x] Zamanında seri (HUD + kayıt), alış toast, prosedürel ding/akor
- [x] Çatı kargo zıplaması, hızda kamera FOV

## M4 — Garaj ✅

- [x] Araç tanımları (katalog: 8 araç, fiyat/hız/boyut/siluet; polis + hırsız kovalama)
- [x] Garaj sahnesi: podyum, araç seçimi, satın alma
- [x] Renk özelleştirme (8 renk paleti, 50 altın/renk)
- [x] Kayıt: açılmış araçlar, seçili araç, boyalar
- [x] Ana menüden ve şehirden garaja geçiş (görev sırasında kilitli)
- [x] Oynarken **MENÜ** / duraklatma, ana menüye çıkış, kaldığın yerden devam

## M5 — İçerik ve Cila 🔜

- [x] Görev bitince konfeti (prosedürel küpler)
- [ ] Kenney/Meshy modelleriyle görsel yükseltme (araçlar, binalar, dekorlar) — Mac’te asset
- [ ] Ses: müzik, motor sesi (prosedürel ding'lerin üzerine)
- [ ] Performans: draw call azaltma (static batching / mesh birleştirme)

## M6 — iOS Yayın 🔜

- [ ] Xcode build doğrulaması, cihaz testleri (iPhone + iPad)
- [ ] Uygulama ikonu ve açılış ekranı
- [ ] TestFlight dağıtımı
- [ ] App Store "Made for Kids" başvuru hazırlığı
