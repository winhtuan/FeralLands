# BossCanvas – Banner & HP Bar

> **Map:** `Boss1Scene.unity`  
> **File liên quan:** `BossCanvas` (Canvas Overlay trong Hierarchy)

---

## 1. Bug: Banner Rơi Liên Tục (ĐÃ SỬA)

### Nguyên Nhân
File `BossBannerAnim.anim` có `m_LoopTime: 1` → animation **lặp vô tận**.

Diễn biến khi bug:
```
[0.0s] Banner hiện từ trên rơi xuống giữa màn
[3.0s] Banner trượt xuống biến mất
[3.0s] LOOP → banner lại xuất hiện từ trên, tiếp tục rơi...
```

### Cách sửa (ĐÃ làm)
```diff
# File: Assets/Animation/AnimationBannerMapBoss1/BossBannerAnim.anim
- m_LoopTime: 1
+ m_LoopTime: 0
```

> **Lưu ý thêm:** Sau khi sửa, animation chỉ chạy 1 lần rồi **freeze** ở frame cuối (Y=1016, Alpha=0). 
> `BossLanding.cs` đã `yield return new WaitForSeconds(3f)` nên timing vẫn đúng.

---

## 2. Cấu Trúc BossCanvas Hiện Tại

```
BossCanvas  (Canvas, Render Mode = Screen Space Overlay, 1920×1080)
├── Component: Canvas (Overlay)
├── Component: CanvasScaler (Scale With Screen Size, Reference 1920×1080)
├── Component: GraphicRaycaster
└── Component: Animator (Controller: BossCanvas.controller)
    │
    └── BossNamePanel  (RectTransform, CanvasGroup)
        ├── Chain_Left   Image xám #333, size 10×1000, pos (-387, 307)
        ├── Chain_Right  Image xám #333, size 10×1000, pos (373, 266)
        ├── BannerBG     Image sprite nền banner, size 921×515
        └── BossName     TextMeshProUGUI = "Hoả Ngục Quỷ", size 61.69
```

### Animation BossBannerAnim (sau khi sửa loop)

| Thời điểm | Y Position | Alpha | Trạng thái |
|-----------|-----------|-------|-----------|
| 0.0s | 983 (trên màn) | 0 | Ẩn |
| 0.5s | 0 (giữa màn) | 1 | Hiện rõ |
| 2.5s | 0 (giữa màn) | 1 | Đứng yên |
| 3.0s | 1016 (dưới màn) | 0 | Biến mất |

---

## 3. Kế Hoạch Làm HP Bar Cho Boss

### 3.1 Thiết Kế UI (thêm vào BossCanvas)

```
BossCanvas
├── BossNamePanel  (banner tên – đã có)
└── BossHPBar      (mới cần tạo)
    ├── HPBackground   Image nền thanh máu (màu tối)
    ├── HPFill         Image fill thanh máu (màu đỏ/cam)
    ├── HPBorder       Image viền ngoài
    └── BossHPLabel    TextMeshPro "Boss HP" hoặc tên boss
```

### 3.2 Cách Tạo HP Bar Trong Unity Editor

**Bước 1: Tạo GameObject mới trong BossCanvas**
1. Chuột phải `BossCanvas` → `UI > Slider` hoặc `UI > Image` (cách thủ công)
2. Đặt tên `BossHPBar`
3. Vị trí khuyến nghị: Anchor = Top Center, pos (0, -50) (dưới đỉnh màn hình)

**Bước 2: Cấu trúc Slider HP (dùng Unity UI Slider)**
```
BossHPBar (Slider component)
├── Background   Image màu xám tối (nền)
├── Fill Area
│   └── Fill     Image màu đỏ → Anchor stretch full
└── Handle Slide Area (có thể xóa nếu không cần handle)
```

**Bước 3: Script điều khiển HP bar**

```csharp
// BossHealth.cs – gắn lên BossPosition
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("Thiết lập")]
    public int maxHP = 100;
    public int currentHP;
    public Slider hpSlider;          // Kéo BossHPBar vào đây
    public GameObject hpBarObject;   // Kéo BossHPBar vào đây để ẩn/hiện

    void Start()
    {
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        // Ẩn thanh máu lúc đầu (hiện sau cutscene)
        if (hpBarObject != null) hpBarObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        
        if (hpSlider != null) hpSlider.value = currentHP;

        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Boss chết!");
        // TODO: chạy animation chết, chuyển scene, v.v.
    }

    // Gọi hàm này từ BossLanding sau khi cutscene xong
    public void ShowHPBar()
    {
        if (hpBarObject != null) hpBarObject.SetActive(true);
    }
}
```

### 3.3 Kết Nối Vào BossLanding.cs (cần sửa)

```csharp
// Thêm field mới vào BossLanding.cs
public BossHealth bossHealth;  // Kéo BossPosition vào đây

// Trong QuyTrinhBossRaMat(), sau bước 3:
if (bossHealth != null) bossHealth.ShowHPBar();
```

### 3.4 Hiện HP Bar Sau Cutscene (timeline với BossTrigger)

Nếu dùng `BossTrigger.cs` thay vì `BossLanding.cs`, thêm vào hàm `KetThucCutscene`:
```csharp
void KetThucCutscene(PlayableDirector director)
{
    if (uiThanhMauBoss != null) uiThanhMauBoss.SetActive(true); // đã có
    // Thêm: kích hoạt BossHealth
    bossObject.GetComponent<BossHealth>()?.ShowHPBar();
}
```

---

## 4. Thứ Tự Làm Tiếp

- [x] Sửa bug banner loop (`m_LoopTime: 0`)
- [ ] Tạo `BossHPBar` UI trong `BossCanvas`
- [ ] Viết script `BossHealth.cs` gắn lên `BossPosition`
- [ ] Kết nối `BossHealth` vào `BossLanding.cs` để hiện HP bar sau banner
- [ ] Làm sprite/animation boss thật (thay `Square` placeholder)
- [ ] Viết `BossAI.cs` cho boss tấn công

---

## 5. Ghi Chú Kỹ Thuật

| Vấn đề | Giải thích |
|--------|-----------|
| Tại sao dùng `Slider` cho HP? | Slider có sẵn `Fill Area` dễ animate. Ngoài ra có thể dùng `Image` với `Fill Amount` |
| HP bar nên Overlay hay World Space? | **Screen Space Overlay** (trong BossCanvas) – đơn giản hơn, không bị che bởi địa hình |
| Khi nào hiện HP bar? | Sau khi banner biến mất xong (cuối Coroutine trong BossLanding.cs) |
| Animate HP giảm mượt? | Dùng `DOTween` hoặc `Coroutine` với `Mathf.Lerp` để lerp từ giá trị cũ sang mới |
