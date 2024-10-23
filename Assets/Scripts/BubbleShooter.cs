using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private RectTransform shooterPivot;
    [SerializeField] private RectTransform firstLineRect;
    [SerializeField] private RectTransform firstLineEndRect;
    [SerializeField] private RectTransform secondLineRect;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float checkRadius = 30f; // 체크할 반경 설정
    [SerializeField] private RectTransform shooterContainer;

    private bool _isMousePressed;

    private void Update()
    {
        _isMousePressed = Input.GetMouseButton(0);
        if (_isMousePressed)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                shooterPivot.parent as RectTransform, Input.mousePosition, null, out var mousePos);

            var direction = (mousePos - (Vector2)shooterPivot.localPosition).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            var firstLineLength = CalculateDistanceToWall(shooterPivot.localPosition, direction, out var hitPoint,
                out var hitRightWall);
            
            firstLineRect.gameObject.SetActive(true);
            firstLineRect.localPosition = shooterPivot.localPosition;
            firstLineRect.rotation = Quaternion.Euler(0, 0, angle);
            firstLineRect.sizeDelta = new Vector2(firstLineRect.sizeDelta.x, firstLineLength);

            firstLineEndRect.gameObject.SetActive(true);

            var reflectedDirection = CalculateReflectionDirection(direction, hitRightWall);
            var reflectedAngle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg - 90f;
            var secondLineLength = CalculateDistanceToWall(hitPoint, reflectedDirection, out _, out _);

            secondLineRect.gameObject.SetActive(true);
            secondLineRect.localPosition = firstLineEndRect.localPosition;
            secondLineRect.rotation = Quaternion.Euler(0, 0, reflectedAngle);
            secondLineRect.sizeDelta = new Vector2(secondLineRect.sizeDelta.x, secondLineLength);
        }
        else
        {
            firstLineRect.gameObject.SetActive(false);
            firstLineEndRect.gameObject.SetActive(false);
            secondLineRect.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && firstLineEndRect.gameObject.activeInHierarchy)
        {
            // 채워진 원 그리기
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // 더 투명한 빨간색
            Gizmos.DrawSphere(firstLineEndRect.position, checkRadius);

            // 와이어프레임 원 그리기
            Gizmos.color = new Color(1f, 1f, 0f, 1f); // 노란색
            Gizmos.DrawWireSphere(firstLineEndRect.position, checkRadius);
        }
    }

    private float CalculateDistanceToWall(Vector2 startPos, Vector2 direction, out Vector2 hitPoint,
                                          out bool hitRightWall)
    {
        var canvasWidth = canvasRect.rect.width;
        var leftBound = -canvasWidth * 0.5f;
        var rightBound = canvasWidth * 0.5f;

        float distance;
        hitRightWall = false;
        hitPoint = Vector2.zero;

        if (Mathf.Abs(direction.x) < 0.0001f)
        {
            distance = canvasRect.rect.height;
        }
        else
        {
            var distToLeftWall = (leftBound - startPos.x) / direction.x;
            var distToRightWall = (rightBound - startPos.x) / direction.x;

            if (direction.x > 0)
            {
                distance = distToRightWall;
                hitRightWall = true;
            }
            else
            {
                distance = distToLeftWall;
                hitRightWall = false;
            }

            distance = Mathf.Abs(distance);
        }

        hitPoint = startPos + direction * distance;

        return distance;
    }

    private Vector2 CalculateReflectionDirection(Vector2 incomingDirection, bool hitRightWall)
    {
        // 벽에 부딪힐 때의 반사 방향 계산
        // 오른쪽 벽: (1,0), 왼쪽 벽: (-1,0)이 법선 벡터
        Vector2 normal = hitRightWall ? Vector2.left : Vector2.right;
        return Vector2.Reflect(incomingDirection, normal);
    }
}