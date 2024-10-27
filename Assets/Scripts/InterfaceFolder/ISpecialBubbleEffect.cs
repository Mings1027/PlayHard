using System.Collections.Generic;
using UnityEngine;

namespace InterfaceFolder
{
    public interface ISpecialBubbleEffect
    {
        float CheckSize { get; set; }
        Collider2D[] Colliders { get; set; }
        void ExecuteSpecialEffect(Bubble triggerBubble);
    }
}