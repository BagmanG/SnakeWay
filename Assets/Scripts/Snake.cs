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
    private List<Vector3> turnPoints = new List<Vector3>();
    private const int radialSegments = 8;

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

        // Initialize path
        for (int i = 0; i < initialLength; i++)
        {
            pathPoints.Add(transform.position - Vector3.forward * i * segmentSize);
        }

        // Create body segments
        for (int i = 1; i < initialLength; i++)
        {
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(transform);
            segment.transform.position = pathPoints[i];
            segments.Add(segment.transform);
        }
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "SnakeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;
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
        moveDirection = newDirection;
        targetPosition = segments[0].position + moveDirection * segmentSize;
        isMoving = true;
        turnPoints.Add(segments[0].position);
    }

    private void MoveSnake()
    {
        if (!isMoving) return;

        segments[0].position = Vector3.MoveTowards(
            segments[0].position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        segments[0].rotation = Quaternion.LookRotation(moveDirection);

        if (Vector3.Distance(segments[0].position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void UpdatePath()
    {
        // Add new path point if head moved enough
        if (pathPoints.Count == 0 ||
            Vector3.Distance(segments[0].position, pathPoints[0]) > segmentSize * 0.5f)
        {
            pathPoints.Insert(0, segments[0].position);
        }

        // Update body segments to follow path
        for (int i = 1; i < segments.Count; i++)
        {
            if (i < pathPoints.Count)
            {
                segments[i].position = pathPoints[i];

                // Calculate direction to next segment
                if (i < pathPoints.Count - 1)
                {
                    Vector3 dir = (pathPoints[i - 1] - pathPoints[i]).normalized;
                    if (dir != Vector3.zero)
                        segments[i].rotation = Quaternion.LookRotation(dir);
                }
            }
        }

        // Cleanup old path points
        while (pathPoints.Count > segments.Count * 2)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
        }

        // Cleanup old turn points
        for (int i = turnPoints.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(turnPoints[i], segments[segments.Count - 1].position) > segmentSize * segments.Count)
            {
                turnPoints.RemoveAt(i);
            }
        }
    }

    private void UpdateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Create circular cross-sections along the path
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 position = pathPoints[i];
            Quaternion rotation = i < segments.Count ? segments[i].rotation : segments[segments.Count - 1].rotation;

            // Adjust rotation at turn points for smooth corners
            if (i > 0 && i < pathPoints.Count - 1 && cornerSmoothing > 0)
            {
                Vector3 prevDir = (pathPoints[i - 1] - pathPoints[i]).normalized;
                Vector3 nextDir = (pathPoints[i] - pathPoints[i + 1]).normalized;

                if (Vector3.Angle(prevDir, nextDir) > 10f)
                {
                    rotation = Quaternion.LookRotation(Vector3.Lerp(prevDir, nextDir, 0.5f));
                }
            }

            // Create vertices for this cross-section
            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(
                    Mathf.Cos(angle) * bodyRadius,
                    Mathf.Sin(angle) * bodyRadius,
                    0
                );

                vertices.Add(position + rotation * localPos - transform.position);
                uvs.Add(new Vector2(j / (float)radialSegments, i / (float)pathPoints.Count));
            }
        }

        // Create triangles between cross-sections
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ = (j + 1) % radialSegments;
                int currentBase = i * radialSegments;
                int nextBase = (i + 1) * radialSegments;

                triangles.Add(currentBase + j);
                triangles.Add(currentBase + nextJ);
                triangles.Add(nextBase + j);

                triangles.Add(currentBase + nextJ);
                triangles.Add(nextBase + nextJ);
                triangles.Add(nextBase + j);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Count == 0) return;

        // Draw path
        Gizmos.color = Color.blue;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }

        // Draw turn points
        Gizmos.color = Color.red;
        foreach (var point in turnPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}