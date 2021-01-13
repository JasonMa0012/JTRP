using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using static Unity.Mathematics.math;

namespace JTRP.CustomAssetPostprocessor
{
    public class AnimationImporter : AssetPostprocessor
    {
        readonly string[] _pos = { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z" };
        readonly string[] _rot = { "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w" };
        readonly string[] _scl = { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" };

        void OnPostprocessAnimation(GameObject gameObject, AnimationClip clip)
        {
            if (!assetPath.Contains("_bip"))
                return;

            var go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;
            var root = GameObject.Instantiate(go, Vector3.zero, Quaternion.identity, null).transform;
            root.gameObject.hideFlags = HideFlags.HideAndDontSave;
            var pelvis = GameObject.Instantiate(go, Vector3.zero, Quaternion.identity, root).transform;
            pelvis.gameObject.hideFlags = HideFlags.HideAndDontSave;
            var spine = GameObject.Instantiate(go, Vector3.zero, Quaternion.identity, root).transform;
            spine.gameObject.hideFlags = HideFlags.HideAndDontSave;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var rootDic = new Dictionary<string, (EditorCurveBinding bind, AnimationCurve curve)>();
            var pelvisDic = new Dictionary<string, (EditorCurveBinding bind, AnimationCurve curve)>();
            var spineDic = new Dictionary<string, (EditorCurveBinding bind, AnimationCurve curve)>();

            foreach (var binding in bindings)
            {
                switch (binding.path)
                {
                    case "Bip001":
                        rootDic.Add(binding.propertyName, (binding, AnimationUtility.GetEditorCurve(clip, binding)));
                        break;
                    case "Bip001/Bip001 Pelvis":
                        pelvisDic.Add(binding.propertyName, (binding, AnimationUtility.GetEditorCurve(clip, binding)));
                        break;
                    case "Bip001/Bip001 Spine":
                        spineDic.Add(binding.propertyName, (binding, AnimationUtility.GetEditorCurve(clip, binding)));
                        break;
                }
            }

            List<Keyframe>[] posKeyList1 = { new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>() };
            List<Keyframe>[] rotKeyList1 = { new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>() };
            List<Keyframe>[] posKeyList2 = { new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>() };
            List<Keyframe>[] rotKeyList2 = { new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>() };
            PerFrame(clip.length, (t) =>
            {
                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                for (var i = 0; i < 3; i++) pos[i] = rootDic[_pos[i]].curve.Evaluate(t);
                for (var i = 0; i < 4; i++) rot[i] = rootDic[_rot[i]].curve.Evaluate(t);
                root.localPosition = pos;
                root.localRotation = rot;

                for (var i = 0; i < 3; i++) pos[i] = pelvisDic[_pos[i]].curve.Evaluate(t);
                for (var i = 0; i < 4; i++) rot[i] = pelvisDic[_rot[i]].curve.Evaluate(t);
                pelvis.localPosition = pos;
                pelvis.localRotation = rot;
                for (var i = 0; i < 3; i++) posKeyList1[i].Add(new Keyframe(t, pelvis.position[i]));
                for (var i = 0; i < 4; i++) rotKeyList1[i].Add(new Keyframe(t, pelvis.rotation[i]));

                for (var i = 0; i < 3; i++) pos[i] = spineDic[_pos[i]].curve.Evaluate(t);
                for (var i = 0; i < 4; i++) rot[i] = spineDic[_rot[i]].curve.Evaluate(t);
                spine.localPosition = pos;
                spine.localRotation = rot;
                for (var i = 0; i < 3; i++) posKeyList2[i].Add(new Keyframe(t, spine.position[i]));
                for (var i = 0; i < 4; i++) rotKeyList2[i].Add(new Keyframe(t, spine.rotation[i]));

            });

            for (int i = 0; i < 4; i++)
            {
                if (i < 3)
                {
                    pelvisDic[_pos[i]].curve.keys = posKeyList1[i].ToArray();
                    AnimationUtility.SetEditorCurve(clip, pelvisDic[_pos[i]].bind, pelvisDic[_pos[i]].curve);
                    spineDic[_pos[i]].curve.keys = posKeyList2[i].ToArray();
                    AnimationUtility.SetEditorCurve(clip, spineDic[_pos[i]].bind, spineDic[_pos[i]].curve);
                    AnimationUtility.SetEditorCurve(clip, rootDic[_pos[i]].bind, AnimationCurve.Constant(0, 0, 0));
                }
                pelvisDic[_rot[i]].curve.keys = rotKeyList1[i].ToArray();
                AnimationUtility.SetEditorCurve(clip, pelvisDic[_rot[i]].bind, pelvisDic[_rot[i]].curve);
                spineDic[_rot[i]].curve.keys = rotKeyList2[i].ToArray();
                AnimationUtility.SetEditorCurve(clip, spineDic[_rot[i]].bind, spineDic[_rot[i]].curve);
                AnimationUtility.SetEditorCurve(clip, rootDic[_rot[i]].bind, AnimationCurve.Constant(0, 0, Quaternion.identity[i]));
            }

            GameObject.DestroyImmediate(go.gameObject);
            GameObject.DestroyImmediate(pelvis.gameObject);
            GameObject.DestroyImmediate(spine.gameObject);
            GameObject.DestroyImmediate(root.gameObject);

        }

        // 遍历每帧
        void PerFrame(float time, UnityAction<float> _event, int frameRate = 30)
        {
            int f = (int)(time * frameRate);
            for (float i = 0; i <= f; i++)
            {
                if (_event != null)
                    _event(i / frameRate);
            }
        }
    }
}//namespace JTRP.CustomAssetPostprocessor
