using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject PauseUI;

    public bool PauseVisible { get { return PauseUI.activeSelf; } }

    public void ShowGameOver()
    {
        GameOverUI.SetActive(true);
    }

    public void SetPauseVisible(bool value)
    {
        PauseUI.SetActive(value);
    }
}
