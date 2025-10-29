﻿using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityCore.Utils.Transform
{
    public static class VectorUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 X0Z(this Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 X0Y(this Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        
        // 2D
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 X0(this Vector2 v)
        {
            v.y = 0f;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
    }
}