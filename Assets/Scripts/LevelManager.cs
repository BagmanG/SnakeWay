using Unity.Android.Gradle;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameManager GameManager;
    public LevelData CurrentLevel;
    public GameObject[] Blocks;
    public GameObject PlayerPrefab;
    public GameObject BlueSnakePrefab;
    public CameraController Camera;

    private GameObject _Player;
    public void LoadLevel()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError("LevelData не назначен!");
            return;
        }
        BuildLevel();
        CreateEntities();
        InitCamera();
    }

    private void InitCamera()
    {
        Camera.target = _Player.transform;
    }

    private void CreateEntities()
    {
        CreatePlayer();
        if (CurrentLevel.blueSnake.Length > 0)
        {
            CreateBlueSnake();
        }
    }

    private void CreatePlayer()
    {
        _Player = Instantiate(PlayerPrefab, new Vector3(CurrentLevel.playerSpawn.x, 1, CurrentLevel.playerSpawn.y), Quaternion.identity);
        var controller = _Player.GetComponent<PlayerController>();
        controller.GameManager = GameManager;
        controller.LevelManager = this;
        controller.cameraController = Camera;
    }

    private void CreateBlueSnake()
    {
        var points2D = CurrentLevel.blueSnake;
        var snake = Instantiate(BlueSnakePrefab, new Vector3(points2D[0].x, 0.73f, points2D[0].y), Quaternion.identity);
        for (int i = 1; i < points2D.Length; i++)
        {
            GameObject emptyObject = new GameObject($"Segment{i}");
            emptyObject.transform.position = new Vector3(points2D[i].x, 0.73f, points2D[i].y);
            emptyObject.transform.rotation = Quaternion.identity;
            emptyObject.transform.parent = snake.transform;
        }
        var snakeController = snake.GetComponent<Snake>();
        snakeController.GameManager = GameManager;
        snakeController.LevelManager = this;
        snakeController.InitSnake();

    }

    private void BuildLevel()
    {
        if (CurrentLevel.grid == null && CurrentLevel.serializedGrid != null)
        {
            CurrentLevel.ConvertTo2D();
        }

        for (int z = 0; z < CurrentLevel.height; z++)
        {
            for (int x = 0; x < CurrentLevel.width; x++)
            {
                if (CurrentLevel.grid[x, z] != 1)
                {
                    GameObject block = Instantiate(GetObjectByID(CurrentLevel.grid[x, z]), new Vector3(x, 0, z), Quaternion.identity);
                    block.transform.parent = this.transform;
                }
            }
        }
    }

    private GameObject GetObjectByID(int id)
    {
        switch (id)
        {
            case 0://Grass
                {
                    return Blocks[0];
                }
            case 2://Tree
                {
                    return Blocks[1];
                }
            case 3://Star
                {
                    return Blocks[2];
                }
            case 4://Finish
                {
                    return Blocks[3];
                }
            case 5://Bushes
                {
                    return Blocks[4];
                }
            case 6://Fir Trees
                {
                    return Blocks[5];
                }
            case 7://ICE
                {
                    return Blocks[6];
                }
        }

        return Blocks[0];
    }
}
