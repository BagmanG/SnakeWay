using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Объект, вокруг которого вращаем камеру
    public float distance = 5.0f; // Начальное расстояние до объекта
    public float minDistance = 2.0f; // Минимальное расстояние
    public float maxDistance = 15.0f; // Максимальное расстояние

    public float xSpeed = 120.0f; // Скорость вращения по X
    public float ySpeed = 120.0f; // Скорость вращения по Y

    public float yMinLimit = -20f; // Ограничение угла по Y (вниз)
    public float yMaxLimit = 80f;  // Ограничение угла по Y (вверх)

    public float zoomSpeed = 10f; // Скорость зума
    public float zoomSmoothing = 5f; // Сглаживание зума

    public float rotationSmoothing = 8f; // Сглаживание вращения

    private float x = 0.0f; // Текущий угол X
    private float y = 0.0f; // Текущий угол Y
    private float currentDistance; // Текущее расстояние
    private float desiredDistance; // Желаемое расстояние

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
            // Вращение камеры при зажатой правой кнопке мыши
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                // Ограничиваем угол по Y
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // Обработка зума колесиком мыши
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                desiredDistance = Mathf.Clamp(desiredDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }

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

    // Метод для ограничения угла
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}