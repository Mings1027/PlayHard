using UnityEngine;
using System.Collections.Generic;
using DataControl;
using Cysharp.Threading.Tasks;
using System.Linq;
using DG.Tweening;

public class StageController : MonoBehaviour
{
    [SerializeField] private StageData stageData;
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private RectTransform bubbleContainer;

    [Header("Movement Settings")] [SerializeField]
    private float moveSpeed = 10f; // 초당 이동 거리

    [SerializeField] private Ease moveEase = Ease.OutQuad;

    private Dictionary<int, List<Vector2Int>> _pathPositions;
    private Dictionary<int, List<Bubble>> _pathBubbles; // 각 경로별 버블 리스트
    private Dictionary<Vector2, Bubble> _positionToBubble;
    private readonly List<Bubble> _activeBubbles = new();
    private float _bubbleSize;

    private void Awake()
    {
        bubbleContainer.anchorMin = new Vector2(0.5f, 1f);
        bubbleContainer.anchorMax = new Vector2(0.5f, 1f);
        bubbleContainer.pivot = new Vector2(0.5f, 1f);
        bubbleContainer.anchoredPosition = Vector2.zero;

        _bubbleSize = bubblePrefab.GetComponent<RectTransform>().rect.width;
        _pathPositions = new Dictionary<int, List<Vector2Int>>();
        _pathBubbles = new Dictionary<int, List<Bubble>>();
        _positionToBubble = new Dictionary<Vector2, Bubble>();
    }

    private void OnEnable()
    {
        EventManager.AddEvent<int, int>(ActionEvent.SelectStage, CreateInitialBubblesAsync);
        EventManager.AddEvent(ActionEvent.CreateStage, () => _ = CreateStageAsync());
        
        FuncManager.AddEvent<Vector2, Bubble>(ActionEvent.GetBubbleAtPoint, GetBubbleAtPoint);
    }

    private void Start()
    {
        InitializeStage();
    }

    private void OnDisable()
    {
        EventManager.RemoveEvent<int, int>(ActionEvent.SelectStage, CreateInitialBubblesAsync);
        EventManager.RemoveEvent(ActionEvent.CreateStage, () => _ = CreateStageAsync());
        
        FuncManager.RemoveEvent<Vector2, Bubble>(ActionEvent.GetBubbleAtPoint, GetBubbleAtPoint);
    }

    private Bubble GetBubbleAtPoint(Vector2 point)
    {
        return _positionToBubble.GetValueOrDefault(point);
    }

    private void InitializeStage()
    {
        if (stageData == null)
        {
            Debug.LogError("StageData is not assigned!");
            return;
        }

        ClearActiveBubbles();
        InitPathPositions();
        InitPathBubbles();
    }

    private void InitPathBubbles()
    {
        _pathBubbles.Clear();
        for (int i = 0; i < stageData.bubblePaths.Count; i++)
        {
            _pathBubbles[i] = new List<Bubble>();
        }
    }

    private void ClearActiveBubbles()
    {
        for (var i = 0; i < _activeBubbles.Count; i++)
        {
            var bubble = _activeBubbles[i];
            if (bubble != null)
            {
                var position = bubble.GetComponent<RectTransform>().anchoredPosition;
                _positionToBubble.Remove(position);
                Destroy(bubble.gameObject);
            }
        }

        _activeBubbles.Clear();
        _pathBubbles?.Clear();
        _positionToBubble.Clear();
    }

    private void InitPathPositions()
    {
        _pathPositions.Clear();
        for (int i = 0; i < stageData.bubblePaths.Count; i++)
        {
            var positions = new List<Vector2Int>();
            for (int j = 0; j < stageData.bubblePaths[i].points.Count; j++)
            {
                var bubblePosition = GetBubblePosition(stageData.bubblePaths[i].points[j].x,
                    stageData.bubblePaths[i].points[j].y);
                positions.Add(bubblePosition);
            }

            _pathPositions[i] = positions;
        }
    }

    // Stage시작 시 호출
    private async UniTask CreateStageAsync()
    {
        EventManager.TriggerEvent(ActionEvent.InitBubbleShooter, stageData.totalBubbles);

        var pathTasks = new UniTask[stageData.bubblePaths.Count];
        for (var pathIndex = 0; pathIndex < stageData.bubblePaths.Count; pathIndex++)
        {
            pathTasks[pathIndex] = CreatePathBubblesAsync(pathIndex);
        }

        await UniTask.WhenAll(pathTasks);
    }

    private async UniTask CreatePathBubblesAsync(int pathIndex)
    {
        var path = stageData.bubblePaths[pathIndex];
        for (int j = 0; j < path.points.Count; j++)
        {
            await CreateAndMoveBubblesAsync(pathIndex);
        }
    }

    private async void CreateInitialBubblesAsync(int pathIndex, int count)
    {
        if (pathIndex >= stageData.bubblePaths.Count)
        {
            Debug.LogWarning($"Path index {pathIndex} is out of range.");
            return;
        }

        var path = stageData.bubblePaths[pathIndex];
        count = Mathf.Min(count, path.points.Count);

        for (int i = 0; i < count; i++)
        {
            await CreateAndMoveBubblesAsync(pathIndex);
        }
    }

    private async UniTask CreateAndMoveBubblesAsync(int pathIndex)
    {
        var path = stageData.bubblePaths[pathIndex];
        if (path.points.Count == 0) return;

        // 1. 첫 번째 위치에 새 버블 생성
        var newBubble = CreateBubbleAtPosition(path.points[0], pathIndex);

        // 2. 현재 경로의 모든 버블을 한 칸씩 뒤로 이동
        if (_pathBubbles[pathIndex].Count > 0)
        {
            var moveTasks = new List<UniTask>();
            var bubbles = _pathBubbles[pathIndex].ToList(); // 현재 상태의 버블 리스트 복사

            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                int nextIndex = i + 1;
                if (nextIndex < path.points.Count)
                {
                    var bubble = bubbles[i];
                    var nextPosition = GetBubblePosition(path.points[nextIndex].x, path.points[nextIndex].y);
                    moveTasks.Add(MoveBubbleAsync(bubble, nextPosition));
                }
                else
                {
                    // 경로를 벗어나는 버블은 제거
                    var bubble = bubbles[i];
                    _pathBubbles[pathIndex].Remove(bubble);
                    _activeBubbles.Remove(bubble);
                    Destroy(bubble.gameObject);
                }
            }

            // 모든 이동이 완료될 때까지 대기
            await UniTask.WhenAll(moveTasks);
        }

        // 3. 새 버블을 경로의 버블 리스트 맨 앞에 추가
        _pathBubbles[pathIndex].Insert(0, newBubble);
    }

    private async UniTask MoveBubbleAsync(Bubble bubble, Vector2Int targetPosition)
    {
        var rectTransform = bubble.GetComponent<RectTransform>();
        var startPosition = rectTransform.anchoredPosition;
        var endPosition = new Vector2(targetPosition.x, targetPosition.y);

        _positionToBubble.Remove(startPosition);

        var distance = Vector2.Distance(startPosition, endPosition);
        var duration = distance / (moveSpeed * _bubbleSize);

        await rectTransform.DOAnchorPos(endPosition, duration).SetEase(moveEase);

        _positionToBubble[endPosition] = bubble;
    }

    private Bubble CreateBubbleAtPosition(Vector2Int position, int pathIndex)
    {
        var bubblePosition = GetBubblePosition(position.x, position.y);

        var newBubble = Instantiate(bubblePrefab, bubbleContainer);
        var bubbleRect = newBubble.GetComponent<RectTransform>();
        bubbleRect.anchoredPosition = bubblePosition;

        newBubble.SetPathIndex(pathIndex);
        _activeBubbles.Add(newBubble);
        _positionToBubble[bubbleRect.anchoredPosition] = newBubble;

        return newBubble;
    }

    public void SetStageData(StageData newStageData)
    {
        stageData = newStageData;
        InitializeStage();
    }

    private Vector2Int GetBubblePosition(int x, int y)
    {
        float xPos;

        if (y % 2 == 0)
        {
            xPos = (x - 5) * _bubbleSize;
        }
        else
        {
            xPos = (x - 4.5f) * _bubbleSize;
        }

        var yPos = -(_bubbleSize / 2 + y * (_bubbleSize * 0.866f));

        return new Vector2Int((int)xPos, (int)yPos);
    }
}