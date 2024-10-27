using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceFolder;
using PoolControl;
using UnityEngine;

namespace BubbleFolder
{
    public class BombBubbleEffect : ISpecialBubbleEffect
    {
        private readonly float _checkSize;
        private readonly Collider2D[] _colliders;

        public BombBubbleEffect(Bubble bubble)
        {
            _checkSize = bubble.transform.localScale.x;
            _colliders = new Collider2D[6];
        }

        public void ExecuteSpecialEffect(Bubble triggerBubble)
        {
            var bubblesToPop = new List<Bubble>();

            var size = Physics2D.OverlapCircleNonAlloc(triggerBubble.transform.position, _checkSize, _colliders);
            if (size <= 0) return;
            for (var i = 0; i < size; i++)
            {
                if (_colliders[i].TryGetComponent(out Bubble bubble))
                {
                    bubble.MarkForPop();
                    bubblesToPop.Add(bubble);
                }
            }

            BombIndicatorAnimation(bubblesToPop).Forget();
        }

        private async UniTask BombIndicatorAnimation(List<Bubble> bubblesToPop)
        {
            var bubbles = new Transform[bubblesToPop.Count];
            var animationTasks = new UniTask[bubblesToPop.Count];

            for (int i = 0; i < bubblesToPop.Count; i++)
            {
                bubbles[i] =
                    PoolObjectManager.Get<Transform>(PoolObjectKey.PopIndicatorBubble, bubblesToPop[i].transform);
                var originalScale = bubbles[i].localScale;
                animationTasks[i] = bubbles[i].DOScale(originalScale * 1.5f, 0.25f)
                                              .SetEase(Ease.OutQuad)
                                              .SetLoops(4, LoopType.Yoyo)
                                              .ToUniTask();
            }

            await UniTask.WhenAll(animationTasks);

            for (int i = 0; i < bubbles.Length; i++)
            {
                bubbles[i].gameObject.SetActive(false);
            }

            if (bubblesToPop.Count > 0)
            {
                UniTaskEventManager.TriggerAsync(UniTaskEvent.PopMatchingBubbles, bubblesToPop).Forget();
            }
        }
    }
}