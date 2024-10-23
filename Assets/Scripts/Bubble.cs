using System;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    private Image image;
    public BubbleType Type { get; private set; }
    public int PathIndex { get; private set; }

    private void Awake()
    {
        image = GetComponent<Image>();
        RandomizeType();
    }

    public void SetPathIndex(int index)
    {
        PathIndex = index;
    }

    private void RandomizeType()
    {
        // BubbleType enum의 모든 값들을 배열로 가져옴
        Array values = Enum.GetValues(typeof(BubbleType));
        // 랜덤한 인덱스 선택
        int randomIndex = UnityEngine.Random.Range(0, values.Length);
        // 선택된 타입 설정
        Type = (BubbleType)values.GetValue(randomIndex);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Color bubbleColor = GetColorForType(Type);
        if (image != null)
        {
            image.color = bubbleColor;
        }
    }

    private Color GetColorForType(BubbleType type)
    {
        switch (type)
        {
            case BubbleType.Red: return Color.red;
            case BubbleType.Blue: return Color.blue;
            case BubbleType.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }
}