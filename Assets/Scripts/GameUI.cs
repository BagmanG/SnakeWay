using UnityEngine;
using UnityEngine.UI;
using YG;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;

    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject FinishUI;
    [SerializeField] private GameObject PauseUI;

    [SerializeField] private Text gameOverText;
    [SerializeField] private Text finishText;

    [SerializeField] private Image[] gameOverStars;
    [SerializeField] private Image[] finishStars;
    [SerializeField] private Sprite givedStar, unGivedStar;

    [SerializeField] private GameObject MobileInputs;
    [SerializeField] private MobileInputController MobileInputController;
    public bool PauseVisible { get { return PauseUI.activeSelf; } }


    private int levelIndex = 0;
    public void Init()
    {
        levelIndex = GameManager.LevelIndex;
        InitForPlatform();
    }

    public void ShowGameOver()
    {
        PreInitUI();
        GameOverUI.SetActive(true);
    }

    public void ShowFinish()
    {
        PreInitUI();
        FinishUI.SetActive(true);
    }

    public void SetPauseVisible(bool value)
    {
        PauseUI.SetActive(value);
    }

    private void PreInitUI()
    {
        LoadGameOverStars();
        gameOverText.text = $"Количетсво ходов: {GameManager.Steps + 1}\nУровень: {levelIndex+1}";
        finishText.text = $"Количетсво ходов: {GameManager.Steps + 1}\nУровень: {levelIndex+1}";
    }


    private void LoadGameOverStars()
    {
        int stars = GameManager.StarsCount;
        for(int i = 0; i < 3; i++)
        {
            gameOverStars[i].sprite = i <= stars -1 ? givedStar : unGivedStar;
            finishStars[i].sprite = i <= stars -1 ? givedStar : unGivedStar;
        }
    }

    private void InitForPlatform()
    {
        bool isDesktop = YG2.envir.isDesktop;
        if (!isDesktop)
        {
            MobileInputs.SetActive(true);
            MobileInputController.Initialize(GameManager.playerController);
        }
    }
}
