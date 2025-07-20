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

    [SerializeField] private GameObject SnowParticles;

    private Biome currentBiome = Biome.Forest;

    public void Init(string levelName)
    {
        ParseBiome(levelName);
        Material.color = currentBiome == Biome.Forest ? ForestColor : WinterColor;

        if(currentBiome == Biome.Winter)
        {
            SnowParticles.SetActive(true);
        }
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
