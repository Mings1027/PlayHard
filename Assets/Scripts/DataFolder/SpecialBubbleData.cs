using System.Collections.Generic;
using BubbleFolder;
using InterfaceFolder;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(fileName = "New Special Bubble", menuName = "Bubble Game/Special Bubble Data")]
    public class SpecialBubbleData : BubbleData
    {
        [SerializeField] private Sprite _overlaySprite;
        public Sprite OverlaySprite => _overlaySprite;


        public ISpecialBubbleEffect GetSpecialEffect(List<Bubble> allBubbles = null)
        {
            if (!isSpecialBubble) return null;
            return specialBubbleType switch
            {
                SpecialBubbleType.RandomSelect => new RandomSelectBubbleEffect(allBubbles),
                SpecialBubbleType.Bomb => new BombBubbleEffect(),
                _ => null
            };
        }
    }
}