using UnityEngine;

public class StarGiver : MonoBehaviour
{
    public GameObject StarActive;
    public GameObject StarPassive;
    private GameManager GameManager;
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();   
    }

    public void GivePlayer()
    {
        GameManager.GivePlayerStar();
        HideStar();
    }

    public void GiveSnake()
    {
        HideStar();
    }

    public void HideStar()
    {
        StarActive.SetActive(false);
        StarPassive.SetActive(true);
    }
}
