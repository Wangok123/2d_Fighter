using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Configs
{
    // [CreateAssetMenu(fileName = "PlayerCfgSO", menuName = "GameTools/Configs/PlayerCfgSO")]
    public class PlayerCfgSO : ScriptableObject
    {
        [Header("Player Info")] public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public float airSpeedMultiplier = 0.8f; // 空中降落时的速度衰减系数

        [Header("Dash Info")] public float dashTime = 0.5f;
        public float dashSpeed = 25f;
        public float dashCooldown = 1f;

        [Header("Wall Slide Info")] public float wallSlideSpeedMultiplier = 0.7f;
        public Vector2 wallJumpForce = new Vector2(10f, 15f);
        public float wallJumpCooldown = 0.3f;
        public float attackSlideTime = 0.1f;

        [Header("Attack Info")] public List<Vector2> attackMovement;
        public double comboResetTime = 0.6f;
    }
}