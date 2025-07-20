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
    private LevelData data;
    private int levelIndex;
    public void Init(int index,int starsCount,LevelData data)
    {
        this.levelIndex = index;
        this.data = data;
        Text.text = $"{index}";
        for(int i = 0; i < 3; i++)
        {
            Stars[i].SetActive(i<starsCount);    
        }
    }

    public void LoadLevel()
    {
        GlobalVars.currentLevelID = levelIndex -1;
        GlobalVars.levelName = data.name;
        GlobalVars.currentBiome = data.GetBiome();
        SceneManager.LoadScene("GameScene");
    }
}
