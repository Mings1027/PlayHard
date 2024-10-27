using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(fileName = "New Bubble", menuName = "Bubble Game/Bubble Data")]
    public class BubbleData : ScriptableObject
    {
        [SerializeField] private BubbleType bubbleType;
        [SerializeField] protected SpecialBubbleType specialBubbleType;
        [SerializeField] private Sprite bubbleSprite;
        [SerializeField] private bool isRandomBubble;
        [SerializeField] protected bool isSpecialBubble;

        public BubbleType BubbleType => bubbleType;
        public SpecialBubbleType SpecialBubbleType => specialBubbleType;
        public Sprite BubbleSprite => bubbleSprite;
        public bool IsRandomBubble => isRandomBubble;
        public bool IsSpecialBubble => isSpecialBubble;
    }
}