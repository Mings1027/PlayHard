using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private SpriteRenderer _bubbleSprite;
    private BubbleType _type;

    private void Awake()
    {
        _bubbleSprite = GetComponent<SpriteRenderer>();
        RandomizeType();
        UpdateVisual();

#if UNITY_EDITOR
        name = $"Bubble_{_type}";
#endif
    }

    private void RandomizeType()
    {
        // BubbleType enum의 모든 값들을 배열로 가져옴
        var values = Enum.GetValues(typeof(BubbleType));
        // 랜덤한 인덱스 선택
        var randomIndex = UnityEngine.Random.Range(0, values.Length);
        // 선택된 타입 설정
        _type = (BubbleType)values.GetValue(randomIndex);
    }

    private void UpdateVisual()
    {
        var bubbleColor = GetColorForType();
        if (_bubbleSprite != null)
        {
            _bubbleSprite.color = bubbleColor;
        }
    }

    public Color GetColorForType()
    {
        return _type switch
        {
            BubbleType.Red => Color.red,
            BubbleType.Blue => Color.blue,
            BubbleType.Yellow => Color.yellow,
            _ => Color.white
        };
    }
}