using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public RectMask2D preMask;
    public Image preBar;
    public RectMask2D realMask;
    public Image realBar;
    public float preSpeed = 2f;
    public float realSpeed = 6f;
    private float preHealth, realHealth, targetHealth;
    private bool initialized;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
        }
    }

    private void Start()
    {
        if (playerHealth == null || preMask == null || realMask == null || preBar == null || realBar == null)
        {
            Debug.LogError("Assign PlayerHealth, both masks, and both images in HealthBar.", this);
            enabled = false;
            return;
        }

        Canvas.ForceUpdateCanvases();
        ConfigureBarRect(preBar.rectTransform);
        ConfigureBarRect(realBar.rectTransform);
        preBar.color = Color.white;
        realBar.color = Color.yellow;
        preBar.raycastTarget = realBar.raycastTarget = false;
        initialized = true;
        UpdateHealthBar(true);
    }

    private void ConfigureBarRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnHealthChanged(int current, int maximum)
    {
        targetHealth = current;
    }

    private void Update()
    {
        if (!initialized || playerHealth == null)
        {
            return;
        }

        realHealth = Mathf.MoveTowards(realHealth, targetHealth, Mathf.Max(0f, realSpeed) * Time.unscaledDeltaTime * playerHealth.maxHealth);
        preHealth = Mathf.MoveTowards(preHealth, targetHealth, Mathf.Max(0f, preSpeed) * Time.unscaledDeltaTime * playerHealth.maxHealth);
        Refresh();
    }

    public void UpdateHealthBar(bool instant = false)
    {
        if (playerHealth == null)
        {
            return;
        }

        targetHealth = playerHealth.CurrentHealth;
        if (!initialized)
        {
            return;
        }

        if (instant)
        {
            preHealth = realHealth = targetHealth;
        }

        Refresh();
    }

    private void Refresh()
    {
        float maximum = Mathf.Max(1, playerHealth.maxHealth);
        realBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, realMask.rectTransform.rect.width * Mathf.Clamp01(realHealth / maximum));
        preBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preMask.rectTransform.rect.width * Mathf.Clamp01(preHealth / maximum));
    }
}
