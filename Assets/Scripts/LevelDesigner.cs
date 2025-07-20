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
    [SerializeField] private Material[] Skyboxes;
    private Biome currentBiome = Biome.Forest;


    public void Init(LevelData levelData)
    {
        ParseBiome(levelData.name);
        Material.color = currentBiome == Biome.Forest ? ForestColor : WinterColor;

        if(currentBiome == Biome.Winter)
        {
            SnowParticles.SetActive(true);
        }
        SetSkybox(levelData.skybox);
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

    private void SetSkybox(LevelSkybox skybox)
    {
        switch (skybox)
        {
            case LevelSkybox.Default:
                RenderSettings.skybox = Skyboxes[0];
                break;
            case LevelSkybox.Sunset:
                RenderSettings.skybox = Skyboxes[1];
                break;
            case LevelSkybox.Space:
                RenderSettings.skybox = Skyboxes[2];
                break;
        }
    }
}
