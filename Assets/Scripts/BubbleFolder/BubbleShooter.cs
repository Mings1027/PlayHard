using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    [SerializeField] private float bubbleSpeed = 10f;

    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;

    [SerializeField] private float maxLineLength = 10f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask bubbleLayer;

    [SerializeField] private BubbleCreator bubbleCreator;
    [SerializeField] private Transform shooterPivot;
    [SerializeField] private Transform readyPivot;

    [SerializeField] private int maxBounces = 3;

    private float _safeAreaTopY;
    private LineRenderer _bubbleLine;
    private bool _isDragging;
    private Vector3 _shooterPosition;
    private List<Vector3> _bubbleLinePoints;
    private GameObject _previewBubble;
    private Bubble _activeBubble;
    private Bubble _readyBubble;
    private Vector3 _snapPosition;
    private bool _hasSnapPosition;
    private bool _isShooting;

    public void Init()
    {
        InitSafeArea();
        InitShooter();
        InitWall();
        CreateBubble().Forget();
        InitPreviewBubble();
    }

    private void InitSafeArea()
    {
        var safeArea = Screen.safeArea;
        var topSafeAreaViewport = new Vector3(0.5f, safeArea.yMax / Screen.height, 0);
        _safeAreaTopY = Camera.main.ViewportToWorldPoint(topSafeAreaViewport).y;
    }

    private void InitShooter()
    {
        _bubbleLine = GetComponent<LineRenderer>();
        _bubbleLine.startWidth = 0.1f;
        _bubbleLine.endWidth = 0.1f;
        _bubbleLine.material = new Material(Shader.Find("Sprites/Default"));

        _bubbleLinePoints = new List<Vector3>();

        var shooterY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.25f, 0)).y;
        _shooterPosition = new Vector3(0, shooterY, 0);
        transform.position = _shooterPosition;
    }

    private void InitWall()
    {
        var mainCamera = Camera.main;
        var leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
        var rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));

        leftWall.size = new Vector2(1, mainCamera.orthographicSize * 2);
        leftWall.transform.position = new Vector3(leftEdge.x - leftWall.size.x * 0.4f, 0, 0);

        rightWall.size = new Vector2(1, mainCamera.orthographicSize * 2);
        rightWall.transform.position = new Vector3(rightEdge.x + rightWall.size.x * 0.4f, 0, 0);
    }

    private void InitPreviewBubble()
    {
        _previewBubble = bubbleCreator.CreatePreviewBubble();
        _previewBubble.SetActive(false);
    }

    private void Update()
    {
        if (_isShooting) return;

        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            UpdateShootingLine();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging && _hasSnapPosition)
            {
                FireBubble().Forget();
            }

            _isDragging = false;
            _bubbleLine.positionCount = 0;
            if (_previewBubble.activeSelf) _previewBubble.SetActive(false);
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

        if (mousePos.y > shooterPivot.position.y)
        {
            var direction = ((Vector2)mousePos - (Vector2)shooterPivot.position).normalized;
            DrawBubbleLine(shooterPivot.position, direction);
        }
        else
        {
            _bubbleLine.positionCount = 0;
            if (_previewBubble.activeSelf) _previewBubble.SetActive(false);
            _hasSnapPosition = false;
        }
    }

    private void DrawBubbleLine(Vector3 startPos, Vector2 direction)
    {
        _bubbleLinePoints.Clear();
        _bubbleLinePoints.Add(startPos);
        _hasSnapPosition = false;

        var currentPos = startPos;
        var currentDir = direction;
        var remainingLength = maxLineLength;

        for (var i = 0; i < maxBounces; i++)
        {
            var wallHit = Physics2D.Raycast(currentPos, currentDir, maxLineLength, wallLayer);
            var bubbleHit = Physics2D.Raycast(currentPos, currentDir, maxLineLength, bubbleLayer);

            var distance = maxLineLength;
            if (wallHit.collider != null) distance = wallHit.distance;
            if (bubbleHit.collider != null && bubbleHit.distance < distance)
            {
                if (bubbleHit.point.y > _safeAreaTopY)
                {
                    _hasSnapPosition = false;
                    _bubbleLine.positionCount = 0;
                    _previewBubble.SetActive(false);
                    return;
                }

                _bubbleLinePoints.Add(bubbleHit.point);
                CalculateSnapPosition(bubbleHit);
                break;
            }

            Debug.DrawRay(currentPos, currentDir * maxLineLength, Color.yellow);

            if (wallHit.collider != null)
            {
                _bubbleLinePoints.Add(wallHit.point);
                currentDir = Vector2.Reflect(currentDir, wallHit.normal).normalized;
                currentPos = wallHit.point + currentDir * 0.01f;
            }
            else
            {
                var endPoint = currentPos + (Vector3)(currentDir * remainingLength);
                if (endPoint.y > _safeAreaTopY)
                {
                    _hasSnapPosition = false;
                    _bubbleLine.positionCount = 0;
                    _previewBubble.SetActive(false);
                    return;
                }

                _bubbleLinePoints.Add(endPoint);
                break;
            }
        }

        if (_hasSnapPosition)
        {
            if (_snapPosition.y > _safeAreaTopY)
            {
                _hasSnapPosition = false;
                _bubbleLine.positionCount = 0;
                _previewBubble.SetActive(false);
                return;
            }

            if (_isShooting)
            {
                _previewBubble.SetActive(false);
            }
            else
            {
                _previewBubble.transform.position = _snapPosition;
                _previewBubble.SetActive(true);
            }

            _bubbleLine.positionCount = _bubbleLinePoints.Count;
            _bubbleLine.SetPositions(_bubbleLinePoints.ToArray());
        }
        else
        {
            _previewBubble.SetActive(false);
            _bubbleLine.positionCount = 0;
        }
    }

    private void CalculateSnapPosition(RaycastHit2D hit)
    {
        var hitBubble = hit.collider.gameObject;
        var bubblePos = hitBubble.transform.position;
        var bubbleSize = bubbleCreator.BubbleSize;
        var verticalSpacing = bubbleSize * 0.866f;

        var hitDirection = (hit.point - (Vector2)bubblePos).normalized;
        var angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;
        var previewSnapPosition = angle switch
        {
            // 우
            >= 330 or < 90 => new Vector3(bubblePos.x + bubbleSize, bubblePos.y, 0),
            // 좌
            >= 90 and < 210 => new Vector3(bubblePos.x - bubbleSize, bubblePos.y, 0),
            // 좌 하단
            >= 210 and < 270 => new Vector3(bubblePos.x - bubbleSize * 0.5f, bubblePos.y - verticalSpacing, 0),
            // 우 하단
            >= 270 and < 330 => new Vector3(bubblePos.x + bubbleSize * 0.5f, bubblePos.y - verticalSpacing, 0),
            _ => Vector3.zero
        };

        if (previewSnapPosition.y > _safeAreaTopY)
        {
            _hasSnapPosition = false;
            return;
        }

        var bubbleHit = Physics2D.OverlapCircle(previewSnapPosition, bubbleSize * 0.4f, bubbleLayer);
        var wallHit = Physics2D.OverlapCircle(previewSnapPosition, bubbleSize * 0.4f, wallLayer);

        if (bubbleHit != null || wallHit != null)
        {
            _hasSnapPosition = false;
        }
        else
        {
            _snapPosition = previewSnapPosition;
            _hasSnapPosition = true;
        }
    }

    private async UniTask FireBubble()
    {
        if (!_hasSnapPosition || _isShooting) return;

        _isShooting = true;
        var bubble = _activeBubble;
        _activeBubble = null;
        bubble.transform.SetParent(null);

        var createBubbleTask = CreateBubble();

        for (var i = 0; i < _bubbleLinePoints.Count - 1; i++)
        {
            var startPos = _bubbleLinePoints[i];
            var endPos = _bubbleLinePoints[i + 1];
            var distance = Vector3.Distance(startPos, endPos);
            var duration = distance / bubbleSpeed;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / duration;
                bubble.transform.position = Vector3.Lerp(startPos, endPos, t);
                await UniTask.Yield();
            }
        }

        bubble.SetPosition(_snapPosition);
        EventManager.TriggerEvent(ActionEvent.AddBubble, bubble);
        EventManager.TriggerEvent(ActionEvent.CheckMatchingBubble, bubble);

        _isShooting = false;

        await UniTaskEventManager.TriggerAsync(UniTaskEvent.ElevateBubbleContainer);

        await createBubbleTask;
    }

    private async UniTask CreateBubble()
    {
        if (_readyBubble == null && _activeBubble == null)
        {
            // 대기 버블 생성
            _readyBubble = bubbleCreator.CreateRandomBubble(readyPivot.position, Quaternion.identity);
            bubbleCreator.DisableBubbleCollider(_readyBubble);

            // 활성 버블 생성
            _activeBubble = bubbleCreator.CreateRandomBubble(shooterPivot.position, Quaternion.identity);
            bubbleCreator.DisableBubbleCollider(_activeBubble);
        }
        // 활성 버블이 발사되어 없어진 경우
        else if (_activeBubble == null)
        {
            // 대기 버블을 활성 버블로 이동
            _activeBubble = _readyBubble;
            await MoveBubbleWithLerp(_activeBubble.transform, shooterPivot.position, 0.3f);

            // 새로운 대기 버블 생성
            _readyBubble = bubbleCreator.CreateRandomBubble(readyPivot.position, Quaternion.identity);
            bubbleCreator.DisableBubbleCollider(_readyBubble);
        }

        UpdateBubbleLineColor(_activeBubble);
    }


    private void UpdateBubbleLineColor(Bubble bubble)
    {
        var bubbleColor = bubble.GetColorForType();
        _bubbleLine.startColor = bubbleColor;
        _bubbleLine.endColor = bubbleColor;
    }

    private static async UniTask MoveBubbleWithLerp(Transform bubble, Vector3 endPos, float duration)
    {
        await bubble.DOMove(endPos, duration).SetEase(Ease.Linear);
        bubble.position = endPos; // 정확한 최종 위치 보장
    }

    private async UniTask SwapBubble()
    {
        _isShooting = true;
        var activePos = _activeBubble.transform.position;
        var readyPos = _readyBubble.transform.position;

        await DOTween.Sequence().Join(_activeBubble.transform.DOMove(readyPos, 0.3f))
                     .Join(_readyBubble.transform.DOMove(activePos, 0.3f));

        (_activeBubble, _readyBubble) = (_readyBubble, _activeBubble);

        var bubbleColor = _activeBubble.GetColorForType();
        _bubbleLine.startColor = bubbleColor;
        _bubbleLine.endColor = bubbleColor;
        _isShooting = false;
    }

    private void OnMouseUp()
    {
        if (_isShooting || _activeBubble == null || _readyBubble == null) return;
        SwapBubble().Forget();
    }
}