using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public int initialLength = 5;
    public float segmentSize = 1f;
    public float bodyRadius = 0.3f;
    [Range(0, 1)] public float cornerSmoothing = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationLerpSpeed = 10f;

    [Header("References")]
    public GameObject headPrefab;
    public Material snakeMaterial;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> pathPoints = new List<Vector3>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private const int radialSegments = 12; // Увеличено для плавности
    private Vector3 lastGoodDirection = Vector3.forward;

    private void Start()
    {
        InitializeSnake();
        GenerateMesh();
    }

    private void Update()
    {
        HandleInput();
        MoveSnake();
        UpdatePath();
        UpdateMesh();
    }

    private void InitializeSnake()
    {
        // Create head
        GameObject head = Instantiate(headPrefab, transform);
        head.name = "Head";
        segments.Add(head.transform);

        // Initialize path with extra points
        for (int i = 0; i < initialLength * 2; i++)
        {
            pathPoints.Add(transform.position - Vector3.forward * i * segmentSize * 0.5f);
        }

        // Create body segments
        for (int i = 1; i < initialLength; i++)
        {
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(transform);
            segment.transform.position = pathPoints[i * 2];
            segments.Add(segment.transform);
        }
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "SnakeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;
        mesh.MarkDynamic(); // Для частых обновлений
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
        // Сохраняем последнее хорошее направление
        lastGoodDirection = moveDirection;
        moveDirection = newDirection;
        targetPosition = segments[0].position + moveDirection * segmentSize;
        isMoving = true;
    }

    private void MoveSnake()
    {
        if (!isMoving) return;

        // Плавное перемещение головы
        segments[0].position = Vector3.MoveTowards(
            segments[0].position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Плавный поворот головы
        segments[0].rotation = Quaternion.Lerp(
            segments[0].rotation,
            Quaternion.LookRotation(moveDirection),
            rotationLerpSpeed * Time.deltaTime
        );

        if (Vector3.Distance(segments[0].position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void UpdatePath()
    {
        // Добавляем точку пути с интервалом
        if (pathPoints.Count == 0 ||
            Vector3.Distance(segments[0].position, pathPoints[0]) > segmentSize * 0.25f)
        {
            pathPoints.Insert(0, segments[0].position);
        }

        // Ограничиваем длину пути
        while (pathPoints.Count > initialLength * 4)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Обновляем позиции сегментов с интерполяцией
        for (int i = 1; i < segments.Count; i++)
        {
            int targetIndex = Mathf.Min(i * 2, pathPoints.Count - 1);
            if (targetIndex >= 1)
            {
                segments[i].position = Vector3.Lerp(
                    segments[i].position,
                    pathPoints[targetIndex],
                    10f * Time.deltaTime
                );

                Vector3 dir = (pathPoints[targetIndex - 1] - pathPoints[targetIndex]).normalized;
                if (dir != Vector3.zero)
                {
                    segments[i].rotation = Quaternion.Lerp(
                        segments[i].rotation,
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

        // Создаем кольца вершин вдоль пути (без изменений)
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Quaternion rotation;
            Vector3 position = pathPoints[i];

            if (i < pathPoints.Count - 1)
            {
                Vector3 dir = (pathPoints[i] - pathPoints[i + 1]).normalized;
                rotation = Quaternion.LookRotation(dir != Vector3.zero ? dir : lastGoodDirection);
            }
            else
            {
                rotation = segments[segments.Count - 1].rotation;
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

        // Измененный порядок вершин для вывернутых нормалей
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                int currentBase = i * radialSegments;
                int nextBase = (i + 1) * radialSegments;

                // Обратный порядок вершин (было 1-2-3, стало 1-3-2)
                triangles.Add(currentBase + j);
                triangles.Add(nextBase + j);
                triangles.Add(nextBase + nextJ);

                triangles.Add(currentBase + j);
                triangles.Add(nextBase + nextJ);
                triangles.Add(currentBase + nextJ);
            }
        }

        // Вывернутая крышка на конце хвоста
        if (pathPoints.Count > 1)
        {
            int centerIndex = vertices.Count;
            vertices.Add(pathPoints[pathPoints.Count - 1] - transform.position);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int lastRingStart = (pathPoints.Count - 1) * radialSegments;
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                // Обратный порядок вершин
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