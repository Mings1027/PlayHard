using System;
using System.Collections.Generic;
using DataControl;
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
    public float BubbleSize { get; private set; }

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

    public Bubble CreateBubble(BubbleType bubbleType, Vector3 position, Quaternion rotation,
                               SpecialBubbleData specialData = null)
    {
        var bubble = Instantiate(bubblePrefab, position, rotation).GetComponent<Bubble>();
        if (_bubbleDataDict.TryGetValue(bubbleType, out var bubbleData))
        {
            bubble.Initialize(bubbleData, specialData);
        }
        else
        {
            Debug.LogError($"BubbleData not found for type {bubbleType}");
        }

        return bubble;
    }

    public Bubble CreateRandomBubble(Vector3 position, Quaternion rotation)
    {
        var randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);
        return CreateBubble(randomType, position, rotation);
    }

    public Bubble CreateRandomBubbleWithChanceOfSpecial(Vector3 position, Quaternion rotation)
    {
        var canCreateSpecialBubble = Random.value < specialBubbleProbability;
        var randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);

        SpecialBubbleData specialData = null;
        if (canCreateSpecialBubble && bubbleGlobalSettingSO.SpecialBubbleData.Length > 0)
        {
            specialData = bubbleGlobalSettingSO.SpecialBubbleData
                [Random.Range(0, bubbleGlobalSettingSO.SpecialBubbleData.Length)];
        }

        return CreateBubble(randomType, position, rotation, specialData);
    }

    public GameObject CreatePreviewBubble()
    {
        return Instantiate(previewBubblePrefab);
    }
    
    public void DisableBubbleCollider(Bubble bubble)
    {
        if (bubble.TryGetComponent(out CircleCollider2D circleCollider2D))
        {
            circleCollider2D.enabled = false;
        }
    }
}