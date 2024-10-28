using System.Collections.Generic;
using UnityEngine;

namespace HelperFolder
{
    public class BubbleMatchHelper
    {
        private readonly LayerMask _bubbleLayer;
        private readonly float _safeAreaTopY;
        private const int MinMatchCount = 3;

        public BubbleMatchHelper(LayerMask bubbleLayer, float safeAreaTopY)
        {
            _bubbleLayer = bubbleLayer;
            _safeAreaTopY = safeAreaTopY;
        }

        public List<Bubble> FindMatchingBubbles(Bubble currentBubble)
        {
            var visited = new HashSet<Bubble>();
            var matchingBubbles = new List<Bubble>();
            var bubbleType = currentBubble.Type;

            FloodFill(currentBubble, bubbleType, visited, matchingBubbles);

            if (matchingBubbles.Count < MinMatchCount)
            {
                matchingBubbles.Clear();
            }

            return matchingBubbles;
        }

        private void FloodFill(Bubble bubble, BubbleType targetType, HashSet<Bubble> visited,
                               List<Bubble> matchingBubbles)
        {
            if (bubble == null || visited.Contains(bubble) ||
                bubble.Type != targetType || bubble.transform.position.y > _safeAreaTopY)
                return;

            visited.Add(bubble);
            matchingBubbles.Add(bubble);

            var neighbors = GetNeighborBubbles(bubble);
            foreach (var neighbor in neighbors)
            {
                FloodFill(neighbor, targetType, visited, matchingBubbles);
            }
        }

        private List<Bubble> GetNeighborBubbles(Bubble bubble)
        {
            var neighbors = new List<Bubble>();
            var bubbleSize = bubble.transform.localScale.x;
            var verticalSpacing = bubbleSize * 0.866f;

            var directions = new Vector2[]
            {
                new(bubbleSize, 0),
                new(bubbleSize * 0.5f, verticalSpacing),
                new(-bubbleSize * 0.5f, verticalSpacing),
                new(-bubbleSize, 0),
                new(-bubbleSize * 0.5f, -verticalSpacing),
                new(bubbleSize * 0.5f, -verticalSpacing)
            };

            foreach (var direction in directions)
            {
                var checkPosition = (Vector2)bubble.transform.position + direction;
                var hit = Physics2D.OverlapCircle(checkPosition, bubbleSize * 0.4f, _bubbleLayer);
                if (hit != null && hit.TryGetComponent(out Bubble hitBubble))
                {
                    neighbors.Add(hitBubble);
                }
            }

            return neighbors;
        }
    }
}