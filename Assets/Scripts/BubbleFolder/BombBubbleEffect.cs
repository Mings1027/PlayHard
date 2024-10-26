using System.Collections.Generic;
using InterfaceFolder;
using UnityEngine;

namespace BubbleFolder
{
    public class BombBubbleEffect : ISpecialBubbleEffect
    {
        public float CheckSize { get; set; }
        public Collider[] Colliders { get; set; }

        public void SetColliders(Bubble bubble)
        {
            CheckSize = bubble.transform.localScale.x * 3;
            Colliders = new Collider[6];
        }

        public List<Bubble> GetBubblesToPop(Bubble triggerBubble)
        {
            var bubblesToPop = new List<Bubble> { triggerBubble };
            var bubbleSize = triggerBubble.transform.localScale.x;
            var verticalSpacing = bubbleSize * 0 * 866f;

            var directions = new Vector2[]
            {
                new(bubbleSize, 0), // 우
                new(bubbleSize * 0.5f, verticalSpacing), // 우상
                new(-bubbleSize * 0.5f, verticalSpacing), // 좌상
                new(-bubbleSize, 0), // 좌
                new(-bubbleSize * 0.5f, -verticalSpacing), // 좌하
                new(bubbleSize * 0.5f, -verticalSpacing) // 우하
            };

            for (var i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var checkPosition = (Vector2)triggerBubble.transform.position + direction;
                var hit = Physics2D.OverlapCircle(checkPosition, bubbleSize * 0.4f, LayerMask.GetMask("Bubble"));

                if (hit != null && hit.TryGetComponent(out Bubble neighborBubble))
                {
                    bubblesToPop.Add(neighborBubble);
                }
            }

            return bubblesToPop;
        }
    }
}