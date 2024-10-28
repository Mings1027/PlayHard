using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceFolder;
using PoolControl;
using UnityEngine;

namespace BubbleFolder
{
    public class RandomSelectBubbleEffect : ISpecialBubbleEffect
    {
        private readonly List<Bubble> _availableBubbles = new();

        public void ExecuteSpecialEffect(Bubble triggerBubble)
        {
            var visibleBubbles = FuncManager.TriggerEvent<List<Bubble>>(FuncEvent.VisibleBubbles);
            _availableBubbles.Clear();

            for (int i = 0; i < visibleBubbles.Count; i++)
            {
                if (!visibleBubbles[i].IsMarkedForPop)
                {
                    _availableBubbles.Add(visibleBubbles[i]);
                }
            }

            if (_availableBubbles.Count == 0) return;

            var randomBubble = visibleBubbles[Random.Range(0, _availableBubbles.Count)];
            if (randomBubble.TryGetComponent(out Bubble bubble))
            {
                MoveToPopBubble(triggerBubble.transform, bubble).Forget();
            }
        }

        private static async UniTask MoveToPopBubble(Transform triggerBubble, Bubble popBubble)
        {
            var popIndicatorBubble = PoolObjectManager.Get<Transform>(PoolObjectKey.PopIndicatorBubble, triggerBubble);
            await popIndicatorBubble.DOMove(popBubble.transform.position, 5).SetSpeedBased(true);

            popIndicatorBubble.gameObject.SetActive(false);

            EventManager.TriggerEvent(ActionEvent.PopSingleBubble, popBubble);
        }
    }
}