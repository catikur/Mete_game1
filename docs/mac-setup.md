# Mac Kurulumu ve iOS Build Rehberi

## 1. Gerekli Araçlar

| Araç | Nereden | Not |
|---|---|---|
| Unity Hub | [unity.com/download](https://unity.com/download) | Unity sürümlerini yönetir |
| Unity 6.3 LTS (6000.3.x) | Unity Hub → Installs → Install Editor | **iOS Build Support** modülünü işaretle |
| Xcode | Mac App Store | iOS build için gerekli (~15 GB, sabırlı ol) |
| Git | Mac'te hazır gelir | — |

Unity Personal lisansı ücretsizdir; ilk açılışta Unity hesabıyla giriş istenir.

> Proje `ProjectSettings/ProjectVersion.txt` içinde 6000.3.x sürümüne sabitlenmiştir.
> Hub'da birebir aynı yama (patch) sürümü yoksa sorun değil — herhangi bir **6000.3.x**
> sürümüyle aç, Unity gerisini halleder.

## 2. Projeyi Açma

```bash
git clone https://github.com/catikur/Mete_game1.git
```

1. Unity Hub → **Add → Add project from disk** → klonladığın klasörü seç.
2. Projeye tıkla. **İlk açılış 5-10 dakika sürer** (paket indirme + import).
3. İlk açılışta `Assets/Editor/ProjectSetup.cs` otomatik çalışır ve şunları kurar:
   - URP render ayarları (`Assets/Settings/`)
   - `Boot` ve `City` sahneleri (`Assets/Scenes/`)
   - Build ayarları, oyun adı, yatay ekran yönelimi, iOS bundle id
   - Temel materyal (`Assets/Resources/Materials/MeteLit`)

   Konsolda şu mesajı görmelisin: `[Mete Oyunu] Kurulum tamam!`
4. `City` sahnesi otomatik açılır → **Play** tuşuna bas.

> Kurulum herhangi bir nedenle çalışmazsa menüden elle tetikleyebilirsin:
> **Mete Oyunu → Projeyi Kur (Setup)**

### Editörde test kontrolleri

- Direksiyon: **Sol/Sağ ok** veya **A/D**
- Geri/Fren: **Aşağı ok**, **S** veya **Boşluk**
- Korna: **H**
- Game penceresinin en-boy oranını **16:9 Landscape** yapmayı unutma.

## 3. İlk Commit'ler Hakkında Not

Unity, her asset için `.meta` dosyası üretir. Projeyi ilk açtıktan sonra `git status`
yeni `.meta` dosyaları ve `Assets/Scenes`, `Assets/Settings` içeriği gösterecek —
bunları commit'lemek **doğru ve gereklidir** (dosya kimlikleri bu şekilde sabitlenir):

```bash
git add Assets ProjectSettings Packages
git commit -m "Unity ilk açılış: otomatik üretilen sahneler ve meta dosyaları"
git push
```

## 4. iPhone/iPad'e Yükleme (Kablo ile, Ücretsiz)

Kendi cihazına test için yüklemek **ücretsizdir** (Apple ID yeterli):

1. Unity: **File → Build Profiles → iOS → Switch Platform** (birkaç dakika sürer).
2. **Build** → bir klasör seç (ör. `Builds/iOS`) → Unity bir Xcode projesi üretir.
3. `Builds/iOS/Unity-iPhone.xcodeproj` dosyasını Xcode ile aç.
4. Sol panelde **Unity-iPhone** hedefini seç → **Signing & Capabilities**:
   - **Team**: Apple ID'ni ekle (Xcode → Settings → Accounts → Add).
   - **Automatically manage signing** işaretli olsun.
5. iPhone/iPad'i kabloyla bağla. Cihazda **Ayarlar → Gizlilik ve Güvenlik →
   Geliştirici Modu**'nu aç (cihaz yeniden başlar).
6. Xcode'da üstteki cihaz listesinden telefonu seç → **▶ Run**.
7. İlk çalıştırmada cihazda: **Ayarlar → Genel → VPN ve Aygıt Yönetimi** →
   geliştirici hesabına güven ver.

> Ücretsiz Apple ID imzası 7 günde bir yenilenmek ister; tekrar Run demen yeterli.

## 5. TestFlight ile Dağıtım (Ücretli Hesap Gerekir)

Aileye/arkadaşlara kablosuz dağıtmak için [Apple Developer Program](https://developer.apple.com/programs/)
üyeliği gerekir (yıllık 99 USD):

1. [App Store Connect](https://appstoreconnect.apple.com)'te yeni uygulama oluştur
   (Bundle ID: `com.metegames.metenoyunu` — `ProjectSetup.cs` içinde değiştirilebilir).
2. Xcode: **Product → Archive** → **Distribute App → App Store Connect → Upload**.
3. App Store Connect → TestFlight sekmesi → test kullanıcılarını e-postayla davet et.

## 6. App Store "Made for Kids" Notları

Yayına hazırlanırken (M5):

- App Store Connect'te **Kids kategorisi** ve yaş bandı (6-8 veya 9-11) seçilir.
- Oyunda reklam, IAP, dış bağlantı ve veri toplama olmadığı için ek izin akışı gerekmez.
- Gizlilik etiketi: "Veri Toplanmıyor" olarak beyan edilir.

## 7. Sık Karşılaşılan Sorunlar

| Sorun | Çözüm |
|---|---|
| "Failed to resolve packages" hatası | İnternet bağlantısını kontrol et; Unity'yi kapatıp aç. Sorun sürerse `Packages/manifest.json` içindeki URP sürümünü Package Manager'ın önerdiği sürüme güncelle. |
| Sahne boş görünüyor | Doğru: sahnede sadece `GameRoot` var; şehir Play'e basınca koddan üretilir. |
| Her şey pembe/mor görünüyor | URP ataması eksik: **Mete Oyunu → Projeyi Kur (Setup)** menüsünü çalıştır. |
| Xcode "Signing" hatası | Team seçtiğinden ve cihazda Geliştirici Modu'nun açık olduğundan emin ol. |
