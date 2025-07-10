using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject GameOverUI;
    public void ShowGameOver()
    {
        GameOverUI.SetActive(true);
    }
}
