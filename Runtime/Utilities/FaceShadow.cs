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

        [Header("=== 挂在头骨上 ===")]
        [SerializeField] bool _debug = true;
        [SerializeField] [Range(0.0001f, 1f)] float _debugR = 0.1f;

        [Header("脸部正方向")]
        [SerializeField] Dir _dir = Dir.forward;
        [SerializeField] bool _invert = false;
        [Space]
        [Header("Self Shadow")]
        [SerializeField] bool _enableSelfShadow = true;
        [Header("调整offset至鼻梁位置")]
        [SerializeField] Vector3 _offset = Vector3.zero;
        [Header("最大射线检测距离")]
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
            if (_invert)
            {
                switch (_dir)
                {
                    case Dir.forward:
                        _forwardDir = -transform.forward;
                        break;
                    case Dir.right:
                        _forwardDir = -transform.right;
                        break;
                    case Dir.up:
                        _forwardDir = -transform.up;
                        break;
                }
            }
            else
            {
                switch (_dir)
                {
                    case Dir.forward:
                        _forwardDir = transform.forward;
                        break;
                    case Dir.right:
                        _forwardDir = transform.right;
                        break;
                    case Dir.up:
                        _forwardDir = transform.up;
                        break;
                }
            }

            if (faceMaterial == null)
                Debug.LogError($"{this.GetType()}:{name} 未分配Face Material!");
        }
        private void OnValidate()
        {
            Start();
        }
        void Update()
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
            {
                Debug.LogError($"{this.GetType()}:{name} 未找到dirLight! 将用摄像机代替");
                dirLight = Camera.main.transform;
            }

            if (!faceMaterial || !dirLight)
                return;
            faceMaterial?.SetVector("_FaceForward", _forwardDir);

            if (!_enableSelfShadow)
                return;
            var hit = Physics.Raycast(transform.position + _offset - dirLight.forward * _max, dirLight.forward, _max, _mask);
            faceMaterial?.SetFloat("_FaceShadowStep", hit ? 1.0f : 0.0f);

        }

#if UNITY_EDITOR
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
#endif
    }
}//namespace JTRP
