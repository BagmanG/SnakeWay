using Unity.VisualScripting;
using UnityEngine;
using YG;

public class HatSetter : MonoBehaviour
{
    public GameObject[] hats;
    public GameObject[] head;
    public void Start()
    {
        int skinId = YG2.GetState("SkinID");
        if (skinId == 0)
        {
            SetHeadVisible(true);
            return;
        }
        else
        {
            SetHat(skinId);
            return;
        }
    }

    public void SetHat(int id)
    {
        for (int i = 0; i < head.Length; i++)
        {
            head[i].SetActive(id == i);
        }
    }

    public void SetHeadVisible(bool value)
    {
        for(int i = 0; i < head.Length; i++)
        {
            head[i].SetActive(value);
        }
    }
}
