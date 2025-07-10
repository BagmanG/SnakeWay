using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    private PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;
    public GameUI UI;
    public void Start()
    {
        LevelManager.LoadLevel();
        playerController = FindObjectOfType<PlayerController>();
        snake = FindObjectOfType<Snake>();

        if (playerController != null)
        {
            playerController.OnMoveComplete += OnPlayerMoveComplete;
        }
    }

    private void OnPlayerMoveComplete()
    {
        // Ход игрока завершен, теперь ход змейки
        isPlayerTurn = false;

        // Получаем текущую позицию игрока
        Vector2Int playerPosition = new Vector2Int(
            Mathf.RoundToInt(playerController.transform.position.x),
            Mathf.RoundToInt(playerController.transform.position.z)
        );

        // Двигаем змейку
        snake.MakeMove(playerPosition);

        // Возвращаем ход игроку
        isPlayerTurn = true;
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnMoveComplete -= OnPlayerMoveComplete;
        }
    }

    public void GameOver()
    {
        UI.ShowGameOver();
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}