using System.Collections.Generic;
using UnityEngine;

namespace InterfaceFolder
{
    public interface ISpecialBubbleEffect
    {
        float CheckSize { get; set; }
        Collider[] Colliders { get; set; }
        void SetColliders(Bubble bubble);
        List<Bubble> GetBubblesToPop(Bubble triggerBubble);
    }
}