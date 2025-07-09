using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f; 

    private Vector3 targetPosition;
    private Quaternion targetRotation; 
    private bool isMoving = false;

    public GameManager GameManager;
    public LevelManager LevelManager;

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
        targetRotation = Quaternion.LookRotation(direction);
        targetPosition = transform.position + direction;
        isMoving = true;
    }
}