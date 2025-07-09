using Unity.Android.Gradle;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData CurrentLevel;
    public GameObject[] Blocks;
    public GameObject PlayerPrefab;
    public void LoadLevel()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError("LevelData не назначен!");
            return;
        }
        BuildLevel();
        CreateEntities();
    }

    private void CreateEntities()
    {
        var player = Instantiate(PlayerPrefab, new Vector3(CurrentLevel.playerSpawn.x, 1, CurrentLevel.playerSpawn.y), Quaternion.identity);
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
                if (CurrentLevel.grid[x, z] == 1)
                {
                    GameObject block = Instantiate(Blocks[0], new Vector3(x, 0, z), Quaternion.identity);
                    block.transform.parent = this.transform;
                }
            }
        }
    }
}
