# 📖 README – Boss1Scene

> **FeralLands | Unity 2D Platformer | Boss Map 1**

---

## 1. Tổng quan về Scene

**Boss1Scene** là màn chiến đấu với Boss đầu tiên của game (**"Thần Lằn Bám Kính"** – theo tên hiển thị trong Banner). Đây là một scene khép kín (closed arena) với vách tường cố định hai bên, không có Parallax nền mở rộng như các map thường.

### Mục đích chính của Scene:
1. **Dungeon Intro Cutscene** – Khi nhân vật bước vào vùng trigger, Boss được đánh thức, camera cutscene chạy, sau đó banner Boss xuất hiện.
2. **Boss Fight** – Sau khi cutscene kết thúc, nhân vật và Boss bắt đầu chiến đấu.

---

## 2. Hierarchy – Các GameObject trong Scene

| GameObject | Mục đích |
|---|---|
| `Main Camera` | Camera chính, gắn `CinemachineBrain` và `CameraFollow` |
| `CM_FullMap` | Cinemachine Camera nhìn toàn bộ map (OrthographicSize = 10) |
| `CM_Hero` | Cinemachine Camera theo nhân vật (OrthographicSize = 2.36) |
| `CM_Boss` | Cinemachine Camera theo Boss (OrthographicSize = 2.68) |
| `CM_BossStatic` | Cinemachine Camera tĩnh (bị tắt mặc định) |
| `BossDirector` | `PlayableDirector` – chạy Timeline cutscene Boss |
| `Dreamshaper` | Nhân vật người chơi (Tag: `Player`) |
| `BossPosition` | Đối tượng Boss (bị tắt mặc định, chỉ bật khi trigger) |
| `BossCanvas` | UI Banner Boss (Panel + Animation), bị tắt mặc định |
| `BossEventTrigger` | Trigger vô hình để phát hiện Player bước vào zone Boss |
| `Environment` | Nhóm chứa Ground, Wall_Left, Wall_Right, BossEventTrigger |
| `tinywow_boss1_...` | Sprite nền (background) của map Boss |
| `EventSystem` | Xử lý Input UI |

### Chi tiết `BossCanvas` (UI Banner):
```
BossCanvas (GameObject - bị tắt mặc định, có Animator)
└── BossNamePanel (Canvas Group)
    ├── Chain_Left     (Image – dây xích trái)
    ├── Chain_Right    (Image – dây xích phải)
    ├── BannerBG       (Image – nền banner Boss)
    └── BossName       (TextMeshProUGUI – "Thần Lằn Bám Kính")
```

---

## 3. Các Script liên quan đến Boss1Scene

### 3.1 `BossWakeUp.cs` – Trigger khóa nhân vật & bật Boss

> **Đường dẫn:** `Assets/Script/TimeLineBoss1/BossWakeUp.cs`  
> **Gắn trên:** `BossEventTrigger` (GameObject trigger vô hình trong Environment)

**Chức năng:**
- Khi nhân vật (`Tag: Player`) bước vào vùng trigger, script này:
  1. Dừng vận tốc nhân vật (`linearVelocity = Vector2.zero`)
  2. **Khóa toàn bộ module điều khiển** bằng `player.SetAllModulesEnabled(false)`
  3. Bật đối tượng Boss lên (`bossObject.SetActive(true)`) → Boss rơi xuống từ trên cao
  4. Hủy trigger (`Destroy(gameObject)`) → chỉ kích hoạt 1 lần

```csharp
// BossWakeUp.cs - OnTriggerEnter2D
playerRb.linearVelocity = Vector2.zero;         // 1. Dừng nhân vật
player.SetAllModulesEnabled(false);              // 2. KHÓA di chuyển
bossObject.SetActive(true);                      // 3. Thả Boss xuống
Destroy(gameObject);                             // 4. Xóa trigger
```

**⚠️ Lưu ý trong Inspector:**
- `player` field = **null** trong scene → Script tự tìm bằng `FindFirstObjectByType<Dreamshaper>()` ✅
- `playerRb` = **đã được gán** (fileID: 1763837068) ✅
- `bossObject` = **đã được gán** (fileID: 452833174) ✅

---

### 3.2 `BossLanding.cs` – Chạy Timeline & Banner, mở khóa sau khi xong

> **Đường dẫn:** `Assets/Script/TimeLineBoss1/BossLanding.cs`  
> **Gắn trên:** `BossPosition` (đối tượng Boss, bị tắt mặc định)

**Chức năng:** Đây là script **trung tâm** điều khiển toàn bộ cutscene Boss. Khi Boss được bật và rơi xuống chạm đất, Coroutine `QuyTrinhBossRaMat()` sẽ chạy theo trình tự:

```
[Boss bật lên] → OnEnable: khóa lại nhân vật (phòng hờ)
      ↓
[Boss rơi chạm Ground] → OnCollisionEnter2D → StartCoroutine
      ↓
Bước 1: bossTimeline.Play()  →  WaitForSeconds(bossTimeline.duration)
      ↓
Bước 2: bossCanvasObject.SetActive(true) → bossCanvasAnimator.Play("BossBannerAnim")
         WaitForSeconds(3f)   ← banner hiển thị trong 3 giây
      ↓
Bước 3: player.SetAllModulesEnabled(true)  ← MỞ KHÓA di chuyển
         playerRb.linearVelocity = Vector2.zero
```

```csharp
// BossLanding.cs - QuyTrinhBossRaMat()
bossTimeline.Play();
yield return new WaitForSeconds((float)bossTimeline.duration);   // Đợi Timeline xong

bossCanvasObject.SetActive(true);
bossCanvasAnimator.Play("BossBannerAnim", 0, 0);
yield return new WaitForSeconds(3f);                             // Đợi Banner 3 giây

player.SetAllModulesEnabled(true);   // ✅ MỞ KHÓA
playerRb.linearVelocity = Vector2.zero;
```

**⚠️ Lưu ý trong Inspector:**
- `player` field = **null** trong scene → Script tự tìm bằng `FindFirstObjectByType<Dreamshaper>()` ✅
- `playerRb` = **null** trong scene → Tự tìm qua `player.GetComponent<Rigidbody2D>()` ✅
- `bossTimeline` = **đã được gán** (BossDirector) ✅
- `bossCanvasAnimator` = **đã được gán** (Animator của BossCanvas) ✅
- `bossCanvasObject` = **đã được gán** (BossCanvas GameObject) ✅

---

### 3.3 `BossTrigger.cs` – Script cũ (KHÔNG SỬ DỤNG)

> **Đường dẫn:** `Assets/Script/TimeLineBoss1/BossTrigger.cs`

Script này là phiên bản cũ được phát triển trước khi có `BossWakeUp`+`BossLanding`. **Hiện tại không gắn vào bất kỳ GameObject nào trong scene.** Code mở khóa di chuyển bên trong bị comment-out:

```csharp
// if (scriptDiChuyen != null) scriptDiChuyen.enabled = true; // ← bị comment!
```

> Script này **lỗi thời**, không có tác dụng trong scene. Có thể xóa an toàn.

---

### 3.4 `Dreamshaper.cs` – Quản lý tất cả module nhân vật

> **Đường dẫn:** `Assets/Script/Player/Dreamshaper.cs`  
> **Gắn trên:** `Dreamshaper` (GameObject nhân vật)

Script trung tâm của nhân vật. Hàm quan trọng nhất:

```csharp
public void SetAllModulesEnabled(bool state)
{
    if (movement != null) movement.enabled = state;  // PlayerMovement
    if (jump     != null) jump.enabled     = state;  // PlayerJump
    if (castOrb  != null) castOrb.enabled  = state;  // PlayerCastOrb
    if (melee    != null) melee.enabled    = state;  // PlayerMeleeAttack
    if (ulti     != null) ulti.enabled     = state;  // PlayerUltimate
}
```

Khi `state = false` → khóa toàn bộ (không di chuyển, không nhảy, không đánh).  
Khi `state = true` → mở khóa toàn bộ.

**⚠️ Quan sát quan trọng trong scene file:**
```yaml
# Dreamshaper GameObject (fileID: 1763837070) - Component Dreamshaper.cs
movement: {fileID: 0}   ← NULL
jump:     {fileID: 0}   ← NULL
castOrb:  {fileID: 0}   ← NULL
melee:    {fileID: 0}   ← NULL
ulti:     {fileID: 0}   ← NULL
action:   {fileID: 0}   ← NULL
```

Tất cả module đều bị `[HideInInspector]` và được khởi tạo trong `Awake()` → đây là **bình thường**, các giá trị sẽ được lấy lại qua `GetComponent<>()` khi runtime.

---

### 3.5 Camera System – Cinemachine

Scene dùng `CinemachineBrain` trên Main Camera để chuyển đổi giữa các camera:

| Camera | TrackingTarget | OrthographicSize | Dùng khi |
|---|---|---|---|
| `CM_FullMap` | Không có | 10 | Nhìn toàn map (Timeline?) |
| `CM_Hero` | Dreamshaper | 2.36 | Theo nhân vật |
| `CM_Boss` | BossPosition | 2.68 | Theo Boss (Timeline) |
| `CM_BossStatic` | Không có | 10 | Bị tắt mặc định |

Timeline `BossDirector` điều khiển việc kích hoạt các camera này theo thứ tự trong cutscene.

---

## 4. Luồng hoạt động đầy đủ khi vào Scene

```
[Load Boss1Scene]
       │
       ▼
[Dreamshaper xuất hiện ở trái màn hình (-13.7, -4.11)]
[BossPosition (Boss) đang BỊ TẮT]
[BossCanvas (bannner UI) đang BỊ TẮT]
       │
       ▼ Player bước phải → chạm BossEventTrigger
[BossWakeUp.OnTriggerEnter2D]
  → linearVelocity = zero         ← phanh nhân vật
  → SetAllModulesEnabled(false)   ← KHÓA DI CHUYỂN
  → BossPosition.SetActive(true)  ← BOSS RƠI TỪ TRÊN XUỐNG
  → Destroy(BossEventTrigger)
       │
       ▼ Boss rơi xuống chạm Ground_Collider (Tag: "Ground")
[BossLanding.OnCollisionEnter2D]
  → StartCoroutine(QuyTrinhBossRaMat)
       │
       ▼ Bước 1: Timeline
  → bossTimeline.Play()
     [Camera chuyển sang CM_Boss hoặc CM_FullMap]
     [Cutscene ~Xs chạy]
  → WaitForSeconds(bossTimeline.duration)
       │
       ▼ Bước 2: Banner
  → BossCanvas.SetActive(true)
  → Animator.Play("BossBannerAnim")
     [Banner "Thần Lằn Bám Kính" hiện lên]
  → WaitForSeconds(3f)
       │
       ▼ Bước 3: Mở khóa
  → SetAllModulesEnabled(true)    ← MỞ KHÓA DI CHUYỂN ✅
  → linearVelocity = zero
       │
       ▼
[BẮT ĐẦU CHIẾN ĐẤU!]
```

---

## 5. ❌ Phân tích BUG: Nhân vật không di chuyển được sau cutscene

### Vấn đề

Sau khi Timeline và Banner chạy xong, nhân vật **vẫn không di chuyển được**.

### Nguyên nhân có thể

#### Nguyên nhân #1 – `player` null trong `BossLanding` (⚠️ Khả năng cao nhất)

Trong Inspector của `BossLanding` (gắn trên `BossPosition`):
```yaml
player: {fileID: 0}   ← NULL
playerRb: {fileID: 0} ← NULL
```

Script dùng `FindFirstObjectByType<Dreamshaper>()` để tự tìm trong `OnEnable()`. **Vấn đề là `OnEnable()` chạy ngay khi `BossPosition.SetActive(true)`** – tức là trước khi `Dreamshaper` có cơ hội khởi tạo đầy đủ. Nếu lúc đó `FindFirstObjectByType` trả về `null`, thì `player` vẫn là `null`, và lệnh:

```csharp
player.SetAllModulesEnabled(true);  // ← KHÔNG CHẠY vì player == null
```

sẽ không mở khóa được!

Console sẽ in ra lỗi:
```
[BossLanding] ❌ player vẫn null → KHÔNG mở khóa được! Kéo Dreamshaper vào Inspector.
```

**👉 Giải pháp:** **Kéo GameObject Dreamshaper vào field `player` của `BossLanding` trong Inspector.**

#### Nguyên nhân #2 – `BossPosition` bị tắt nên `OnEnable` không chạy đúng thời điểm

Khi `BossWakeUp` gọi `bossObject.SetActive(true)`, `OnEnable` của `BossLanding` chạy ngay. Nếu `FindFirstObjectByType<Dreamshaper>()` thành công thì `player` có giá trị. Nhưng nếu scene chưa load xong hoặc `Dreamshaper` chưa có trong scene lúc đó thì sẽ fail.

#### Nguyên nhân #3 – `BossCanvas` Animator bị lỗi

Nếu animation `"BossBannerAnim"` không tồn tại trong Animator Controller, hoặc Animator bị `null`, thì `WaitForSeconds(3f)` không chạy, Coroutine bị ngắt → không đến bước mở khóa.

---

## 6. ✅ Cách sửa Bug

### Sửa nhanh nhất – Kéo trực tiếp trong Inspector

Trong Unity Editor:
1. Chọn `BossPosition` trong Hierarchy
2. Ở component `BossLanding`, tìm field **`Player`** và **`PlayerRb`**
3. Kéo GameObject `Dreamshaper` vào field `Player`
4. Kéo `Dreamshaper > Rigidbody2D` vào field `PlayerRb`

### Sửa bằng code – Đảm bảo fallback trong `OnEnable`

Nếu muốn giữ tự động tìm, thêm fallback trong coroutine:

```csharp
// BossLanding.cs - Bước 3 (đảm bảo tìm lại nếu vẫn null)
if (player == null)
    player = FindFirstObjectByType<Dreamshaper>();

if (player != null)
{
    player.SetAllModulesEnabled(true);
    Debug.Log("[BossLanding] ✅ Đã mở khóa toàn bộ Player modules → FIGHT!");
}
```

---

## 7. Tóm tắt file Script & trạng thái

| Script | Vị trí | Được dùng? | Chức năng |
|---|---|---|---|
| `BossWakeUp.cs` | `BossEventTrigger` | ✅ Đang dùng | Trigger khoá nhân vật, bật Boss |
| `BossLanding.cs` | `BossPosition` (Boss) | ✅ Đang dùng | Chạy Timeline → Banner → Mở khóa |
| `BossTrigger.cs` | Không gắn đâu | ❌ Không dùng | Script cũ, có thể xóa |
| `Dreamshaper.cs` | `Dreamshaper` (Player) | ✅ Đang dùng | Quản lý module điều khiển nhân vật |
| `IntroController.cs` | Scene khác | ❌ Không dùng | Điều khiển video intro, chuyển scene |

---

## 8. Kết luận

**Code mở khóa di chuyển ĐÃ CÓ** trong `BossLanding.cs` ở bước 3 của `QuyTrinhBossRaMat()`. Tuy nhiên, do field `player` chưa được gán trong Inspector, nếu `FindFirstObjectByType<Dreamshaper>()` thất bại (trả về null), hàm mở khóa sẽ **không được gọi** và nhân vật sẽ bị khóa mãi mãi.

**→ Giải pháp:** Kéo `Dreamshaper` vào field `Player` của `BossLanding` trong Inspector của `BossPosition`.
