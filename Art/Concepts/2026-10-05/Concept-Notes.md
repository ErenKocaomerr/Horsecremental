# Horsecremental — Pixel Art Konseptleri

Üretim: 5 Ekim 2026. Araç: yerleşik image_gen.

Her pano üstte ahır, altta yarış ekranı içerir. Bunlar görsel yön belirleme konseptleridir; hazır tile set veya animasyon sprite sheet değildir.

## Mevcut oyuna uyum

Mevcut README temel alındı: yandan görünüm, dört yarış şeridi, jokeysiz atlar, engeller, hız/ivmelenme/boost yükseltmeleri. Dünya için mevcut 640×360 referans çözünürlük ve 24 PPU, üretim aşamasında tutarlı piksel ölçeğinin başlangıç noktası olabilir. Konsept görselleri bu çözünürlüğün teknik olarak doğrulanmış assetleri değildir.

## Görsel yönler

1. **[Sıcak çiftlik](01-sicak-ciftlik.png):** Bal rengi ahşap, toprak tonları ve sakin yeşiller. Ahırı büyütme hissi için önerilen başlangıç.
2. **[Canlı arcade](02-canli-arcade.png):** Güçlü renk ayrımı ve daha belirgin şeritler. Yarışta okunabilirlik önceliği.
3. **[Atmosferik kırsal](03-atmosferik-kirsal.png):** Eski ahşap, kehribar ışık, serin gölgeler ve katmanlı doğa. Çevre sanatı daha fazla ayrıntı gerektirir.

Öneri: 1'in dünyası ile 2'nin yarış okunabilirliğini birleştir. Tek piksel ızgarası, sınırlı palet, at başına az sayıda ana ton ve sakin arka plan kullan. Oyuncuyu yalnızca renkle değil küçük bir işaretle de ayırt et.

## İncelenecek oyunlar

| Oyun | İlişki | İnceleme odağı |
|---|---|---|
| [Pocket Stables](https://store.steampowered.com/app/1918530/Pocket_Stables/) | Pixel art; çiftlik yönetimi, at eğitimi ve yarış | İlk incelenecek örnek: ahır–antrenman–yarış ilişkisi, küçük at siluetleri ve bilgi yoğunluğu |
| [Stardew Valley](https://store.steampowered.com/app/413150/Stardew_Valley/) | Pixel art çiftlik oyunu; görsel referans | Ahşap, saman ve bitki malzemelerinin ayrılması; çevre ve arayüz renkleri |
| [Rival Stars Horse Racing](https://store.steampowered.com/app/1166860/Rival_Stars_Horse_Racing_Desktop_Edition/) | 3D at yarışı ve yetiştirme; mekanik referans | Yarış geri bildirimi, at kimliği, ahır ve ilerleme sunumu |
| [Horse Farm](https://store.steampowered.com/app/821670/Horse_Farm/) | Renkli at çiftliği yönetimi; tema referansı | Ahır gelişiminin görünür hale getirilmesi ve çiftlik düzeni |

Kaynaklar oyunların Steam mağaza sayfalarıdır. İnceleme odakları tasarım önerileridir.

## Tam üretim promptları

### 01-sicak-ciftlik

```text
Use case: stylized-concept.
Asset type: pixel art game visual direction concept board for an existing horse incremental game, not a sprite sheet.
Create a landscape board containing TWO clearly separate wide gameplay scenes stacked vertically, top STABLE and bottom RACE, each a coherent approximately 16:9 game screen. Narrow separating border. Only small pixel-font labels "STABLE" and "RACE"; no other written text, no logo, no watermark.
Both scenes must use the same horse design, same pixel size and world art language.
STRICT hand-crafted-looking 2D PIXEL ART throughout: visibly discrete square pixels, intentional pixel clusters, hard aliased contours, limited palette, stepped shading. Nearest-neighbor enlarged low-resolution game aesthetic. No smooth brushwork, no gradients, no blur, no vector art, no 3D render, no voxel.
STABLE: readable side-on cutaway wooden horse stable with three stalls, a prominent golden chestnut player horse in profile, hay, water trough, tack and small trophy shelf. Ground and stall arrangement should feel usable in a game management hub. A modest bottom HUD area with three compact icon-only upgrade cards for speed, acceleration and boost; no tiny text.
RACE: side-scrolling track, all FOUR horses visibly running toward the RIGHT on FOUR parallel horizontal lanes, no jockeys or people on horses, same chestnut player horse with subtle gold marker in foreground lane, three distinctly colored rival horses in other lanes. Simple low jump hurdles ahead and small pixel dust puffs. Strong readable horse silhouettes and clearly separated lanes; foreground must stay visually clean. Fence and landscape backgrounds. Compact icon-only race HUD for progress and boost.
Treat horses as the hero, usable realistic equine proportions stylized into pixels, no oversized cartoon heads. Scenery supports readability. No depiction of a curve or oval racetrack.
Direction 01: warm welcoming rural stable and daytime country race. Medium-detail 16-bit-inspired pixels, honey-colored timber, terracotta accents, soft sage grasses, cream highlights, cool deep brown shadows. Warm sunlight expressed entirely with flat pixel color clusters. Top stable interior is cozy and organized, sunlight through open doorway. Bottom race uses a pale blue sky, quiet distant tree line, sandy tan track. Gentle restrained saturation and wood-framed HUD. The feeling is a small friendly stable gradually becoming successful.
```

### 02-canli-arcade

```text
Use case: stylized-concept.
Asset type: pixel art game visual direction concept board for an existing horse incremental game, not a sprite sheet.
Create a landscape board containing TWO clearly separate wide gameplay scenes stacked vertically, top STABLE and bottom RACE, each a coherent approximately 16:9 game screen. Narrow separating border. Only small pixel-font labels "STABLE" and "RACE"; no other written text, no logo, no watermark.
Both scenes must use the same horse design, same pixel size and world art language.
STRICT hand-crafted-looking 2D PIXEL ART throughout: visibly discrete square pixels, intentional pixel clusters, hard aliased contours, limited palette, stepped shading. Nearest-neighbor enlarged low-resolution game aesthetic. No smooth brushwork, no gradients, no blur, no vector art, no 3D render, no voxel.
STABLE: readable side-on cutaway wooden horse stable with three stalls, a prominent golden chestnut player horse in profile, hay, water trough, tack and small trophy shelf. Ground and stall arrangement should feel usable in a game management hub. A modest bottom HUD area with three compact icon-only upgrade cards for speed, acceleration and boost; no tiny text.
RACE: side-scrolling track, all FOUR horses visibly running toward the RIGHT on FOUR parallel horizontal lanes, no jockeys or people on horses, same chestnut player horse with subtle gold marker in foreground lane, three distinctly colored rival horses in other lanes. Simple low jump hurdles ahead and small pixel dust puffs. Strong readable horse silhouettes and clearly separated lanes; foreground must stay visually clean. Fence and landscape backgrounds. Compact icon-only race HUD for progress and boost.
Treat horses as the hero, usable realistic equine proportions stylized into pixels, no oversized cartoon heads. Scenery supports readability. No depiction of a curve or oval racetrack.
Direction 02: energetic arcade sports presentation with bold chunky pixel clusters and simplified clean shapes. Bright leafy greens, sky cyan, warm orange timber, dark navy outline shadows, gold accent on player. Fewer environmental details and strongest separation of gameplay foreground from background. Stable feels cheerful and immediately legible. Race uses sporty checker-pattern finish accents, bold striped hurdles, compact distant stands, clear four lanes, energetic pixel dust. HUD uses compact dark navy panels and bright simple icons. This is entirely pixel art, deliberately coarse 16-bit game readability, with natural equine proportions retained.
```

### 03-atmosferik-kirsal

```text
Use case: stylized-concept.
Asset type: pixel art game visual direction concept board for an existing horse incremental game, not a sprite sheet.
Create a landscape board containing TWO clearly separate wide gameplay scenes stacked vertically, top STABLE and bottom RACE, each a coherent approximately 16:9 game screen. Narrow separating border. Only small pixel-font labels "STABLE" and "RACE"; no other written text, no logo, no watermark.
Both scenes must use the same horse design, same pixel size and world art language.
STRICT hand-crafted-looking 2D PIXEL ART throughout: visibly discrete square pixels, intentional pixel clusters, hard aliased contours, limited palette, stepped shading. Nearest-neighbor enlarged low-resolution game aesthetic. No smooth brushwork, no gradients, no blur, no vector art, no 3D render, no voxel.
STABLE: readable side-on cutaway wooden horse stable with three stalls, a prominent golden chestnut player horse in profile, hay, water trough, tack and small trophy shelf. Ground and stall arrangement should feel usable in a game management hub. A modest bottom HUD area with three compact icon-only upgrade cards for speed, acceleration and boost; no tiny text.
RACE: side-scrolling track, all FOUR horses visibly running toward the RIGHT on FOUR parallel horizontal lanes, no jockeys or people on horses, same chestnut player horse with subtle gold marker in foreground lane, three distinctly colored rival horses in other lanes. Simple low jump hurdles ahead and small pixel dust puffs. Strong readable horse silhouettes and clearly separated lanes; foreground must stay visually clean. Fence and landscape backgrounds. Compact icon-only race HUD for progress and boost.
Treat horses as the hero, usable realistic equine proportions stylized into pixels, no oversized cartoon heads. Scenery supports readability. No depiction of a curve or oval racetrack.
Direction 03: richer atmospheric rural pixel art with carefully shaped medium-small pixel clusters, elegant horse silhouettes and a restrained earthy palette. Weathered walnut wood, moss and muted teal vegetation, ochre hay, cool indigo shadows, amber light. Stable has warm lantern light rendered as stepped hard pixel color bands, aged timber and stone footings. Race takes place in late golden afternoon with distant layered rolling hills and a few tall trees; a clean ochre track and subtle warm rim highlights separate horses. Backgrounds are detailed but lower-contrast than horses. Refined dark wood HUD with small brass accents. NO bloom or smooth lighting: atmospheric effects must all remain stepped pixel art.
```


