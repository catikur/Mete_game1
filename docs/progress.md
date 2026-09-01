# İlerleme ve bağlam özeti

Bu dosya, Mete'nin Oyunu'nun **şu ana kadarki tüm kararlarını, kod durumunu ve Mac test döngüsünü** tek yerde tutar. Yeni bir oturum / ajan buradan başlayabilir.

Son güncelleme: **2026-09-01** — iPhone kontrolleri: sol GAZ/GERİ/BİP, sağ şeffaf yön joystick'i.

Repo: `https://github.com/catikur/Mete_game1`  
Dal şablonu: `cursor/<kısa-ad>-26ab`  
Sahip: catikur. Dil: Türkçe (oyuncu metinleri ve bu dokümanlar).

---

## Oyun nedir?

5–12 yaş (ağırlık 6–9) için **iPhone / iPad**, yatay ekran, GTA 2 tarzı kuş bakışı şehir sürüşü. Şiddet yok, kaybetme yok, reklam/IAP/internet yok. Made for Kids hedefi.

Çocuk şehirde araba sürer, yardımsever görevleri tamamlar, altın ve yıldız kazanır. Garaj (henüz yok) ile araç açılacak.

**Tasarım sütunları (değişmez):**

1. Tek parmak kontrol — okuma bilmeyen 5 yaş oynayabilmeli.
2. Görev asla başarısız olmaz. Süre bitince de teslim edilir; bonus kaçar, ceza yok.
3. Kısa döngü, sürekli ödül (kutlama yazısı, ses, yıldız).
4. Güvenli içerik.

---

## Teknoloji

| Konu | Karar |
|---|---|
| Motor | Unity **6.3 LTS (6000.3.x / 6000.3.23f1)** + **URP** |
| Geliştirme | Mac; Cloud Agent Unity Editor çalıştıramaz |
| Şehir / UI / araç | **Çalışma zamanında C#'tan üretilir** — sahne dosyaları neredeyse boş |
| İlk Mac açılışı | `Assets/Editor/ProjectSetup.cs` (menü: **Mete Oyunu → Projeyi Kur**) |
| Girdi | Eski Input Manager + UI (Input System yok) |
| Kayıt | `Application.persistentDataPath/save.json` |
| Görsel | Şimdilik Unity primitive'leri; Kenney + Meshy sonra (M5) |

Cloud Agent **Unity açamaz**. Değişiklikler kod + doküman; görsel doğrulama Mac'te Play ile yapılır.

---

## Mac'te dene (kullanıcının döngüsü)

1. Unity'yi kapat.
2. `git checkout main && git pull origin main` (veya ilgili PR dalı).
3. Unity Hub → **Unity 6.3 LTS** ile aç.
4. Pembe/boş sahne: **Mete Oyunu → Projeyi Kur**.
5. `City` sahnesi, Game view **16:9 landscape**, **Play**.

Editör kontrolleri: **W** gaz, **A/D** direksiyon, **S** geri, **H** korna. Game penceresine tıklayıp basılı tutmak da gazdır.

---

## Kontroller (güncel)

| Girdi | Aksiyon |
|---|---|
| Sol alt **GAZ** | Basılı tut = hızlan, bırak = yavaşlayıp dur |
| Sol alt **GERİ** | Basılı tut = geri git, bırak = çabuk dur |
| Sol alt **BİP** | Korna — yakındaki yayalar zıplar |
| Sağ alt şeffaf **joystick** | Yukarı/aşağı/sağ/sol: araç o yöne döner (ekran yukarı = kuzey) |

Playtest: tam ekran kaydırarak dönmek iPhone'da zordu; gaz ve yön ayrıldı. İleride sağ alta direksiyon da konabilir. HUD ipucu: `Sol: gaz / geri / bip • Sağ: yön`.

Editör: **W** gaz, **A/D** dönüş, **S** geri, **H** korna. Game görünümünde joystick fareyle de sürüklenir.

---

## Şehir

- 6×6 blok grid, ~226 m kenar, sabit seed `20260831` — şehir her oyunda aynı.
- Yollar (sağ şerit), kaldırımlar, şerit, yaya geçidi, pastel binalar, parklar, çevre çiti.
- Kavşak lambaları: tüm kavşaklar aynı faz (K-G yeşil, sonra D-B). NPC durur; oyuncu durmak zorunda değil.
- İlk kez kırmızıda yavaşça durursa günde bir kez **"Dikkatli sürüş! +1 YILDIZ"**.
- ~16 NPC araç, ~24 yaya, park halindeki arabalar.
- Çarpışma: kaza/takla yok; oyuncu yavaşlar, NPC yoluna devam eder.
- Görev teklifi açıkken oyuncu aracı kilitli; şehir yaşamaya devam eder.
- Gün değişince (oturum ortası dahil) günlük sayaç sıfırlanır.

---

## Görevler

Türler: **Kurye, Taksi, Kayıp hayvan, Okul servisi, Hızlı teslimat**.

Akış: teklif (BAŞLA) → alış halkası → kargo çatıya biner → bırakış halkası → kutlama → yeni teklif.

Üretim: günlük tarih + görev sırası tohumu. Alış–bırakış 60–160 m. Günde 5 hedef; sonrası bonus görev, oyun durmaz.

### İki aşamalı süre (2026-08-31)

Teklifte **Kolay / Orta / Zor** ve iki süre görünür: **AL** ve **TESLİM**.

1. **BAŞLA** sonrası 1. geri sayım: oyuncudan **ilk adrese** (alış). HUD: `AL` + nokta `● ○`.
2. Alıştan sonra 2. geri sayım: **teslimat**. HUD: `TESLİM` + `● ●`.

Formül (`MissionClock` / `GameConfig`):

- `süre ≈ mesafe / 6 m/s + 18 sn tampon`, tür ve zorluk çarpanı, 5'e yuvarla, en az **25 sn**.
- Tür: hayvan biraz bol, okul servisi sıkı, hızlı teslimat en sıkı.
- Zorluk: Kolay ×1.25, Orta ×1, Zor ×0.90 (Zor görev +10 altın).
- Renk: yeşil → sarı → kırmızı; `0:00 GEÇ` kırmızı nabız. **Süre bitince görev BATMAZ.**

Ödül:

- Görev tamamlama: her zaman **1 yıldız** + mesafe altını.
- Her zamanında bacak: **+1 yıldız** ve **+5 altın** (iki bacak zamanında = 3 yıldız).
- İkisi de zamanında: **seri** artar, `+5 × seri` altın. Bir bacak gecikirse seri sıfırlanır.
- HUD sağ üst: `SERİ ×N` (N≥2). Ana menüde rekor seri.

Alışta toast: `ZAMANINDA! ALDIN!` veya `ALDIN!`. Başlangıçta `HADİ!` + ding.

Yönlendirme: büyük sarı HUD oku + metre; 12 m içinde `HEMEN YANINDA!`. Hedefte halka + ışık sütunu. Araç üstünde 3D ok **yok** (karışıyordu). Sol üst: sikke + **ALTIN**, yıldız + **YILDIZ**.

---

## Eğlence ekleri (bu tur)

- Hızlanınca kamera FOV 50→58 (GTA hissi).
- Alış ding, teslimat akoru, BAŞLA “go” sesi (prosedürel, dosyasız).
- Çatıdaki kargo zıplar / döner.
- Hedefe yaklaşınca ok büyür.

---

## Kayıt şeması (`SaveData`)

`coins`, `stars`, `totalMissionsCompleted`, `dailyDate`, `dailyCompleted`, `courtesyAwarded`, `currentStreak`, `bestStreak`, `unlockedVehicleIds` (şimdilik `taksi`), `selectedVehicleId`.

Garaj henüz bağlı değil; alanlar hazır.

---

## Kod haritası

```
Assets/Scripts/
  Core/         GameBootstrap, GameConfig, SaveManager, SaveData, PartFactory, MaterialLibrary, Sfx
  City/         CityBuilder, CityLayout, CardinalDir (UnityEngine.Compass ile çakışmasın diye Compass değil)
  Traffic/      TrafficSystem, TrafficCar, Pedestrian
  Vehicle/      VehicleController, VehicleFactory
  Controls/     DriveInput
  CameraRig/    FollowCamera
  Missions/     Mission, MissionClock, MissionGenerator, MissionManager, MissionMarker, CargoBob
  UI/           HudController, SteerJoystick, HoldButton, UIFactory, MainMenuController
Assets/Editor/  ProjectSetup.cs
docs/           game-design, roadmap, mac-setup, asset-pipeline, progress (bu dosya)
```

Sahneler Play'de koddan: `Boot` menü, `City` oyun. `GameRoot` + `GameBootstrap`.

---

## Birleşmiş PR'lar (main)

| PR | Konu |
|---|---|
| #1 | M0–M2: iskelet, prosedürel şehir, arcade araç, görev, JSON kayıt |
| #2 | Şehir hayatı: ışık, NPC, yaya, park araç, korna, nezaket yıldızı, teklifte freeze, günlük reset |
| #3 | CS0104: `Compass` → `CardinalDir` |
| #4 | Dokunmatik sürüş + büyük HUD oku + iri görev işaretleri |
| #5 | Araç üstü ok kaldırıldı; ALTIN / YILDIZ etiketli ikonlar |
| #6 | İki aşamalı görev süreleri, zorluk, seri, tempo |

Playtest sırası: boş şehir → Compass hatası → görev oku görünmüyor → dokunmatik gaz/fren → ikonlar belirsiz → süreler → **iPhone'da kaydırarak dönmek zor (bu dal: gaz butonu + joystick)**.

---

## Bilinçli olarak henüz yok

- Garaj sahnesi ve 7 araç (sıradaki büyük iş, M4)
- Sağ altta direksiyon (şimdilik joystick; istenince değiştirilir)
- Kenney/Meshy modeller, müzik, motor sesi, konfeti (M5)
- TestFlight / Made for Kids başvurusu (M6)
- Görev başarısızlığı, can, kaza fiziği, kırmızı ışık cezası — **yapılmayacak**

---

## Bu dalda değişen dosyalar (gaz + yön joystick)

- `DrivePad.cs` kaldırıldı (tam ekran kaydırma)
- `SteerJoystick.cs`, `DriveInput.cs`, `VehicleController.cs`, `HoldButton.cs`, `HudController.cs`
- Kontroller: sol GAZ/GERİ/BİP, sağ şeffaf joystick (ekran yönü = araç burnu)
