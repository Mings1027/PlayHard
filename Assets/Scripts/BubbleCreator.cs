using System;
using System.Collections.Generic;
using System.Linq;
using DataControl;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleCreator : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private GameObject previewBubblePrefab;
    [SerializeField] private BubbleData[] bubbleDataList;

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
            bubble.Initialize(bubbleData, specialData, FuncManager.TriggerEvent<List<Bubble>>(FuncEvent.AllBubbles));
        }
        else
        {
            Debug.LogError($"BubbleData not found for type {bubbleType}");
        }

        return bubble;
    }

    public Bubble CreateRandomBubble(Vector3 position, Quaternion rotation)
    {
        var existingBubbleTypeInStage = FuncManager.TriggerEvent<HashSet<BubbleType>>(FuncEvent.ExistingBubbleType);
        BubbleType randomType;
        if (existingBubbleTypeInStage.Count == 0)
        {
            randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);
        }
        else
        {
            randomType = existingBubbleTypeInStage.ElementAt(Random.Range(0, existingBubbleTypeInStage.Count));
        }

        return CreateBubble(randomType, position, rotation);
    }

    public GameObject CreatePreviewBubble()
    {
        return Instantiate(previewBubblePrefab);
    }
}