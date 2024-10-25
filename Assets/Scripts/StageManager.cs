using UnityEngine;
using System.Collections.Generic;
using DataControl;
using Cysharp.Threading.Tasks;
using PoolControl;

public class StageManager : MonoBehaviour
{
    [SerializeField] private BubbleCreator bubbleCreator;
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private LayerMask bubbleLayer;
    [SerializeField] private PoolObjectKey popObjectKey;

    [SerializeField] private BubbleGlobalSetting bubbleGlobalSetting;

    private const int MinMatchCount = 3;
    private StageData _currentStage;
    private List<Bubble> _allBubbles;

    private void Awake()
    {
        _allBubbles = new List<Bubble>();
        bubbleShooter.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UniTaskEventManager.AddEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        EventManager.AddEvent<Bubble>(ActionEvent.CheckAndPopMatches, CheckAndPopMatches);
    }

    private void OnDisable()
    {
        UniTaskEventManager.RemoveEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        EventManager.RemoveEvent<Bubble>(ActionEvent.CheckAndPopMatches, CheckAndPopMatches);
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
                    Quaternion.identity);
            }
            
            _allBubbles.Add(bubble);

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
        foreach (var bubble in bubbles)
        {
            bubble.gameObject.SetActive(false);
            var popParticleObj = PoolObjectManager
                                 .Get<ParticleSystem>(popObjectKey, bubble.transform,
                                     bubble.transform.localScale).main;
            popParticleObj.startColor = new ParticleSystem.MinMaxGradient(bubble.GetColorForType());
            await UniTask.Delay(100, cancellationToken: destroyCancellationToken);
        }

        for (int i = bubbles.Count - 1; i >= 0; i--)
        {
            _allBubbles.Remove(bubbles[i]);
            Destroy(bubbles[i].gameObject);
        }
    }
}