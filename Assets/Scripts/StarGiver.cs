using UnityEngine;

public class StarGiver : MonoBehaviour
{
    public void GivePlayer()
    {
        Debug.Log("Игрок взял звезду");
    }

    public void GiveSnake()
    {
        Destroy(gameObject);
    }
}
