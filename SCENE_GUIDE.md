# Hướng Dẫn Chi Tiết: Parallax và Timeline trong Project

Tài liệu này giải thích chi tiết cơ chế hoạt động của **Parallax (MapBeach)** và **Timeline (Boss1Scene)** từ cách thiết lập trong Editor đến logic trong Code.

---

## 1. Parallax (Hiệu Ứng Cuộn Cảnh) - Scene `MapBeach`

### A. Cách Thức Hoạt Động (Concept)
Parallax là hiệu ứng tạo ảo giác về độ sâu bằng cách làm cho các lớp hình nền ở xa di chuyển chậm hơn so với các lớp ở gần khi Camera di chuyển.

Trong project này, có 2 cơ chế chính được sử dụng trong script `InfiniteParallax.cs` (hoặc `ParallaxController.cs`):
1.  **Hiệu ứng Băng Chuyền (Treadmill):** Di chuyển cả cụm Background đi theo Camera để người chơi không bao giờ đi ra khỏi mép bản đồ.
2.  **Cuộn Vân Bề Mặt (Texture Offset):** Di chuyển hình ảnh bên trong (Texture) của vật liệu (Material) để tạo cảm giác trôi, thay vì di chuyển vật thể thật sự.

### B. Thiết Lập Trong Unity Editor
1.  **Hierarchy:**
    *   Tạo một đối tượng cha (ví dụ: `Backgrounds`).
    *   Bên trong chứa các lớp con (Child) là các Quad hoặc Sprite.
    *   **Quan trọng:** Sắp xếp độ sâu **Z** khác nhau.
        *   `Z = 0`: Gần nhất (mặt đất, sẽ trôi nhanh nhất).
        *   `Z = 10` hoặc cao hơn: Xa hơn (như mây, núi, trôi chậm hơn).
2.  **Material:**
    *   Các tấm ảnh Background phải được để chế độ **Wrap Mode: Adjust/Repeat** (để có thể cuộn vô tận).
    *   Shader sử dụng thường là `Sprites/Default` hoặc `Universal Render Pipeline/2D/Sprite-Lit`.
3.  **Script:**
    *   Kéo script `InfiniteParallax.cs` vào đối tượng cha `Backgrounds`.

### C. Phân Tích Code (`InfiniteParallax.cs`)

Script này tự động tính toán tốc độ trôi dựa trên khoảng cách Z.

#### 1. Khởi Tạo (`Start`)
```csharp
void Start() {
    cam = Camera.main.transform; // Lấy Camera chính
    
    // Duyệt qua tất cả các con (Layer) bên trong
    int childCount = transform.childCount;
    for (int i = 0; i < childCount; i++) {
        // Lưu lại Renderer để sau này chỉnh Texture Offset
        backgrounds[i] = transform.GetChild(i).GetComponent<Renderer>();
        // Lưu lại vị trí Z để tính độ xa gần
        zDepths[i] = transform.GetChild(i).position.z;
    }
}
```

#### 2. Cập Nhật Mỗi Khung Hình (`LateUpdate`)
Dùng `LateUpdate` để xử lý sau khi Camera đã di chuyển xong (tránh rung giật).

```csharp
void LateUpdate() {
    // BƯỚC 1: Kéo cả cụm Map đi theo Camera
    // Giúp map luôn nằm ngay trước mặt Camera, không bị trôi mất
    transform.position = new Vector3(cam.position.x, transform.position.y, transform.position.z);

    // BƯỚC 2: Tính toán và Cuộn hình
    for (int i = 0; i < backgrounds.Length; i++) {
        // Công thức Parallax: 1 chia cho (Độ sâu + 1)
        // Z=0 (Gần) -> Factor = 1 (Trôi nhanh = tốc độ nhân vật)
        // Z=9 (Xa)  -> Factor = 0.1 (Trôi rất chậm)
        float parallaxFactor = 1f / (Mathf.Abs(zDepths[i]) + 1f);

        // Tính offset: Vị trí Cam * Tốc độ * Hệ số xa gần
        float offset = cam.position.x * scrollSpeed * parallaxFactor * direction;

        // Cập nhật Offset cho Material
        // Đây là bước làm cho hình ảnh "trượt" trên bề mặt vật thể
        Vector2 newOffset = new Vector2(offset, 0); 
        backgrounds[i].material.SetTextureOffset("_BaseMap", newOffset);
    }
}
```
> **Tóm lại:** Code lấy vị trí X của Camera để "cuộn" ảnh nền ngược chiều hoặc cùng chiều, nhanh hay chậm tùy vào độ xa Z của lớp đó.

---

## 2. Timeline (Cutscene Boss) - Scene `Boss1Scene`

### A. Cách Thức Hoạt Động
Timeline trong Unity cho phép biên tập chuỗi sự kiện theo thời gian (giống video editor): di chuyển camera, chạy animation, kích hoạt/hủy kích hoạt vật thể, và gọi hàm từ script.

Hệ thống cutscene Boss 1 gồm 3 giai đoạn chính xử lý bởi 3 script:
1.  **BossTrigger:** Chạm vào thì bắt đầu Cutscene.
2.  **BossWakeUp:** Kích hoạt Boss rơi xuống.
3.  **BossLanding:** Boss chạm đất thì rung camera/gầm rú.

### B. Thiết Lập Trong Unity Editor
1.  **GameObject `TimelineManager` (hoặc tương tự):** Chứa component `PlayableDirector`.
    *   File `.playable`: `BossDirectorTimeline.playable` được gán vào đây.
2.  **Trigger Volume:** Một ô vuông Collider (Is Trigger) đặt trên đường đi của nhân vật.
    *   Gắn script `BossTrigger`.
3.  **Boss Object:** Ban đầu có thể bị tắt (`SetActive(false)`), hoặc script Boss AI bị tắt.

### C. Phân Tích Code

#### 1. Kích Hoạt Cutscene (`BossTrigger.cs`)
Gắn trên một `BoxCollider2D` (Is Trigger). Khi Player đi qua:

```csharp
private void OnTriggerEnter2D(Collider2D other) {
    // Kiểm tra đúng là Player và chưa từng kích hoạt trước đó
    if (other.CompareTag("Player") && !daKichHoat) {
        daKichHoat = true;
        
        // 1. Dừng nhân vật lại (Khóa velocity để không bị trôi)
        if (rbPlayer != null) rbPlayer.linearVelocity = Vector2.zero;

        // 2. Chạy Timeline
        if (bossTimeline != null) {
            bossTimeline.Play(); // <--- BẮT ĐẦU SHOW DIỄN
            
            // Đăng ký sự kiện: Khi diễn xong thì trả lại quyền điều khiển
            bossTimeline.stopped += KetThucCutscene;
        }
    }
}

// Khi Timeline chạy xong (hết thanh thời gian)
void KetThucCutscene(PlayableDirector director) {
    // Bật thanh máu Boss lên
    if (uiThanhMauBoss != null) uiThanhMauBoss.SetActive(true);
    // Mở khóa điều khiển cho nhân vật (nếu có script)
}
```

#### 2. Đánh Thức Boss (`BossWakeUp.cs`)
Script này có thể được gắn vào một vật thể mà Timeline sẽ kích hoạt, hoặc dùng Signal Emitter. Tuy nhiên, theo code hiện tại, nó dùng `OnTriggerEnter2D` (có vẻ là một trigger thứ 2 hoặc được Timeline kích hoạt gián tiếp bằng cách bật vật thể chứa nó).

```csharp
public GameObject bossObject;

private void OnTriggerEnter2D(Collider2D other) {
    // Khi thứ gì đó (có thể là một Dummy Object trong Timeline bay qua) chạm vào
    // Hoặc Player chạm vào điểm thứ 2
    if (other.CompareTag("Player")) {
        // Kích hoạt Boss thật
        if (bossObject != null) bossObject.SetActive(true); 
        
        // Tự hủy trigger này để không gọi lại
        Destroy(gameObject);
    }
}
```
*Lưu ý: Nếu dùng Timeline chuẩn, người ta thường dùng Activation Track để bật BossActive thay vì script này. Nhưng script này đóng vai trò "cầu chì" dự phòng hoặc kích hoạt vật lý.*

#### 3. Boss Tiếp Đất (`BossLanding.cs`)
Gắn trên người Boss. Khi Boss (vừa được bật lên và rơi xuống) chạm đất:

```csharp
private void OnCollisionEnter2D(Collision2D collision) {
    // Chỉ xử lý lần đầu tiên chạm đất
    if (collision.gameObject.CompareTag("Ground") && !daTiepDat) {
        daTiepDat = true;
        
        // Chạy một Timeline khác (Intro gầm rú, rung camera)
        // hoặc tiếp tục Timeline chính
        if (bossTimeline != null) bossTimeline.Play();
    }
}
```

### D. Luồng Hoạt Động Tổng Quát (A-Z)
1.  **Người chơi đi bộ tới vùng `BossTrigger`.**
2.  `BossTrigger` phát hiện -> Dừng người chơi -> Gọi `bossTimeline.Play()`.
3.  **Timeline chạy các track:**
    *   Camera Track: Zoom vào chỗ Boss sẽ xuất hiện.
    *   Animation Track: Có thể diễn hoạt gì đó.
4.  **Kích hoat Boss:**
    *   Có thể Timeline bật Game Object Boss, hoặc Player lướt qua trigger `BossWakeUp`.
    *   Boss hiện ra và rơi tự do (do có Rigidbody).
5.  **Boss chạm đất:**
    *   `BossLanding` phát hiện va chạm `Ground`.
    *   Kích hoạt hiệu ứng tiếp đất (bụi, âm thanh, hoặc đoạn Timeline rung lắc).
6.  **Kết thúc:**
    *   Timeline chạy hết.
    *   `BossTrigger` nhận sự kiện `stopped` -> Bật thanh máu UI -> Trả lại điều khiển cho tớ.
    *   **BOSS FIGHT BẮT ĐẦU!**
