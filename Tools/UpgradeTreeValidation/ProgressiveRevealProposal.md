# Upgrade Tree: aşamalı keşif ve arka plan önerisi

Durum: Kullanıcı onayıyla uygulandı. Arka plan, kullanıcının ek isteğine uygun
olarak prosedürel UI shader'ı ile oluşturuldu. Sonuçlar
`Tools/ProgressiveUpgradeTreeValidation/Validation.md` dosyasında.

## Oyuncunun göreceği davranış

- Açılmış düğümler ve onların doğrudan bağlı sonraki yükseltmeleri görünür.
- Daha uzak düğümler ve bunlara giden çizgiler gizlenir.
- Henüz satın alınmamış görünen düğümler sönük ikon ve kilit işareti taşır.
- Ön koşulu tamamlanmış ve bedeli karşılanabilen düğümün çerçevesi vurgulanır.
- Satın alma sonrası kilit kalkar; o düğümün sonraki adımları kısa bir belirme animasyonuyla görünür.
- Gizli düğümler tıklama, hover bilgi paneli veya klavye odağı alamaz.
- Kaynak artışı yalnızca satın alınabilirlik vurgusunu değiştirir; tek başına gizli dalları açmaz.

Mevcut Playground ayarlarında Speed ve Farm başlangıçta açık. İlk görünümde
Speed, Farm, Acceleration ve Grow Speed bulunur. Acceleration satın alınınca
Boost Power ve Training Speed görünür; Training Speed satın alınınca Training
Reward görünür. Playground tarafından devre dışı bırakılan Blacksmith,
Farm Capacity, Forge Speed ve Metal Efficiency bu akışla etkinleştirilmez.

## Görsel yön

Referanstaki gibi koyu lacivert/mor zemin, düşük kontrastlı arazi ve ağaç
silüetleri, hafif piksel dokusu ve koyu kenarlar. At oyunu temasına uygun
uzak ahır, çit ve tepe silüetleri kullanılabilir. Açık düğümler sıcak krem/altın,
kilitli düğümler soluk gri tonlarda, bağlantılar ince ve sakin görünür.

Arka plan Tree Panel'in Image materyalindeki shader ile çizilir. Terrain noise,
mor sis, ağaç silüetleri, piksel dokusu ve vignette shader içinde üretilir.
Arazi sabittir; sis çok yavaş hareket eder ve Drift Speed sıfırlanarak
durdurulabilir. Dekoratif Image bileşenlerinde raycast kapatılır.

## Uygulama kapsamı

- Merkezi ağaç sunumu ilerleme olayına göre görünürlüğü yönetir; gizlenen
  düğümlerin yeniden açılması için aktif nesne durumunu düğümün kendi event
  aboneliğine bırakmaz.
- UpgradeTreeNodeUI, UpgradeIconState ve gerekirse UIDetailTarget ile kilit,
  görünürlük, bilgi ve odak davranışları uyumlu hale getirilir.
- Tree Panel düzeni kökten dallanan, okunaklı bir yerleşime taşınır.
- Yeni arka plan asset'i ve sunum ayarları eklenir; mevcut sahnedeki yalnızca
  Upgrade Tree sunumu hedeflenir. Genel Milestone kurulumunu yeniden çalıştırmak
  yerine sınırlı bir Editor kurulum aracı kullanılır.
- Mevcut fiyatlar, ön koşullar, başlangıç açılışları ve Playground'ın devre dışı
  bıraktığı düğümler korunur.

## Doğrulama

Unity derlemesi ve Console kontrolü; başlangıç görünümü, ardışık satın alma,
yetersiz bakiye, kaynak hilesinin gizli düğüm açmaması, görünmez çizgiler,
gizli düğümlerde mouse/klavye erişimi ve yeniden ekran açılışı doğrulanır.
İlgili mevcut EditMode/PlayMode testleri çalıştırılır ve UI ekran görüntüsüyle
görsel sonuç kontrol edilir.
