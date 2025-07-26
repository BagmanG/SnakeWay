using UnityEngine;
using UnityEngine.UI;
using YG;

public class HatManager : MonoBehaviour
{
    public GameObject[] hatsBuyButton;
    public GameObject[] hatsSetButton;
    public GameObject firstHatSetButton;
    public Color equipColor;
    public Color equipedColor;
    void OnEnable() => LoadSkins();

    private void LoadSkins()
    {
        int skinId = YG2.GetState("SkinID");
        for(int i = 0; i < hatsBuyButton.Length;i++)
        {
            hatsBuyButton[i].SetActive(YG2.GetState($"Hat{i+1}") == 0);
            hatsSetButton[i].SetActive(YG2.GetState($"Hat{i+1}") == 1);
            hatsSetButton[i].GetComponent<Image>().color = equipColor;
            hatsSetButton[i].transform.GetChild(0).GetComponent<Text>().text = Lang.Get("equip");
            firstHatSetButton.GetComponent<Image>().color = equipColor;
            firstHatSetButton.transform.GetChild(0).GetComponent<Text>().text = Lang.Get("equip");
            if (YG2.GetState($"Hat{i + 1}") == 1 && skinId == i+1){
                hatsSetButton[i].GetComponent<Image>().color = equipedColor;
                hatsSetButton[i].transform.GetChild(0).GetComponent<Text>().text = Lang.Get("equiped");
            }
        }
        if(skinId == 0)
        {
            firstHatSetButton.GetComponent<Image>().color = equipedColor;
            firstHatSetButton.transform.GetChild(0).GetComponent<Text>().text = Lang.Get("equiped");
        }
    }

    public void SetSkin(int id)
    {
        YG2.SetState("SkinID",id);
        LoadSkins();
    }

    public void Buy(string id)
    {
        YG2.BuyPayments(id);
    }
}
