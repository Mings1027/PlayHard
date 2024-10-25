using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DataControl
{
    [CreateAssetMenu(fileName = "New Bubble", menuName = "Bubble Game/Bubble Data")]
    public class BubbleData : ScriptableObject
    {
        [SerializeField] private BubbleType bubbleType;
        [SerializeField] private Sprite bubbleSprite;
        [SerializeField] private bool isRandomBubble;

        public BubbleType BubbleType => bubbleType;
        public Sprite BubbleSprite => bubbleSprite;
        public bool IsRandomBubble => isRandomBubble;

    }
}