
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
        currentScore += points;
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
            scoreText.text = "Score: " + currentScore;
        }
    }

    public int GetScore()
    {
        return currentScore;
    }
}
