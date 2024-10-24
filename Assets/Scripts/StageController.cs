using System;
using UnityEngine;
using System.Collections.Generic;
using DataControl;
using Cysharp.Threading.Tasks;

public class StageController : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private BoxCollider2D bubbleContainer;
    [SerializeField] private GameObject bubbleShooter;
    [SerializeField] private float bubbleMoveSpeed = 5f;
    [SerializeField] private float spawnInterval = 0.5f;

    private Camera _mainCamera;
    private StageData _currentStage;
    private readonly Dictionary<BubblePath, List<GameObject>> _pathBubbles = new();

    private void Awake()
    {
        _mainCamera = Camera.main;
        bubbleShooter.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.AddEvent<StageData>(ActionEvent.CreateStage, CreateStage);
    }

    private void OnDisable()
    {
        EventManager.RemoveEvent<StageData>(ActionEvent.CreateStage, CreateStage);
    }

    private async void CreateStage(StageData stageData)
    {
        _currentStage = stageData;
        SetupLayoutCollider();
        await CreateBubblesForAllPaths();
        bubbleShooter.SetActive(true);
    }

    private void SetupLayoutCollider()
    {
        var screenHeight = 2f * _mainCamera.orthographicSize;
        var screenWidth = screenHeight * _mainCamera.aspect;

        var safeArea = Screen.safeArea;
        var topSafeAreaWorld = _mainCamera.ScreenToWorldPoint(new Vector3(0, safeArea.yMax, 0)).y;
        
        var containerSize = bubbleContainer.size;
        containerSize.x = screenWidth;
        containerSize.y = _currentStage.height;
        bubbleContainer.size = containerSize;

        var containerPosition = bubbleContainer.transform.position;
        containerPosition.x = 0;
        containerPosition.y = topSafeAreaWorld - containerSize.y / 2f;
        bubbleContainer.transform.position = containerPosition;
    }

    private async UniTask CreateBubblesForAllPaths()
    {
        var bubbleSize = bubbleContainer.size.x / _currentStage.Width;

        var taskCount = _currentStage.bubblePaths.Count;
        var tasks = new UniTask[taskCount];

        for (var i = 0; i < taskCount; i++)
        {
            var path = _currentStage.bubblePaths[i];
            tasks[i] = CreateBubblesForPath(path, bubbleSize);
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask CreateBubblesForPath(BubblePath path, float bubbleSize)
    {
        _pathBubbles[path] = new List<GameObject>();

        for (var i = 0; i < path.points.Count; i++)
        {
            var bubbles = _pathBubbles[path];
            var bubbleCount = bubbles.Count;

            if (bubbleCount > 0)
            {
                // 기존 버블들 동시에 이동
                var moveTasks = new UniTask[bubbleCount];

                for (var j = bubbleCount - 1; j >= 0; j--)
                {
                    var targetPosition = CalculateBubblePosition(path.points[j + 1]);
                    moveTasks[j] = MoveBubble(bubbles[j], targetPosition);
                }

                await UniTask.WhenAll(moveTasks);
            }

            // 새 버블 생성 (첫 번째 위치에)
            var spawnPosition = CalculateBubblePosition(path.points[0]);
            var bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
            bubble.transform.localScale = Vector3.one * bubbleSize;
            bubble.transform.SetParent(bubbleContainer.transform);
            bubbles.Insert(0, bubble);

            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: destroyCancellationToken);
        }
    }

    private Vector3 CalculateBubblePosition(Vector2Int point)
    {
        var containerTopPosition = bubbleContainer.transform.position +
                                   Vector3.up * (bubbleContainer.size.y / 2f);

        var bubbleSize = bubbleContainer.size.x / _currentStage.Width;
        var bubbleRadius = bubbleSize / 2f;
        var verticalSpacing = bubbleSize * 0.866f;

        containerTopPosition.y -= bubbleRadius;

        var bubblePosition = containerTopPosition;
        var xOffset = point.y % 2 == 1 ? bubbleSize * 0.5f : 0f;
        bubblePosition.x += (point.x - (_currentStage.Width - 1) / 2f) * bubbleSize + xOffset;
        bubblePosition.y -= point.y * verticalSpacing;

        return bubblePosition;
    }

    private async UniTask MoveBubble(GameObject bubble, Vector3 targetPosition)
    {
        var elapsedTime = 0f;
        var startPosition = bubble.transform.position;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * bubbleMoveSpeed;
            bubble.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            await UniTask.Yield(destroyCancellationToken);
        }

        bubble.transform.position = targetPosition;
    }
}