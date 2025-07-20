using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;

    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject PauseUI;

    [SerializeField] private Text gameOverText;

    [SerializeField] private Image[] gameOverStars;
    [SerializeField] private Sprite givedStar, unGivedStar;


    public bool PauseVisible { get { return PauseUI.activeSelf; } }


    private int levelIndex = 0;
    private void Start()
    {
        levelIndex = GameManager.LevelIndex;
    }

    public void ShowGameOver()
    {
        PreInitGameOver();
        GameOverUI.SetActive(true);
    }

    public void SetPauseVisible(bool value)
    {
        PauseUI.SetActive(value);
    }

    private void PreInitGameOver()
    {
        LoadGameOverStars();
        gameOverText.text = $"Количетсво ходов: {GameManager.Steps + 1}\nУровень: {levelIndex}";
    }


    private void LoadGameOverStars()
    {
        int stars = GameManager.StarsCount;
        for(int i = 0; i < 3; i++)
        {
            gameOverStars[i].sprite = i <= stars -1 ? givedStar : unGivedStar;
        }
    }
}
