# Mete'nin Oyunu 🚗

5-12 yaş arası çocuklar için, yukarıdan bakışlı (GTA 2 kamera tarzı) **şehir sürüş ve görev oyunu**.
iPhone ve iPad'de çalışır, Unity ile Mac üzerinde geliştirilir.

**Neredeyiz, ne kararlaştırıldı?** Kısa bağlam: [docs/progress.md](docs/progress.md).

## Oyun Nedir?

Rengarenk, low-poly bir şehirde araba sürersin. Şiddet yok, kaybetme yok:

- **Görevler:** Paket teslim et, yolcu taşı, kayıp kediyi sahibine götür, öğrencileri okula bırak. Görevler her gün yenilenir ve hiç bitmez.
- **Süre:** Her görevde iki geri sayım vardır (ilk adrese, sonra teslime). Süre bitince görev batmaz; zamanında gidersen ekstra yıldız alırsın.
- **Şehir hayatı:** Başka arabalar, yayalar, trafik lambaları, yaya geçitleri. Kırmızıda durmak zorunlu değil; durursan küçük bir yıldız ödülü var.
- **Ödüller:** Altın, yıldız, zamanında seri.
- **Garaj:** Altınla 7 araç aç, renk seç, şehirde onunla sür.

## Hızlı Başlangıç (Mac)

1. [Unity Hub](https://unity.com/download) kur ve içinden **Unity 6.3 LTS (6000.3.x)** sürümünü **iOS Build Support** modülüyle yükle.
2. Bu repoyu klonla ve Unity Hub'da **Add → Add project from disk** ile klasörü seç, projeyi aç.
   İlk açılış birkaç dakika sürer; proje kendini otomatik kurar (sahneler, render ayarları). Konsolda `[Mete Oyunu] Kurulum tamam!` mesajını görürsün.
3. `Assets/Scenes/City` sahnesi açılır — **Play** tuşuna bas ve sür!

Güncel kodu denemek: Unity kapalıyken `git checkout main && git pull origin main`, sonra Hub'dan aç. Pembe/boş sahne olursa menüden **Mete Oyunu → Projeyi Kur**.

Ayrıntılı kurulum ve iPhone/iPad'e yükleme: [docs/mac-setup.md](docs/mac-setup.md)

## Kontroller

| Platform | Gaz | Yön | Fren | Geri | Korna |
|---|---|---|---|---|---|
| iPhone/iPad | Sol alt **GAZ** | Sağ alt şeffaf **joystick** (çubuk yönü = araç burnu) | GAZ'ı bırak | Sol alt **GERİ** | Sol alt **BİP** |
| Editör (test) | W veya Yukarı ok | A/D veya joystick'i sürükle | tuşu bırak | S veya Aşağı ok | **H** |

Sol el: gaz / geri / bip. Sağ el: yön. Tam ekran kaydırma yok.

## Dokümantasyon

| Doküman | İçerik |
|---|---|
| [docs/progress.md](docs/progress.md) | **Bağlam özeti:** kararlar, PR geçmişi, kod haritası, test döngüsü |
| [docs/game-design.md](docs/game-design.md) | Oyun tasarımı: görevler, süreler, ekonomi, güvenlik |
| [docs/roadmap.md](docs/roadmap.md) | Yol haritası (M0–M3b bitti, sırada garaj) |
| [docs/mac-setup.md](docs/mac-setup.md) | Mac kurulumu, iOS build ve TestFlight |
| [docs/asset-pipeline.md](docs/asset-pipeline.md) | Kenney ve Meshy ile 3D model üretim akışı |

## Proje Yapısı

```
Assets/
├── Editor/          Projeyi ilk açılışta otomatik kuran script
├── Scripts/
│   ├── Core/        Oyun başlatma, kayıt, ayarlar, materyaller, kısa SFX
│   ├── City/        Prosedürel şehir üretimi
│   ├── Traffic/     Trafik ışıkları, NPC araçlar, yayalar
│   ├── Vehicle/     Araç fiziği ve araç oluşturma
│   ├── Controls/    Dokunmatik + klavye girişi
│   ├── CameraRig/   Takip kamerası (look-ahead + hız FOV)
│   ├── Missions/    Görev üretici, iki aşamalı süre, işaretler
│   ├── Garage/      Garaj: podyum, satın al, boya
│   └── UI/          Kod ile üretilen arayüz (HUD, menü, butonlar)
├── Scenes/          Boot, City, Garage — otomatik oluşturulur
└── Settings/        URP render ayarları — otomatik oluşturulur
docs/                Tasarım, ilerleme ve kurulum dokümanları
```

## Teknoloji

- **Unity 6.3 LTS** (6000.3.x) + **URP** (mobil performans)
- Şehir, araçlar ve arayüz tamamen **koddan üretilir** — ilk prototip hiçbir 3D model dosyasına ihtiyaç duymaz; görsel yükseltme [asset pipeline](docs/asset-pipeline.md) ile yapılır.
- Çocuk güvenliği: reklam yok, uygulama içi satın alma yok, internet bağlantısı gerekmez, veri toplanmaz.
