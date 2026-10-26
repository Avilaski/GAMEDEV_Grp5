using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public SpawnController songController;
    public string gameplaySceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";
    private bool gameOver, mapStopped;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            Image background = gameOverPanel.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0f, 0f, 0f, 0.85f);
            }

            if (gameOverPanel.transform.childCount == 0)
            {
                AddText(gameOverPanel.transform, "Game Over", new Vector2(0, 90), new Vector2(400, 70), 42);
                AddButton("Retry", new Vector2(0, 0), Retry);
                AddButton("Main Menu", new Vector2(0, -65), ReturnToMainMenu);
            }

            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void LateUpdate()
    {
        if (gameOver && !mapStopped)
        {
            mapStopped = true;
            if (songController != null)
            {
                songController.StopMap();
            }
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void AddButton(string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(gameOverPanel.transform, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240, 48);
        go.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f);
        go.GetComponent<Button>().onClick.AddListener(action);
        AddText(go.transform, label, Vector2.zero, new Vector2(240, 48), 24);
    }

    private void AddText(Transform parent, string label, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
    }
}
