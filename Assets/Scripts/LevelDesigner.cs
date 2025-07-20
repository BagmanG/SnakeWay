using UnityEngine;

public enum Biome
{
    Forest,Winter
}

public class LevelDesigner : MonoBehaviour
{
    [SerializeField] private Color ForestColor;
    [SerializeField] private Color WinterColor;
    [SerializeField] private Material Material;

    private Biome currentBiome = Biome.Forest;

    public void Init(string levelName)
    {
        ParseBiome(levelName);
        Material.color = currentBiome == Biome.Forest ? ForestColor : WinterColor;
    }

    private void ParseBiome(string levelName)
    {
        if (levelName.Contains("Forest"))
        {
            currentBiome = Biome.Forest;
            return;
        }
        if (levelName.Contains("Winter"))
        {
            currentBiome = Biome.Winter;
            return;
        }
    }
}
