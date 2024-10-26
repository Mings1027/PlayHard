using System.Collections.Generic;
using InterfaceFolder;
using UnityEngine;

namespace BubbleFolder
{
    public class RandomSelectBubbleEffect : ISpecialBubbleEffect
    {
        private readonly List<Bubble> _allBubbles;

        public RandomSelectBubbleEffect(List<Bubble> allBubbles)
        {
            _allBubbles = allBubbles;
        }

        public float CheckSize { get; set; }
        public Collider[] Colliders { get; set; }

        public void SetColliders(Bubble bubble)
        {
            CheckSize = bubble.transform.localScale.x * 20;
            Colliders = new Collider[20];
        }

        public List<Bubble> GetBubblesToPop(Bubble triggerBubble)
        {
            var bubblesToPop = new List<Bubble>();
            var availableBubbles = new List<Bubble>();

            for (int i = 0; i < _allBubbles.Count; i++)
            {
                if (_allBubbles[i] != triggerBubble)
                {
                    availableBubbles.Add(_allBubbles[i]);
                }
            }

            if (availableBubbles.Count > 0)
            {
                var randomBubble = availableBubbles[Random.Range(0, availableBubbles.Count)];
                bubblesToPop.Add(randomBubble);
            }

            return bubblesToPop;
        }
    }
}