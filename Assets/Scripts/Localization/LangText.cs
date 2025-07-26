using UnityEngine;
using UnityEngine.UI;
using YG;

public class LangText : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private bool mobile = false;
    private void OnEnable()
    {
        if (mobile == false)
        {
            GetComponent<Text>().text = Lang.Get(key);
        }
        else
        {
            if (!YG2.envir.isDesktop)
            {
                GetComponent<Text>().text =  Lang.Get("mobile_"+key);
            }
            else
            {
                GetComponent<Text>().text = Lang.Get(key);
            }
        }
    }
}
