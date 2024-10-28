using DataControl;
using InterfaceFolder;
using PoolControl;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    private SpriteRenderer _bubbleSprite;
    private SpriteRenderer _overlaySprite;
    public BubbleType Type { get; private set; }
    private ISpecialBubbleEffect _specialEffect;
    private CircleCollider2D _circleCollider;
    private bool _isSpecialBubble;
    [field: SerializeField] public bool IsMarkedForPop { get; private set; }
    public bool IsSpecialBubble => _isSpecialBubble;

    private void Awake()
    {
        _bubbleSprite = GetComponent<SpriteRenderer>();
        _circleCollider = GetComponent<CircleCollider2D>();
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
            BubbleType.Cyan => Color.cyan,
            BubbleType.Yellow => Color.yellow,
            _ => Color.white
        };

    public void Pop()
    {
        if (!gameObject.activeSelf) return;
        if (IsMarkedForPop) return;
        gameObject.SetActive(false);
        IsMarkedForPop = true;
        if (_isSpecialBubble && _specialEffect != null)
        {
            _specialEffect.ExecuteSpecialEffect(this);
        }

        PlayPopEffect();
    }

    public void SetPosition(Vector3 snapPosition)
    {
        transform.position = snapPosition;
        _circleCollider.enabled = true;
    }

    private void PlayPopEffect()
    {
        var popParticle = PoolObjectManager
            .Get<ParticleSystem>(PoolObjectKey.PopBubbleEffect, transform, transform.localScale);

        var mainModule = popParticle.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(GetColorForType());
    }
}