using UnityEngine;

namespace UIControl
{
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool debugMode;
        private RectTransform _rectTransform;
        private readonly Vector2 _portraitSize = new(1080, 1920); // 16:9 비율의 기준 해상도

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Init();
        }

        private void Init()
        {
            // SafeArea 계산
            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = anchorMin + safeArea.size;

            // 스크린 좌표를 0~1 범위의 비율로 변환
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // 기준 해상도에 맞춰 크기 조정
            var screenRatio = (float)Screen.height / Screen.width;
            var targetRatio = _portraitSize.y / _portraitSize.x;
            
            float width, height;
            
            if (screenRatio > targetRatio) // 화면이 더 길쭉한 경우
            {
                width = _portraitSize.x;
                height = width * screenRatio;
            }
            else // 화면이 더 넓적한 경우
            {
                height = _portraitSize.y;
                width = height / screenRatio;
            }

            // RectTransform 설정
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.sizeDelta = new Vector2(width, height);

            // SafeArea에 맞춰 크기 조정
            var safeWidth = width * (anchorMax.x - anchorMin.x);
            var safeHeight = height * (anchorMax.y - anchorMin.y);
            
            // SafeArea 오프셋 적용
            var offsetY = (height * anchorMin.y) - (height * (1 - anchorMax.y));
            _rectTransform.anchoredPosition = new Vector2(0, offsetY * 0.5f);
            _rectTransform.sizeDelta = new Vector2(safeWidth, safeHeight);

            if (debugMode)
            {
                Debug.Log($"Screen Size: {Screen.width}x{Screen.height}");
                Debug.Log($"Safe Area: {safeArea}");
                Debug.Log($"Anchors: {anchorMin} to {anchorMax}");
                Debug.Log($"Final Size: {safeWidth}x{safeHeight}");
                Debug.Log($"Offset Y: {offsetY}");
            }
        }

// #if UNITY_EDITOR
//         private void OnValidate()
//         {
//             if (Application.isPlaying)
//             {
//                 Init().Forget();
//             }
//         }
// #endif
    }
}