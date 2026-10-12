# Progressive Upgrade Tree — uygulama ve doğrulama

Uygulandı: 6 Ekim 2026. Sahne: `Assets/Racing/Scenes/Milestone3World.unity`.

## Davranış

Başlangıçta Speed, Farm, Acceleration ve Grow Speed görünür. Acceleration
satın alınınca Boost Power ve Training Speed, Training Speed satın alınınca
Training Reward görünür. Bağlantılar düğümlerle beraber 0.25 saniyede belirir.
Satın alınmamış düğümlerde kilit işareti vardır; bedeli karşılanabilenlerde
altın çerçeve vurgusu bulunur. Açılmış düğümler sekizgen ve sıcak tonludur.

Gizlenen düğümler event aboneliğini korur, CanvasGroup üzerinden görünmez ve
etkileşimsiz kalır. Button ve Tab dolaşımı gizli düğümleri atlar. Playground'ın
devre dışı bıraktığı dört düğümün aktif durumları değiştirilmedi. Mevcut
başlangıç açılışları, fiyatlar ve ön koşullar değişmedi.

## Shader ve ayarlar

- Kaynak: `Assets/Racing/Shaders/UpgradeTreeBackground.shader`.
- Materyal: `Assets/Racing/Generated/UI/UpgradeTreeBackground.mat`.
- Sunum: `Assets/Racing/Config/UIPresentation.asset`.
- Editör menüsü: `Horsecremental/Style Progressive Upgrade Tree`.

Night Color, Purple Mist ve Forest Shadows renkleri; Dark Edges kenar
kararmasını; Pixel Width piksel çözünürlüğünü; Terrain Scale arazi ölçeğini;
Pixel Grain ince dokuyu; Mist Drift Speed sis hareketini ayarlar. Drift Speed
0 ile animasyon durur. UI stencil/clip desteği vardır, dekoratif katmanlar
raycast almaz. Renk uzayından bağımsız ince gren için doku renk değerine
oransal olarak uygulanır.

Menü mevcut ağacı yerinde günceller ve sahneyi kaydeder. Genel Milestone
kurulumu yeniden çalıştırılmadı. MinimalUISetup, sonraki UI kurulumlarında da
aynı sunumu bağlar. Materyal ve konum ayarları sonraki çalıştırmalarda korunur.

## Kontroller

- UpgradeTreeTests: 12/12 EditMode testi geçti.
- UpgradeTreePlayModeTests: 3/3 PlayMode testi geçti; son sahne düzenlemesinden
  sonra tekrar doğrulandı.
- Yeni entegrasyon testi: başlangıçta 4 düğüm, hilenin görünürlüğü açmaması,
  gizli düğüme tıklama ve bilgi paneli engeli, gizli bağlantılar, satın alma ile
  sıradaki düğümlerin belirmesi, kilit kalkması ve ekran geçişlerinde durumun
  korunması.
- Shader destekleniyor; ShaderUtil mesaj listesi boş. Son Console kontrolünde
  hata veya uyarı yok.
- Klavye dolaşımının gerçek Next handler'ı 30 kez çağrıldı; tüm seçilen hedefler
  CanInspect koşulunu sağladı, gizli düğüm seçilmedi.
- Mevcut MinimalUI testlerinin Capture/ValidateLayout yardımcıları Play Mode'da
  doğrudan çalıştırıldı: 1920×1080, 1280×720, 1024×768, 2560×1080 oranlarında
  düğümler ekran içinde, düğümler arası çakışma yok, bilgi paneli düğümleri
  kapatmıyor. 1024×768 çıktısı ayrıca görsel olarak kontrol edildi.
- MinimalUI test grubunu ve assembly'yi MCP test runner üzerinden seçme
  denemeleri 0 test döndürdü; bunlar geçmiş test olarak sayılmadı. İlgili
  klavye ve layout kontrolleri yukarıdaki doğrudan doğrulamayla tamamlandı.
- Sahne GUID'i korundu: `8789f1f4a37cec24183629f77a4555ec`.
- Son durumda Unity Edit Mode'da, sahne kaydedilmiş durumda. Kontrol sırasında
  verilen kaynaklar ve alınan yükseltmeler Play Mode sonlanınca sıfırlandı.

## Görsel kanıt

- `Screenshots/tree-initial-1.png`: son shader ile başlangıçta 4 düğüm.
- `Screenshots/tree-after-acceleration.png`: 6 görünen düğüm.
- `Screenshots/tree-after-training.png`: 7 görünen düğüm.
- `Screenshots/shader-tree-*.png`: farklı oranlarda açık ağaç ve bilgi paneli.

Başlangıç shader denemesinin kumlu görüntüsü `tree-initial.png` olarak
tutuldu; son görünüm için `tree-initial-1.png` kullanılmalı.
