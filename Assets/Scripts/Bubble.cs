using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private SpriteRenderer _bubbleSprite;
    public BubbleType Type { get; private set; }

    private void Awake()
    {
        _bubbleSprite = GetComponent<SpriteRenderer>();

#if UNITY_EDITOR
        name = $"Bubble_{Type}";
#endif
    }

    public void SetType(BubbleType type, Sprite sprite)
    {
        Type = type;
        _bubbleSprite.sprite = sprite;
    }

    public Color GetColorForType()
    {
        return Type switch
        {
            BubbleType.Red => Color.red,
            BubbleType.Blue => Color.blue,
            BubbleType.Yellow => Color.yellow,
            _ => Color.white
        };
    }
}