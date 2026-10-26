
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public GameObject winningPanel;

    [Header("Final Score UI")]
    public TMP_Text gameOverScoreText;
    public TMP_Text winningScoreText;

    [Header("Gameplay")]
    public SpawnController songController;
    public string gameplaySceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio Sources")]
    public AudioSource defeatAudioSource;
    public AudioSource gameOverMusicSource;
    public AudioSource victoryMusicSource;

    [Header("Audio Start Times")]
    public float gameOverMusicStartTime = 1f;
    public float victoryMusicStartTime = 4f;

    [Header("Defeat Sound")]
    public float defeatSoundDuration = 1.5f;

    private bool gameOver;
    private bool gameWon;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winningPanel != null)
            winningPanel.SetActive(false);

        Time.timeScale = 1f;
        gameOver = false;
        gameWon = false;
    }

    private void OnEnable()
    {
        if (songController != null)
            songController.SongFinished += ShowWinning;
    }

    private void OnDisable()
    {
        if (songController != null)
            songController.SongFinished -= ShowWinning;
    }

    public void ShowGameOver()
    {
        if (gameOver || gameWon)
            return;

        gameOver = true;

        // Stop gameplay and the song immediately.
        if (songController != null)
            songController.StopMap();

        // Play the defeat sound from the beginning.
        if (defeatAudioSource != null)
        {
            defeatAudioSource.time = 0f;
            defeatAudioSource.Play();
        }

        StartCoroutine(ShowGameOverAfterSound());
    }

    private IEnumerator ShowGameOverAfterSound()
    {
        if (defeatAudioSource != null &&
            defeatAudioSource.clip != null)
        {
            float waitTime = Mathf.Min(
                defeatSoundDuration,
                defeatAudioSource.clip.length
            );

            yield return new WaitForSecondsRealtime(waitTime);
        }

        UpdateFinalScoreUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Pause gameplay, but allow UI audio to continue.
        Time.timeScale = 0f;

        PlayFromTime(gameOverMusicSource, gameOverMusicStartTime);
    }

    public void ShowWinning()
    {
        if (gameOver || gameWon)
            return;

        gameWon = true;

        UpdateFinalScoreUI();

        if (winningPanel != null)
            winningPanel.SetActive(true);

        Time.timeScale = 0f;

        PlayFromTime(victoryMusicSource, victoryMusicStartTime);
    }

    private void UpdateFinalScoreUI()
    {
        if (ScoreManager.Instance == null)
            return;

        // Display only the number on the end screens.
        string finalScore =
            ScoreManager.Instance.GetScore().ToString();

        if (gameOverScoreText != null)
            gameOverScoreText.text = finalScore;

        if (winningScoreText != null)
            winningScoreText.text = finalScore;
    }

    private void PlayFromTime(AudioSource source, float startTime)
    {
        if (source == null || source.clip == null)
            return;

        source.Stop();
        source.time = Mathf.Clamp(
            startTime,
            0f,
            Mathf.Max(0f, source.clip.length - 0.01f)
        );
        source.Play();
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
}
