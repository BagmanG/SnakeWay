using UnityEngine;

public class StarGiver : MonoBehaviour
{
    public GameObject StarActive;
    public GameObject StarPassive;
    private GameManager GameManager;
    private bool Gived = false;
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();   
    }

    public void GivePlayer()
    {
        if (Gived) return;
        Gived = true;
        GameManager.GivePlayerStar();
        HideStar();
    }

    public void GiveSnake()
    {
        if (Gived) return;
        Gived = true;
        HideStar();
    }

    public void HideStar()
    {
        StarActive.SetActive(false);
        StarPassive.SetActive(true);
    }
}
