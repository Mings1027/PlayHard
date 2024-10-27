using BubbleFolder;
using InterfaceFolder;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(fileName = "New Special Bubble", menuName = "Bubble Game/Special Bubble Data")]
    public class SpecialBubbleData : BubbleData
    {
        [SerializeField] private Sprite overlaySprite;
        public Sprite OverlaySprite => overlaySprite;

        public ISpecialBubbleEffect GetSpecialEffect(Bubble bubble)
        {
            if (!isSpecialBubble) return null;
            return specialBubbleType switch
            {
                SpecialBubbleType.RandomSelect => new RandomSelectBubbleEffect(),
                SpecialBubbleType.Bomb => new BombBubbleEffect(bubble),
                _ => null
            };
        }
    }
}