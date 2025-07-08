using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SmoothSnake : MonoBehaviour
{
    [Header("Snake Settings")]
    public int initialLength = 5;
    public float segmentSize = 1f;
    public float bodyWidth = 0.3f;
    public float bodyHeight = 0.2f;
    public Color snakeColor = Color.green;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float turnSmoothness = 2f;

    [Header("References")]
    public GameObject headPrefab;
    public Material snakeMaterial;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> segmentPositions = new List<Vector3>();
    private List<Quaternion> segmentRotations = new List<Quaternion>();
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.forward;
    private Vector3 targetPosition;
    private Vector3 lastMoveDirection;
    private float bodyRadius = 0.15f;

    private void Start()
    {
        InitializeSnake();
        GenerateMesh();
    }

    private void Update()
    {
        HandleInput();
        MoveSnake();
        UpdateMesh();
    }

    private void InitializeSnake()
    {
        // Create head
        GameObject head = Instantiate(headPrefab, transform);
        head.name = "Head";
        segments.Add(head.transform);
        segmentPositions.Add(transform.position);
        segmentRotations.Add(transform.rotation);

        // Create body segments
        for (int i = 1; i < initialLength; i++)
        {
            GameObject segment = new GameObject($"Segment_{i}");
            segment.transform.SetParent(transform);
            segment.transform.localPosition = new Vector3(0, 0, -i * segmentSize);
            segments.Add(segment.transform);
            segmentPositions.Add(segment.transform.position);
            segmentRotations.Add(segment.transform.rotation);
        }
    }

    private void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = snakeMaterial;

        // Create circular cross-sections for smooth bends
        int radialSegments = 12; // More segments = smoother bends
        float lengthPerSegment = segmentSize;

        for (int i = 0; i < initialLength; i++)
        {
            float t = i / (float)(initialLength - 1);
            float radius = Mathf.Lerp(bodyWidth * 0.5f, bodyWidth * 0.3f, t);

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * bodyHeight,
                    -i * lengthPerSegment
                );
                vertices.Add(pos);
            }
        }

        // Create triangles between segments
        for (int i = 0; i < initialLength - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = i * radialSegments + j;
                int next = i * radialSegments + (j + 1) % radialSegments;
                int below = (i + 1) * radialSegments + j;
                int belowNext = (i + 1) * radialSegments + (j + 1) % radialSegments;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(belowNext);

                triangles.Add(current);
                triangles.Add(belowNext);
                triangles.Add(below);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void HandleInput()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Vector3.forward;
            StartMovement();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Vector3.back;
            StartMovement();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Vector3.left;
            StartMovement();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Vector3.right;
            StartMovement();
        }
    }

    private void StartMovement()
    {
        targetPosition = segments[0].position + moveDirection * segmentSize;
        isMoving = true;
        lastMoveDirection = moveDirection;
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

        // Smooth rotation for head
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            segments[0].rotation = Quaternion.Slerp(
                segments[0].rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Check if head reached target
        if (Vector3.Distance(segments[0].position, targetPosition) < 0.01f)
        {
            isMoving = false;
            UpdateBodyPositions();
        }

        // Update body segments with smooth following
        for (int i = 1; i < segments.Count; i++)
        {
            Vector3 targetPos = segmentPositions[i - 1];
            Vector3 direction = (targetPos - segments[i].position).normalized;

            // Smooth movement
            segments[i].position = Vector3.Lerp(
                segments[i].position,
                targetPos - direction * segmentSize * 0.9f,
                turnSmoothness * Time.deltaTime
            );

            // Smooth rotation
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                segments[i].rotation = Quaternion.Slerp(
                    segments[i].rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void UpdateBodyPositions()
    {
        // Shift positions
        for (int i = segmentPositions.Count - 1; i > 0; i--)
        {
            segmentPositions[i] = segmentPositions[i - 1];
        }
        segmentPositions[0] = segments[0].position;

        // Shift rotations
        for (int i = segmentRotations.Count - 1; i > 0; i--)
        {
            segmentRotations[i] = segmentRotations[i - 1];
        }
        segmentRotations[0] = segments[0].rotation;
    }

    private void UpdateMesh()
    {
        // Update vertices based on current segment positions and rotations
        int radialSegments = 12;
        float lengthPerSegment = segmentSize;

        for (int i = 0; i < segments.Count; i++)
        {
            float t = i / (float)(segments.Count - 1);
            float radius = Mathf.Lerp(bodyWidth * 0.5f, bodyWidth * 0.3f, t);

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * Mathf.PI * 2f / radialSegments;
                Vector3 localPos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * bodyHeight,
                    0
                );

                // Transform vertex position according to segment transform
                int vertexIndex = i * radialSegments + j;
                vertices[vertexIndex] = segments[i].TransformPoint(localPos) - transform.position;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void OnDrawGizmos()
    {
        if (segments == null || segments.Count == 0) return;

        // Draw segment positions
        Gizmos.color = Color.red;
        for (int i = 0; i < segments.Count; i++)
        {
            Gizmos.DrawSphere(segments[i].position, 0.1f);
            if (i > 0)
            {
                Gizmos.DrawLine(segments[i - 1].position, segments[i].position);
            }
        }
    }
}