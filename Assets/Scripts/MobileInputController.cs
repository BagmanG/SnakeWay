using UnityEngine;
using UnityEngine.UI;

public class MobileInputController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;

    private PlayerController playerController;

    public void Initialize(PlayerController player)
    {
        playerController = player;

        // Назначаем обработчики для кнопок
        upButton.onClick.AddListener(playerController.MoveUp);
        downButton.onClick.AddListener(playerController.MoveDown);
        leftButton.onClick.AddListener(playerController.MoveLeft);
        rightButton.onClick.AddListener(playerController.MoveRight);
    }

    private void OnDestroy()
    {
        // Важно очищать listeners при уничтожении объекта
        upButton.onClick.RemoveAllListeners();
        downButton.onClick.RemoveAllListeners();
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();
    }
}