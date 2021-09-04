using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using Pencil_4;

namespace Pencil_4
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    [AddComponentMenu("Pencil+ 4/Pencil Line Effect")]
    public class PencilLineEffect : MonoBehaviour
    {
        [SerializeField]
        public GameObject LineListObject;

        [SerializeField]
        public EffectMode Mode;

        [SerializeField]
        public List<GameObject> RenderElements = new List<GameObject>();

        Material lineDispMaterial;
        PencilLineRenderer pencilRenderer;
        public Material LineDispMaterial { get { return lineDispMaterial; } }
        public PencilLineRenderer PencilRenderer { get { return pencilRenderer; } }

        public void ChangeTextureSize(Func<Camera, Size> textureSize)
        {
            pencilRenderer.TextureSize = textureSize;
        }

        private void InitialSetup()
        {
            var camera = GetComponent<Camera>();
            if (camera != null)
            {
                if (this.pencilRenderer == null)
                {
                    this.pencilRenderer = new PencilLineRenderer(camera, (targetCam) =>
                    {
                        return new Size { Width = camera.pixelWidth, Height = camera.pixelHeight };
                    });
                }

                Shader lineDispShader = Shader.Find("Hidden/Pcl4LineCameraEffect");
                lineDispMaterial = new Material(lineDispShader);
            }
        }

        void OnEnable()
        {
            InitialSetup();
        }

        public bool isPostProsessingEnabled { get
            {
                if (Mode == EffectMode.CameraEffect)
                {
                    return false;
                }
                return true;
            }
        }

        public bool isRendering {
            get
            {
                return _isRendering;
            }
            set
            {
                if (value != _isRendering)
                {
                    _isRendering = value;
                    if (_isRendering)
                    {
                        var renderElementLines = RenderElements
                            .Where(x => x != null && x.activeSelf)
                            .Select(x => x.GetComponent<RenderElementsLineNode>())
                            .Where(x => x != null && x.isActiveAndEnabled && x.TargetTexture != null)
                            .ToList();
                        pencilRenderer.BeginRenderProcess(renderElementLines);
                    }
                    else
                    {
                        pencilRenderer.EndRenderProcess();
                    }
                    _preprepared = false;
                }
            }
        }
        private bool _isRendering = false;

        private bool _preprepared = false;
        private void Preprepare()
        {
            if (LineListObject != null)
            {
                var lineListNode = LineListObject.GetComponent<LineListNode>();
                if (lineListNode != null)
                {
                    double timeout = -1;

                    if (lineListNode.enabled
                        && lineListNode.gameObject.activeSelf)
                    {
#if UNITY_EDITOR
                        if (!EditorApplication.isPlaying)
                        {
                            switch (RenderMode.GameViewRenderMode)
                            {
                                case RenderMode.Mode.On200ms:
                                    timeout = 0.200;
                                    break;
                                case RenderMode.Mode.On1000ms:
                                    timeout = 1.000;
                                    break;
                                default:
                                    timeout = 0;
                                    break;
                            }
                        }
#endif
                    }
                    else
                    {
                        // LineListNodeがアクティブでない場合はラインの表示をしない
                        timeout = 0;
                    }

                    // LateUpdateのInvoke経由で処理が走るとき、
                    // 可視であってもOnBecameVisibleがコールされる前のオブジェクトが存在し、
                    // 正しい頂点情報を得られないことがあるので、アニメーションを強制的に更新する
                    if (timeout < 0 && Mode == EffectMode.PostProcessing)
                    {
                        foreach (var animator in Component.FindObjectsOfType<Animator>())
                        {
                            if (animator.isActiveAndEnabled && animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
                            {
                                var cullingMode = animator.cullingMode;
                                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                                animator.Update(0.0f);
                                animator.cullingMode = cullingMode;
                            }
                        }
                        foreach (var animation in Component.FindObjectsOfType<Animation>())
                        {
                            if (animation.isActiveAndEnabled && animation.cullingType != AnimationCullingType.AlwaysAnimate)
                            {
                                animation.Sample();
                            }
                        }
                    }
                    
                    //
                    pencilRenderer.RenderPreprocessAsync(lineListNode, timeout, Mode);
                }
            }

            _preprepared = true;
        }

        void Update()
        {
            // SRP使用時、OnEnable()がコールされずに処理が回ることがあるため、
            // Update内でも初期化を試みる
            if (this.pencilRenderer == null)
            {
                InitialSetup();
            }

            // SRP使用時、カメラのレンダリングイベントが実行されない場合があるので、
            // Update内で念のためフラグをリセットする
            this.isRendering = false;
        }

        void LateUpdate()
        {
            this.isRendering = true;

            if (Mode == EffectMode.PostProcessing)
            {
                Invoke("Preprepare", 0);
            }
        }

        void OnPreRender()
        {
            if (!_preprepared)
            {
                this.isRendering = true;
                Preprepare();
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            // 描画
            if (!isPostProsessingEnabled && pencilRenderer.RenderPreprocessWait())
            {
                lineDispMaterial.SetTexture("_LineTex", pencilRenderer.Texture);
                Graphics.Blit(src, dest, lineDispMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }

            //
            this.isRendering = false;
        }


        void OnDisable()
        {
            if (pencilRenderer != null)
            {
                pencilRenderer.Dispose();
                pencilRenderer = null;
            }
        }
    }
}