using UnityEngine;
using UnityEngine.UI;
using YG;

public class SoundVisibility : MonoBehaviour
{
    public Image img;
    public Sprite onSprite, offSprite;
    void Start()
    {
        bool isOn = YG2.GetState("Music") == 0;
        if (isOn)
        {
            AudioListener.volume = 1;
            img.sprite = onSprite;
        }
        else
        {
            AudioListener.volume = 0;
            img.sprite = offSprite;
        }
    }

    public void Clicked()
    {
        AudioListener.volume = AudioListener.volume == 1 ? 0 : 1;
        YG2.SetState("Music", AudioListener.volume == 1 ? 0 : 1);
        img.sprite = AudioListener.volume == 1 ? onSprite : offSprite;
    }
}
