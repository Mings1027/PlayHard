using Cysharp.Threading.Tasks;
using DG.Tweening;
using InterfaceFolder;
using PoolControl;
using UnityEngine;

namespace BubbleFolder
{
    public class RandomSelectBubbleEffect : ISpecialBubbleEffect
    {
        public float CheckSize { get; set; }
        public Collider2D[] Colliders { get; set; }

        public RandomSelectBubbleEffect(Bubble bubble)
        {
            CheckSize = bubble.transform.localScale.x * 3;
            Colliders = new Collider2D[1];
        }

        public void ExecuteSpecialEffect(Bubble triggerBubble)
        {
            Debug.Log(triggerBubble.name);
            var size = Physics2D.OverlapCircleNonAlloc(triggerBubble.transform.position, CheckSize, Colliders);
            if (size <= 0) return;
            for (int i = 0; i < size; i++)
            {
                if (Colliders[i].TryGetComponent(out Bubble bubble))
                {
                    Debug.Log(Colliders[i].GetHashCode());
                    MoveToPopBubble(triggerBubble.transform, bubble).Forget();
                }
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