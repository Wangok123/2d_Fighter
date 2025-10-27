using Quantum.QuantumView.Base;
using UnityEngine;

namespace Quantum.QuantumView
{
    public class CharacterView : QuantumEntityViewComponent<CustomViewContext>
    {
        /// <summary>
        /// Direction the character is currently facing: 1 for right, -1 for left.
        /// </summary>
        [HideInInspector] public int LookDirection;
    }
}