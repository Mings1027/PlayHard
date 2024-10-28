using System;
using System.Collections.Generic;
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
            specialBubbleData = null;
            bubblePosition = position;
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

#if UNITY_EDITOR

        public void AddBubbleData(Vector2Int position, BubbleData bubbleData)
        {
            BubbleDataAndPosition existingBubble = null;
            for (int i = 0; i < bubbleDataPositions.Count; i++)
            {
                if (bubbleDataPositions[i].bubblePosition == position)
                {
                    existingBubble = bubbleDataPositions[i];
                    break;
                }
            }

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
                    existingBubble.specialBubbleData = null;
                }
            }
            else
            {
                if (bubbleData is SpecialBubbleData)
                {
                    Debug.LogWarning("Cannot add SpecialBubbleData without a base BubbleData");
                    return;
                }

                bubbleDataPositions.Add(new BubbleDataAndPosition(bubbleData, position));
            }
        }

        public void RemoveBubbleData(Vector2Int position)
        {
            BubbleDataAndPosition bubbleData = null;
            for (var i = 0; i < bubbleDataPositions.Count; i++)
            {
                if (bubbleDataPositions[i].bubblePosition == position)
                {
                    bubbleData = bubbleDataPositions[i];
                    break;
                }
            }

            if (bubbleData != null)
            {
                if (bubbleData.specialBubbleData != null)
                {
                    bubbleData.specialBubbleData = null;
                }
                else
                {
                    for (var i = 0; i < bubbleDataPositions.Count; i++)
                    {
                        if (bubbleDataPositions[i].bubblePosition == position)
                        {
                            bubbleDataPositions.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Special 버블 데이터 조회를 위한 새로운 메서드
        public SpecialBubbleData GetSpecialBubbleDataAt(Vector2Int position)
        {
            for (var i = 0; i < bubbleDataPositions.Count; i++)
            {
                if (bubbleDataPositions[i].bubblePosition == position)
                {
                    return bubbleDataPositions[i].specialBubbleData;
                }
            }

            return null;
        }

        // 기본 버블 데이터만 조회하는 메서드
        public BubbleData GetBaseBubbleDataAt(Vector2Int position)
        {
            for (var i = 0; i < bubbleDataPositions.Count; i++)
            {
                if (bubbleDataPositions[i].bubblePosition == position)
                {
                    return bubbleDataPositions[i].bubbleData;
                }
            }

            return null;
        }
#endif
    }
}