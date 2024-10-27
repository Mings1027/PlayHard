using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(fileName = "Bubble Global Setting", menuName = "Bubble Game/Global Setting")]
    public class BubbleGlobalSettingSO : ScriptableObject
    {
        [SerializeField] private SpecialBubbleData[] specialBubbleData;
        public SpecialBubbleData[] SpecialBubbleData => specialBubbleData;
    }
}