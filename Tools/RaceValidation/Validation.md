# Yarış görüntüsü, boost ve sonuç tablosu doğrulaması

Unity **6000.6.2f1** ve projenin kurulu paketleriyle doğrulandı. Testler açık editöre müdahale etmeyen geçici bir proje kopyasında çalıştırıldı; doğrulanan sahne ve sprite asset'leri ana projeye aktarıldı. Geçici proje ve derleyici çıktıları temizlendi.

- **EditMode: 9/9 geçti.** Boost basışlarının yığılması, +5 sınırı, saniyede 3 azalma, countdown/bitiş giriş engeli, zıplama, engel cezaları, üç AI hızı ve restart durumu doğrulandı. Sahne Play kullanmadan iki kez açıldı; dört at, tek kamera, başlangıç UI ve kalıcı sprite/texture referansları kontrol edildi. Kaydedilmiş at sprite'larının UV verisi ve Pixel Perfect Camera ayarları doğrulandı.
- **PlayMode: 2/2 geçti.** Input System üzerinden Space/E, Space'i basılı tutmanın tekrar üretmemesi, bırakıp yeniden basma, countdown giriş engeli, oyuncunun erken/geç bitirmesi, her yarışçının doğru sıra/ad/süresi, bekleyen satırların başlangıç sırası ve bitişle güncellenmesi doğrulandı. Oyuncu bitirdikten sonra boost reddedildi; çubuk ve geri bildirim hemen temizlendi. Diğer atlar koşarken restart, boşaltılan sonuç satırları ve sabit nesne sayısı kontrol edildi. Beklenmeyen oyun Console hatası alınmadı.
- **30/60/120 FPS:** Test, her kare hızında 30 görüntü karesi boyunca koşu/boost/zıplama konumlarını önceki-güncel sabit simülasyon değerleriyle karşılaştırdı. Maksimum hata 0,0001'in altında kaldı; geriye görsel hareket gözlenmedi. Reset konumu ve zıplama geçmişi kontrol edildi. Bu, hesaplamanın doğruluğunu sınar; monitördeki öznel hareket hissini ölçmez.
- **Görüntü incelemesi:** URP render request ile 1280×720, 1920×1080 ve 1024×768 ekran görüntüleri üretildi. At sprite'ları net, komşu animasyon satırlarından sızma görünmüyor. 4:3 görüntüde Windowbox kenar boşlukları oluşuyor. Başlangıç, bütün sonuçlar ve oyuncu önde bitirdiğinde bekleyen sonuç satırları incelendi. Ekran görüntüsü yardımcı kodu Canvas modunu geçici olarak değiştirip eski değerleri geri yükler; gerçek sahne UI'ı Screen Space Overlay kullanır.
- **Yeniden oluşturma:** Rebuild menüsü tekrar çalıştırıldı. Dört `Horse*Frames.png.meta` dosyasının SHA-256 değerleri tamamen aynı kaldı; texture GUID'leri, sprite kimlikleri, pivotlar ve import ayarları korundu. Yeniden kaydedilmiş sahne EditMode ve PlayMode testlerinden geçti.

Son EditMode çalışması: **2026-10-05 15:40 UTC**. Son PlayMode çalışması: **2026-10-05 15:43 UTC**.

Ham sonuçlar: `editmode-results.xml`, `playmode-results.xml`. Unity günlükleri: `unity-editmode.log`, `unity-playmode.log`, `unity-rebuild.log`. Lisans servisinin access-token mesajı test çalışmasını engellemedi; oyun kaynaklı hata veya C# derleme hatası yok.

Görüntüler: `race-start.png`, `race-gameplay.png`, `race-gameplay-1920x1080.png`, `race-gameplay-1024x768.png`, `race-result.png`, `race-results-pending.png`.

Testleri Unity Test Runner'dan tekrar çalıştırabilirsiniz. Batch testleri sentetik klavye girişlerini kabul etmek için Input System odak ayarlarını geçici olarak değiştirir ve ardından geri yükler. Normal kullanımda Game görünümüne odaklanın. Piksel netliğini editörde **1x** Game görünümü ölçeğinde değerlendirin.
