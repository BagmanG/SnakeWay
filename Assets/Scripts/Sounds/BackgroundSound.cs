using UnityEngine;

public class BackgroundSound : MonoBehaviour
{
    private static BackgroundSound _instance;

    void Awake()
    {
        // Проверяем, существует ли уже экземпляр
        if (_instance != null && _instance != this)
        {
            // Если да, уничтожаем новый объект
            Destroy(gameObject);
        }
        else
        {
            // Если нет, сохраняем текущий экземпляр
            _instance = this;
            DontDestroyOnLoad(gameObject); // Делаем объект неуничтожаемым
        }
    }
}