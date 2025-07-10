using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public float segmentSize = 1f;
    public float headRadius = 0.3f;
    public float tailRadius = 0.1f;
    [Range(0, 1)] public float cornerSmoothing = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationLerpSpeed = 10f;
    public float followSmoothness = 10f;

    [Header("References")]
    public Material snakeMaterial;

    private List<Transform> bodySegments = new List<Transform>();
    private Transform head;
    private List<Vector3> pathPoints = new List<Vector3>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private const int radialSegments = 12;
    private Vector3 lastGoodDirection = Vector3.forward;
    private float[] segmentRadii;
    private List<Vector3> segmentVelocities;
    public GameManager GameManager;
    public LevelManager LevelManager;

    // Добавляем переменные для поиска пути
    private PathFinder pathFinder;
    private List<Vector2Int> currentPath;
    private int currentPathIndex;
    private Vector2Int lastPlayerPosition;

    public void InitSnake()
    {
        // Инициализация пути
        pathFinder = new PathFinder(LevelManager.CurrentLevel);

        // Находим голову (первый дочерний объект)
        if (transform.childCount == 0)
        {
            Debug.LogError("No child objects found for snake!");
            return;
        }

        head = transform.GetChild(0);
        head.name = "Head";

        // Находим все сегменты (дочерние объекты с "Segment" в имени)
        bodySegments = new List<Transform>();
        for (int i = 1; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Segment"))
            {
                bodySegments.Add(child);
            }
        }

        // Сортируем сегменты по имени
        bodySegments = bodySegments.OrderBy(t => t.name).ToList();

        // Инициализируем скорости для плавного движения
        segmentVelocities = new List<Vector3>();
        for (int i = 0; i < bodySegments.Count; i++)
        {
            segmentVelocities.Add(Vector3.zero);
        }

        // Вычисляем радиусы для каждого сегмента
        CalculateSegmentRadii();

        InitializePath();
        GenerateMesh();
    }

    private void CalculateSegmentRadii()
    {
        segmentRadii = new float[bodySegments.Count + 1];
        segmentRadii[0] = headRadius; // Head radius

        if (bodySegments.Count > 0)
        {
            float radiusStep = (headRadius - tailRadius) / bodySegments.Count;

            for (int i = 1; i < segmentRadii.Length; i++)
            {
                segmentRadii[i] = headRadius - radiusStep * i;
            }
        }
    }

    private void InitializePath()
    {
        pathPoints.Clear();
        pathPoints.Add(head.localPosition); // Head position (0,0,0)

        // Position body segments with proper spacing based on their actual positions
        for (int i = 0; i < bodySegments.Count; i++)
        {
            pathPoints.Add(bodySegments[i].localPosition);
        }
    }

    private void Update()
    {
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

    public void MakeMove(Vector2Int playerPosition)
    {
        if (isMoving) return;

        Vector2Int snakeHeadPos = new Vector2Int(
            Mathf.RoundToInt(head.position.x),
            Mathf.RoundToInt(head.position.z)
        );

        // Всегда пересчитываем путь, чтобы учитывать новые препятствия
        lastPlayerPosition = playerPosition;

        // Получаем позиции хвоста как препятствия
        List<Vector2Int> tailPositions = new List<Vector2Int>();
        foreach (Transform segment in bodySegments)
        {
            tailPositions.Add(new Vector2Int(
                Mathf.RoundToInt(segment.position.x),
                Mathf.RoundToInt(segment.position.z)
            ));
        }

        // Ищем путь к игроку
        currentPath = pathFinder.FindPath(snakeHeadPos, playerPosition, tailPositions);
        currentPathIndex = 0;

        // Если путь найден - двигаемся по нему
        if (currentPath != null && currentPath.Count > 0)
        {
            Vector2Int nextPos = currentPath[0];
            Vector3 direction = new Vector3(nextPos.x - snakeHeadPos.x, 0, nextPos.y - snakeHeadPos.y);
            ChangeDirection(direction.normalized);
        }
        else
        {
            // Если пути нет - остаемся на месте
            isMoving = false;
            Debug.Log("Нет возможного пути, змея остается на месте");
        }
    }

    private void CheckStarCollision()
    {
        // Получаем все объекты со StarGiver на сцене
        StarGiver[] stars = FindObjectsOfType<StarGiver>();
        if (stars == null || stars.Length == 0) return;

        Vector2Int headPos = new Vector2Int(
            Mathf.RoundToInt(head.position.x),
            Mathf.RoundToInt(head.position.z)
        );

        foreach (StarGiver star in stars)
        {
            Vector2Int starPos = new Vector2Int(
                Mathf.RoundToInt(star.transform.position.x),
                Mathf.RoundToInt(star.transform.position.z)
            );

            if (headPos == starPos)
            {
                star.GiveSnake();
                break;
            }
        }
    }


    private void ChangeDirection(Vector3 newDirection)
    {
        Vector2Int newPos = new Vector2Int(
            Mathf.RoundToInt(head.position.x + newDirection.x * segmentSize),
            Mathf.RoundToInt(head.position.z + newDirection.z * segmentSize)
        );

        if (!pathFinder.IsWalkable(newPos))
        {
            isMoving = false;
            return;
        }

        lastGoodDirection = moveDirection;
        moveDirection = newDirection;
        targetPosition = head.localPosition + moveDirection * segmentSize;
        isMoving = true;
    }

    private void MoveSnake()
    {
        // Update head rotation based on movement direction
        if (moveDirection != Vector3.zero)
        {
            head.rotation = Quaternion.Lerp(
                head.rotation,
                Quaternion.LookRotation(moveDirection),
                rotationLerpSpeed * Time.deltaTime
            );
        }

        if (!isMoving) return;

        // Move head
        head.localPosition = Vector3.MoveTowards(head.localPosition, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(head.localPosition, targetPosition) < 0.01f)
        {
            isMoving = false;
            CheckStarCollision(); // Добавляем проверку звезды
        }
    }

    private void UpdatePath()
    {
        // Only add new point when head has moved enough
        if (pathPoints.Count == 0 || Vector3.Distance(head.localPosition, pathPoints[0]) > segmentSize * 0.9f)
        {
            pathPoints.Insert(0, head.localPosition);
        }

        // Maintain path length
        int maxPathPoints = bodySegments.Count + 1;
        while (pathPoints.Count > maxPathPoints)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Update body segments with smooth movement
        for (int i = 0; i < bodySegments.Count; i++)
        {
            int targetIndex = Mathf.Min(i + 1, pathPoints.Count - 1);
            Vector3 targetPos = pathPoints[targetIndex];

            Vector3 currentVelocity = segmentVelocities[i];
            bodySegments[i].localPosition = Vector3.SmoothDamp(
                bodySegments[i].localPosition,
                targetPos,
                ref currentVelocity,
                1f / followSmoothness
            );
            segmentVelocities[i] = currentVelocity;

            // Calculate direction with smoothing
            Vector3 dir;
            if (targetIndex > 0)
            {
                dir = (pathPoints[targetIndex - 1] - pathPoints[targetIndex]).normalized;

                // Apply corner smoothing for non-tail segments
                if (targetIndex < pathPoints.Count - 1)
                {
                    Vector3 nextDir = (pathPoints[targetIndex] - pathPoints[targetIndex + 1]).normalized;
                    dir = Vector3.Lerp(dir, nextDir, cornerSmoothing).normalized;
                }
            }
            else
            {
                dir = (head.localPosition - bodySegments[i].localPosition).normalized;
            }

            if (dir != Vector3.zero)
            {
                bodySegments[i].rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    private void UpdateMesh()
    {
        // Use actual positions for smooth mesh
        List<Vector3> meshPoints = new List<Vector3>();
        meshPoints.Add(head.localPosition);
        for (int i = 0; i < bodySegments.Count; i++)
        {
            meshPoints.Add(bodySegments[i].localPosition);
        }

        if (meshPoints.Count < 2) return;

        vertices.Clear();
        triangles.Clear();

        // Create vertex rings along the path
        for (int i = 0; i < meshPoints.Count; i++)
        {
            Quaternion rotation;
            Vector3 position = meshPoints[i];
            float currentRadius = segmentRadii[Mathf.Min(i, segmentRadii.Length - 1)];

            if (i == 0) // Head
            {
                rotation = head.rotation;
            }
            else
            {
                Vector3 dir = (meshPoints[i - 1] - meshPoints[i]).normalized;

                if (i < meshPoints.Count - 1)
                {
                    Vector3 nextDir = (meshPoints[i] - meshPoints[i + 1]).normalized;
                    dir = Vector3.Lerp(dir, nextDir, cornerSmoothing).normalized;
                }

                rotation = Quaternion.LookRotation(dir != Vector3.zero ? dir : lastGoodDirection);
            }

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * currentRadius, Mathf.Sin(angle) * currentRadius, 0);
                vertices.Add(position + rotation * localPos);
            }
        }

        // Create triangles
        for (int i = 0; i < meshPoints.Count - 1; i++)
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

        // Create tail cap
        int centerIndex = vertices.Count;
        vertices.Add(meshPoints[meshPoints.Count - 1]);

        int lastRingStart = (meshPoints.Count - 1) * radialSegments;
        for (int j = 0; j < radialSegments; j++)
        {
            int nextJ = (j + 1) % radialSegments;
            triangles.Add(centerIndex);
            triangles.Add(lastRingStart + nextJ);
            triangles.Add(lastRingStart + j);
        }

        // Update mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}