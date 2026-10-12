# Milestone 2: Yarış → Coins → At yükseltme

**Horsecremental > Open Milestone 2 Stable** menüsünü kullanın veya `Assets/Racing/Scenes/Milestone2Stable.unity` sahnesini açın ve Play'e basın. **YARIŞI BAŞLAT** yarışa geçer ve geri sayımı başlatır. Yarışı bitirin, Coins ödülünü alın, **AHIRA DÖN** ile yükseltme satın alın ve tekrar yarışın. Build Settings sırası Stable → Race olarak hazırdır.

İlerleme yalnızca oyun çalışırken korunur; Play durunca sıfırlanır. Hız, ivmelenme ve boost yükseltmeleri gerçek yarış simülasyonuna uygulanır. Ekonomi ayarları `Config/Progression.asset` içindedir. Ayrıntılı uygulama/dosya/test raporu: [Milestone2.md](Milestone2.md). **20 EditMode + 3 PlayMode testi geçti.** Son doğrulamalar `Tools/Milestone2Validation` altındadır.

## Milestone 1 yarış mekanikleri

Unity'de **Horsecremental > Open Milestone 1 Race** menüsünü kullanın veya
`Assets/Racing/Scenes/Milestone1Race.unity` sahnesini açın. Pist, atlar, kamera ve başlangıç arayüzü Play öncesinde de görünür. **Play**, ardından **Yarışı Başlat** düğmesine basın. Klavye için Game görünümüne odaklanın.

- Alt şeritteki altın renkli at oyuncudur. İleri hareket otomatiktir.
- **Space'e tekrar tekrar basın:** Temel at için her basış +1 boost ekler; üst sınır +5'tir ve saniyede 3 azalır. Boost yükseltmeleri basış gücünü ve üst sınırı artırır. Basılı tutmak otomatik tekrar yapmaz. Cooldown yoktur; çubuk mevcut boost miktarını gösterir.
- **E:** Yalnızca yerdeyken zıplar. Turuncu engelden önce sarı yaklaşım işaretinin yakınında zıplayın.
- Engeli kaçırmak kısa bir yavaşlama ve kırmızı görsel geri bildirim oluşturur; atın ilerlemesini engellemez.
- Üç rakip otomatik koşar ve zıplar. Dört at ve beş engel vardır.
- Oyuncu bitirince tablo açılır: sıra, at adı ve iki ondalık basamaklı bitiş süresi. Henüz bitirmeyenler **Yarışıyor** olarak görünür ve bitirdikçe tablo güncellenir. Oyuncunun satırı altın renkle vurgulanır.
- Ana sonuç düğmesi **AHIRA DÖN**'dür. **Tekrar Yarış (Debug)** diğer atlar hâlâ koşarken de kullanılabilir; her restart yeni 3–2–1–GO sayımıyla başlar.

## Ayarlar ve sahne düzeni

`Assets/Racing/Config` içindeki at statları: Speed, Acceleration, Boost Power (üst sınır), Boost Per Tap, Boost Decay Per Second, Jump Power, Gravity ve engel cezası. Eski boost duration/cooldown alanlarının yerine basış gücü ve azalma hızı gelir.

`PrototypeRace.asset`: Pist uzunluğu, at konfigürasyonları, sprite sheet'ler, engel mesafeleri/boyutları ve AI zıplama zamanlaması.

Pist, görseller, kamera ve arayüz sahneye kaydedilir. Stat değişiklikleri Play'de doğrudan okunur. Pist, katılımcı sayısı, sprite sheet veya engel düzeni değiştiğinde **Horsecremental > Rebuild Milestone 1 Race** menüsüyle sahneyi yenileyin. Menü yalnızca prototip kökünün **Generated Race** altını yeniden oluşturur; kalıcı sprite asset GUID'lerini korur. Kaydedilmiş sahne kapatılıp açıldığında da görünür.

`RacePrototype` Play sırasında yalnızca mevcut nesneleri bağlar; yeni pist/at/UI üretmez. Oluşturulan sprite kareleri `Assets/Racing/Generated` altında saklanır.

## Görüntü ve hareket

Simülasyon sabit 0,02 saniyelik adımlarla çalışır. Atların görsel X konumu ve zıplama yüksekliği görüntü kareleri arasında interpolasyonla çizilir; Cinemachine kamera güncellemesi at görsellerinden sonra çalışır ve görsel hedefi takip eder. Stable ve diğer sahne kameraları da CinemachineBrain ile yönetilir. Bütün sahne Canvas'ları Scale With Screen Size, 1920×1080 kullanır. Reset ve bitişte geçmiş konumlar temizlenir.

URP Pixel Perfect Camera: 24 PPU, 640×360 referans, Upscale Render Texture, Point filtre ve Windowbox. 1280×720 ve 1920×1080 tam piksel katlarıdır. Diğer boyutlarda kenar boşlukları olabilir. Game görünümünü **1x** ölçekle kontrol edin; Unity editörünün Game görünümünü kesirli yakınlaştırması önizlemeyi bulanıklaştırabilir. Pixel-art hareketi doğal piksel adımları içerir.

## Testler

**Window > General > Test Runner**:

- **EditMode:** Boost yığılması/sınırı/azalması, zıplama, engel cezası, AI bitişleri ve reset; kaydedilmiş sahnenin Play öncesinde görünmesi ve yeniden açılması.
- **PlayMode:** Input System Space/E girişleri, Space'i basılı tutmanın tekrar üretmemesi, iki yarış sonucu, canlı sonuç satırları, restart, nesne çoğalmaması; 30/60/120 FPS simülasyon zamanlarında görsel interpolasyon.

Doğrulama raporu ve ekran görüntüleri `Tools/RaceValidation` altında tutulur.

## Sınırlar

Sabit şeritler, basit altı karelik at animasyonu ve placeholder pist kullanılır. Jockey veya gerçekçi fizik yoktur. Hız, zıplama ya da engel boyutlarında büyük değişiklikler AI zıplama zamanlamasının yeniden ayarlanmasını gerektirebilir. Stable ve Race build sahne listesinde hazırdır.

Milestone 2 ekonomi, Stable ve runtime ilerleme döngüsü uygulanmıştır. Kalıcı save/load henüz yoktur; Milestone 3 otomatik başlatılmamıştır.

At görselleri: projedeki **Horse Pack by loota9**, CC BY 4.0. Orijinal bilgi: `Assets/Sprites/Horse Pack/Horse Pack/Read Me.txt`. https://linktr.ee/loota9
