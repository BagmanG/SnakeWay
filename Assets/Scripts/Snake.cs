using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Snake : MonoBehaviour
{
    [Header("Snake Settings")]
    public int initialLength = 5;
    public float segmentSize = 1f;
    public float bodyWidth = 0.3f;
    public float bodyHeight = 0.2f;
    public Color snakeColor = Color.green;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 30f;

    [Header("References")]
    public GameObject headPrefab;
    public Material snakeMaterial;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> positions = new List<Vector3>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private List<Vector3> pathPoints = new List<Vector3>();

    private void Start()
    {
        InitializeSnake();
        GenerateMesh();
    }

    private void Update()
    {
        HandleInput();
        MoveSnake();
        UpdateBodyPositions();
        UpdateMesh();
    }

    private void InitializeSnake()
    {
        // Create head
        GameObject head = Instantiate(headPrefab, transform);
        head.name = "Head";
        segments.Add(head.transform);
        positions.Add(transform.position);

        // Create initial path points
        for (int i = 0; i < initialLength; i++)
        {
            pathPoints.Add(transform.position - Vector3.forward * i * segmentSize);
        }

        // Create body segments
        for (int i = 1; i < initialLength; i++)
        {
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(transform);
            segment.transform.position = transform.position - Vector3.forward * i * segmentSize;
            segments.Add(segment.transform);
            positions.Add(segment.transform.position);
        }
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;

        // Create vertices for straight segments only
        for (int i = 0; i < initialLength; i++)
        {
            AddStraightSegmentVertices(i);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void AddStraightSegmentVertices(int segmentIndex)
    {
        float width = bodyWidth;
        float height = bodyHeight;

        // Front face vertices
        vertices.Add(new Vector3(-width / 2, -height / 2, -segmentIndex * segmentSize));
        vertices.Add(new Vector3(width / 2, -height / 2, -segmentIndex * segmentSize));
        vertices.Add(new Vector3(width / 2, height / 2, -segmentIndex * segmentSize));
        vertices.Add(new Vector3(-width / 2, height / 2, -segmentIndex * segmentSize));

        // Create triangles for straight segments only
        if (segmentIndex > 0)
        {
            int baseIndex = segmentIndex * 4;

            // Front face
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);

            // Connect to previous segment (straight connection)
            triangles.Add(baseIndex - 4);
            triangles.Add(baseIndex - 3);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex - 3);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex);

            triangles.Add(baseIndex - 3);
            triangles.Add(baseIndex - 2);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex - 2);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);

            triangles.Add(baseIndex - 2);
            triangles.Add(baseIndex - 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex - 1);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 2);

            triangles.Add(baseIndex - 1);
            triangles.Add(baseIndex - 4);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex - 4);
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 3);
        }
    }

    private void HandleInput()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (moveDirection != Vector3.back)
                ChangeDirection(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (moveDirection != Vector3.forward)
                ChangeDirection(Vector3.back);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (moveDirection != Vector3.right)
                ChangeDirection(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (moveDirection != Vector3.left)
                ChangeDirection(Vector3.right);
        }
    }

    private void ChangeDirection(Vector3 newDirection)
    {
        moveDirection = newDirection;
        targetPosition = segments[0].position + moveDirection * segmentSize;
        isMoving = true;

        // Add turn point
        pathPoints.Insert(0, segments[0].position);
    }

    private void MoveSnake()
    {
        if (!isMoving) return;

        // Move head
        segments[0].position = Vector3.MoveTowards(
            segments[0].position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Instant 90° rotation
        segments[0].rotation = Quaternion.LookRotation(moveDirection);

        if (Vector3.Distance(segments[0].position, targetPosition) < 0.01f)
        {
            isMoving = false;
            positions[0] = segments[0].position;
        }
    }

    private void UpdateBodyPositions()
    {
        // Update body segments with exact spacing
        for (int i = 1; i < segments.Count; i++)
        {
            // Find target position in path
            int targetIndex = Mathf.Min(i, pathPoints.Count - 1);
            if (targetIndex >= 0 && targetIndex < pathPoints.Count)
            {
                segments[i].position = pathPoints[targetIndex];

                // Only rotate if direction changed
                if (targetIndex > 0 && targetIndex < pathPoints.Count - 1)
                {
                    Vector3 dir = (pathPoints[targetIndex - 1] - pathPoints[targetIndex]).normalized;
                    if (dir != Vector3.zero)
                        segments[i].rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }

    private void UpdateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        // Create vertices for each segment
        for (int i = 0; i < segments.Count; i++)
        {
            float width = bodyWidth;
            float height = bodyHeight;

            // Get segment transform
            Vector3 pos = segments[i].position - transform.position;
            Quaternion rot = segments[i].rotation;

            // Add vertices for this segment
            vertices.Add(pos + rot * new Vector3(-width / 2, -height / 2, 0));
            vertices.Add(pos + rot * new Vector3(width / 2, -height / 2, 0));
            vertices.Add(pos + rot * new Vector3(width / 2, height / 2, 0));
            vertices.Add(pos + rot * new Vector3(-width / 2, height / 2, 0));

            // Create triangles if not first segment
            if (i > 0)
            {
                int baseIndex = i * 4;

                // Front face
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);

                // Connect to previous segment
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);

                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 2);

                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 3);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (segments == null || segments.Count == 0) return;

        // Draw path points
        Gizmos.color = Color.blue;
        foreach (var point in pathPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        // Draw segment connections
        Gizmos.color = Color.red;
        for (int i = 1; i < segments.Count; i++)
        {
            Gizmos.DrawLine(segments[i - 1].position, segments[i].position);
        }
    }
}