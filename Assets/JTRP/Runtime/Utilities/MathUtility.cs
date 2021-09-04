using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace JTRP.Utility
{
    public static class MathUtility
    {
        public static float2 ToF2(this Vector2 vector2)
        {
            return new float2(vector2.x, vector2.y);
        }
        public unsafe static float2[] ToF2(this Vector2[] vector2)
        {
            var f2 = new float2[vector2.Length];
            fixed (void* v2Ptr = vector2)
            {
                fixed (void* f2Ptr = f2)
                {
                    UnsafeUtility.MemCpy(f2Ptr, v2Ptr, vector2.Length * (long)UnsafeUtility.SizeOf<float2>());
                }
            }
            return f2;
        }
        public static Vector2 ToV2(this float2 f2)
        {
            return new Vector2(f2.x, f2.y);
        }
        public unsafe static Vector2[] ToV2(this float2[] f2)
        {
            var v2 = new Vector2[f2.Length];
            fixed (void* f2Ptr = f2)
            {
                fixed (void* v2Ptr = v2)
                {
                    UnsafeUtility.MemCpy(v2Ptr, f2Ptr, f2.Length * (long)UnsafeUtility.SizeOf<Vector2>());
                }
            }
            return v2;
        }



        public static float3 ToF3(this Vector3 vector3)
        {
            return new float3(vector3.x, vector3.y, vector3.z);
        }
        public unsafe static float3[] ToF3(this Vector3[] vector3)
        {
            var f3 = new float3[vector3.Length];
            fixed (void* v3Ptr = vector3)
            {
                fixed (void* f3Ptr = f3)
                {
                    UnsafeUtility.MemCpy(f3Ptr, v3Ptr, vector3.Length * (long)UnsafeUtility.SizeOf<float3>());
                }
            }
            return f3;
        }
        public static Vector3 ToV3(this float3 f3)
        {
            return new Vector3(f3.x, f3.y, f3.z);
        }
        public unsafe static Vector3[] ToV3(this float3[] f3)
        {
            var v3 = new Vector3[f3.Length];
            fixed (void* f3Ptr = f3)
            {
                fixed (void* v3Ptr = v3)
                {
                    UnsafeUtility.MemCpy(v3Ptr, f3Ptr, f3.Length * (long)UnsafeUtility.SizeOf<Vector3>());
                }
            }
            return v3;
        }


        public static float4 ToF4(this Vector4 vector4)
        {
            return new float4(vector4.x, vector4.y, vector4.z, vector4.w);
        }
        public unsafe static float4[] ToF4(this Vector4[] vector4)
        {
            var f4 = new float4[vector4.Length];
            fixed (void* v4Ptr = vector4)
            {
                fixed (void* f4Ptr = f4)
                {
                    UnsafeUtility.MemCpy(f4Ptr, v4Ptr, vector4.Length * (long)UnsafeUtility.SizeOf<float4>());
                }
            }
            return f4;
        }
        public static Vector4 ToV4(this float4 f4)
        {
            return new Vector4(f4.x, f4.y, f4.z, f4.w);
        }
        public unsafe static Vector4[] ToV4(this float4[] f4)
        {
            var v4 = new Vector4[f4.Length];
            fixed (void* f4Ptr = f4)
            {
                fixed (void* v4Ptr = v4)
                {
                    UnsafeUtility.MemCpy(v4Ptr, f4Ptr, f4.Length * (long)UnsafeUtility.SizeOf<Vector4>());
                }
            }
            return v4;
        }
    }
}// namespace JTRP.Utility
