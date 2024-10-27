using System;
using Cysharp.Threading.Tasks;
using DataControl;
using InterfaceFolder;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private SpriteRenderer _bubbleSprite;
    private SpriteRenderer _overlaySprite;
    public BubbleType Type { get; private set; }
    private ISpecialBubbleEffect _specialEffect;
    private bool _isSpecialBubble;

    private void Awake()
    {
        _bubbleSprite = GetComponent<SpriteRenderer>();
    }

    public void Initialize(BubbleData bubbleData, SpecialBubbleData specialData = null)
    {
        Type = bubbleData.BubbleType;

#if UNITY_EDITOR
        name = $"Bubble_{Type}";
#endif
        _bubbleSprite.sprite = bubbleData.BubbleSprite;

        if (specialData != null)
        {
            _isSpecialBubble = specialData.IsSpecialBubble;
            _specialEffect = specialData.GetSpecialEffect(this);
            CreateOverlay(specialData.OverlaySprite);
        }
        else if (_overlaySprite != null)
        {
            _overlaySprite.enabled = false;
        }
    }

    private void CreateOverlay(Sprite overlaySprite)
    {
        if (_overlaySprite != null)
        {
            _overlaySprite.sprite = overlaySprite;
            _overlaySprite.enabled = true;
            return;
        }

        var overlayObject = new GameObject();
        overlayObject.transform.SetParent(transform);
        overlayObject.transform.localPosition = Vector3.zero;
        overlayObject.transform.localScale = Vector3.one;
        _overlaySprite = overlayObject.AddComponent<SpriteRenderer>();
        _overlaySprite.sprite = overlaySprite;
        _overlaySprite.sortingOrder = _bubbleSprite.sortingOrder + 1;
    }

    public Color GetColorForType() =>
        Type switch
        {
            BubbleType.Red => Color.red,
            BubbleType.Blue => Color.blue,
            BubbleType.Yellow => Color.yellow,
            _ => Color.white
        };

    public async UniTask ExecuteSpecialEffect()
    {
        if (!_isSpecialBubble || _specialEffect == null) return;
        GetComponent<CircleCollider2D>().enabled = false;
        await UniTask.Yield(destroyCancellationToken);
        _specialEffect.ExecuteSpecialEffect(this);
    }
}