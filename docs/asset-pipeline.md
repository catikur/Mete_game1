# Asset Üretim Akışı (Kenney + Meshy)

İlk prototip tamamen Unity primitive'leriyle (kutu, silindir, küre) çalışır — hiçbir model
dosyası gerekmez. Bu doküman, M4'te görselleri gerçek 3D modellerle değiştirme akışını anlatır.

## Strateji

1. **Temel set: Kenney (ücretsiz, CC0)** — şehir, yol ve araç modellerinin ana kaynağı.
2. **Özel araçlar: Meshy (AI ile üretim)** — katalogda olmayan, oyuna özgü sevimli araçlar için.
3. Kod tarafı hazır: araç ve şehir üreticileri primitive yerine prefab kullanacak şekilde
   genişletilecek (bkz. `VehicleFactory`, `CityBuilder`).

## 1. Kenney Paketleri (kenney.nl)

[Kenney](https://kenney.nl/assets) tüm paketlerini **CC0** (kaynak gösterme gerektirmez,
ticari kullanım serbest) lisansla dağıtır. Önerilen paketler:

| Paket | İçerik |
|---|---|
| **Car Kit** | ~50 low-poly araç: taksi, ambulans, itfaiye, polis, yarış arabası... |
| **City Kit (Roads)** | Modüler yol parçaları, kavşaklar |
| **City Kit (Suburban)** / **Commercial** | Binalar, evler, dükkânlar |
| **Nature Kit** | Ağaçlar, çalılar, park öğeleri |

### İçe aktarma

1. Paketi indir, zip içinden `Models/FBX format/` klasörünü bul.
2. Unity'de `Assets/Art/Kenney/<PaketAdı>/` klasörüne sürükle.
3. Model import ayarları: **Scale Factor** kontrol et (Kenney modelleri genelde 1 birim = 1 m),
   **Generate Colliders** kapalı kalsın (collider'ları kod ekliyor).
4. Materyaller: model başına tek renk paleti tekstürü gelir — URP'ye otomatik dönüşmezse
   model seçiliyken **Materials → Extract Materials** yap, shader'ı `Universal Render Pipeline/Lit` seç.

## 2. Meshy ile Özel Araç Üretimi (meshy.ai)

[Meshy](https://www.meshy.ai) metinden 3D model üretir. Ücretsiz katman deneme için yeterli;
üretilen modellerin ticari kullanım hakları için mevcut plan koşullarını kontrol et.

### Prompt şablonu

Tutarlı bir görsel dil için hep aynı stil kalıbını kullan:

```
cute cartoon low-poly <ARAÇ>, bright cheerful colors, toy-like proportions,
rounded edges, simple flat shading, game-ready asset, single mesh, no background
```

Örnekler: `ice cream truck`, `school bus`, `tow truck with hook`, `little fire truck`.

### Üretim adımları

1. **Text to 3D** ile üret → beğendiğin varyantı seç → **Refine**.
2. Poligon hedefi: araç başına **< 10.000 üçgen** (mobil bütçesi).
3. **FBX** formatında dışa aktar (Unity glTF/GLB'yi doğrudan okumaz).
4. `Assets/Art/Meshy/Vehicles/` klasörüne at; tekstürü aynı klasöre koy.
5. Ölçek kontrolü: araç uzunluğu ~4 m olacak şekilde import Scale Factor ayarla
   (sahneye at, primitive taksiyle karşılaştır).

## 3. Modellerin Oyuna Bağlanması (M4'te yapılacak)

Plan şu şekilde:

- `Assets/Resources/Vehicles/<araç-id>.prefab` varsa `VehicleFactory` primitive gövde yerine
  bu prefabı yükleyecek; yoksa primitive'e düşecek (fallback).
- `CityBuilder` bina/ağaç üretiminde aynı desenle `Resources/City/` prefablarını arayacak.
- Böylece modeller **teker teker, oyunu hiç bozmadan** eklenebilir.

## 4. Kontrol Listesi (her yeni model için)

- [ ] Üçgen sayısı bütçede mi? (araç < 10k, bina < 2k, ağaç < 500)
- [ ] Ölçek doğru mu? (araç ~4 m, bina katı ~3 m)
- [ ] Pivot noktası modelin **alt-merkezinde** mi?
- [ ] Materyal URP Lit mi? (pembe görünüyorsa değildir)
- [ ] Tek materyal/tekstür mü? (draw call bütçesi)
- [ ] Lisans notu `docs/asset-licenses.md` dosyasına eklendi mi? (dosyayı ilk modelle birlikte oluştur)
