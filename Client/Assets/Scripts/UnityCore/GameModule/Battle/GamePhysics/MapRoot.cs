using UnityEngine;

namespace UnityCore.GameModule.Battle.GamePhysics
{
    public class MapRoot : MonoBehaviour
    {
        [SerializeField] private Transform transCameraRoot;
        [SerializeField] private Transform transEnvCollider;
        [SerializeField] private Transform blueTower;
        [SerializeField] private Transform redTower;
        [SerializeField] private Transform blueCrystal;
        [SerializeField] private Transform redCrystal;
    }
}