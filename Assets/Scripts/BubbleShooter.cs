using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private Transform shooterPivotParent;
    [SerializeField] private Transform shooterPivot;
    [SerializeField] private Transform readyPivot;

    [SerializeField] private int maxBounces = 3;

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
        InitShooter();
        InitWall();
        CreateBubble().Forget();
        InitPreviewBubble();
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
        shooterPivotParent.position = _shooterPosition;
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

        for (int i = 0; i < maxBounces; i++)
        {
            var wallHit = Physics2D.Raycast(currentPos, currentDir, maxLineLength, wallLayer);
            var bubbleHit = Physics2D.Raycast(currentPos, currentDir, maxLineLength, bubbleLayer);

            var distance = maxLineLength;
            if (wallHit.collider != null) distance = wallHit.distance;
            if (bubbleHit.collider != null && bubbleHit.distance < distance)
            {
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
                _bubbleLinePoints.Add(currentPos + (Vector3)(currentDir * remainingLength));
                break;
            }
        }

        if (_hasSnapPosition)
        {
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

        // hit.point가 hitBubble.x보다 크면 오른쪽아래
        // hit.point가 hitBubble.x보다 작으면 왼쪽아래
        var previewSnapPosition = Vector3.zero;

        if (angle >= 330 || angle < 60)
            previewSnapPosition = new Vector3(bubblePos.x + bubbleSize, bubblePos.y, 0);
        else if (angle >= 120 && angle < 210)
            previewSnapPosition = new Vector3(bubblePos.x - bubbleSize, bubblePos.y, 0);
        else if (angle >= 210 && angle < 270)
            previewSnapPosition = new Vector3(bubblePos.x - bubbleSize * 0.5f, bubblePos.y - verticalSpacing, 0);
        else if (angle >= 270 && angle < 330)
            previewSnapPosition = new Vector3(bubblePos.x + bubbleSize * 0.5f, bubblePos.y - verticalSpacing, 0);

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

        for (int i = 0; i < _bubbleLinePoints.Count - 1; i++)
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

        bubble.transform.position = _snapPosition;
        bubble.GetComponent<Collider2D>().enabled = true;
        EventManager.TriggerEvent(ActionEvent.AddBubble, bubble);
        EventManager.TriggerEvent(ActionEvent.CheckAndPopMatches, bubble);

        _isShooting = false;

        await UniTaskEventManager.TriggerAsync(UniTaskEvent.ElevateBubbleContainer);

        await createBubbleTask;
    }

    private async UniTask CreateBubble()
    {
        // 두 버블 모두 없는 경우 (게임 시작 시)
        if (_readyBubble == null && _activeBubble == null)
        {
            // 대기 버블 생성
            _readyBubble = bubbleCreator.CreateRandomBubble(shooterPivot.position, Quaternion.identity);
            _readyBubble.transform.position = readyPivot.position;
            _readyBubble.GetComponent<Collider2D>().enabled = false;

            // 활성 버블 생성
            _activeBubble = bubbleCreator.CreateRandomBubble(shooterPivot.position, Quaternion.identity);
            _activeBubble.transform.position = shooterPivot.position;
            _activeBubble.GetComponent<Collider2D>().enabled = false;

            // 라인 렌더러 색상 설정
            var bubbleColor = _activeBubble.GetColorForType();
            _bubbleLine.startColor = bubbleColor;
            _bubbleLine.endColor = bubbleColor;
        }
        // 활성 버블이 발사되어 없어진 경우
        else if (_activeBubble == null)
        {
            // 대기 버블을 활성 버블로 이동
            _activeBubble = _readyBubble;
            await MoveBubbleWithLerp(_activeBubble.transform,
                readyPivot.position,
                shooterPivot.position,
                0.3f); // 이동 시간 0.3초

            // 새로운 대기 버블 생성
            _readyBubble = bubbleCreator.CreateRandomBubble(shooterPivot.position, Quaternion.identity);
            _readyBubble.transform.position = readyPivot.position;
            _readyBubble.GetComponent<Collider2D>().enabled = false;

            // 라인 렌더러 색상 업데이트
            var bubbleColor = _activeBubble.GetColorForType();
            _bubbleLine.startColor = bubbleColor;
            _bubbleLine.endColor = bubbleColor;
        }
    }

    private async UniTask MoveBubbleWithLerp(Transform bubble, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            bubble.position = Vector3.Lerp(startPos, endPos, progress);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        bubble.position = endPos; // 정확한 최종 위치 보장
    }
}