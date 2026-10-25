
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    public RectMask2D preMask;
    public Image preBar;

    public RectMask2D realMask;
    public Image realBar;

    [Header("Animation Settings")]
    public float preSpeed = 2f;
    public float realSpeed = 6f;

    private RectTransform preBarRect;
    private RectTransform realBarRect;

    private float fullBarWidth;
    private float preHealth;
    private float realHealth;

    private bool initialized = false;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError(
                "PlayerHealth is not assigned to HealthBar!"
            );
            enabled = false;
            return;
        }

        if (preMask == null || realMask == null ||
            preBar == null || realBar == null)
        {
            Debug.LogError(
                "Assign both masks and both bar images in HealthBar."
            );
            enabled = false;
            return;
        }

        preBarRect = preBar.rectTransform;
        realBarRect = realBar.rectTransform;

        // Both bars should have the same full width.
        fullBarWidth = realMask.rectTransform.rect.width;

        if (fullBarWidth <= 0f)
        {
            Debug.LogError(
                "Health bar width is zero. Check the mask's Rect Transform."
            );
            enabled = false;
            return;
        }
        
        ConfigureBarRect(preBarRect);
        ConfigureBarRect(realBarRect);

        preBar.color = Color.white;
        realBar.color = Color.yellow;

        UpdateHealthBar(true);
    }

    void ConfigureBarRect(RectTransform barRect)
    {
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 0.5f);

        barRect.anchoredPosition = Vector2.zero;
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = new Vector2(0f, 0f);
    }

    void Update()
    {
        if (!initialized || playerHealth == null)
            return;

        float targetHealth = playerHealth.GetCurrentHealth();

        realHealth = Mathf.MoveTowards(
            realHealth,
            targetHealth,
            realSpeed * Time.deltaTime * playerHealth.maxHealth
        );

        preHealth = Mathf.MoveTowards(
            preHealth,
            targetHealth,
            preSpeed * Time.deltaTime * playerHealth.maxHealth
        );

        Refresh();
    }

    public void UpdateHealthBar(bool instant = false)
    {
        if (playerHealth == null)
            return;

        if (instant || !initialized)
        {
            preHealth = playerHealth.GetCurrentHealth();
            realHealth = playerHealth.GetCurrentHealth();
            initialized = true;

            Refresh();
        }
    }

    void Refresh()
    {
        if (playerHealth == null || playerHealth.maxHealth <= 0)
            return;

        float maxHealth = playerHealth.maxHealth;

        float realRatio = Mathf.Clamp01(realHealth / maxHealth);
        float preRatio = Mathf.Clamp01(preHealth / maxHealth);

        float realWidth = fullBarWidth * realRatio;
        float preWidth = fullBarWidth * preRatio;

        realBarRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            realWidth
        );

        preBarRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            preWidth
        );
    }
}
