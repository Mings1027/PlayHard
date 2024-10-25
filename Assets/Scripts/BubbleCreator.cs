using System;
using DataControl;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleCreator : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private GameObject previewBubblePrefab;
    [SerializeField] private BubbleGlobalSetting bubbleGlobalSetting;
    public float BubbleSize { get; private set; }

    private void Awake()
    {
        var mainCamera = Camera.main;
        var screenHeight = 2f * mainCamera.orthographicSize;
        var screenWidth = screenHeight * mainCamera.aspect;

        var containerWidth = screenWidth;

        BubbleSize = containerWidth / 11;
        bubblePrefab.transform.localScale = Vector3.one * BubbleSize;
        previewBubblePrefab.transform.localScale = Vector3.one * BubbleSize;
    }

    public Bubble CreateBubble(BubbleType bubbleType, Vector3 position, Quaternion rotation)
    {
        var bubble = Instantiate(bubblePrefab, position, rotation).GetComponent<Bubble>();
        bubble.SetType(bubbleType, bubbleGlobalSetting.BubbleTypeSpriteDict[bubbleType].BubbleSprite);
        return bubble;
    }

    public Bubble CreateRandomBubble(Vector3 position, Quaternion rotation)
    {
        var randomType = (BubbleType)Random.Range(0, Enum.GetValues(typeof(BubbleType)).Length);
        var bubble = Instantiate(bubblePrefab, position, rotation).GetComponent<Bubble>();
        bubble.SetType(randomType, bubbleGlobalSetting.BubbleTypeSpriteDict[randomType].BubbleSprite);
        return bubble;
    }

    public GameObject CreatePreviewBubble()
    {
        return Instantiate(previewBubblePrefab);
    }
}