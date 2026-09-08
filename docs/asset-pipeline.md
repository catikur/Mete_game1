# Asset Üretim Akışı (Kenney + Meshy)

İlk prototip Unity primitive'leriyle çalışır. **M5** ile Kenney CC0 modelleri `Resources` altına kondu;
yüklenemezse primitive'e düşülür. Meshy özel araçları aynı kancaya takılır.

## Strateji

1. **Temel set: Kenney (CC0)** — araç, bina, ağaç. Cloud Agent bu paketleri indirdi.
2. **Özel araçlar: Meshy** — hesap gerekir. Şu an dosya yok; dondurma kamyonu Kenney van + primitive top.
3. Kod: `KenneyLibrary` → `VehicleFactory` / `CityBuilder`. Prefab yoksa primitive.

Lisans özeti: [asset-licenses.md](asset-licenses.md).

## 1. Kenney (repoda)

| Katalog id | Kenney FBX |
|---|---|
| `taksi` | `taxi` |
| `minibus` | `van` |
| `kamyonet` | `truck-flat` |
| `ambulans` | `ambulance` |
| `polis` | `police` |
| `itfaiye` | `firetruck` |
| `dondurma` | `van` + primitive külah/top (pakette dondurma kamyonu yok) |
| `yaris` | `race` |

NPC: `sedan` / `suv` / `hatchback-sports`. Hırsız: `sedan-sports`. Park: taksi/sedan/suv/van/hatchback.

Binalar: `building-type-a` … `u`. Ağaçlar: `tree-large`, `tree-small`.

Yollar **prosedürel kalır** (City Kit Roads alınmadı — grid zaten kodda).

Klasör:

```
Assets/Resources/Kenney/Vehicles/*.fbx + Textures/colormap.png
Assets/Resources/Kenney/City/*.fbx + Textures/colormap.png
```

### Mac'te ilk açılış

1. Unity 6.3 LTS projeyi açar; FBX import edilir (`.meta` oluşur — commit et).
2. `KenneyAssetPostprocessor`: collider kapalı, ölçek 1, colormap nokta örnekleme, URP.
3. Pembe model: **Mete Oyunu → Projeyi Kur** veya **Kenney Materyallerini URP'ye Çevir**.
4. Play. Konsol: `[Mete Oyunu] Kenney modelleri yüklendi.`
5. FBX henüz yoksa / import kırıkssa primitive görünür; oyun oynanır.

Kenney araçları collider boyutuna ölçeklenir. Binalar parsel genişliğine, ağaçlar hedef yüksekliğe sığar.

Garaj boyası colormap’i yumuşak çarpar (`MaterialPropertyBlock`). Cam da biraz boyanır — primitive boya kadar temiz değil, kabul.

## 2. Meshy (Mac + hesap)

Cloud Agent Meshy modeli üretemez. Mac’te:

1. [meshy.ai](https://www.meshy.ai) Text to 3D → Refine → FBX, 10 binden az üçgen.
2. Prompt kalıbı:

```
cute cartoon low-poly <ARAÇ>, bright cheerful colors, toy-like proportions,
rounded edges, simple flat shading, game-ready asset, single mesh, no background
```

3. `Assets/Resources/Vehicles/<araç-id>` (ör. `dondurma`) olarak koy. Aynı id Kenney’den önce yüklenir.
4. Ölçek: `KenneyLibrary` collider kutusuna sığdırır.
5. Lisansı `docs/asset-licenses.md` tablosuna yaz.

## 3. Bağlama (yapıldı)

- `Resources/Vehicles/<id>` **veya** `Resources/Kenney/Vehicles/<model>` varsa o kullanılır.
- `CityBuilder` bina/ağaç/park araçlarında Kenney arar.
- Yoksa primitive (oyun bozulmaz).

## 4. Kontrol listesi (yeni model)

- [ ] Üçgen bütçesi (araç 10k altı, bina 2k altı, ağaç 500 altı)
- [ ] Pivot alt-merkez (Kenney böyle)
- [ ] Materyal URP Lit (pembe değil)
- [ ] `docs/asset-licenses.md` güncel
