# Horse Woodland Expansion

Yeni görseller, verilen orman/platformer paketi ve at sprite'ları referans alınarak built-in ImageGen ile üretildi.

- `WoodlandTerrain.png`: üstten görülen çim/toprak; 4 x 4 düzen, 16 parça.
- `WoodlandObstacles.png`: şeffaf zemin; kütük, tek/çift çıtalı bariyer, taş yığını, saman, ahşap kapı, kırık çit, yosunlu kütük, kütük kökü, dal ve alçak taş duvar; 12 parça.
- `ReferenceHorse.png`: gönderilen görselden ImageGen ile ayrılmış, şeffaf ve statik kahverengi at; stil/ölçek referansı. Animasyon içermez.

Unity menüsü: **Horsecremental > Woodland Expansion > Prepare Assets**. Bu işlem yeni görselleri Point filtre, mipmaps kapalı, sıkıştırmasız Sprite olarak içe aktarır; isimlendirilmiş sprite'lar, Tile assetleri ve görsel engel prefabları oluşturur. Mevcut oyun sahnelerini veya yarış kodunu değiştirmez.

**Create Preview Scene** ayrı bir sahne oluşturur; mevcut oyun sahnelerini kaydetmez veya üzerine yazmaz. Gönderilen at referansını, çim/toprak pistini ve yeni engelleri birlikte gösterir. Önizleme statiktir; yarış veya atlama mekaniği eklemez. Editörde isimsiz sahne açıksa önce onu kaydedin.

At referansı 24 PPU ve 64 x 48 frame kullanır. Yeni kaynak PNG'ler AI çıktısıdır, native 32 px tile garantisi yoktur; importer dünya ölçüsünü atla eşleştirir. Terrain 32 kaynak-oyun pikseline karşılık gelen 1.333 dünya birimi olarak, engeller yaklaşık 64 kaynak-oyun pikseli genişliğinde hücreler olarak ayarlanır. Engel sprite pivotları görünür nesnenin tabanına konur.

Tile Palette'te `Tiles` klasöründeki assetleri kullanın. Kenar/köşe parçaları elle yerleştirilir. AI üretimi yol geçişleri birebir matematiksel autotile garantisi vermez; nihai pistte tekrar izlerini görsel olarak kontrol edin.

`Prefabs` görsel engellerdir; collider veya gameplay değerleri eklenmemiştir. Mevcut yarışa bağlama ayrı bir değişikliktir.

Üretim istemleri: `GenerationPrompts.json`.
