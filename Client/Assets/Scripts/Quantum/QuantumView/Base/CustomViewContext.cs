using Photon.Deterministic;
using UnityEngine;

namespace Quantum.QuantumView.Base
{
    public class CustomViewContext : MonoBehaviour, IQuantumViewContext
    {
        public PlayerRef LocalPlayer;
        public EntityRef LocalPlayerEntity;
        public FPVector2 LocalCharacterLastDirection;
    }
}