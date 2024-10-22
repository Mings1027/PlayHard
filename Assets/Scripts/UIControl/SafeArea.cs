using UnityEngine;

namespace UIControl
{
    public class SafeArea : MonoBehaviour
    {
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            var safeAreaMin = Screen.safeArea.min;
            if (safeAreaMin.y <= 0) return;
            var rectTransform = GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var tempHeight = rectTransform.rect.height;
            tempHeight -= safeAreaMin.y * 2;

            rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, tempHeight);
        }
    }
}