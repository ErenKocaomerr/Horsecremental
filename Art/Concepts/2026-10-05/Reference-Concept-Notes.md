# Referanslara göre pixel art konseptleri

Araç: yerleşik image_gen. Üç kullanıcı görseli stil referansı olarak doğrudan araca verildi.

- [Ahır](04-referans-ahir.png): üç bölmeli ahşap ahır, beyaz/kahverengi/koyu atlar, saman ve su kovaları.
- [Yarış](05-referans-yaris.png): yandan görünen dört şeritli toprak pist, jokeysiz dört at, ahşap engeller ve katmanlı orman.

Görsel yön: belirgin kare pikseller, koyu seçici konturlar, sınırlı gölge tonları, kahverengi toprak/ahşap, zeytin ve orman yeşilleri, açık camgöbeği gökyüzü. İlk referans at anatomisi ve sprite gölgelendirmesini; diğer iki referans çevre ve malzeme dilini belirler. Çıktılar konsepttir, teknik olarak doğrulanmış pixel-perfect oyun assetleri veya animasyon kareleri değildir.

## Tam üretim promptları

### 04-referans-ahir

```text
Use case: stylized-concept.
Asset type: standalone 16:9 landscape pixel art game environment concept for Horsecremental.
Input images: Image 1 is the EXACT horse sprite style and proportion reference (white, brown and charcoal horses). Image 2 is the vegetation, fence, soil tile and restrained pixel palette reference. Image 3 is the side-view scene construction, building materials, pixel scale and tree foliage reference. All three are style references; produce a NEW original horse-game scene, not a collage or rearrangement of the supplied images.
Primary requirement: Match the modest-resolution asset-pack pixel art shown in the references VERY closely. Prioritize Image 1 for every horse silhouette, head, mane, tail, leg proportions and shading. Horses should look like sprites from that same pack integrated into a matching environment, approximately 75–90 logical pixels across, with long legs and natural proportions.
Style: true coarse 2D pixel art on one uniform low-resolution grid, about a 400–480 pixel-wide logical game canvas enlarged with nearest-neighbor. Every visible logical pixel should be a chunky square. Hard stair-step silhouettes, dark near-black/brown/green selective outlines, large contiguous color clusters, 3–4 flat shades per material, flat diffuse daylight. Earth browns, forest and olive greens, desaturated cyan sky and muted gray-lavender accents taken from the references. Simple broad color regions with small controlled pixel texture accents. Straight side-on orthographic gameplay view, not isometric and no cinematic perspective. Keep background simpler and lower contrast than horses. Trees should match the broad leafy pixel clusters and conifer silhouettes of images 2 and 3.
Avoid: painterly artwork, smooth curves, antialiasing, blur, bloom, gradients, volumetric light, realistic rendered horses, high-resolution microdetail, decorative illustration finish, 3D, voxel, chibi proportions, golden fantasy horses, glossy UI, title text, watermark, any people or jockeys. Do not recreate the greenhouse or farming crop display from references; translate their pixel art language into horse stable and race environments. NO HUD: focus solely on environment and horses.
Scene: wooden horse stable interior with a broad side-on cutaway view. Three simple open-front stalls across the screen, holding a white horse on the left, chestnut horse in the center and charcoal horse on the right. Horses face right and closely match Image 1. Minimal worn warm brown timber posts and plank walls, gray metal fittings, ochre hay heaps and hay bales, simple water buckets and one tack hook. Broad dark soil/stone horizontal floor strip at bottom, grass edging by open stable door. Through the open door and small rear windows see cyan daylight, olive tree silhouettes and a simple timber fence. Stable appears practical and charming, roomy and readable, using the same pixel density as horse sprites. No dramatic lighting, no dense decorative clutter. Full scene edge to edge.
```

### 05-referans-yaris

```text
Use case: stylized-concept.
Asset type: standalone 16:9 landscape pixel art game environment concept for Horsecremental.
Input images: Image 1 is the EXACT horse sprite style and proportion reference (white, brown and charcoal horses). Image 2 is the vegetation, fence, soil tile and restrained pixel palette reference. Image 3 is the side-view scene construction, building materials, pixel scale and tree foliage reference. All three are style references; produce a NEW original horse-game scene, not a collage or rearrangement of the supplied images.
Primary requirement: Match the modest-resolution asset-pack pixel art shown in the references VERY closely. Prioritize Image 1 for every horse silhouette, head, mane, tail, leg proportions and shading. Horses should look like sprites from that same pack integrated into a matching environment, approximately 75–90 logical pixels across, with long legs and natural proportions.
Style: true coarse 2D pixel art on one uniform low-resolution grid, about a 400–480 pixel-wide logical game canvas enlarged with nearest-neighbor. Every visible logical pixel should be a chunky square. Hard stair-step silhouettes, dark near-black/brown/green selective outlines, large contiguous color clusters, 3–4 flat shades per material, flat diffuse daylight. Earth browns, forest and olive greens, desaturated cyan sky and muted gray-lavender accents taken from the references. Simple broad color regions with small controlled pixel texture accents. Straight side-on orthographic gameplay view, not isometric and no cinematic perspective. Keep background simpler and lower contrast than horses. Trees should match the broad leafy pixel clusters and conifer silhouettes of images 2 and 3.
Avoid: painterly artwork, smooth curves, antialiasing, blur, bloom, gradients, volumetric light, realistic rendered horses, high-resolution microdetail, decorative illustration finish, 3D, voxel, chibi proportions, golden fantasy horses, glossy UI, title text, watermark, any people or jockeys. Do not recreate the greenhouse or farming crop display from references; translate their pixel art language into horse stable and race environments. NO HUD: focus solely on environment and horses.
Scene: active straight countryside horse race, camera tracking from the side. Four parallel horizontal flat dirt lanes occupying the bottom half to two thirds of the frame, lane boundaries indicated by thin restrained pale earthy lines, not railings between lanes. EXACTLY four separate horses, one white, one chestnut, one charcoal and one darker bay, one per lane, running RIGHT in distinct readable galloping poses at staggered horizontal positions. No riders. Player is the chestnut horse with a tiny unobtrusive ochre downward pixel arrow overhead. Preserve the anatomy and chunky shading of the standing horse sprite reference while adapting it to running. Low wooden jump hurdles ahead on the right, compact two-tone pixel dust puffs behind hooves. Single wooden paddock fence behind the whole track, a strip of grass and a few tiny weeds, broad deciduous trees and conifer silhouettes against a desaturated cyan sky, distant muted forest layers. Flat daylight, moderate color, spare composition, sprite-pack environment. Full scene edge to edge.
```


