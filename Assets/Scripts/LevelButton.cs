using System;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField] private GameObject[] Stars;

    public void Init(int index,int starsCount)
    {
        Text.text = $"{index}";
        for(int i = 0; i < 3; i++)
        {
            Stars[i].SetActive(i<starsCount);    
        }

    }
}
