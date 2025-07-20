using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    public PlayerController playerController;
    private Snake snake;
    private bool isPlayerTurn = true;
    public GameUI UI;
    public int StarsCount = 0;
    public GameObject[] Stars;
    private Action moveCompleteAction;
    bool completed = false;
    public int Steps = 0;
    public int LevelIndex = 0;
    public GameLevels GameLevels;
    public void Start()
    {
        LevelManager.CurrentLevel = GlobalVars.currentBiome == Biome.Forest ? GameLevels.ForestLevels[GlobalVars.currentLevelID] : GameLevels.WinterLevels[GlobalVars.currentLevelID];
        StarsCount = 0;
        Steps = 0;
        LevelManager.LoadLevel();
        playerController = FindObjectOfType<PlayerController>();
        snake = FindObjectOfType<Snake>();
        Application.targetFrameRate = 120;
        if (playerController != null)
        {
            moveCompleteAction = () => StartCoroutine(OnPlayerMoveComplete());
            playerController.OnMoveComplete += moveCompleteAction;
        }

        Debugger.Instance?.Log("=== Level Started ===");
        Debugger.Instance?.Log($"Player Spawn: {LevelManager.CurrentLevel.playerSpawn}");

        for (int y = 0; y < LevelManager.CurrentLevel.height; y++)
        {
            string row = "";
            for (int x = 0; x < LevelManager.CurrentLevel.width; x++)
            {
                row += LevelManager.CurrentLevel.grid[x, y] + " ";
            }
            Debugger.Instance?.Log("Grid Row " + y + ": " + row);
        }
        LoadLevelIndex();
        UI.Init();
    }

    private void LoadLevelIndex()
    {
        LevelIndex = GlobalVars.currentLevelID;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { PauseGame(); return; }
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.R)) { TryAgain(); return; }
        #endif
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator OnPlayerMoveComplete()
    {
        isPlayerTurn = false;

        Vector2Int playerPosition = new Vector2Int(
            Mathf.RoundToInt(playerController.transform.position.x),
            Mathf.RoundToInt(playerController.transform.position.z)
        );

        Debugger.Instance?.Log($"Player moved to {playerPosition}");

        Snake[] snakes = FindObjectsOfType<Snake>();
        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        foreach (Snake snake in snakes)
        {
            for (int i = 0; i < snake.transform.childCount; i++)
            {
                Transform segment = snake.transform.GetChild(i);
                Vector2Int pos = new Vector2Int(
                    Mathf.RoundToInt(segment.position.x),
                    Mathf.RoundToInt(segment.position.z)
                );
                occupiedCells.Add(pos);
            }
        }

        Dictionary<Snake, Vector2Int> plannedMoves = new Dictionary<Snake, Vector2Int>();
        Dictionary<Snake, List<Vector2Int>> plannedBodies = new Dictionary<Snake, List<Vector2Int>>();

        foreach (Snake snake in snakes)
        {
            List<Vector2Int> dynamicObstacles = new List<Vector2Int>(occupiedCells);
            foreach (var other in snakes)
            {
                if (other != snake)
                {
                    dynamicObstacles.AddRange(other.GetPlannedBodyPositions());
                }
            }

            Vector2Int nextCell = snake.PeekNextMove(playerPosition, dynamicObstacles);
            plannedMoves[snake] = nextCell;
            plannedBodies[snake] = snake.GetPlannedBodyPositions();

            Debugger.Instance?.Log($"Snake {snake.name} head at {snake.transform.GetChild(0).position}, planned move: {nextCell}");
        }

        HashSet<Vector2Int> conflictCells = new HashSet<Vector2Int>();
        HashSet<Vector2Int> allPlanned = new HashSet<Vector2Int>();

        foreach (var move in plannedMoves.Values)
        {
            if (allPlanned.Contains(move) || occupiedCells.Contains(move))
            {
                conflictCells.Add(move);
            }
            allPlanned.Add(move);
        }

        foreach (Snake snake in snakes)
        {
            Vector2Int plannedMove = plannedMoves[snake];

            // Новый код: змея может ходить на свою текущую позицию
            Vector2Int currentHead = new Vector2Int(
                Mathf.RoundToInt(snake.transform.GetChild(0).position.x),
                Mathf.RoundToInt(snake.transform.GetChild(0).position.z)
            );

            bool isSelfBlocking = (plannedMove == currentHead);

            if (isSelfBlocking || (!conflictCells.Contains(plannedMove) && !occupiedCells.Contains(plannedMove)))
            {
                Debugger.Instance?.Log($"Snake {snake.name} executes move to {plannedMove}");
                yield return StartCoroutine(snake.Step());

                for (int i = 0; i < snake.transform.childCount; i++)
                {
                    Transform segment = snake.transform.GetChild(i);
                    Vector2Int pos = new Vector2Int(
                        Mathf.RoundToInt(segment.position.x),
                        Mathf.RoundToInt(segment.position.z)
                    );
                    occupiedCells.Add(pos);
                }
            }
            else
            {
                Debugger.Instance?.Log($"Snake {snake.name} blocked (planned: {plannedMove})");
            }
        }

        isPlayerTurn = true;
        Steps++;
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
        if (!completed)
        {
            Debugger.Instance?.Log("=== GAME OVER ===");
            UI.ShowGameOver();
        }
    }

    public void TryAgain()
    {
        YG2.InterstitialAdvShow();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GivePlayerStar()
    {
        StarsCount++;
        for (int i = 0; i < 3; i++)
        {
            Stars[i].SetActive(StarsCount - 1 >= i);
        }
    }

    public void LevelCompleted()
    {
        if (completed == false)
        {
            SaveLevelInfo();
            Debugger.Instance?.Log("=== LEVEL COMPLETED ===");
            Debug.Log("LEVEL COMPLTETED");
            completed = true;
            UI.ShowFinish();
        }
    }

    public void PauseGame()
    {
        UI.SetPauseVisible(!UI.PauseVisible);
    }

    public void UnPauseGame()
    {
        UI.SetPauseVisible(false);
    }

    public static int ExtractNumber(string text)
    {
        int number = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsDigit(text[i]))
            {
                number = number * 10 + (text[i] - '0');
            }
        }
        return number;
    }

    public void SkipLevel()
    {
        YG2.RewardedAdvShow("SkipLevel", () =>
        {
            ///Ставим 3 звезды на текущий уровень
            YG2.SetState(LevelManager.CurrentLevel.name, 3);
            GoNextLevel();
            ///TODO
        });
    }

    public void GoNextLevel()
    {
        ///TODO
        //Переход на след.биом
        if (GlobalVars.currentLevelID == 9 && GlobalVars.currentBiome == Biome.Forest)
        {
            GlobalVars.currentLevelID = 0;
            GlobalVars.levelName = $"Winter1";
            GlobalVars.currentBiome = Biome.Winter;
            SceneManager.LoadScene("GameScene");
            return;
        }
        if (GlobalVars.currentLevelID == 9 && GlobalVars.currentBiome == Biome.Winter)
        {
            GoToMenu();
            return;
        }
        GlobalVars.currentLevelID++;
        string levelBiome = GlobalVars.currentBiome == Biome.Forest ? "Forest" : "Winter";
        GlobalVars.levelName = $"{levelBiome}{GlobalVars.currentLevelID}";
        SceneManager.LoadScene("GameScene");
    }

    private void SaveLevelInfo()
    {
        if(YG2.GetState(LevelManager.CurrentLevel.name) < StarsCount)
            YG2.SetState(LevelManager.CurrentLevel.name,StarsCount);
    }

    public bool IsLevelCompleted() => completed;
}
