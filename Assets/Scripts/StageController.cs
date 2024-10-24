using System;
using UnityEngine;
using System.Collections.Generic;
using DataControl;
using Cysharp.Threading.Tasks;
using System.Threading;

public class StageController : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private StageData currentStage;
    [SerializeField] private BoxCollider2D bubbleContainer;
    [SerializeField] private float bubbleMoveSpeed = 5f;
    [SerializeField] private float spawnInterval = 0.5f;

    private Camera mainCamera;
    private Dictionary<BubblePath, List<GameObject>> pathBubbles = new();
    private CancellationTokenSource cts;

    private void Awake()
    {
        mainCamera = Camera.main;
        cts = new CancellationTokenSource();
    }

    private void Start()
    {
        SetStageData(currentStage);
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private void SetupLayoutCollider()
    {
        float screenHeight = 2f * mainCamera.orthographicSize;
        float screenWidth = screenHeight * mainCamera.aspect;

        Vector2 containerSize = bubbleContainer.size;
        containerSize.x = screenWidth;
        containerSize.y = currentStage.height;
        bubbleContainer.size = containerSize;

        Vector3 containerPosition = bubbleContainer.transform.position;
        containerPosition.x = 0;
        containerPosition.y = screenHeight / 2f - containerSize.y / 2f;
        bubbleContainer.transform.position = containerPosition;
    }

    private Vector3 CalculateBubblePosition(Vector2Int point)
    {
        Vector3 containerTopPosition = bubbleContainer.transform.position + 
                                     Vector3.up * (bubbleContainer.size.y / 2f);

        float bubbleSize = bubbleContainer.size.x / currentStage.Width;
        float bubbleRadius = bubbleSize / 2f;
        float horizontalSpacing = bubbleSize;
        float verticalSpacing = bubbleSize * 0.866f;

        containerTopPosition.y -= bubbleRadius;

        Vector3 bubblePosition = containerTopPosition;
        float xOffset = point.y % 2 == 1 ? horizontalSpacing * 0.5f : 0f;
        bubblePosition.x += (point.x - (currentStage.Width - 1) / 2f) * horizontalSpacing + xOffset;
        bubblePosition.y -= point.y * verticalSpacing;

        return bubblePosition;
    }

    private async UniTask CreateBubblesForPath(BubblePath path, float bubbleSize)
    {
        pathBubbles[path] = new List<GameObject>();

        for (int i = 0; i < path.points.Count; i++)
        {
            // 기존 버블들 동시에 이동
            var moveTasks = new List<UniTask>();
            List<GameObject> bubbles = pathBubbles[path];
            
            for (int j = bubbles.Count - 1; j >= 0; j--)
            {
                Vector3 targetPosition = CalculateBubblePosition(path.points[j + 1]);
                moveTasks.Add(MoveBubble(bubbles[j], targetPosition));
            }

            // 새 버블 생성 (첫 번째 위치에)
            Vector3 spawnPosition = CalculateBubblePosition(path.points[0]);
            GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
            bubble.transform.localScale = Vector3.one * bubbleSize;
            bubble.transform.SetParent(bubbleContainer.transform);
            bubbles.Insert(0, bubble);

            // 모든 이동이 완료될 때까지 대기
            if (moveTasks.Count > 0)
            {
                await UniTask.WhenAll(moveTasks);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: cts.Token);
        }
    }

    private async UniTask MoveBubble(GameObject bubble, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = bubble.transform.position;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * bubbleMoveSpeed;
            bubble.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            await UniTask.Yield(cts.Token);
        }

        bubble.transform.position = targetPosition;
    }

    private async void SetStageData(StageData stageData)
    {
        currentStage = stageData;
        SetupLayoutCollider();

        float bubbleSize = bubbleContainer.size.x / currentStage.Width;
        
        // 모든 경로에 대해 동시에 버블 생성 시작
        var tasks = new List<UniTask>();
        foreach (var path in currentStage.bubblePaths)
        {
            tasks.Add(CreateBubblesForPath(path, bubbleSize));
        }

        // 모든 경로의 버블 생성이 완료될 때까지 대기
        await UniTask.WhenAll(tasks);
    }
}