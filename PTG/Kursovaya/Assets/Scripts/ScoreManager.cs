using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;    //ссылка на единственный экземпляр счётчика

    public Text scoreText;                  //текстовое поле на Canvas
    public int pointsPerMonster = 10;       //сколько очков даёт один монстр

    int score = 0;                          //текущее количество очков

    void Awake()
    {
        instance = this;
        Redraw();
    }

    public void AddScore()
    {
        score += pointsPerMonster;
        Redraw();
    }

    void Redraw()
    {
        if (scoreText != null)
            scoreText.text = "Очки: " + score;
    }
}
