using System.Collections.Generic;
using UnityEngine;

namespace HelperFolder
{
    public class BubbleMatchHelper
    {
        private readonly LayerMask _bubbleLayer;
        private readonly float _safeAreaTopY;
        private const int MinMatchCount = 3;
        private readonly HashSet<Bubble> _visited;
        private readonly List<Bubble> _matchingBubbles;

        public static float HexagonAngle => Mathf.PI / 3f;

        public BubbleMatchHelper(LayerMask bubbleLayer, float safeAreaTopY)
        {
            _bubbleLayer = bubbleLayer;
            _safeAreaTopY = safeAreaTopY;
            _visited = new HashSet<Bubble>();
            _matchingBubbles = new List<Bubble>();
        }

        public List<Bubble> FindMatchingBubbles(Bubble currentBubble)
        {
            _visited.Clear();
            _matchingBubbles.Clear();
            var bubbleType = currentBubble.Type;

            FloodFill(currentBubble, bubbleType);

            if (_matchingBubbles.Count < MinMatchCount)
            {
                _matchingBubbles.Clear();
            }

            return _matchingBubbles;
        }

        private void FloodFill(Bubble bubble, BubbleType targetType)
        {
            if (_visited.Contains(bubble) || bubble.Type != targetType || bubble.transform.position.y > _safeAreaTopY)
                return;

            _visited.Add(bubble);
            _matchingBubbles.Add(bubble);

            var neighbors = GetNeighborBubbles(bubble);
            for (var i = 0; i < neighbors.Count; i++)
            {
                FloodFill(neighbors[i], targetType);
            }
        }

        private List<Bubble> GetNeighborBubbles(Bubble bubble)
        {
            var neighbors = new List<Bubble>();
            var bubbleSize = bubble.transform.localScale.x;

            // 0도부터 60도씩 더해지면서 계산
            // angle = 0, cos(0) = 1, sin(0) = 0
            // angle = 60, cos(60) = 0.5, sin(60) = 0.866
            // angle = 120, cos(120) = -0.5, sin(120) = 0.866
            // angle = 180, cos(180) = -1, sin(180) = 0
            // angle = 240, cos(240) = -0.5, sin(240) = -0.866
            // angle = 300, cos(300) = 0.5, sin(300) = -0.866
            for (int i = 0; i < 6; i++)
            {
                var angle = i * HexagonAngle;
                var direction = new Vector2(bubbleSize * Mathf.Cos(angle), bubbleSize * Mathf.Sin(angle));
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