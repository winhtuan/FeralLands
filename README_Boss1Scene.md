# Boss1Scene – Tài Liệu Chi Tiết

> **Cập nhật lần cuối:** 2026-02-21  
> **Map:** `FeralLands/Assets/Scenes/Boss1Scene.unity`  
> **Mục tiêu của scene:** Màn chiến Boss đầu tiên, có cutscene intro dùng Timeline + hiện banner tên boss.

---

## 1. Tổng Quan Hierarchy (Cấu Trúc Scene)

```
Boss1Scene
├── Environment (rỗng – group)
│   ├── Ground_Collider      ← BoxCollider2D, Tag="Ground", dùng để BossLanding phát hiện tiếp đất
│   ├── Wall_Left            ← BoxCollider2D đứng bên trái, gắn script BossIntroTrigger (disabled)
│   ├── Wall_Right           ← BoxCollider2D đứng bên phải (chặn Player thoát arena)
│   └── BossEventTrigger     ← BoxCollider2D **IsTrigger=true**, gắn BossWakeUp.cs (ĐANG HOẠT ĐỘNG)
│
├── tinywow_boss1_...        ← SpriteRenderer fullscreen, là background ảnh của map boss
│
├── Dreamshaper              ← Nhân vật Player (Tag="Player")
│   ├── Rigidbody2D (GravityScale=3, Freeze Rotation Z)
│   ├── BoxCollider2D
│   ├── Animator (controller Dreamshaper)
│   ├── PlayerHealth (maxHealth=100)
│   ├── CameraFollow (offset Y=2, lockY=true)
│   ├── AttackPoint
│   └── FirePoint
│
├── BossPosition             ← Boss (tạm thời là "Square" sprite), INACTIVE lúc đầu
│   ├── Rigidbody2D (GravityScale=3, rơi xuống khi SetActive)
│   ├── BoxCollider2D
│   ├── BossLanding.cs       ← Script chính điều phối chuỗi sự kiện sau khi boss tiếp đất
│   └── Square               ← SpriteRenderer (sprite boss placeholder)
│
├── BossDirector             ← PlayableDirector chứa BossDirectorTimeline.playable
│
├── CM_FullMap               ← CinemachineCamera, OrthographicSize=10 (zoom toàn map)
├── CM_Hero                  ← CinemachineCamera, OrthographicSize=2.36 (nhìn theo Player)
│   └── CameraAnchor
└── CM_Boss                  ← CinemachineCamera, OrthographicSize=2.68 (nhìn theo Boss)
│
├── Main Camera              ← Camera chính (Orthographic, size=2.68)
│   ├── CinemachineBrain     ← Blend mượt giữa các Cinemachine cameras (Style=Ease, Time=2s)
│   └── CameraFollow         ← script follow nhưng player=null (Cinemachine đảm nhận)
│
└── BossCanvas               ← Canvas Overlay (1920×1080), Animator dùng BossCanvas.controller
    └── BossNamePanel        ← Panel chứa banner boss (được animate bởi BossBannerAnim)
        ├── Chain_Left       ← Image (màu xám đậm #333), Size 10×1000, pos (-387, 307) – xích trái
        ├── Chain_Right      ← Image (màu xám đậm #333), Size 10×1000, pos (373, 266) – xích phải
        ├── BannerBG         ← Image sprite banner, Size 921×515 – nền banner boss
        └── BossName         ← TextMeshProUGUI, text = "Hoả Ngục Quỷ", fontSize=61.69
```

---

## 2. Timeline Intro Boss (`BossDirectorTimeline.playable`)

### Cấu Trúc Timeline

Timeline chỉ có **1 track duy nhất**: `Cinemachine Track`, gồm **4 clip camera** nối tiếp nhau:

| Thứ tự | Tên Clip      | Bắt đầu (s) | Thời lượng (s) | Camera Virtual     | Tác dụng                                    |
|--------|---------------|-------------|----------------|--------------------|---------------------------------------------|
| 1      | `CM_FullMap`  | 0.0         | 1.0            | `CM_FullMap`       | Zoom ra toàn map (OrthographicSize=10)      |
| 2      | `CM_Hero`     | 0.5         | 3.45           | `CM_Hero`          | Blend sang camera nhìn Player (blend in 0.5s)|
| 3      | `CM_Boss`     | 1.5         | 4.5            | `CM_Boss`          | Blend sang camera nhìn Boss (blend in 2.45s)|
| 4      | `CM_FullMap`  | 5.5         | 0.5            | `CM_FullMap`       | Kết thúc – blend về toàn map (blend in 0.5s)|

> **Tổng thời lượng Timeline:** ~6 giây  
> **Blend mặc định (CinemachineBrain):** Style = Ease, Time = 2s

### Diễn biến Camerawork

```
[0.0s] ──► CM_FullMap (zoom toàn map, OrthographicSize=10)
[0.5s] ──► Bắt đầu blend sang CM_Hero (nhìn Player)
[1.5s] ──► Bắt đầu blend sang CM_Boss (nhìn Boss đang rơi, blend in dài 2.45s = mượt)
[5.5s] ──► Blend về CM_FullMap
[6.0s] ──► Timeline kết thúc → BossLanding.cs tiếp tục
```

---

## 3. Cách Kích Hoạt Intro (Trigger System)

### Luồng sự kiện hoàn chỉnh

```
Player bước vào vùng Wall_Left
        │
        ▼ (BossWakeUp.cs – KHÔNG được dùng nữa, xem ghi chú)
        │
        ▼ (BossTrigger.cs – chạy thực tế trên Wall_Left, nhưng disabled)
        │
        ▼ ──► BossPosition.SetActive(true)
                │
                ▼ Boss rơi xuống (Rigidbody2D GravityScale=3)
                │
                ▼ Chạm Ground_Collider (Tag="Ground")
                │
                ▼ BossLanding.OnCollisionEnter2D()
                │
                ▽─────────────────────────────────────────────────┐
         [Bước 1] BossDirector.Play()                             │
                  → Timeline camera 6 giây chạy                  │
                  → yield return WaitForSeconds(6s)               │
                ▼                                                  │
         [Bước 2] BossCanvasAnimator.Play("BossBannerAnim")       │
                  → Banner kéo xuống 3 giây                       │
                  → yield return WaitForSeconds(3s)               │
                ▼                                                  │
         [Bước 3] Debug.Log("FIGHT!")                             │
                  playerScript.enabled = true ─────────────────────┘
```

### Chi tiết Scripts

#### `BossWakeUp.cs` (**Script ĐANG được dùng** – gắn trên `BossEventTrigger`, Enabled)
- **Vị trí trong scene:** `Environment > BossEventTrigger`, Position=(-12.46, -3.16), Scale=0.646, BoxCollider2D là **Trigger**
- **Cách hoạt động:** `OnTriggerEnter2D` khi Player chạm → dừng Player (`rbPlayer.linearVelocity = zero`) → tắt `playerScript` → `bossObject.SetActive(true)` → `Destroy(gameObject)` (xóa trigger để không kích hoạt lại)
- **Kết nối trong Inspector:**
  - `bossObject` → `BossPosition` (Boss cần bật lên)
  - `playerScript` → component trên `Dreamshaper`
  - `playerRb` → Rigidbody2D của `Dreamshaper`
- **Đây là bước KHỞI ĐẦU của toàn bộ chuỗi sự kiện** – nếu tắt object này thì boss không bao giờ rơi → timeline không chạy.

#### `BossTrigger.cs` (Script phiên bản mới, gắn trên `Wall_Left` nhưng **Disabled**)
- **Cách hoạt động:** `OnTriggerEnter2D` → phanh Player (`rbPlayer.linearVelocity = Vector2.zero`) → `bossTimeline.Play()` → đăng ký event `bossTimeline.stopped` → khi xong hiện thanh máu boss.
- **Trạng thái trong scene:** `m_Enabled: 0` (bị disable). Script giao tiếp `BossTimeline` nhưng **chưa mở playerScript**.
- **Lưu ý:** Script này kéo `BossDirector` vào field `bossTimeline`, nhưng field `player` và `uiThanhMauBoss` đang **trống (null)**.

#### `BossLanding.cs` (**Script ĐANG được dùng** – gắn trên `BossPosition`, Enabled)
- **Cách hoạt động:** `OnCollisionEnter2D` khi BossPosition chạm Ground → chạy Coroutine `QuyTrinhBossRaMat()`:
  1. Play BossDirectorTimeline → chờ xong
  2. Play animation `BossBannerAnim` → chờ 3 giây
  3. Unlock Player
- **Kết nối trong Inspector:**
  - `bossTimeline` → `BossDirector` (PlayableDirector)
  - `bossCanvasAnimator` → `BossCanvas` (Animator)
  - `playerScript` → component trên `Dreamshaper`
  - `bossObject` → chính `BossPosition` (tự tham chiếu)

---

## 4. Banner Boss (`BossBannerAnim`)

### Cấu Trúc Animator

```
BossCanvas (Animator Controller: BossCanvas.controller)
└── Base Layer
    └── State: BossBannerAnim (default state, speed=1)
        └── Motion: BossBannerAnim.anim
```

> Animator **không có transition** nào – chỉ có 1 state duy nhất. Play bằng code: `animator.Play("BossBannerAnim")`.

### Chi Tiết Animation `BossBannerAnim.anim`

Animation dài **3 giây** (StopTime=3), sample rate 60fps, áp dụng lên child object `BossNamePanel`:

#### Đường cong `m_AnchoredPosition.y` (trục Y của BossNamePanel)

| Thời gian | Giá trị Y | Ý nghĩa                          |
|-----------|-----------|----------------------------------|
| 0.0s      | 983       | BossNamePanel ở **trên màn hình** (ngoài tầm nhìn) |
| 0.5s      | 0         | BossNamePanel **vào đúng giữa** màn hình |
| 2.5s      | 0         | Giữ nguyên giữa màn                      |
| 3.0s      | 1016.4    | BossNamePanel **biến mất xuống dưới** màn hình |

#### Đường cong `m_AnchoredPosition.x`

| Thời gian | Giá trị X | Ý nghĩa             |
|-----------|-----------|---------------------|
| 0.0s      | 0         | Không di chuyển ngang |
| 2.5s      | 0         | Không di chuyển ngang |

#### Đường cong `m_Alpha` (CanvasGroup Alpha của BossNamePanel)

| Thời gian | Alpha | Ý nghĩa                  |
|-----------|-------|--------------------------|
| 0.0s      | 0     | Banner **trong suốt** (ẩn) |
| 0.5s      | 1     | Banner **hiện đầy đủ**     |
| 3.0s      | 0     | Banner **fade out**        |

### Tóm Tắt Hiệu Ứng Banner

```
[0.0s] Banner ở ngoài màn hình trên (Y=983), Alpha=0 (ẩn)
[0.5s] Banner trượt xuống vào giữa màn (Y=0), Alpha=1 (hiện rõ)
[2.5s] Banner đứng yên ở giữa màn
[3.0s] Banner tiếp tục trượt xuống biến mất (Y=1016), đồng thời fade out (Alpha=0)
```

**Nội dung hiển thị trong banner:**
- `BannerBG`: Ảnh nền banner (Sprite từ Asset, màu gần trắng `#FFFAFA`)
- `Chain_Left`: Hình thanh xích bên trái (màu xám đậm `#333333`)
- `Chain_Right`: Hình thanh xích bên phải (màu xám đậm `#333333`)
- `BossName`: Text **"Hoả Ngục Quỷ"**, TextMeshProUGUI, màu trắng, size 61.69

---

## 5. Hệ Thống Camera (Cinemachine)

| Object          | Script                  | OrthographicSize | Follow Target       | Ghi Chú                         |
|-----------------|-------------------------|------------------|---------------------|---------------------------------|
| `Main Camera`   | Camera + CinemachineBrain | 2.68           | –                   | Blend mượt, Style=Ease, 2s      |
| `CM_FullMap`    | CinemachineCamera        | **10.0**         | Không               | Nhìn toàn bộ bản đồ boss        |
| `CM_Hero`       | CinemachineCamera        | **2.36**         | `Dreamshaper` (Player) | Nhìn theo nhân vật           |
| `CM_Boss`       | CinemachineCamera        | **2.68**         | `BossPosition`      | Nhìn theo boss khi boss rơi xuống |

**Binding trong BossDirector (PlayableDirector):**
- `CinemachineTrack` ← bind vào `Main Camera` (CinemachineBrain nhận output)
- `CM_FullMap`, `CM_Hero`, `CM_Boss` ← bind qua `m_ExposedReferences` vào đúng CinemachineCamera tương ứng

---

## 6. Trạng Thái Hiện Tại & Những Gì Chưa Làm

### ✅ Đã Hoàn Thành
- [x] Timeline intro boss: 4 clip camera, camerawork zoom in/out mượt mà (~6s)
- [x] Script `BossLanding.cs` hoạt động: Detect tiếp đất → chạy Timeline → hiện Banner → unlock Player
- [x] Banner boss: Animation 3s (slide in, hold, slide out + fade)
- [x] Tên boss trên Banner: **"Hoả Ngục Quỷ"**
- [x] Boss placeholder: `BossPosition` (Square sprite) có Rigidbody2D tự rơi
- [x] Walls arena: `Wall_Left`, `Wall_Right` (BoxCollider2D chặn Player)
- [x] Ground collider cho arena: `Ground_Collider` (Tag=Ground)

### ⚠️ Còn Dang Dở / Lưu Ý
- [ ] `BossTrigger.cs` trên `Wall_Left` đang **Disabled** – hiện tại `BossWakeUp.cs` trên `BossEventTrigger` đóng vai trò tương tự
- [ ] Boss thật chưa có sprite/animation boss xịn – chỉ dùng `Square` placeholder
- [ ] AI của Boss chưa được viết (dòng code `bossObject.GetComponent<BossAI>()` đang comment out trong `BossLanding.cs`)
- [ ] Thanh máu Boss (`uiThanhMauBoss`) trong `BossTrigger.cs` chưa được kéo vào
- [ ] `BossIntroTrigger` (class cũ) – là code commented out trong `BossTrigger.cs`, đã được thay thế

---

## 7. Vị Trí Các File Quan Trọng

| File | Đường Dẫn |
|------|-----------|
| Scene chính | `Assets/Scenes/Boss1Scene.unity` |
| Timeline asset | `Assets/Animation/TimeLineBoss1/BossDirectorTimeline.playable` |
| Animation Banner | `Assets/Animation/AnimationBannerMapBoss1/BossBannerAnim.anim` |
| Animator Controller Banner | `Assets/Animation/AnimationBannerMapBoss1/BossCanvas.controller` |
| Script – BossLanding | `Assets/Script/TimeLineBoss1/BossLanding.cs` |
| Script – BossTrigger | `Assets/Script/TimeLineBoss1/BossTrigger.cs` |
| Script – BossWakeUp (cũ) | `Assets/Script/TimeLineBoss1/BossWakeUp.cs` |

---

## 8. Sơ Đồ Luồng Hoàn Chỉnh

```
[GAME START]
      │
      ▼
Dreamshaper xuất hiện tại vị trí ban đầu
BossPosition = INACTIVE (boss chưa hiện)
      │
      ▼ Player đi sang phải
[BossEventTrigger phát hiện Player – BossWakeUp.cs]
      │  dừng Player + gọi BossPosition.SetActive(true) + tự Destroy()
      ▼
BossPosition kích hoạt → Rigidbody2D bắt đầu rơi (GravityScale=3)
      │
      ▼ Boss rơi chạm Ground_Collider (Tag="Ground")
[BossLanding.OnCollisionEnter2D] → bắt đầu Coroutine
      │
      ├─► [Phase 1 ~6s] BossDirectorTimeline chạy:
      │     0.0s: Camera zoom toàn map
      │     0.5s: Camera blend sang nhìn Player
      │     1.5s: Camera blend sang nhìn Boss (mượt 2.45s)
      │     5.5s: Camera blend về toàn map
      │
      ├─► [Phase 2 ~3s] BossBannerAnim chạy:
      │     0.0s: Banner ẩn trên màn
      │     0.5s: Banner trượt xuống giữa, fade in rõ
      │     2.5s: Banner hiện "Hoả Ngục Quỷ" + xích 2 bên
      │     3.0s: Banner fade out, trượt xuống biến mất
      │
      └─► [Phase 3] FIGHT!
            playerScript.enabled = true (Player có thể điều khiển trở lại)
            Debug.Log("FIGHT!")
```
