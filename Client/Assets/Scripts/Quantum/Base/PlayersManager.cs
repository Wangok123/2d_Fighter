using System.Collections;
using System.Collections.Generic;
using Quantum;
using Quantum.QuantumView;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    private Dictionary<EntityRef, PlayerViewController> _playersByEntityRefs = new Dictionary<EntityRef, PlayerViewController>();
}
