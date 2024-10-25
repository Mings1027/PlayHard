using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataControl
{
    [Serializable]
    public class BubbleTypeAndData
    {
        public BubbleType bubbleType;
        public BubbleData bubbleData;
    }

    [CreateAssetMenu(fileName = "New Bubble", menuName = "Bubble Game/Bubble Data")]
    public class BubbleGlobalSetting : ScriptableObject
    {
        [SerializeField] private BubbleTypeAndData[] bubbleTypeAndDatas;
        private Dictionary<BubbleType, BubbleData> bubbleTypeSpriteDict;
        public Dictionary<BubbleType, BubbleData> BubbleTypeSpriteDict => bubbleTypeSpriteDict;

        private void OnEnable()
        {
            bubbleTypeSpriteDict = new Dictionary<BubbleType, BubbleData>();
            for (int i = 0; i < bubbleTypeAndDatas.Length; i++)
            {
                bubbleTypeSpriteDict.Add(bubbleTypeAndDatas[i].bubbleType, bubbleTypeAndDatas[i].bubbleData);
            }
        }
    }
}