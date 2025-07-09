using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;

    public GameManager GameManager;
    public LevelManager LevelManager;

    // —обытие дл€ уведомлени€ о завершении хода
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

                // ”ведомл€ем GameManager о завершении хода
                OnMoveComplete?.Invoke();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Move(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector3.right);
        }
    }

    void Move(Vector3 direction)
    {
        // ѕровер€ем, можно ли двигатьс€ в этом направлении
        Vector3 newPosition = transform.position + direction;
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.z));

        if (gridPos.x >= 0 && gridPos.x < LevelManager.CurrentLevel.width &&
            gridPos.y >= 0 && gridPos.y < LevelManager.CurrentLevel.height &&
            LevelManager.CurrentLevel.grid[gridPos.x, gridPos.y] == 0)
        {
            targetRotation = Quaternion.LookRotation(direction);
            targetPosition = newPosition;
            isMoving = true;
        }
    }
}