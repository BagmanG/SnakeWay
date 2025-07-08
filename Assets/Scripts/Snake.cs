using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public float segmentSize = 1f;
    public float bodyRadius = 0.3f;
    [Range(0, 1)] public float cornerSmoothing = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationLerpSpeed = 10f;

    [Header("References")]
    public GameObject headPrefab; // Префаб головы
    public Material snakeMaterial;

    private List<Transform> bodySegments = new List<Transform>();
    private Transform head;
    private List<Vector3> pathPoints = new List<Vector3>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private const int radialSegments = 12;
    private Vector3 lastGoodDirection = Vector3.forward;

    private void Start()
    {
        // Создаем голову из префаба
        if (headPrefab != null)
        {
            head = Instantiate(headPrefab, transform).transform;
            head.name = "Head";
        }
        else
        {
            Debug.LogError("Head prefab is not assigned!");
            return;
        }

        // Собираем все дочерние объекты как сегменты тела (исключая голову)
        foreach (Transform child in transform)
        {
            if (child != head)
            {
                bodySegments.Add(child);
            }
        }

        InitializePath();
        GenerateMesh();
    }

    private void InitializePath()
    {
        // Инициализируем путь на основе позиции головы и сегментов тела
        pathPoints.Clear();

        // Добавляем точку для головы
        pathPoints.Add(head.position);

        // Добавляем точки для сегментов тела
        for (int i = 0; i < bodySegments.Count; i++)
        {
            pathPoints.Add(bodySegments[i].position);

            // Добавляем промежуточную точку для плавности
            if (i < bodySegments.Count - 1)
            {
                pathPoints.Add(Vector3.Lerp(bodySegments[i].position, bodySegments[i + 1].position, 0.5f));
            }
        }
    }

    private void Update()
    {
        HandleInput();
        MoveSnake();
        UpdatePath();
        UpdateMesh();
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "SnakeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;
        mesh.MarkDynamic();
    }

    private void HandleInput()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (moveDirection != Vector3.back) ChangeDirection(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (moveDirection != Vector3.forward) ChangeDirection(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (moveDirection != Vector3.right) ChangeDirection(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (moveDirection != Vector3.left) ChangeDirection(Vector3.right);
        }
    }

    private void ChangeDirection(Vector3 newDirection)
    {
        lastGoodDirection = moveDirection;
        moveDirection = newDirection;
        targetPosition = head.position + moveDirection * segmentSize;
        isMoving = true;
    }

    private void MoveSnake()
    {
        if (!isMoving) return;

        // Плавное перемещение головы
        head.position = Vector3.MoveTowards(
            head.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Плавный поворот головы
        head.rotation = Quaternion.Lerp(
            head.rotation,
            Quaternion.LookRotation(moveDirection),
            rotationLerpSpeed * Time.deltaTime
        );

        if (Vector3.Distance(head.position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void UpdatePath()
    {
        // Добавляем новую точку пути с интервалом
        if (pathPoints.Count == 0 ||
            Vector3.Distance(head.position, pathPoints[0]) > segmentSize * 0.25f)
        {
            pathPoints.Insert(0, head.position);
        }

        // Ограничиваем длину пути
        while (pathPoints.Count > (bodySegments.Count + 1) * 2) // +1 для головы
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Обновляем позиции сегментов тела с интерполяцией
        for (int i = 0; i < bodySegments.Count; i++)
        {
            int targetIndex = Mathf.Min(i * 2 + 1, pathPoints.Count - 1); // +1 чтобы пропустить голову
            if (targetIndex >= 1 && targetIndex < pathPoints.Count)
            {
                bodySegments[i].position = Vector3.Lerp(
                    bodySegments[i].position,
                    pathPoints[targetIndex],
                    10f * Time.deltaTime
                );

                // Улучшенное определение направления
                Vector3 dir;
                if (targetIndex > 0)
                {
                    dir = (pathPoints[targetIndex - 1] - pathPoints[targetIndex]).normalized;
                }
                else
                {
                    dir = (head.position - bodySegments[i].position).normalized;
                }

                if (dir != Vector3.zero)
                {
                    bodySegments[i].rotation = Quaternion.Lerp(
                        bodySegments[i].rotation,
                        Quaternion.LookRotation(dir),
                        rotationLerpSpeed * Time.deltaTime
                    );
                }
            }
        }
    }

    private void UpdateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        if (pathPoints.Count < 2) return;

        // Создаем кольца вершин вдоль пути
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Quaternion rotation;
            Vector3 position = pathPoints[i];

            // Улучшенное определение направления для поворотов
            if (i == 0) // Первая точка (голова)
            {
                rotation = head.rotation;
            }
            else if (i < pathPoints.Count - 1) // Средние точки
            {
                Vector3 dirToPrev = (pathPoints[i - 1] - pathPoints[i]).normalized;
                Vector3 dirToNext = (pathPoints[i] - pathPoints[i + 1]).normalized;
                Vector3 averagedDir = Vector3.Lerp(dirToPrev, dirToNext, cornerSmoothing).normalized;
                rotation = Quaternion.LookRotation(averagedDir != Vector3.zero ? averagedDir : lastGoodDirection);
            }
            else // Последняя точка (хвост)
            {
                Vector3 dir = (pathPoints[i - 1] - pathPoints[i]).normalized;
                rotation = Quaternion.LookRotation(dir != Vector3.zero ? dir : lastGoodDirection);
            }

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(
                    Mathf.Cos(angle) * bodyRadius,
                    Mathf.Sin(angle) * bodyRadius,
                    0
                );

                vertices.Add(position + rotation * localPos - transform.position);
                uvs.Add(new Vector2(j / (float)(radialSegments - 1), i / (float)pathPoints.Count));
            }
        }

        // Создаем треугольники для меша
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                int currentBase = i * radialSegments;
                int nextBase = (i + 1) * radialSegments;

                triangles.Add(currentBase + j);
                triangles.Add(nextBase + j);
                triangles.Add(nextBase + nextJ);

                triangles.Add(currentBase + j);
                triangles.Add(nextBase + nextJ);
                triangles.Add(currentBase + nextJ);
            }
        }

        // Крышка на конце хвоста
        if (pathPoints.Count > 1)
        {
            int centerIndex = vertices.Count;
            vertices.Add(pathPoints[pathPoints.Count - 1] - transform.position);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int lastRingStart = (pathPoints.Count - 1) * radialSegments;
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                triangles.Add(centerIndex);
                triangles.Add(lastRingStart + nextJ);
                triangles.Add(lastRingStart + j);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}