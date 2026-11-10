# Horsecremental — orijinal mekanik promptları

Eski sohbetlerdeki kullanıcı mesajlarından ve gönderilmiş metin eklerinden çıkarılmıştır. Prompt metinleri yeniden yazılmamıştır. Metin eklerinin kopyaları SHA-256 ile doğrulanmıştır.

Bu dosyalar geliştirme geçmişidir. Önceki aşamaların bazı kararları sonraki promptlarla değişmiştir; mevcut projeye bütün promptları sırayla yeniden uygulamak için hazırlanmış bir talimat dizisi değildir.

| Sıra | Orijinal prompt | Kapsam |
| --- | --- | --- |
| 1 | [Basic Horse Racing](01-Basic-Horse-Racing.txt) | Milestone 1: otomatik koşu, AI, Space boost, E zıplama, engeller, geri sayım, bitiş ve restart. |
| 2 | [Yarış görüntüsü, tap boost ve sonuç tablosu](02-Race-Visuals-Tap-Boost-Leaderboard.txt) | İnterpolasyon, pixel-perfect görüntü, kalıcı sahne nesneleri, her Space basışında boost ve bütün atların süreleri. |
| 3 | [Core Incremental Loop](03-Core-Incremental-Loop.txt) | Milestone 2: yarış → Coins → ahır → Speed / Acceleration / Boost Power upgrade → tekrar yarış. |
| 4 | [Unified World, Training, Farm](04-Unified-World-Training-Farm.txt) | Milestone 3: tek dünya, Cinemachine geçişleri, pasif Training, zor Main Race, boost metre, havuç üretimi/besleme ve game feel. |
| 5 | [Blacksmith, Equipment, Production](05-Blacksmith-Equipment-Production.txt) | Milestone 4: Metal, ekipman kademeleri, crafting, üretim upgrade'leri ve ekonomi dengesi. |
| 6 | [Upgrade Tree](06-Upgrade-Tree.txt) | Upgrade ön koşulları, Coin ve Main Race sırasından kazanılan Race Token ile açılışlar. |
| 7 | [Interactive Stable / Farm](07-Interactive-Stable-Farm.txt) | Milestone 4.5: etkileşimli dünya, görünür Training atları, fiziksel çiftlik/hasat/toplama, sadeleştirilmiş demirci. |
| 8 | [Demo Core Loop Master Brief](08-Demo-Core-Loop-Master-Brief.txt) | Yeni demo hedefi; manuel Training, Training Tokens, gündüz/gece ve zamanlanmış Main Race tasarımı. Bu metin tümünü hemen uygulamayı istemez. |
| 9 | [Manual Training Races](09-Manual-Training-Races.txt) | Training I–III, birincilikle sonraki kademeyi açma, Coins + Training Tokens, gerçek at upgrade'leri; pasif Training'in devreden çıkarılması. |

## Kaynaklar

- 1: `Build horse racing prototype` sohbetindeki `729ef332-c0fd-4ce5-ab6d-cd4a9fbf7553` metin eki.
- 2: aynı sohbetin `PLEASE IMPLEMENT THIS PLAN` kullanıcı mesajı.
- 3: `Build coin-to-upgrade loop` sohbetindeki `bb44a9ee-2ea6-4747-8151-f8b1296f9fb5` metin eki.
- 4: yerel metin eklerinden `bc8b0700-38a8-4d4e-ba5d-c51b1f419781`; `c7f12bea-5c58-4718-9e8e-7351539f967c` ekiyle birebir aynı. Kaynak sohbet bu taramada eşleştirilmedi.
- 5: `Add blacksmith progression` sohbetindeki `534b27af-4c44-41fc-ba88-68ebd8fc5cbb` metin eki.
- 6: `Upgradeleri upgrade tree’ye bağla` sohbetindeki ilk kullanıcı mesajı.
- 7: `Inspect project and improve Stable` sohbetindeki `1cdf5959-8a59-4f5c-ac42-a5da578d64dc` metin eki.
- 8: `Inspect horse racing project` sohbetindeki `162e346b-df89-4ace-9367-9df8548d8f85` metin eki.
- 9: aynı sohbetin `Implement ONLY the new Training Race system` kullanıcı mesajı.

## Sonraki kararların etkisi

- İlk prompttaki cooldown boost, 2. promptta her basışın güç eklediği sisteme, 4. promptta yeni boost metre davranışına evrildi.
- Önceki pasif Training tasarımları, 9. prompttaki manuel Training yarışlarıyla değişti.
- Demirci/crafting eski geliştirme aşamasıdır; güncel aktif demoda demirci görsel amaçlıdır.
- 8. prompttaki gündüz/gece ve Main Race zamanlaması hedef tasarımdır. 9. prompt bu sistemlerin uygulanmasını özellikle kapsam dışında tutar.
