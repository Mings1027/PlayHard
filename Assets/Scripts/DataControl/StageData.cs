using System;
using UnityEngine;

namespace DataControl
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Stage", menuName = "Bubble Game/Stage Data", order = 1)]
    public class StageData : ScriptableObject, ISerializationCallbackReceiver
    {
        public int width = 8; // 전체 그리드 너비
        public int height = 10; // 전체 그리드 높이

        [NonSerialized] public BubbleType[,] bubbleGrid; // 실제 사용하는 2차원 배열

        [SerializeField] private BubbleType[] serializedGrid; // 직렬화를 위한 1차원 배열

        public void InitializeGrid()
        {
            var oldGrid = bubbleGrid; // 기존 그리드 저장
            bubbleGrid = new BubbleType[width, height];

            // 기존 데이터가 있다면 복사
            if (oldGrid != null)
            {
                int minWidth = Mathf.Min(oldGrid.GetLength(0), width);
                int minHeight = Mathf.Min(oldGrid.GetLength(1), height);

                for (int y = 0; y < minHeight; y++)
                {
                    int rowWidth = (y % 2 == 0) ? minWidth : Mathf.Min(minWidth - 1, width - 1);
                    for (int x = 0; x < rowWidth; x++)
                    {
                        bubbleGrid[x, y] = oldGrid[x, y];
                    }
                }
            }
        }

        // 직렬화 전에 2차원 배열을 1차원으로 변환
        public void OnBeforeSerialize()
        {
            if (bubbleGrid == null) return;

            serializedGrid = new BubbleType[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    serializedGrid[y * width + x] = bubbleGrid[x, y];
                }
            }
        }

        // 역직렬화 후 1차원 배열을 2차원으로 변환
        public void OnAfterDeserialize()
        {
            if (serializedGrid == null) return;

            bubbleGrid = new BubbleType[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bubbleGrid[x, y] = serializedGrid[y * width + x];
                }
            }
        }
    }
}