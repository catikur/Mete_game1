# Mete'nin Oyunu 🚗

5-12 yaş arası çocuklar için, yukarıdan bakışlı (GTA 2 kamera tarzı) **şehir sürüş ve görev oyunu**.
iPhone ve iPad'de çalışır, Unity ile Mac üzerinde geliştirilir.

## Oyun Nedir?

Rengarenk, low-poly bir şehirde araba sürersin. Şiddet yok, kaybetme yok:

- **Görevler:** Paket teslim et, yolcu taşı, kayıp kediyi sahibine götür, öğrencileri okula bırak... Görevler her gün yenilenir ve hiç bitmez.
- **Şehir hayatı:** Başka arabalar, yayalar, trafik lambaları, yaya geçitleri. Kırmızıda durmak zorunlu değil; durursan küçük bir yıldız ödülü var.
- **Ödüller:** Görev tamamladıkça altın ve yıldız kazanırsın.
- **Garaj:** Kazandığın altınla yeni araçlar açar, renklerini değiştirirsin.

## Hızlı Başlangıç (Mac)

1. [Unity Hub](https://unity.com/download) kur ve içinden **Unity 6.3 LTS (6000.3.x)** sürümünü **iOS Build Support** modülüyle yükle.
2. Bu repoyu klonla ve Unity Hub'da **Add → Add project from disk** ile klasörü seç, projeyi aç.
   İlk açılış birkaç dakika sürer; proje kendini otomatik kurar (sahneler, render ayarları). Konsolda `[Mete Oyunu] Kurulum tamam!` mesajını görürsün.
3. `Assets/Scenes/City` sahnesi açılır — **Play** tuşuna bas ve sür!

Ayrıntılı kurulum ve iPhone/iPad'e yükleme adımları için: [docs/mac-setup.md](docs/mac-setup.md)

## Kontroller

| Platform | Gaz | Direksiyon | Fren | Geri | Korna |
|---|---|---|---|---|---|
| iPhone/iPad | Ekrana basılı tut | Sağa/sola kaydır | Parmağı kaldır | Sol alt GERİ | Sağ alt BİP |
| Editör (test) | W veya Yukarı ok | A/D veya Sol/Sağ ok | tuşu bırak | S veya Aşağı ok | **H** |

Tek parmak: bas = gaz, kaydır = dön, bırak = fren.

## Dokümantasyon

| Doküman | İçerik |
|---|---|
| [docs/game-design.md](docs/game-design.md) | Oyun tasarım dokümanı: görevler, ekonomi, garaj, güvenlik |
| [docs/roadmap.md](docs/roadmap.md) | Yol haritası ve mevcut durum |
| [docs/mac-setup.md](docs/mac-setup.md) | Mac kurulumu, iOS build ve TestFlight rehberi |
| [docs/asset-pipeline.md](docs/asset-pipeline.md) | Kenney ve Meshy ile 3D model üretim akışı |

## Proje Yapısı

```
Assets/
├── Editor/          Projeyi ilk açılışta otomatik kuran script
├── Scripts/
│   ├── Core/        Oyun başlatma, kayıt sistemi, ayarlar, materyaller
│   ├── City/        Prosedürel şehir üretimi
│   ├── Traffic/     Trafik ışıkları, NPC araçlar, yayalar
│   ├── Vehicle/     Araç fiziği ve araç oluşturma
│   ├── Controls/    Dokunmatik + klavye girişi
│   ├── CameraRig/   Takip kamerası
│   ├── Missions/    Görev üretici, görev akışı, hedef işaretleri
│   └── UI/          Kod ile üretilen arayüz (HUD, menü, butonlar)
├── Scenes/          Boot (menü) ve City (oyun) — otomatik oluşturulur
└── Settings/        URP render ayarları — otomatik oluşturulur
docs/                Tasarım ve kurulum dokümanları
```

## Teknoloji

- **Unity 6.3 LTS** (6000.3.x) + **URP** (mobil performans)
- Şehir, araçlar ve arayüz tamamen **koddan üretilir** — ilk prototip hiçbir 3D model dosyasına ihtiyaç duymaz; görsel yükseltme [asset pipeline](docs/asset-pipeline.md) ile yapılır.
- Çocuk güvenliği: reklam yok, uygulama içi satın alma yok, internet bağlantısı gerekmez, veri toplanmaz.
