using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý thanh máu Boss theo cách tiếp cận Dual-Layer.
/// Đã fix: Thêm hiệu ứng Fade mượt mà khi chuyển Phase bằng CanvasGroup.
/// </summary>
public class BossUIController : MonoBehaviour
{
    [Header("Boss Reference")]
    public BossHealth bossHealth;

    [Header("Phase 1 - Blue (HP >= 50%)")]
    public GameObject phase1Root;
    public Image      blueFillImage;
    private CanvasGroup phase1CG;

    [Header("Phase 2 - Red (HP < 50%)")]
    public GameObject phase2Root;
    public Image      redFillImage;
    private CanvasGroup phase2CG;

    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;

    private bool isPhase2 = false;
    private Coroutine fadeRoutine;

    private void OnValidate()
    {
        if (phase1Root == null) phase1Root = transform.Find("Phase1_Blue")?.gameObject;
        if (phase2Root == null) phase2Root = transform.Find("Phase2_Red")?.gameObject;
        
        if (blueFillImage == null && phase1Root != null) 
            blueFillImage = phase1Root.transform.Find("Fill_Blue")?.GetComponent<Image>();
            
        if (redFillImage == null && phase2Root != null) 
            redFillImage = phase2Root.transform.Find("Fill_Red")?.GetComponent<Image>();
    }

    private void Awake()
    {
        // Tự động lấy hoặc thêm CanvasGroup nếu chưa có
        if (phase1Root != null) phase1CG = phase1Root.GetComponent<CanvasGroup>() ?? phase1Root.AddComponent<CanvasGroup>();
        if (phase2Root != null) phase2CG = phase2Root.GetComponent<CanvasGroup>() ?? phase2Root.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (bossHealth == null) return;
        bossHealth.OnHealthChanged += UpdateHealthBar;

        isPhase2 = false; 
        InitUIVisibility();
        CheckCurrentHP();
    }

    private void Start()
    {
        CheckCurrentHP();
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void CheckCurrentHP()
    {
        if (bossHealth == null) return;
        UpdateHealthBar(bossHealth.GetCurrentHP(), bossHealth.GetMaxHP());
    }

    private void UpdateHealthBar(int currentHP, int maxHP)
    {
        if (maxHP <= 0) 
        {
            isPhase2 = false;
            SyncPhaseVisibility();
            SetFill(1f);
            return;
        }

        float percent = (float)currentHP / maxHP;
        SetFill(percent);

        bool shouldBePhase2 = percent < 0.5f;
        if (shouldBePhase2 != isPhase2)
        {
            isPhase2 = shouldBePhase2;
            SyncPhaseVisibility();
        }
    }

    private void SetFill(float percent)
    {
        float clamped = Mathf.Clamp01(percent);
        if (blueFillImage != null) blueFillImage.fillAmount = clamped;
        if (redFillImage  != null) redFillImage.fillAmount  = clamped;
    }

    /// <summary>Khởi tạo hiển thị ngay lập tức không fade.</summary>
    private void InitUIVisibility()
    {
        if (phase1Root != null) phase1Root.SetActive(!isPhase2);
        if (phase2Root != null) phase2Root.SetActive(isPhase2);

        if (phase1CG != null) phase1CG.alpha = isPhase2 ? 0f : 1f;
        if (phase2CG != null) phase2CG.alpha = isPhase2 ? 1f : 0f;
    }

    /// <summary>Chuyển phase kèm hiệu ứng Fade.</summary>
    private void SyncPhaseVisibility()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTransitionRoutine());
    }

    private IEnumerator FadeTransitionRoutine()
    {
        // Luôn đảm bảo cả 2 GameObject đều Active khi đang Fade
        if (phase1Root != null) phase1Root.SetActive(true);
        if (phase2Root != null) phase2Root.SetActive(true);

        float elapsed = 0f;
        float startAlpha1 = (phase1CG != null) ? phase1CG.alpha : 0f;
        float startAlpha2 = (phase2CG != null) ? phase2CG.alpha : 0f;

        float targetAlpha1 = isPhase2 ? 0f : 1f;
        float targetAlpha2 = isPhase2 ? 1f : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (phase1CG != null) phase1CG.alpha = Mathf.Lerp(startAlpha1, targetAlpha1, t);
            if (phase2CG != null) phase2CG.alpha = Mathf.Lerp(startAlpha2, targetAlpha2, t);

            yield return null;
        }

        // Sau khi fade xong, disable object hoàn toàn để tối ưu và tránh lỗi hit test
        if (phase1CG != null) phase1CG.alpha = targetAlpha1;
        if (phase2CG != null) phase2CG.alpha = targetAlpha2;

        if (phase1Root != null) phase1Root.SetActive(!isPhase2);
        if (phase2Root != null) phase2Root.SetActive(isPhase2);
        
        fadeRoutine = null;
    }
}
