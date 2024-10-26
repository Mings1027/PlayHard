using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DataControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolControl;

public class StageManager : MonoBehaviour
{
    [SerializeField] private BubbleCreator bubbleCreator;
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private LayerMask bubbleLayer;
    [SerializeField] private PoolObjectKey popObjectKey;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveThreshold = 0.1f;

    private const int MinMatchCount = 3;
    private StageData _currentStage;
    private List<Bubble> _allBubbles;
    private List<Bubble> _markedForDestroy;
    private HashSet<BubbleType> _existingBubbles;
    private bool _isMoving;

    private void Awake()
    {
        _allBubbles = new List<Bubble>();
        _markedForDestroy = new List<Bubble>();
        _existingBubbles = new HashSet<BubbleType>();
        bubbleShooter.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UniTaskEventManager.AddEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        UniTaskEventManager.AddEvent(UniTaskEvent.ElevateBubbleContainer, ElevateBubbleContainer);
        UniTaskEventManager.AddEvent<List<Bubble>>(UniTaskEvent.PopBubbles, PopBubbles);

        EventManager.AddEvent<Bubble>(ActionEvent.CheckAndPopMatches, CheckAndPopMatches);
        EventManager.AddEvent<Bubble>(ActionEvent.AddBubble, AddBubble);

        FuncManager.AddEvent(FuncEvent.AllBubbles, () => _allBubbles);
        FuncManager.AddEvent(FuncEvent.ExistingBubbleType, () => _existingBubbles);
    }

    private void OnDisable()
    {
        UniTaskEventManager.RemoveEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        UniTaskEventManager.RemoveEvent(UniTaskEvent.ElevateBubbleContainer, ElevateBubbleContainer);
        UniTaskEventManager.RemoveEvent<List<Bubble>>(UniTaskEvent.PopBubbles, PopBubbles);

        EventManager.RemoveEvent<Bubble>(ActionEvent.CheckAndPopMatches, CheckAndPopMatches);
        EventManager.RemoveEvent<Bubble>(ActionEvent.AddBubble, AddBubble);

        FuncManager.RemoveEvent(FuncEvent.AllBubbles, () => _allBubbles);
        FuncManager.RemoveEvent(FuncEvent.ExistingBubbleType, () => _existingBubbles);
    }

    private async UniTask CreateStage(StageData stageData)
    {
        _currentStage = stageData;
        bubbleShooter.Init();
        await CreateBubblesFromPositions();
        bubbleShooter.gameObject.SetActive(true);
    }

    private async UniTask CreateBubblesFromPositions()
    {
        for (var i = 0; i < _currentStage.BubbleDataPositions.Count; i++)
        {
            var bubblePosition = _currentStage.BubbleDataPositions[i];
            var worldPosition = CalculateBubblePosition(bubblePosition.bubblePosition);

            Bubble bubble;
            if (bubblePosition.bubbleData.IsRandomBubble)
            {
                bubble = bubbleCreator.CreateRandomBubble(worldPosition, Quaternion.identity);
            }
            else
            {
                bubble = bubbleCreator.CreateBubble(bubblePosition.bubbleData.BubbleType, worldPosition,
                    Quaternion.identity, bubblePosition.specialBubbleData);
            }

            bubble.transform.SetParent(bubbleContainer);

            _allBubbles.Add(bubble);
            _existingBubbles.Add(bubble.Type);

            await UniTask.Yield(destroyCancellationToken);
        }
    }

    private Vector3 CalculateBubblePosition(Vector2Int point)
    {
        var containerTopPosition = bubbleContainer.position + Vector3.up * (_currentStage.height / 2f);

        var bubbleSize = bubbleCreator.BubbleSize;
        var bubbleRadius = bubbleSize / 2f;
        var verticalSpacing = bubbleSize * 0.866f;

        containerTopPosition.y -= bubbleRadius;

        var bubblePosition = containerTopPosition;
        var xOffset = point.y % 2 == 1 ? bubbleSize * 0.5f : 0f;
        bubblePosition.x += (point.x - (_currentStage.Width - 1) / 2f) * bubbleSize + xOffset;
        bubblePosition.y -= point.y * verticalSpacing;

        return bubblePosition;
    }

    private void CheckAndPopMatches(Bubble currentBubble)
    {
        var matchingBubbles = FindMatchingBubbles(currentBubble);
        if (matchingBubbles.Count > 0)
        {
            PopMatchingBubbles(matchingBubbles).Forget();
        }
    }

    private List<Bubble> FindMatchingBubbles(Bubble startBubble)
    {
        var visited = new HashSet<Bubble>();
        var matchingBubbles = new List<Bubble>();
        var bubbleType = startBubble.Type;

        FloodFill(startBubble, bubbleType, visited, matchingBubbles);

        if (matchingBubbles.Count < MinMatchCount)
        {
            matchingBubbles.Clear();
        }

        return matchingBubbles;
    }

    private void FloodFill(Bubble bubble, BubbleType targetType, HashSet<Bubble> visited,
                           List<Bubble> matchingBubbles)
    {
        if (bubble == null || visited.Contains(bubble) || bubble.Type != targetType)
            return;

        visited.Add(bubble);
        matchingBubbles.Add(bubble);

        var neighbors = GetNeighborBubbles(bubble);
        foreach (var neighbor in neighbors)
        {
            FloodFill(neighbor, targetType, visited, matchingBubbles);
        }
    }

    private List<Bubble> GetNeighborBubbles(Bubble bubble)
    {
        var neighbors = new List<Bubble>();
        var bubbleSize = bubble.transform.localScale.x;
        var verticalSpacing = bubbleSize * 0.866f;

        var directions = new Vector2[]
        {
            new(bubbleSize, 0),
            new(bubbleSize * 0.5f, verticalSpacing),
            new(-bubbleSize * 0.5f, verticalSpacing),
            new(-bubbleSize, 0),
            new(-bubbleSize * 0.5f, -verticalSpacing),
            new(bubbleSize * 0.5f, -verticalSpacing)
        };

        foreach (var direction in directions)
        {
            var checkPosition = (Vector2)bubble.transform.position + direction;
            var hit = Physics2D.OverlapCircle(checkPosition, bubbleSize * 0.4f, bubbleLayer);
            if (hit != null && hit.TryGetComponent(out Bubble hitBubble))
            {
                neighbors.Add(hitBubble);
            }
        }

        return neighbors;
    }

    private async UniTask PopMatchingBubbles(List<Bubble> bubbles)
    {
        for (var i = 0; i < bubbles.Count; i++)
        {
            _allBubbles.Remove(bubbles[i]);
            _existingBubbles.Remove(bubbles[i].Type);
        }

        await PopBubbles(bubbles);

        for (var i = 0; i < bubbles.Count; i++)
        {
            bubbles[i].ExecuteSpecialEffect();
        }
    }

    private async UniTask PopBubbles(List<Bubble> bubbles)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            var bubble = bubbles[i];
            bubble.gameObject.SetActive(false);
            var popParticleObj = PoolObjectManager
                                 .Get<ParticleSystem>(popObjectKey, bubble.transform,
                                     bubble.transform.localScale).main;
            popParticleObj.startColor = new ParticleSystem.MinMaxGradient(bubble.GetColorForType());
            _markedForDestroy.Add(bubble);

            await UniTask.Delay(100, cancellationToken: destroyCancellationToken);
        }
    }

    private void AddBubble(Bubble bubble)
    {
        _allBubbles.Add(bubble);
        _existingBubbles.Add(bubble.Type);
        bubble.transform.SetParent(bubbleContainer);
    }

    private async UniTask ElevateBubbleContainer()
    {
        if (_allBubbles.Count == 0) return;
        var lowestBubble = _allBubbles.Min(b => b.transform.position.y);
        _isMoving = true;

        if (lowestBubble < 0)
        {
            // 아래쪽 버블이 화면 밖으로 나갔을 때 위로 올림
            await bubbleContainer.DOMoveY(bubbleContainer.position.y + Mathf.Abs(lowestBubble), 0.2f)
                                 .SetEase(Ease.OutQuint);
        }
        else if (lowestBubble > 0)
        {
            // 가장 낮은 버블이 y=0 위치까지만 내려가도록 조정
            await bubbleContainer.DOMoveY(bubbleContainer.position.y - lowestBubble, 0.2f)
                                 .SetEase(Ease.OutQuint);
        }

        _isMoving = false;
    }

    private void EndStage()
    {
        for (int i = _markedForDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(_markedForDestroy[i].gameObject);
        }
    }
}