using UnityEngine;
using YG;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject LevelButtonPrefab;
    [SerializeField] private Transform LevelsRoot;
    private Biome currentBiome = Biome.Forest;

    public GameObject SelectBiomeObject,SelectLevelObject;
    public GameLevels Levels;

    private void Start()
    {
        ConsumePurchases();
    }


    private void ConsumePurchases()
    {
        YG2.ConsumePurchases();
    }

    public void SelectBiome(int biomeID)
    {
        currentBiome = biomeID == 0 ? Biome.Forest : Biome.Winter;
        LoadLevels();
        SelectBiomeObject.SetActive(false);
        SelectLevelObject.SetActive(true);
    }

    private void LoadLevels()
    {
        foreach (Transform child in LevelsRoot)
        {
            Destroy(child.gameObject);
        }
        var levelPresetName = (currentBiome == Biome.Forest) ? "Forest" : "Winter";
        var levels = (currentBiome == Biome.Forest) ? Levels.ForestLevels : Levels.WinterLevels;
        for (int i = 0; i < levels.Length; i++)
        {
            LevelButton lb = Instantiate(LevelButtonPrefab,LevelsRoot).GetComponent<LevelButton>();
            lb.Init(i + 1, YG2.GetState($"{levelPresetName}{i + 1}"), levels[i]);
        }
    }

    private void OnEnable()
    {
        YG2.onPurchaseSuccess += SuccessPurchased;
        YG2.onPurchaseFailed += FailedPurchased;
    }

    private void OnDisable()
    {
        YG2.onPurchaseSuccess -= SuccessPurchased;
        YG2.onPurchaseFailed -= FailedPurchased;
    }

    private void SuccessPurchased(string id)
    {
        if (id == "WinterBiome")
        {
            YG2.SetState("WinterBiomePurchased",1);
        }
    }

    private void FailedPurchased(string id)
    {
        // Покупка не была совершена
    }

    public void BuyWinterBiome()
    {
        YG2.BuyPayments("WinterBiomePurchased");
    }
}
