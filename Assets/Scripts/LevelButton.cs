using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField] private GameObject Locked;
    private LevelData data;
    private int levelIndex;
    
    private bool needBuy;
    [SerializeField] private GameObject stars3;
    [SerializeField] private GameObject stars2;
    [SerializeField] private GameObject stars1;
    public void Init(int index,int starsCount,LevelData data)
    {
        this.levelIndex = index;
        this.data = data;
        Text.text = $"{index}";
        if(starsCount == 1)
        {
            stars1.SetActive(true);
        }
        if (starsCount == 2)
        {
            stars2.SetActive(true);
        }
        if (starsCount == 3)
        {
            stars3.SetActive(true);
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
            GameObject.FindFirstObjectByType<MainMenu>().SetBuyWinterModal(true);
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
