using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataControl
{
    [Serializable]
    public class BubblePath
    {
        public List<Vector2Int> points = new();
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Stage", menuName = "Bubble Game/Stage Data", order = 1)]
    public class StageData : ScriptableObject
    {
        public int Width { get; private set; } = 11;
        public int height = 10;
        public List<BubblePath> bubblePaths = new();

        public int totalBubbles;

        [SerializeField] private int totalPoints;

        public int TotalPoints
        {
            get => totalPoints;
            private set => totalPoints = value;
        }

        public void UpdateTotalPoints()
        {
            TotalPoints = bubblePaths.Sum(path => path.points.Count);
        }

        // 특정 위치에 버블이 있는지 확인하는 메서드
        public bool HasBubbleAt(Vector2Int position)
        {
            for (var i = 0; i < bubblePaths.Count; i++)
            {
                var path = bubblePaths[i];
                if (path.points.Contains(position))
                    return true;
            }

            return false;
        }

        // 특정 위치의 버블 경로 가져오기
        public BubblePath GetBubblePath(Vector2Int position)
        {
            return bubblePaths.Find(path => path.points.Contains(position));
        }

    }
}