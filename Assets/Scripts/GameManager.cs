using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    private PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;
    public GameUI UI;
    public int StarsCount = 0;
    public GameObject[] Stars;
    private Action moveCompleteAction;
    public void Start()
    {
        StarsCount = 0;
        LevelManager.LoadLevel();
        playerController = FindObjectOfType<PlayerController>();
        snake = FindObjectOfType<Snake>();

        if (playerController != null)
        {
            moveCompleteAction = () => StartCoroutine(OnPlayerMoveComplete());
            playerController.OnMoveComplete += moveCompleteAction;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    private IEnumerator OnPlayerMoveComplete()
    {
        isPlayerTurn = false;

        Vector2Int playerPosition = new Vector2Int(
            Mathf.RoundToInt(playerController.transform.position.x),
            Mathf.RoundToInt(playerController.transform.position.z)
        );

        Snake[] snakes = FindObjectsOfType<Snake>();
        foreach (Snake snake in snakes)
        {
            snake.MakeMove(playerPosition);
            snake.CheckPlayerCollision();

            while (snake.IsMoving())
            {
                yield return null;
            }
        }

        isPlayerTurn = true;
    }


    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnMoveComplete -= moveCompleteAction;
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

    public void GivePlayerStar()
    {
        StarsCount++;
        for(int i = 0; i < 3;i++)
        {
            Stars[i].SetActive(StarsCount-1 >= i);
        }
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Completed!");
    }

    public void PauseGame()
    {
        UI.SetPauseVisible(!UI.PauseVisible);
    }
}