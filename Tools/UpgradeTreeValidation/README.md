# Upgrade Tree uygulamasi

Mevcut `Assets/Racing/Scenes/Milestone3World.unity` sahnesine uygulandi ve
kaydedildi. Play'e basin, alt menudeki **UPGRADE TREE** dugmesini kullanin.
Manuel component veya referans atamasi gerekmiyor.

## Oyun kurallari

- Baslangicta Stable, Training, Main Race ve Speed acik.
- Acceleration, Boost Power ve uretim gelistirmeleri Coin ile tek seferlik acilir.
- Farm 5 Race Token; Farm'dan sonra Blacksmith 10 Race Token gerektirir.
- Main Race 1./2./3./4. icin 5/3/2/1 Race Token verir. Sonuncu bitirmek de
  ilerleme saglar. Training token vermez.
- Yarisin mevcut Coin ve Metal odullerine token eklenir. Yaris kimligi
  uzerinden uc kaynak tek seferde ve yalnizca bir kez odenir.
- Acilmamis dugumler model seviyesinde engellenir. Farm acilana kadar havuc
  uretmez; Blacksmith acilana kadar craft baslatilamaz.
- Acilislar seviye satin almasindan ayridir; acildiktan sonra mevcut Coin
  seviye maliyetleri, uretim sinirlari ve ekipmanin Coin/Metal bedelleri devam eder.

## K hilesi

Game gorunumune odaklanin ve **K** tusuna basin: **100.000 Coin + 1.000 Race
Token**. Her yeni basista tekrar verilir. Basili tutmak tekrar tetiklemez.
Dugumleri otomatik acmaz; agac on kosullari devam eder. Yarisin odul hakkini
tuketmez. Bu tus oyuncu build'lerinde de aktiftir.

## Ayarlar

- `Assets/Racing/Config/UpgradeTree.asset`: tum dugumler, on kosullar,
  Coin/Token secimi, ilk acilis bedelleri, baslangic kilitleri ve hile miktarlari.
- `Assets/Racing/Config/Progression.asset`: siralamaya gore Race Token odulleri.
- `Horsecremental > Setup Upgrade Tree`: agac arayuzunu ve referanslarini mevcut
  sahneye yeniden uygular. Mevcut race/world nesnelerini yeniden olusturmaz.
- Milestone 4 kurulum araci da yeni agac baglantilarini uygular.

Saf C# kurallari `UpgradeTreeRules` icinde; bakiye ve kilitler merkezi
`PlayerProgression` modelinde. `UpgradeTreeSettings` Inspector konfigurasyonunu
bu modele aktarir. K girdisi ayri `DebugResourceCheat` component'indedir.

Ilerleme mevcut `ProgressionSession` ile sahne gecislerinde tutulur. Projede
kalici save/load henuz bulunmadigindan Play durunca sifirlanir.

## Dogrulama

45 benzersiz EditMode testi dogrulandi: ilk tam kosuda 44 test gecti;
yeni sahne testinin runtime sahne adini Editor dosya yolu olarak kullanmasi
duzeltildi ve o testin tekrar kosusu gecti. Oyun kodunda derleme hatasi yok.

Tam PlayMode kosusunda **6/6 test gecti**. Eski craft, uretim, yaris/input ve
uzun ilerleme dongusu testleri korundu. Eski ozellik testlerinin kurulumu,
kilitlerin tam bedelini odeyip acarak onceki bakiye ve seviye varsayimlarini
korur; yeni oyuncu deneyimi ayri testlerle kilitli baslangictan dogrulanir.

Yeni PlayMode testleri bes gercek sonunculukla 5 token kazanmayi, Farm'i UI
uzerinden acmayi, Coin/Token bakiyelerini, on kosullari, K basma/basili tutma/
yeniden basma davranisini ve sahneler arasi ortak modeli kontrol eder.

Goruntuler `Screenshots` altinda: 1920x1080, 1280x720, 1024x768 ve
2560x1080. Kayitli sahnenin GUID'i korunur. Degisiklik oncesi dosyalar
`BackupBeforeImplementation` altinda tutulur.

Gorsel incelemede kartlarin icinden gecen baglanti cizgileri duzeltildi;
cizgiler artik kart kenarlarinda biter ve kilitli kartlarin arka plani opaktir.
Son sahne kaydindan sonra agac/K tusu PlayMode testleri tekrar calistirildi:
**2/2 gecti**. Son goruntuler 16:9, 4:3 ve genis ekran oranlarinda incelendi;
kartlar, maliyetler ve navigasyon gorunur. Console'da hata veya uyari yok.
