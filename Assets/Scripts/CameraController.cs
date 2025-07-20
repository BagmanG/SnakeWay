using UnityEngine;
using YG;

public class CameraController : MonoBehaviour
{
    public Transform target; // Объект, вокруг которого вращаем камеру
    public float distance = 5.0f; // Начальное расстояние до объекта
    public float minDistance = 2.0f; // Минимальное расстояние
    public float maxDistance = 15.0f; // Максимальное расстояние

    [Header("Rotation Settings")]
    public float xSpeed = 120.0f; // Скорость вращения по X (для компьютера)
    public float ySpeed = 120.0f; // Скорость вращения по Y (для компьютера)

    [Space]
    public float mobileXSpeed = 60.0f; // Скорость вращения по X (для мобильных)
    public float mobileYSpeed = 60.0f; // Скорость вращения по Y (для мобильных)

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f; // Скорость зума колесиком мыши

    [Header("Angle Limits")]
    public float yMinLimit = -20f; // Ограничение угла по Y (вниз)
    public float yMaxLimit = 80f;  // Ограничение угла по Y (вверх)

    [Header("Smoothing")]
    public float zoomSmoothing = 5f; // Сглаживание зума
    public float rotationSmoothing = 8f; // Сглаживание вращения

    private float x = 0.0f; // Текущий угол X
    private float y = 0.0f; // Текущий угол Y
    private float currentDistance; // Текущее расстояние
    private float desiredDistance; // Желаемое расстояние

    private Vector2? lastMousePosition; // Для отслеживания предыдущей позиции мыши/тача
    private int? touchId; // Для отслеживания конкретного тача

    void Start()
    {
        // Инициализация углов
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        currentDistance = distance;
        desiredDistance = distance;

        // Если target не назначен, попробуем найти объект с тегом "Player"
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            HandleRotationInput();
            HandleZoomInput();

            // Плавное изменение расстояния
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomSmoothing);

            // Вычисляем новую позицию камеры
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance) + target.position;

            // Плавное вращение и перемещение
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSmoothing);
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * rotationSmoothing);
        }
    }

    private void HandleRotationInput()
    {
        // Обработка ввода для компьютера (мышь)
        if (YG2.envir.isDesktop)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                if (lastMousePosition.HasValue)
                {
                    Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition.Value;
                    x += delta.x * xSpeed * 0.02f;
                    y -= delta.y * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                }
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lastMousePosition = null;
            }
        }
        // Обработка ввода для мобильных устройств (тач)
        else
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchId = touch.fingerId;
                    lastMousePosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved && touchId == touch.fingerId)
                {
                    if (lastMousePosition.HasValue)
                    {
                        Vector2 delta = touch.position - lastMousePosition.Value;
                        // Используем меньшую скорость для мобильных устройств
                        x += delta.x * mobileXSpeed * 0.02f;
                        y -= delta.y * mobileYSpeed * 0.02f;
                        y = ClampAngle(y, yMinLimit, yMaxLimit);
                    }
                    lastMousePosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended && touchId == touch.fingerId)
                {
                    touchId = null;
                    lastMousePosition = null;
                }
            }
        }
    }

    private void HandleZoomInput()
    {
        if (YG2.envir.isDesktop)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                desiredDistance = Mathf.Clamp(desiredDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }
        }
    }

    // Публичные методы для изменения дистанции
    public void ZoomIn()
    {
        desiredDistance = Mathf.Clamp(desiredDistance - 1f, minDistance, maxDistance);
    }

    public void ZoomOut()
    {
        desiredDistance = Mathf.Clamp(desiredDistance + 1f, minDistance, maxDistance);
    }

    // Метод для ограничения угла
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}