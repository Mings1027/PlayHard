using System;
using System.Collections.Generic;
using DataControl;
using EventControl;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleCreator : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private GameObject previewBubblePrefab;
    [SerializeField] private BubbleData[] bubbleDataList;
    [SerializeField] private BubbleGlobalSettingSO bubbleGlobalSettingSO;
    [SerializeField] private float specialBubbleProbability = 0.3f;

    private Dictionary<BubbleType, BubbleData> _bubbleDataDict;
    public static float BubbleSize { get; private set; }

    private void Awake()
    {
        _bubbleDataDict = new Dictionary<BubbleType, BubbleData>();
        for (int i = 0; i < bubbleDataList.Length; i++)
        {
            _bubbleDataDict[bubbleDataList[i].BubbleType] = bubbleDataList[i];
        }

        var mainCamera = Camera.main;
        var screenHeight = 2f * mainCamera.orthographicSize;
        var screenWidth = screenHeight * mainCamera.aspect;

        var containerWidth = screenWidth;

        BubbleSize = containerWidth / 11;
        bubblePrefab.transform.localScale = Vector3.one * BubbleSize;
        previewBubblePrefab.transform.localScale = Vector3.one * BubbleSize;
    }

    private void OnEnable()
    {
        BubbleEventManager.AddEvent<BubbleType, Vector3, SpecialBubbleData, Bubble>(BubbleEvent.CreateBubble, CreateBubble);
        BubbleEventManager.AddEvent<Vector3, Bubble>(BubbleEvent.RandomShooterBubble, RandomShooterBubble);
        BubbleEventManager.AddEvent<Vector3, Bubble>(BubbleEvent.RandomStageBubble, RandomStageBubble);
        BubbleEventManager.AddEvent(BubbleEvent.CreatePreviewBubble, CreatePreviewBubble);
    }

    private void OnDisable()
    {
        BubbleEventManager.RemoveEvent<BubbleType, Vector3, SpecialBubbleData, Bubble>(BubbleEvent.CreateBubble, CreateBubble);
        BubbleEventManager.RemoveEvent<Vector3, Bubble>(BubbleEvent.RandomShooterBubble, RandomShooterBubble);
        BubbleEventManager.RemoveEvent<Vector3, Bubble>(BubbleEvent.RandomStageBubble, RandomStageBubble);
        BubbleEventManager.RemoveEvent(BubbleEvent.CreatePreviewBubble, CreatePreviewBubble);
    }

    private Bubble CreateBubble(BubbleType bubbleType, Vector3 position,
                                SpecialBubbleData specialData = null)
    {
        var bubble = Instantiate(bubblePrefab, position, Quaternion.identity).GetComponent<Bubble>();
        if (_bubbleDataDict.TryGetValue(bubbleType, out var bubbleData))
        {
            bubble.Initialize(bubbleData, specialData);
        }

        return bubble;
    }

    private Bubble RandomShooterBubble(Vector3 position)
    {
        var randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);
        return CreateBubble(randomType, position);
    }

    private Bubble RandomStageBubble(Vector3 position)
    {
        var canCreateSpecialBubble = Random.value < specialBubbleProbability;
        var randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);

        SpecialBubbleData specialData = null;
        if (canCreateSpecialBubble && bubbleGlobalSettingSO.SpecialBubbleData.Length > 0)
        {
            specialData = bubbleGlobalSettingSO.SpecialBubbleData
                [Random.Range(0, bubbleGlobalSettingSO.SpecialBubbleData.Length)];
        }

        return CreateBubble(randomType, position, specialData);
    }

    private GameObject CreatePreviewBubble()
    {
        return Instantiate(previewBubblePrefab);
    }
}