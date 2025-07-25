using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject LevelButtonPrefab;
    [SerializeField] private Transform LevelsRoot;
    private Biome currentBiome = Biome.Forest;

    public GameObject SelectBiomeObject,SelectLevelObject,MainObject;
    public GameObject ReviewButton;
    public GameLevels Levels;
    public GameObject AuthPlayerInfo;
    public GameObject AuthPlayerButton;
    public GameObject WinterBiomeAds;
    public GameObject WhyAuthObject;
    private void Start()
    {
        ConsumePurchases();
        MainObject.SetActive(true);
        CheckReview();
        CheckAuth();
        WinterBiomeAds.SetActive(!GlobalVars.WinterPurchased());
    }

    private void CheckReview()
    {
        ReviewButton.SetActive(YG2.reviewCanShow);
    }

    private void CheckAuth()
    {
        AuthPlayerInfo.SetActive(YG2.player.auth);
        AuthPlayerButton.SetActive(!YG2.player.auth);
        WhyAuthObject.SetActive(!YG2.player.auth);
    }

    public void Auth()
    {
        YG2.OpenAuthDialog();
    }

    public void UpdateData()
    {
        SceneManager.LoadScene("MainMenu");
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
        YG2.onGetSDKData += UpdateData;
        YG2.onPurchaseSuccess += SuccessPurchased;
        YG2.onPurchaseFailed += FailedPurchased;
    }

    private void OnDisable()
    {
        YG2.onPurchaseSuccess -= SuccessPurchased;
        YG2.onPurchaseFailed -= FailedPurchased;
        YG2.onGetSDKData -= UpdateData;
    }

    private void SuccessPurchased(string id)
    {
        if (id == "WinterBiomePurchased")
        {
            YG2.SetState("WinterBiomePurchased",1);
            UpdateData();
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
