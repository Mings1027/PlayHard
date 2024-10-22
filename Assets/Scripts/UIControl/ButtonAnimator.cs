using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class ButtonAnimator : MonoBehaviour
    {
        [SerializeField] private float bounceScale = 1.2f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float pressedScale = 0.9f;

        private Button button;
        private Vector3 originScale;
        private Sequence bounceSequence;
        private bool isPressed;

        private void Awake()
        {
            button = GetComponentInChildren<Button>();
            if (button == null)
            {
                Debug.LogError("ButtonAnimator requires a Button component!");
                return;
            }

            InitTween();
            SetupButtonEvents();
        }

        private void OnDestroy()
        {
            bounceSequence?.Kill();
        }
        
        private void InitTween()
        {
            originScale = GetComponentInChildren<Button>().transform.localScale;
            bounceSequence = DOTween.Sequence();
            
            var scaleFirst = new Vector3(originScale.x / bounceScale, originScale.y * bounceScale, originScale.z);
            var scaleSecond = new Vector3(originScale.x * bounceScale, originScale.y / bounceScale, originScale.z);
            
            bounceSequence
                .Append(transform.DOScale(scaleFirst, duration).SetEase(Ease.InOutQuad))
                .Append(transform.DOScale(scaleSecond, duration).SetEase(Ease.InOutQuad))
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        
        private void SetupButtonEvents()
        {
            EventTrigger trigger = gameObject.GetComponent<EventTrigger>() 
                                   ?? gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener(_ => OnButtonPress());
            trigger.triggers.Add(pointerDown);

            EventTrigger.Entry pointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener(_ => OnButtonRelease());
            trigger.triggers.Add(pointerUp);
        }
        
        private void OnButtonPress()
        {
            if (isPressed) return;
            isPressed = true;
            
            transform.DOScale(pressedScale, 0.1f).SetEase(Ease.OutQuad);
        }

        private void OnButtonRelease()
        {
            if (!isPressed) return;
            isPressed = false;

            transform.DOScale(originScale, 0.1f).SetEase(Ease.OutQuad);
        }
    }
}