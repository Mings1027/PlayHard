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
            var allBubbles = FuncManager.TriggerEvent<List<Bubble>>(FuncEvent.AllBubbles);
            _availableBubbles.Clear();

            for (int i = 0; i < allBubbles.Count; i++)
            {
                if (!allBubbles[i].IsMarkedForPop)
                {
                    _availableBubbles.Add(allBubbles[i]);
                }
            }

            if (_availableBubbles.Count == 0) return;

            var randomBubble = allBubbles[Random.Range(0, _availableBubbles.Count)];
            randomBubble.MarkForPop();
            if (randomBubble.TryGetComponent(out Bubble bubble))
            {
                MoveToPopBubble(triggerBubble.transform, bubble).Forget();
            }
        }

        private static async UniTask MoveToPopBubble(Transform triggerBubble, Bubble popBubble)
        {
            var popIndicatorBubble = PoolObjectManager.Get<Transform>(PoolObjectKey.PopIndicatorBubble, triggerBubble);
            await popIndicatorBubble.DOMove(popBubble.transform.position, 1);

            popIndicatorBubble.gameObject.SetActive(false);

            EventManager.TriggerEvent(ActionEvent.PopSingleBubble, popBubble);
        }
    }
}