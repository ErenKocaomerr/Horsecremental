# Upgrade Tree ve Race Token uygulama plani

Durum: Kullanici onayi ile kod, ayarlar ve mevcut oyun sahnesine uygulandi.

## Baslangic

Stable, otomatik Training, Main Race ve Speed gelistirmesi acik.
Acceleration, Boost Power, Farm, Blacksmith ve alti uretim gelistirmesi kilitli.
Coin, Metal, Carrot ve Race Token bakiyeleri sifirdan baslar.
Kilitli Farm havuc uretmez; toplama ve besleme yapilamaz.
Kilitli Blacksmith craft baslatamaz.
Main Race ilk andan itibaren ulasilabilir: oyuncu sonuncu olsa da token kazanabilir.

## Agac ve ilk acilis maliyetleri

| Dugum | On kosul | Acilis bedeli |
|---|---|---|
| Speed | Yok | Baslangicta acik |
| Acceleration | Speed | 80 Coin |
| Boost Power | Acceleration | 150 Coin |
| Training Speed | Acceleration | 150 Coin |
| Training Reward | Training Speed | 180 Coin |
| Unlock Farm | Speed | 5 Race Token |
| Grow Speed | Farm | 120 Coin |
| Farm Capacity | Grow Speed | 100 Coin |
| Unlock Blacksmith | Farm | 10 Race Token |
| Forge Speed | Blacksmith | 150 Coin |
| Metal Efficiency | Forge Speed | 200 Coin |

Acilislar tek seferliktir. Acilan at ve uretim gelistirmelerinin sonraki seviye
satin almalari mevcut Coin maliyetleri ve seviye sinirlari ile devam eder.
Ekipmanin kademeli craft sistemi Blacksmith acilinca kullanilabilir; mevcut
Coin + Metal maliyetleri korunur.

## Yaris odulleri

| Main Race sonucu | Race Token |
|---|---|
| 1. | 5 |
| 2. | 3 |
| 3. | 2 |
| 4. | 1 |

Mevcut Coin ve Metal yaris odullerine token eklenir.
Training yalnizca mevcut Coin ve Metal odullerini verir.
Coin, Metal ve token ayni tamamlanmis yaris kimligi ile bir kez odenir.
Eski veya terk edilmis yaris kimligi odul veremez.

## Hile tusu

Game gorunumune odaklanip K tusuna basin: her basista 100.000 Coin ve 1.000
Race Token verilir. Basili tutmak tekrar odul vermez. Hile dugumleri otomatik
acmaz; agactaki on kosullar ve odeme kurallari devam eder. Hile mevcut yarisi
bitirmez veya yarisin odul hakkini tuketmez. Tekrar basmak yeniden kaynak verir.
Miktarlar UpgradeTree.asset icindeki Debug Coins ve Debug Tokens alanlarindan
degistirilebilir.

## Teknik baglantilar

- Saf C# agac kurallari ve merkezi PlayerProgression kilit/token durumu.
- ScriptableObject uzerinden duzenlenebilir on kosullar ve acilis maliyetleri.
- ProgressionSettings uzerinden siralamaya gore token odulleri.
- PlayerProgression.TryUpgrade, TryUpgradeProduction, TryStartCraft, AdvanceFarm,
  CollectCarrots ve TryFeed icinde model seviyesinde kilit kontrolu.
- GameplayWorld ve AreaNavigationUI icinde Farm/Blacksmith erisim kontrolu.
- Upgrade Tree ekrani: dallar, baglanti cizgileri, maliyetler, on kosullar,
  kilit/acik/satin alinabilir durumlari ve bakiye.
- Stable ve uretim kartlarinda kilit sebebi ve agaca yonlendirme.
- Kaynak ust cubugu ve yaris sonucunda Race Token gosterimi.
- Unity editor menu araci ile mevcut Milestone3World sahnesine uygulama;
  sahne ve mevcut asset GUID'leri korunur.
- Mevcut ProgressionSession ile sahne gecislerinde korunur. Projede kalici
  save/load bulunmadigi icin Play durunca ilerleme sifirlanmaya devam eder.

## Dogrulama

- Yeni oyuncunun yalnizca baslangic dugumleri acik.
- On kosul saglanmadan satin alma basarisiz, bakiyeler degismiyor.
- Coin dugumu token, token dugumu Coin harcamiyor.
- Yetersiz bakiye veya tekrar acilis para dusurmuyor.
- Kilitli Farm uretim/toplama/besleme yapmiyor; acilinca uretim basliyor.
- Kilitli Blacksmith craft yapmiyor; acilinca mevcut craft akisi calisiyor.
- Her siralama dogru token veriyor; Training token vermiyor.
- Tek yaris iki kez odenmiyor; bakiyelerde int tasmasi engelleniyor.
- PlayMode'da yeni baslangic, ilk Main Race, token kazanimi, Farm acilisi,
  agac on kosullari ve UI uzerinden satin alma dogrulanir.
- Mevcut testlerde tum ozelliklerin baslangicta acik oldugunu varsayan kurulumlar,
  yeni acilis kurallarina gore guncellenir; ilgili EditMode/PlayMode testleri calistirilir.
- Agac ve kilitli/acik kartlar farkli ekran oranlarinda goruntulenir.
