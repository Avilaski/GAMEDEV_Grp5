using UnityEngine;
using UnityEngine.UI;

public class BlinkingImage : MonoBehaviour
{
    public float blinkSpeed = 0.5f;

    private Image image;
    private float timer;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkSpeed)
        {
            timer = 0f;
            image.enabled = !image.enabled;
        }
    }
}