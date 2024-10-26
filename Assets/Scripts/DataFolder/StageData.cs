using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataControl
{
    [Serializable]
    public class BubbleDataAndPosition
    {
        public BubbleData bubbleData;
        public SpecialBubbleData specialBubbleData;
        public Vector2Int bubblePosition;

        public BubbleDataAndPosition(BubbleData data, Vector2Int position)
        {
            bubbleData = data;
            bubblePosition = position;
            specialBubbleData = null;
        }

        public BubbleDataAndPosition(BubbleData data, SpecialBubbleData specialData, Vector2Int position)
        {
            bubbleData = data;
            specialBubbleData = specialData;
            bubblePosition = position;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Stage", menuName = "Bubble Game/Stage Data", order = 1)]
    public class StageData : ScriptableObject
    {
        public int Width { get; private set; } = 11;
        public int height = 10;

        [SerializeField] private List<BubbleDataAndPosition> bubbleDataPositions = new();
        public List<BubbleDataAndPosition> BubbleDataPositions => bubbleDataPositions;
        public int bubbleAmmo;

        public void AddBubbleData(Vector2Int position, BubbleData bubbleData)
        {
            var existingBubble = bubbleDataPositions.FirstOrDefault(b => b.bubblePosition == position);

            if (existingBubble != null)
            {
                // 새로 추가하려는 버블이 SpecialBubbleData인 경우
                if (bubbleData is SpecialBubbleData specialBubbleData)
                {
                    existingBubble.specialBubbleData = specialBubbleData;
                }
                // 일반 BubbleData인 경우
                else
                {
                    existingBubble.bubbleData = bubbleData;
                    existingBubble.specialBubbleData = null; // Special 데이터 초기화
                }
            }
            else
            {
                // 새 위치에 버블 추가
                if (bubbleData is SpecialBubbleData specialBubbleData)
                {
                    // SpecialBubbleData는 단독으로 추가할 수 없음
                    Debug.LogWarning("Cannot add SpecialBubbleData without a base BubbleData");
                    return;
                }

                bubbleDataPositions.Add(new BubbleDataAndPosition(bubbleData, position));
            }
        }

        public void RemoveBubbleData(Vector2Int position)
        {
            bubbleDataPositions.RemoveAll(b => b.bubblePosition == position);
        }

        public BubbleData GetBubbleDataAt(Vector2Int position)
        {
            var bubbleData = bubbleDataPositions.FirstOrDefault(b => b.bubblePosition == position);
            return bubbleData?.specialBubbleData ?? bubbleData?.bubbleData;
        }

        // Special 버블 데이터 조회를 위한 새로운 메서드
        public SpecialBubbleData GetSpecialBubbleDataAt(Vector2Int position)
        {
            return bubbleDataPositions.FirstOrDefault(b => b.bubblePosition == position)?.specialBubbleData;
        }

        // 기본 버블 데이터만 조회하는 메서드
        public BubbleData GetBaseBubbleDataAt(Vector2Int position)
        {
            return bubbleDataPositions.FirstOrDefault(b => b.bubblePosition == position)?.bubbleData;
        }
    }
}