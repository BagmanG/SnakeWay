using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;

    public GameManager GameManager;
    public LevelManager LevelManager;
    public CameraController cameraController; // Ссылка на контроллер камеры

    public event Action OnMoveComplete;

    private void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isMoving = false;

                CheckSnakeCollision();
                CheckStarCollision(); // Добавляем проверку звезды
                CheckTileAfterMovement();
                OnMoveComplete?.Invoke();
            }
            return;
        }

        // Получаем направление движения относительно камеры
        Vector3Int moveDirection = GetCameraRelativeGridDirection();

        if (moveDirection != Vector3Int.zero)
        {
            Move(moveDirection);
        }
    }

    private void CheckStarCollision()
    {
        // Получаем все объекты со StarGiver на сцене
        StarGiver[] stars = FindObjectsOfType<StarGiver>();
        if (stars == null || stars.Length == 0) return;

        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        foreach (StarGiver star in stars)
        {
            Vector2Int starPos = new Vector2Int(
                Mathf.RoundToInt(star.transform.position.x),
                Mathf.RoundToInt(star.transform.position.z)
            );

            if (playerPos == starPos)
            {
                star.GivePlayer();
                break;
            }
        }
    }
    private bool CheckIceSlide(Vector3Int direction)
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Проверяем, стоим ли мы на льду (ID 7)
        if (LevelManager.CurrentLevel.grid[currentPos.x, currentPos.y] != 7)
            return false;

        // Используем цикл вместо рекурсии для проверки скольжения
        while (true)
        {
            // Проверяем следующий блок в направлении движения
            Vector2Int nextPos = currentPos + new Vector2Int(direction.x, direction.z);

            // Если следующий блок за пределами уровня или не проходимый - останавливаемся
            if (nextPos.x < 0 || nextPos.x >= LevelManager.CurrentLevel.width ||
                nextPos.y < 0 || nextPos.y >= LevelManager.CurrentLevel.height ||
                (LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y] != 0 &&
                 LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y] != 3 &&
                 LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y] != 4 &&
                 LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y] != 7))
            {
                return false;
            }

            // Если следующий блок не лед - это конечная точка скольжения
            if (LevelManager.CurrentLevel.grid[nextPos.x, nextPos.y] != 7)
            {
                return true;
            }

            // Продолжаем проверять следующий блок
            currentPos = nextPos;
        }
    }
    private void CheckSnakeCollision()
    {
        // Получаем все объекты змей на сцене
        Snake[] snakes = FindObjectsOfType<Snake>();
        if (snakes == null || snakes.Length == 0) return;

        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        foreach (Snake snake in snakes)
        {
            // Проверяем столкновение с головой змеи
            Vector2Int headPos = new Vector2Int(
                Mathf.RoundToInt(snake.transform.GetChild(0).position.x),
                Mathf.RoundToInt(snake.transform.GetChild(0).position.z)
            );

            if (playerPos == headPos)
            {
                GameManager.GameOver();
                return;
            }

            // Проверяем столкновение с хвостом змеи
            for (int i = 1; i < snake.transform.childCount; i++)
            {
                Transform segment = snake.transform.GetChild(i);
                Vector2Int segmentPos = new Vector2Int(
                    Mathf.RoundToInt(segment.position.x),
                    Mathf.RoundToInt(segment.position.z)
                );

                if (playerPos == segmentPos)
                {
                    GameManager.GameOver();
                    return;
                }
            }
        }
    }

    private void CheckTileAfterMovement()
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Проверяем, стоит ли игрок на элементе с ID 4 и есть ли звезды
        if (LevelManager.CurrentLevel.grid[currentPos.x, currentPos.y] == 4 &&
            GameManager.StarsCount > 0)
        {
            GameManager.LevelCompleted();
        }
    }

    Vector3Int GetCameraRelativeGridDirection()
    {
        // Получаем базовые направления камеры (без наклона вверх/вниз)
        Vector3 forward = cameraController.transform.forward;
        Vector3 right = cameraController.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Определяем, какое направление выбрано (WASD)
        bool moveForward = Input.GetKeyDown(KeyCode.W);
        bool moveBackward = Input.GetKeyDown(KeyCode.S);
        bool moveLeft = Input.GetKeyDown(KeyCode.A);
        bool moveRight = Input.GetKeyDown(KeyCode.D);

        // Выбираем ближайшую ось (X или Z) для дискретного движения
        if (moveForward || moveBackward)
        {
            // Движение вперед/назад по наиболее выраженной оси камеры
            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
                return new Vector3Int((int)Mathf.Sign(forward.x), 0, 0) * (moveForward ? 1 : -1);
            else
                return new Vector3Int(0, 0, (int)Mathf.Sign(forward.z)) * (moveForward ? 1 : -1);
        }
        else if (moveLeft || moveRight)
        {
            // Движение влево/вправо по наиболее выраженной оси камеры
            if (Mathf.Abs(right.x) > Mathf.Abs(right.z))
                return new Vector3Int((int)Mathf.Sign(right.x), 0, 0) * (moveRight ? 1 : -1);
            else
                return new Vector3Int(0, 0, (int)Mathf.Sign(right.z)) * (moveRight ? 1 : -1);
        }

        return Vector3Int.zero;
    }

    void Move(Vector3Int direction)
    {
        Vector3 newPosition = transform.position + direction;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.z));

        if (gridPos.x >= 0 && gridPos.x < LevelManager.CurrentLevel.width &&
            gridPos.y >= 0 && gridPos.y < LevelManager.CurrentLevel.height &&
            (LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 0 ||
             LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 3 ||
             LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 4 ||
             LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 7))
        {
            targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            targetPosition = newPosition;
            isMoving = true;

            // Если мы движемся на лед, проверяем скольжение
            if (LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 7)
            {
                StartCoroutine(CheckSlideAfterMove(direction));
            }
        }
    }

    private IEnumerator CheckSlideAfterMove(Vector3Int direction)
    {
        // Ждем завершения текущего движения
        while (isMoving)
        {
            yield return null;
        }

        // Проверяем, нужно ли скользить дальше
        if (CheckIceSlide(direction))
        {
            // Добавляем небольшую задержку для визуального эффекта
            yield return new WaitForSeconds(0.1f);

            // Продолжаем движение в том же направлении
            Move(direction);
        }
    }
}