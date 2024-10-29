using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DataControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventControl;
using HelperFolder;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Transform bubbleContainer;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private LayerMask bubbleLayer;

    private StageData _currentStage;
    private List<Bubble> _allBubbles;
    private List<Bubble> _visibleBubbles;

    private List<Bubble> _normalBubbles;
    private List<Bubble> _specialBubbles;
    private float _safeAreaTopY;

    private BubbleMatchHelper _bubbleMatchHelper;

    private void Awake()
    {
        _allBubbles = new List<Bubble>();
        _visibleBubbles = new List<Bubble>();
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
        UniTaskEventManager.AddEvent(UniTaskEvent.EndStage, EndStage);

        BubbleEventManager.AddAsyncEvent<List<Bubble>>(BubbleEvent.PopMatchingBubbles, PopMatchingBubbles);
        BubbleEventManager.AddEvent<Bubble>(BubbleEvent.CheckMatchingBubble, CheckMatchingBubble);
        BubbleEventManager.AddEvent<Bubble>(BubbleEvent.AddBubble, AddBubble);
        BubbleEventManager.AddEvent<Bubble>(BubbleEvent.PopBubble, PopOneBubble);

        FuncManager.AddEvent(FuncEvent.VisibleBubbles, GetVisibleBubbles);
    }

    private void OnDisable()
    {
        UniTaskEventManager.RemoveEvent<StageData>(UniTaskEvent.CreateStage, CreateStage);
        UniTaskEventManager.RemoveEvent(UniTaskEvent.ElevateBubbleContainer, ElevateBubbleContainer);
        UniTaskEventManager.RemoveEvent(UniTaskEvent.EndStage, EndStage);

        BubbleEventManager.RemoveAsyncEvent<List<Bubble>>(BubbleEvent.PopMatchingBubbles, PopMatchingBubbles);

        BubbleEventManager.RemoveEvent<Bubble>(BubbleEvent.CheckMatchingBubble, CheckMatchingBubble);
        BubbleEventManager.RemoveEvent<Bubble>(BubbleEvent.AddBubble, AddBubble);
        BubbleEventManager.RemoveEvent<Bubble>(BubbleEvent.PopBubble, PopOneBubble);

        FuncManager.RemoveEvent(FuncEvent.VisibleBubbles, GetVisibleBubbles);
    }

    private async UniTask CreateStage(StageData stageData)
    {
        _currentStage = stageData;
        await CreateBubblesFromPositions();
        EventManager.TriggerEvent(ActionEvent.SetRemainingCountText, _currentStage.BubbleAmmo);
        bubbleShooter.gameObject.SetActive(true);
        bubbleShooter.SetBubbleCount(stageData.BubbleAmmo);
        bubbleShooter.Init();
    }

    private async UniTask CreateBubblesFromPositions()
    {
        if (_currentStage.BossStage)
        {
            // 보스 스테이지 버블 움직임 구현
        }
        else
        {
            await CreateNormalStagePattern();
        }
    }

    private async UniTask CreateNormalStagePattern()
    {
        for (var i = 0; i < _currentStage.BubbleDataPositions.Count; i++)
        {
            var bubblePosition = _currentStage.BubbleDataPositions[i];
            var worldPosition = CalculateBubblePosition(bubblePosition.bubblePosition);

            Bubble bubble;
            if (bubblePosition.bubbleData.IsRandomBubble)
            {
                bubble = BubbleEventManager.TriggerEvent<Vector3, Bubble>(BubbleEvent.RandomStageBubble, worldPosition);
            }
            else
            {
                bubble = BubbleEventManager.TriggerEvent<BubbleType, Vector3, SpecialBubbleData, Bubble>(
                    BubbleEvent.CreateBubble, bubblePosition.bubbleData.BubbleType, worldPosition,
                    bubblePosition.specialBubbleData);
            }

            bubble.transform.SetParent(bubbleContainer);

            _allBubbles.Add(bubble);

            await UniTask.Yield(destroyCancellationToken);
        }
    }

    private Vector3 CalculateBubblePosition(Vector2Int point)
    {
        var bubbleSize = BubbleCreator.BubbleSize;
        var verticalSpacing = bubbleSize * Mathf.Sin(BubbleMatchHelper.HexagonAngle);

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
        var xOffset = point.y % 2 == 1
            ? bubbleSize * Mathf.Cos(BubbleMatchHelper.HexagonAngle)
            : 0;
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
        bubble.Pop();
    }

    private List<Bubble> GetVisibleBubbles()
    {
        _visibleBubbles.Clear();
        for (var i = 0; i < _allBubbles.Count; i++)
        {
            if (_allBubbles[i].transform.position.y <= _safeAreaTopY && _allBubbles[i].gameObject.activeSelf)
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
            await bubbleContainer.DOMoveY(bubbleContainer.position.y + Mathf.Abs(lowestBubble), 0.5f)
                                 .SetEase(Ease.OutQuint);
        }
        else
        {
            // 가장 낮은 버블이 y=0 위치까지만 내려가도록 조정
            await bubbleContainer.DOMoveY(bubbleContainer.position.y - lowestBubble, 0.5f)
                                 .SetEase(Ease.OutQuint);
        }
    }

    private async UniTask EndStage()
    {
        bubbleShooter.gameObject.SetActive(false);
        EventManager.TriggerEvent(ActionEvent.DisplayTouchBlockPanel);

        var allExistingBubble = new List<Bubble>(bubbleContainer.childCount);
        for (int i = 0; i < bubbleContainer.childCount; i++)
        {
            allExistingBubble.Add(bubbleContainer.GetChild(i).GetComponent<Bubble>());
        }

        _allBubbles.Clear();
        _visibleBubbles.Clear();
        _normalBubbles.Clear();
        _specialBubbles.Clear();

        for (var i = allExistingBubble.Count - 1; i >= 0; i--)
        {
            var bubble = allExistingBubble[i];
            bubble.PopDestroy();
            await UniTask.Yield(destroyCancellationToken);
            allExistingBubble.Remove(bubble);
            Destroy(bubble.gameObject);
        }

        EventManager.TriggerEvent(ActionEvent.DisplayGameOverPanel);
    }
}