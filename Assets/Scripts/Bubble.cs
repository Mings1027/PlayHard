using System;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    private Image image;

    public BubbleType Type { get; private set; }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize(BubbleType type)
    {
        Type = type;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // 색상 설정
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
            case BubbleType.Green: return Color.green;
            case BubbleType.Yellow: return Color.yellow;
            case BubbleType.Purple: return new Color(1f, 0f, 1f);
            default: return Color.white;
        }
    }
}