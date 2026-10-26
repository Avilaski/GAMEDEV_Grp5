
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public int monsterHitPoints = 100;
    public int monsterMissPenalty = -100;
    public int crateHitPenalty = -50;

    [Header("UI")]
    public TMP_Text scoreText;

    [Tooltip("Enable for gameplay UI; disable for game-over and winning screens.")]
    public bool showScoreLabel = true;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        currentScore = Mathf.Max(0, currentScore + points);
        UpdateScoreUI();
    }

    public void MonsterHit()
    {
        AddScore(monsterHitPoints);
    }

    public void MonsterMissed()
    {
        AddScore(monsterMissPenalty);
    }

    public void CrateHit()
    {
        AddScore(crateHitPenalty);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            string label = showScoreLabel ? "Score: " : "";
            scoreText.text = label + currentScore;
        }
    }

    public int GetScore()
    {
        return currentScore;
    }
}
