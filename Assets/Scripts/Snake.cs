using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    public float followSmoothness = 10f;

    [Header("References")]
    public GameObject headPrefab;
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
        if (headPrefab == null)
        {
            Debug.LogError("Head prefab is not assigned!");
            return;
        }

        // Create head
        head = Instantiate(headPrefab, transform).transform;
        head.name = "Head";

        // Get and sort body segments
        bodySegments = GetComponentsInChildren<Transform>()
            .Where(t => t != transform && t != head && t.name.Contains("Segment"))
            .OrderBy(t => t.name)
            .ToList();

        InitializePath();
        GenerateMesh();
    }

    private void InitializePath()
    {
        pathPoints.Clear();

        // Initialize path with head position
        pathPoints.Add(head.position);

        // Add body segments positions with intermediate points
        for (int i = 0; i < bodySegments.Count; i++)
        {
            pathPoints.Add(bodySegments[i].position);

            // Add extra point between segments for smoothness
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

        if (Input.GetKeyDown(KeyCode.W) && moveDirection != Vector3.back)
        {
            ChangeDirection(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S) && moveDirection != Vector3.forward)
        {
            ChangeDirection(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.A) && moveDirection != Vector3.right)
        {
            ChangeDirection(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) && moveDirection != Vector3.left)
        {
            ChangeDirection(Vector3.right);
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

        // Move head
        head.position = Vector3.MoveTowards(head.position, targetPosition, moveSpeed * Time.deltaTime);
        head.rotation = Quaternion.Lerp(head.rotation, Quaternion.LookRotation(moveDirection), rotationLerpSpeed * Time.deltaTime);

        if (Vector3.Distance(head.position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void UpdatePath()
    {
        // Add new path point when head moves far enough
        if (pathPoints.Count == 0 || Vector3.Distance(head.position, pathPoints[0]) > segmentSize * 0.25f)
        {
            pathPoints.Insert(0, head.position);
        }

        // Maintain path length (2 points per segment + buffer)
        int maxPathPoints = (bodySegments.Count + 1) * 3;
        while (pathPoints.Count > maxPathPoints)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Update body segments positions
        for (int i = 0; i < bodySegments.Count; i++)
        {
            int targetIndex = Mathf.Clamp((i + 1) * 2, 0, pathPoints.Count - 1);

            bodySegments[i].position = Vector3.Lerp(
                bodySegments[i].position,
                pathPoints[targetIndex],
                followSmoothness * Time.deltaTime
            );

            // Calculate direction
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

    private void UpdateMesh()
    {
        if (pathPoints.Count < 2) return;

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Create vertex rings along the path
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Quaternion rotation;
            Vector3 position = pathPoints[i];

            if (i == 0) // Head
            {
                rotation = head.rotation;
            }
            else if (i < pathPoints.Count - 1) // Body
            {
                Vector3 dirToPrev = (pathPoints[i - 1] - pathPoints[i]).normalized;
                Vector3 dirToNext = (pathPoints[i] - pathPoints[i + 1]).normalized;
                Vector3 smoothedDir = Vector3.Lerp(dirToPrev, dirToNext, cornerSmoothing).normalized;
                rotation = Quaternion.LookRotation(smoothedDir != Vector3.zero ? smoothedDir : lastGoodDirection);
            }
            else // Tail
            {
                Vector3 dir = (pathPoints[i - 1] - pathPoints[i]).normalized;
                rotation = Quaternion.LookRotation(dir != Vector3.zero ? dir : lastGoodDirection);
            }

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * bodyRadius, Mathf.Sin(angle) * bodyRadius, 0);
                vertices.Add(position + rotation * localPos - transform.position);
                uvs.Add(new Vector2(j / (float)(radialSegments - 1), i / (float)pathPoints.Count));
            }
        }

        // Create triangles
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

        // Create tail cap
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

        // Update mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}