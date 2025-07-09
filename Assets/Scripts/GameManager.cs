using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LevelManager LevelManager;
    public void Start()
    {
        LevelManager.LoadLevel();
    }
}
