using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    [Serializable]
    public class BubbleDataAndPosition
    {
        public BubbleData bubbleData;
        public Vector2Int bubblePosition;

        public BubbleDataAndPosition(BubbleData data, Vector2Int position)
        {
            bubbleData = data;
            bubblePosition = position;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Stage", menuName = "Bubble Game/Stage Data", order = 1)]
    public class StageData : ScriptableObject
    {
        public int Width { get; private set; } = 11;
        public int height = 10;

        [FormerlySerializedAs("bubblePositions")] [SerializeField] private List<BubbleDataAndPosition> bubbleDataPositions = new();
        public List<BubbleDataAndPosition> BubbleDataPositions => bubbleDataPositions;
        public int totalBubbles;

        [SerializeField] private int totalPoints;

        public void AddBubbleData(Vector2Int position, BubbleData bubbleData)
        {
            bubbleDataPositions.RemoveAll(b => b.bubblePosition == position);
            bubbleDataPositions.Add(new BubbleDataAndPosition(bubbleData, position));
        }

        public void RemoveBubbleData(Vector2Int position)
        {
            bubbleDataPositions.RemoveAll(b => b.bubblePosition == position);
        }

        public BubbleData GetBubbleDataAt(Vector2Int position)
        {
            return bubbleDataPositions.FirstOrDefault(b => b.bubblePosition == position)?.bubbleData;
        }
    }
}