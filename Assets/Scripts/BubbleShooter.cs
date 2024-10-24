using System.Collections.Generic;
using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    private LineRenderer _bubbleLine;
    private bool _isDragging;
    private Vector3 _shooterPosition;

    [SerializeField] private float bubbleSpeed = 10f;
    [SerializeField] private float bubbleDetectionRadius = 0.5f;

    [SerializeField] private BoxCollider2D bubbleContainer;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;

    [SerializeField] private float maxLineLength = 10f;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask bubbleLayer;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform shooterPivotParent;
    [SerializeField] private Transform shooterPivot;

    private void Awake()
    {
        InitShooter();
        InitWall();
        CreateBubble();
    }

    private void InitShooter()
    {
        _bubbleLine = GetComponent<LineRenderer>();
        _bubbleLine.startWidth = 0.1f;
        _bubbleLine.endWidth = 0.1f;
        _bubbleLine.material = new Material(Shader.Find("Sprites/Default"));

        var shooterY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.25f, 0)).y;
        _shooterPosition = new Vector3(0, shooterY, 0);
        shooterPivotParent.position = _shooterPosition;
    }

    private void InitWall()
    {
        var mainCamera = Camera.main;
        var leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
        var rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));

        leftWall.transform.position = new Vector3(leftEdge.x, 0, 0);
        leftWall.size = new Vector2(1, mainCamera.orthographicSize * 2);

        rightWall.transform.position = new Vector3(rightEdge.x, 0, 0);
        rightWall.size = new Vector2(1, mainCamera.orthographicSize * 2);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            UpdateShootingLine();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            _bubbleLine.positionCount = 0;
        }

        if (_isDragging)
        {
            UpdateShootingLine();
        }
    }

    private void UpdateShootingLine()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        var direction = ((Vector2)mousePos - (Vector2)shooterPivot.position).normalized;
        DrawBubbleLine(shooterPivot.position, direction);
    }

    private void DrawBubbleLine(Vector3 startPos, Vector2 direction)
    {
        var points = new List<Vector3>();
        points.Add(startPos);

        var currentPos = startPos;
        var currentDir = direction;
        var remainingLength = maxLineLength;

        for (int i = 0; i < maxBounces; i++)
        {
            var hit = Physics2D.Raycast(currentPos, currentDir, maxLineLength, wallLayer);

            Debug.DrawRay(currentPos, currentDir * maxLineLength, Color.yellow);
            if (hit.collider != null && hit.distance > 0.01f)
            {
                points.Add(hit.point);
                currentDir = Vector2.Reflect(currentDir, hit.normal).normalized;
                currentPos = hit.point + (currentDir * 0.01f);
                remainingLength -= hit.distance;

                if (remainingLength <= 0.01f) break;

                // 히트 포인트 디버그
                Debug.Log($"Hit point {i}: {hit.point}, Normal: {hit.normal}");
            }
            else
            {
                points.Add(currentPos + (Vector3)(currentDir * remainingLength));
                break;
            }
        }

        // 최소 2개의 점이 있는지 확인
        if (points.Count < 2)
        {
            points.Add(startPos + (Vector3)(direction * maxLineLength));
        }

        _bubbleLine.positionCount = points.Count;
        _bubbleLine.SetPositions(points.ToArray());
    }

    private void CreateBubble()
    {
        var bubble = Instantiate(bubblePrefab, shooterPivot);
        var bubbleSize = bubbleContainer.size.x / 11;
        bubble.transform.position = shooterPivot.position;
        bubble.transform.localScale = Vector3.one * bubbleSize;
        var bubbleColor = bubble.GetComponent<Bubble>().GetColorForType();
        _bubbleLine.startColor = bubbleColor;
        _bubbleLine.endColor = bubbleColor;
    }
}