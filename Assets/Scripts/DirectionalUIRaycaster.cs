using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionalUIRaycaster : MonoBehaviour
{
    [SerializeField] private RectTransform shootPivot;
    [SerializeField] private RectTransform canvasRect;

    [Header("First Line"), SerializeField] private RectTransform firstLineRect;
    [SerializeField] private RectTransform firstLineEndPoint;

    [Header("Second Line"), SerializeField]
    private RectTransform secondLineRect;

    [SerializeField] private RectTransform secondLineEndPoint;

    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private Vector3 _mousePos;
    private Bubble _hitBubble;
    private Vector3 direction;

    private bool _isReflection;
    private Vector3 _reflectionDirection;
    private Vector2 _hitPoint;

    private PointerEventData _pointerEventData;
    private List<RaycastResult> _raycastResults;

    private void Awake()
    {
        if (graphicRaycaster == null) graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        _pointerEventData = new PointerEventData(EventSystem.current);
        _raycastResults = new List<RaycastResult>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ShootRay();
            UpdateLineLength();
            CheckBubbleHit();
        }
    }

    private void ShootRay()
    {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos = Camera.main.WorldToScreenPoint(mouseWorldPos);
        direction = (_mousePos - shootPivot.position).normalized;
    }

    private void UpdateLineLength()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, shootPivot.position, null,
            out var localShootPos);
        var canvasHalfWidth = canvasRect.rect.width * 0.5f;
        float maxLength;
        if (direction.x > 0) // 오른쪽
        {
            maxLength = (canvasHalfWidth - localShootPos.x) / Mathf.Abs(direction.x);
        }
        else // 왼쪽
        {
            maxLength = (-canvasHalfWidth - localShootPos.x) / Mathf.Abs(direction.x);
        }

        maxLength = Mathf.Abs(maxLength);

        firstLineRect.sizeDelta = new Vector2(firstLineRect.sizeDelta.x, maxLength);
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        firstLineRect.localPosition = shootPivot.localPosition;
        firstLineRect.rotation = Quaternion.Euler(0, 0, angle);

        UpdateReflectionLine(firstLineEndPoint.localPosition, canvasHalfWidth);
    }

    private void UpdateReflectionLine(Vector2 hitPoint, float canvasHalfWidth)
    {
        var normal = direction.x > 0 ? Vector2.left : Vector2.right;
        _reflectionDirection = Vector2.Reflect(direction, normal);

        float reflectionMaxLength;
        if (_reflectionDirection.x > 0)
        {
            reflectionMaxLength = (canvasHalfWidth - hitPoint.x) / Mathf.Abs(_reflectionDirection.x);
        }
        else
        {
            reflectionMaxLength = (-canvasHalfWidth - hitPoint.x) / Mathf.Abs(_reflectionDirection.x);
        }

        reflectionMaxLength = Mathf.Abs(reflectionMaxLength);

        secondLineRect.sizeDelta = new Vector2(secondLineRect.sizeDelta.x, reflectionMaxLength * 2);
        var reflectionAngle = Mathf.Atan2(_reflectionDirection.y, _reflectionDirection.x) * Mathf.Rad2Deg - 90f;
        secondLineRect.localPosition = hitPoint;
        secondLineRect.rotation = Quaternion.Euler(0, 0, reflectionAngle);
    }

    private void CheckBubbleHit()
    {
       
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_mousePos, 30);
    }
}