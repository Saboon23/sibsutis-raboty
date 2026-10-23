using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;
    public Text shotsText;
    public Image powerBar;

    public bool Finished => finished;
    int shots;
    bool finished;

    void Start()
    {
        Time.timeScale = 1f;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        Redraw();
    }

    public void AddShot()
    {
        shots++;
        Redraw();
    }

    public void SetPower(float value)
    {
        if (powerBar != null) powerBar.fillAmount = value;
    }

    void Redraw()
    {
        if (shotsText != null) shotsText.text = "Ударов: " + shots;
    }

    public void Win()
    {
        if (finished) return;
        finished = true;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Lose()
    {
        if (finished) return;
        finished = true;
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
        else SceneManager.LoadScene(0);
    }
}
