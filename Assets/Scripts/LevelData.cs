using UnityEngine;

public enum LevelSkybox
{
    Default,Space,Sunset
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 5;
    public int height = 5;
    public int[,] grid;
    public Vector2Int playerSpawn;
    public int[] serializedGrid;

    public Vector2[] blueSnake;
    public Vector2[] redSnake;
    public LevelSkybox skybox = LevelSkybox.Default;

    public void ConvertTo2D()
    {
        grid = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y] = serializedGrid[y * width + x];
            }
        }
    }

    public Biome GetBiome()
    {
        if (name.Contains("Forest"))
        {
            return Biome.Forest;
        }
        if (name.Contains("Winter"))
        {
            return Biome.Winter;
        }
        return Biome.Forest;
    }
}