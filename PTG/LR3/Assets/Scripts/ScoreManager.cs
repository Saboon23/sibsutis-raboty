using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public Text scoreText;
    int score;

    void Awake()
    {
        instance = this;
        Redraw();
    }

    public void Add(int points)
    {
        score += points;
        Redraw();
    }

    void Redraw()
    {
        if (scoreText != null) scoreText.text = "Очки: " + score;
    }
}
