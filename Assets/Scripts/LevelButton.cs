using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField] private GameObject[] Stars;
    [SerializeField] private GameObject Locked;
    private LevelData data;
    private int levelIndex;
    
    private bool needBuy;

    public void Init(int index,int starsCount,LevelData data)
    {
        this.levelIndex = index;
        this.data = data;
        Text.text = $"{index}";
        for(int i = 0; i < 3; i++)
        {
            Stars[i].SetActive(i<starsCount);    
        }
        CheckNeedBuy();
    }

    public void LoadLevel()
    {
        if (needBuy == false)
        {
            GlobalVars.currentLevelID = levelIndex - 1;
            GlobalVars.levelName = data.name;
            GlobalVars.currentBiome = data.GetBiome();
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            //TODO
            YG2.BuyPayments("WinterBiomePurchased");
        }
    }

    void CheckNeedBuy()
    {
        //Winter Biome
        if(data.GetBiome() == Biome.Winter && levelIndex >= 3 && GlobalVars.WinterPurchased() == false)
        {
            needBuy = true;
            Locked.SetActive(true);
        }
    }
}
