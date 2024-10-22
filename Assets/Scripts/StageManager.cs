using System;
using Cysharp.Threading.Tasks;
using DataControl;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageData currentStage;
    [SerializeField] private GameObject bubblePrefab; // UI 버블 프리팹
    [SerializeField] private RectTransform bubbleContainer; // 버블들을 담을 UI 컨테이너

    private GameObject[,] _bubbleObjects;
    private float _bubbleSize;

    private void Awake()
    {
        // bubbleContainer의 앵커와 피벗 설정
        if (bubbleContainer != null)
        {
            bubbleContainer.anchorMin = new Vector2(0.5f, 1f);
            bubbleContainer.anchorMax = new Vector2(0.5f, 1f);
            bubbleContainer.pivot = new Vector2(0.5f, 1f);

            // 초기 위치 설정 (상단 중앙)
            bubbleContainer.anchoredPosition = Vector2.zero;
        }

        _bubbleSize = bubblePrefab.GetComponent<RectTransform>().rect.width;
    }

    private void OnEnable()
    {
        UIEventManager.AddEvent(UIEvent.StartStage, () => CreateStage());
    }

    private void OnDisable()
    {
        UIEventManager.RemoveEvent(UIEvent.StartStage, () => CreateStage());
    }

    private async UniTask CreateStage()
    {
        ClearCurrentStage();

        _bubbleObjects = new GameObject[currentStage.width, currentStage.height];

        for (int y = 0; y < currentStage.height; y++)
        {
            int rowWidth = (y % 2 == 0) ? currentStage.width : currentStage.width - 1;

            for (int x = 0; x < rowWidth; x++)
            {
                BubbleType bubbleType = currentStage.bubbleGrid[x, y];
                if (bubbleType != BubbleType.None)
                {
                    Vector2 position = GetBubblePosition(x, y);
                    CreateBubble(bubbleType, position, x, y);
                }

                await UniTask.Delay(50);
            }
        }
    }

    private Vector2 GetBubblePosition(int x, int y)
    {
        float xPos;
    
        if (y % 2 == 0) // 짝수 줄 (11개)
        {
            // 6번째 버블(x=5)이 화면 중앙에 오도록 조정
            // 첫 번째 버블의 x좌표는 중앙에서 (5 * bubbleSize)만큼 왼쪽으로
            xPos = (x - 5) * _bubbleSize;
        }
        else // 홀수 줄 (10개)
        {
            // 5번째와 6번째 버블 사이가 화면 중앙에 오도록 조정
            // 첫 번째 버블의 x좌표는 중앙에서 (4.5 * bubbleSize)만큼 왼쪽으로
            xPos = (x - 4.5f) * _bubbleSize;
        }

        // y 좌표는 상단에서 시작하여 버블 크기만큼 아래로
        float yPos = -(_bubbleSize / 2 + y * (_bubbleSize * 0.866f));

        return new Vector2(xPos, yPos);
    }

    private void CreateBubble(BubbleType type, Vector2 position, int x, int y)
    {
        GameObject bubble = Instantiate(bubblePrefab, bubbleContainer);
        bubble.name = $"Bubble_{x}_{y}";

        // RectTransform 설정
        RectTransform rectTransform = bubble.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        // 구슬 컴포넌트 설정
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.Initialize(type);
        }

        _bubbleObjects[x, y] = bubble;
    }

    private void ClearCurrentStage()
    {
        if (_bubbleObjects != null)
        {
            for (int y = 0; y < _bubbleObjects.GetLength(1); y++)
            {
                for (int x = 0; x < _bubbleObjects.GetLength(0); x++)
                {
                    if (_bubbleObjects[x, y] != null)
                    {
                        Destroy(_bubbleObjects[x, y]);
                    }
                }
            }
        }
    }
}