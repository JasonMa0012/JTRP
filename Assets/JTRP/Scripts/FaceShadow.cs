using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JTRP
{
    [ExecuteInEditMode]
    public class FaceShadow : MonoBehaviour
    {
        enum Dir
        {
            forward,
            right,
            up
        }

        [SerializeField] bool _debug = true;
        [SerializeField] [Range(0.0001f, 1f)] float _debugR = 0.1f;

        [Header("脸部正方向")]
        [SerializeField] Dir _dir = Dir.forward;
        [SerializeField] bool _invert = false;
        [Header("调整offset至鼻梁位置")]
        [SerializeField] Vector3 _offset = Vector3.zero;
        [Header("最大检测距离")]
        [SerializeField] float _max = 1000f;
        [Header("需要碰撞的场景layer")]
        [SerializeField] LayerMask _mask = new LayerMask();
        [Header("场景主平行光（未分配则自动搜索第一个）")]
        public Transform dirLight;
        [Header("脸材质球")]
        public Material faceMaterial;
        Vector3 _forwardDir;
        private void Start()
        {
            if (dirLight == null)
            {
                foreach (var l in FindObjectsOfType<Light>())
                {
                    if (l.type == LightType.Directional)
                    {
                        dirLight = l.transform;
                        break;
                    }
                }
            }
            if (dirLight == null)
                Debug.LogError($"{this.GetType()}:{name} 未找到dirLight!");
            if (faceMaterial == null)
                Debug.LogError($"{this.GetType()}:{name} 未分配Face Material!");
        }
        void Update()
        {
            if (!faceMaterial || !dirLight)
                return;
            switch (_dir)
            {
                case Dir.forward:
                    _forwardDir = transform.forward.normalized;
                    break;
                case Dir.right:
                    _forwardDir = transform.right.normalized;
                    break;
                case Dir.up:
                    _forwardDir = transform.up.normalized;
                    break;
            }
            _forwardDir *= _invert ? -1 : 1;
            var hit = Physics.Raycast(transform.position + _offset - dirLight.forward * _max, dirLight.forward, _max, _mask);
            faceMaterial?.SetVector("_FaceForward", _forwardDir);
            faceMaterial?.SetVector("_FaceCenter", transform.position + _offset);
            faceMaterial?.SetFloat("_FaceShadowStep", hit ? 1.0f : 0.0f);


            // var mesh = GetComponent<MeshRenderer>()
        }

        private void OnDrawGizmos()
        {
            if (!(UnityEditor.Selection.activeGameObject?.name == name) || !_debug)
                return;
            Gizmos.DrawSphere(transform.position + _offset, _debugR);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + _offset - dirLight.forward * _max, transform.position + _offset);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position + _offset, transform.position + _offset + _forwardDir);
        }
    }
}//namespace JTRP

namespace JTRP.Editor
{
    using UnityEditor;
    public class FaceShadowWindow : EditorWindow
    {
        Transform dirLight;
        Transform faceBone;
        Material faceMaterial;
        [MenuItem("JTRP/Create Face Shadow")]
        static void Creat()
        {
            GetWindow<FaceShadowWindow>(true);
        }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("不填则自动查找第一个方向光", MessageType.Info);
            dirLight = EditorGUILayout.ObjectField("场景方向光", dirLight, typeof(Transform), true) as Transform;
            // faceBone = EditorGUILayout.ObjectField("脸部骨骼", faceBone, typeof(Transform), true) as Transform;
            faceMaterial = EditorGUILayout.ObjectField("脸部材质球", faceMaterial, typeof(Material), true) as Material;
            EditorGUILayout.HelpBox("选择脸部骨骼点击生成", MessageType.Info);
            if (GUILayout.Button("生成"))
            {
                faceBone = Selection.activeGameObject.transform;
                var c = faceBone.gameObject.AddComponent<FaceShadow>();
                Undo.RegisterCreatedObjectUndo(c, "Add FaceShadow");
                c.dirLight = dirLight;
                c.faceMaterial = faceMaterial;
            }
        }
    }
}