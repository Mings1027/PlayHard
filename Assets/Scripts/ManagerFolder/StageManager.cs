using UnityEngine;
using System.Collections.Generic;
using DataControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HelperFolder;

public class StageManager : MonoBehaviour
{
    [SerializeField] private BubbleCreator bubbleCreator;
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private LayerMask bubbleLayer;

    private StageData _currentStage;
    private List<Bubble> _allBubbles;
    private List<Bubble> _visibleBubbles;
    private List<Bubble> _markedForDestroy;

    private List<Bubble> _normalBubbles;
    private List<Bubble> _specialBubbles;
    private float _safeAreaTopY;

    private BubbleMatchHelper _bubbleMatchHelper;

    private void Awake()
    {
        _allBubbles = new List<Bubble>();
        _visibleBubbles = new List<Bubble>();
        _markedForDestroy = new List<Bubble>();
        _normalBubbles = new List<Bubble>();
        _specialBubbles = new List<Bubble>();

        var safeArea = Screen.safeArea;
        _safeAreaTopY = Camera.main.ScreenToWorldPoint(new Vector3(0, safeArea.yMax, 0)).y;

        _bubbleMatchHelper = new BubbleMatchHelper(bubbleLayer, _safeAreaTopY);

        bubbleShooter.gameObject.SetActive(false);
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    private void OnEnable()
    {
        UniTaskEventManager.AddEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        UniTaskEventManager.AddEvent(UniTaskEvent.ElevateBubbleContainer, ElevateBubbleContainer);
        UniTaskEventManager.AddEvent<List<Bubble>>(UniTaskEvent.PopMatchingBubbles, PopMatchingBubbles);

        EventManager.AddEvent<Bubble>(ActionEvent.CheckMatchingBubble, CheckMatchingBubble);
        EventManager.AddEvent<Bubble>(ActionEvent.AddBubble, AddBubble);
        EventManager.AddEvent<Bubble>(ActionEvent.PopBubble, PopOneBubble);

        FuncManager.AddEvent(FuncEvent.VisibleBubbles, GetVisibleBubbles);
    }

    private void OnDisable()
    {
        UniTaskEventManager.RemoveEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        UniTaskEventManager.RemoveEvent(UniTaskEvent.ElevateBubbleContainer, ElevateBubbleContainer);
        UniTaskEventManager.RemoveEvent<List<Bubble>>(UniTaskEvent.PopMatchingBubbles, PopMatchingBubbles);

        EventManager.RemoveEvent<Bubble>(ActionEvent.CheckMatchingBubble, CheckMatchingBubble);
        EventManager.RemoveEvent<Bubble>(ActionEvent.AddBubble, AddBubble);
        EventManager.RemoveEvent<Bubble>(ActionEvent.PopBubble, PopOneBubble);

        FuncManager.RemoveEvent(FuncEvent.VisibleBubbles, GetVisibleBubbles);
    }

    private async UniTask CreateStage(StageData stageData)
    {
        _currentStage = stageData;
        await CreateBubblesFromPositions();
        bubbleShooter.Init();
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
                bubble = bubbleCreator.CreateRandomBubbleWithChanceOfSpecial(worldPosition, Quaternion.identity);
            }
            else
            {
                bubble = bubbleCreator.CreateBubble(bubblePosition.bubbleData.BubbleType, worldPosition,
                    Quaternion.identity, bubblePosition.specialBubbleData);
            }

            bubble.transform.SetParent(bubbleContainer);

            _allBubbles.Add(bubble);

            await UniTask.Yield(destroyCancellationToken);
        }
    }

    private Vector3 CalculateBubblePosition(Vector2Int point)
    {
        var bubbleSize = bubbleCreator.BubbleSize;
        var verticalSpacing = bubbleSize * 0.866f;

        // 가장 낮은 y를 찾음
        float lowestY = 0;
        for (var i = 0; i < _currentStage.BubbleDataPositions.Count; i++)
        {
            var pos = _currentStage.BubbleDataPositions[i].bubblePosition;
            lowestY = Mathf.Max(lowestY, pos.y);
        }

        // 컨테이너의 시작 위치 계산 (가장 낮은 버블이 y=0에 오도록)
        var containerTopPosition = bubbleContainer.position;
        containerTopPosition.y = lowestY * verticalSpacing;

        var bubblePosition = containerTopPosition;
        var xOffset = point.y % 2 == 1 ? bubbleSize * 0.5f : 0f;
        bubblePosition.x += (point.x - (_currentStage.Width - 1) / 2f) * bubbleSize + xOffset;
        // y position 계산을 뒤집어서 아래에서부터 위로 생성되도록 함
        bubblePosition.y -= point.y * verticalSpacing;

        return bubblePosition;
    }

    private void CheckMatchingBubble(Bubble currentBubble)
    {
        var matchingBubbles = _bubbleMatchHelper.FindMatchingBubbles(currentBubble);
        if (matchingBubbles.Count > 0)
        {
            PopMatchingBubbles(matchingBubbles).Forget();
        }
    }

    private async UniTask PopMatchingBubbles(List<Bubble> bubbles)
    {
        _normalBubbles.Clear();
        _specialBubbles.Clear();

        CheckBubbleType(bubbles);

        await PopBubbles(_normalBubbles);
        await PopBubbles(_specialBubbles);
    }

    private void CheckBubbleType(List<Bubble> bubbles)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            if (bubbles[i].IsSpecialBubble)
            {
                _specialBubbles.Add(bubbles[i]);
            }
            else
            {
                _normalBubbles.Add(bubbles[i]);
            }
        }
    }

    private async UniTask PopBubbles(List<Bubble> bubbles)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            var bubble = bubbles[i];
            PopOneBubble(bubble);
            await UniTask.Delay(100, cancellationToken: destroyCancellationToken);
        }
    }

    private void PopOneBubble(Bubble bubble)
    {
        _allBubbles.Remove(bubble);
        _markedForDestroy.Add(bubble);
        bubble.Pop();
    }

    private List<Bubble> GetVisibleBubbles()
    {
        _visibleBubbles.Clear();
        for (var i = 0; i < _allBubbles.Count; i++)
        {
            if (_allBubbles[i].transform.position.y <= _safeAreaTopY)
            {
                _visibleBubbles.Add(_allBubbles[i]);
            }
        }

        return _visibleBubbles;
    }

    private void AddBubble(Bubble bubble)
    {
        _allBubbles.Add(bubble);
        bubble.transform.SetParent(bubbleContainer);
    }

    private async UniTask ElevateBubbleContainer()
    {
        if (_allBubbles.Count == 0) return;
        var lowestBubble = float.MaxValue;
        for (var i = 0; i < _allBubbles.Count; i++)
        {
            var bubbleY = _allBubbles[i].transform.position.y;
            if (bubbleY < lowestBubble)
            {
                lowestBubble = bubbleY;
            }
        }

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
    }

    [ContextMenu("Pop All Bubbles")]
    private void EndStage()
    {
        for (var i = _markedForDestroy.Count - 1; i >= 0; i--)
        {
            var bubble = _markedForDestroy[i];
            _markedForDestroy.Remove(bubble);
            Destroy(bubble);
        }
    }
}