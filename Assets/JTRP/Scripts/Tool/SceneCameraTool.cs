using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

#if !UNITY_EDITOR
#error This script must be placed under "Editor/" directory.
#endif

public class SceneCameraTool : EditorWindow
{
    [SerializeField]
    Camera targetCam;
    [SerializeField]
    bool syncCam = false;
    [SerializeField]
    bool showSceneCamData = true;
    [SerializeField]
    bool _syncFOV = false;
    [SerializeField]
    CamProperties controledProperties = new CamProperties();

    bool resetFovOnce = false;

    EditorWindow _gameView = null;
    EditorWindow gameView
    {
        get
        {
            if (_gameView != null)
                return _gameView;

            System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
            System.Type type = assembly.GetType("UnityEditor.GameView");
            _gameView = EditorWindow.GetWindow(type);
            return gameView;
        }
    }

    Transform targetParent
    {
        get
        {
            if (targetCam != null)
                return targetCam.transform.parent;
            else
                return null;
        }
    }

    static SceneCameraTool instance = null;

    [MenuItem("Window/SceneView Cam Tool")]
    static void Init()
    {
        SceneCameraTool window = (SceneCameraTool)EditorWindow.GetWindow(typeof(SceneCameraTool), false, "Cam Tool");
        window.Show();
        instance = window;
    }

    void OnEnable()
    {
        instance = this;
        targetCam = Camera.main;

        UnityEditor.SceneView.beforeSceneGui -= OnPreSceneGUI;
        UnityEditor.SceneView.beforeSceneGui += OnPreSceneGUI;
    }

    private void OnDisable()
    {
        UnityEditor.SceneView.beforeSceneGui -= OnPreSceneGUI;
    }

    public static void SRepaint()
    {
        if (instance != null)
            instance.Repaint();
    }

    static void OnPreSceneGUI(SceneView sceneView)
    {
        if (SceneView.lastActiveSceneView != sceneView)
            return;

        instance.OnPreSceneGUI();
    }

    void OnPreSceneGUI()
    {
        var sceneViewCam = SceneView.lastActiveSceneView.camera;

        if (resetFovOnce)
            resetFovOnce = false;
        else
            sceneViewCam.fieldOfView = controledProperties.fov; // Trick to control the scene cam fov. We need to override it every time or it will get reset by the Unity Editor

        if (sceneViewCam.transform.hasChanged)
            SRepaint();


        // Update the position changes from scene view control
        controledProperties.Copy(sceneViewCam, targetParent);

        if (syncCam && targetCam != null)
        {
            controledProperties.Paste(targetCam, _syncFOV);
            gameView.Repaint();
        }
    }

    void OnGUI()
    {
        if (SceneView.lastActiveSceneView == null)
            return;

        // Camera data transfer controls
        EditorGUILayout.BeginHorizontal();
        {
            GUI.enabled = false;

            EditorGUILayout.ObjectField(SceneView.lastActiveSceneView.camera, typeof(Camera), true, GUILayout.MaxWidth(150));

            GUI.enabled = (targetCam != null) && !syncCam;
            if (GUILayout.Button("<-"))
            {
                controledProperties.Copy(targetCam);
                SetSceneCamTransformData();
            }

            GUI.enabled = targetCam != null;
            syncCam = EditorGUILayout.Toggle(syncCam, "IN LockButton", GUILayout.MaxWidth(15));

            GUI.enabled = (targetCam != null) && !syncCam;
            if (GUILayout.Button("->"))
                controledProperties.Paste(targetCam, _syncFOV);

            GUI.enabled = true;
            EditorGUIUtility.labelWidth = 80;
            targetCam = (Camera)EditorGUILayout.ObjectField(targetCam, typeof(Camera), true, GUILayout.MaxWidth(150));
            EditorGUIUtility.labelWidth = -1;
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
            SceneView.lastActiveSceneView.Repaint();

        // Scene camera Inspector
        HorizontalLine.Draw();
        EditorGUILayout.LabelField(EditorGUIUtility.ObjectContent(SceneView.lastActiveSceneView.camera, typeof(Camera)));

        EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
        if (showSceneCamData) //TODO foldout arrow
        {
            GUI.changed = false;

            EditorGUIUtility.wideMode = true;

            DrawTransformData();

            GUILayout.BeginHorizontal();
            controledProperties.fov = EditorGUILayout.Slider(new GUIContent("Field Of View"), controledProperties.fov, 0.1f, 200f, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
                resetFovOnce = true;

            _syncFOV = EditorGUILayout.Toggle("Sync FOV", _syncFOV);
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                SetSceneCamTransformData();
                SceneView.lastActiveSceneView.Repaint();
            }
        }
    }

    void DrawTransformData()
    {
        controledProperties.localPosition = EditorGUILayout.Vector3Field("Position", controledProperties.localPosition);
        Vector3 newLocalRotEuler = EditorGUILayout.Vector3Field("Rotation", controledProperties.localRotEuler);
        if (newLocalRotEuler != controledProperties.localRotEuler)
        {
            controledProperties.localRotEuler = newLocalRotEuler;
            controledProperties.localRotation = Quaternion.Euler(newLocalRotEuler);
        }
    }

    void SetSceneCamTransformData()
    {
        Vector3 globalPosition = controledProperties.localPosition;
        if (targetParent != null)
            globalPosition = targetParent.TransformPoint(controledProperties.localPosition);

        Quaternion globalRotation = controledProperties.localRotation;
        if (targetParent != null)
            globalRotation = targetParent.transform.rotation * globalRotation;

        SetSceneCamTransformData(globalPosition, globalRotation);
    }

    void SetSceneCamTransformData(Vector3 position, Quaternion rotation)
    {
        // Can't set transform of camera :(
        // It internally updates every frame:
        //      cam.position = pivot + rotation * new Vector3(0, 0, -cameraDistance)
        // Info: https://forum.unity.com/threads/moving-scene-view-camera-from-editor-script.64920/#post-3388397

        var scene_view = UnityEditor.SceneView.lastActiveSceneView;

        scene_view.rotation = rotation;
        scene_view.pivot = position + rotation * new Vector3(0, 0, scene_view.cameraDistance);
    }

    [System.Serializable]
    private class CamProperties
    {
        public float fov = 55f;
        public Vector3 localPosition = Vector3.zero;
        public Quaternion localRotation = Quaternion.identity;
        public Vector3 localRotEuler = Vector3.zero;

        SerializedObject serializedTargetTransform;
        SerializedProperty serializedEulerHintProp;

        public void Copy(Camera sceneCamera, Transform relativeParent)
        {
            fov = sceneCamera.fieldOfView;

            Transform targetTransform = sceneCamera.transform;

            Quaternion newLocalRotation;
            if (relativeParent != null)
            {
                localPosition = relativeParent.InverseTransformPoint(targetTransform.position);
                newLocalRotation = Quaternion.Inverse(relativeParent.rotation) * targetTransform.rotation;
            }
            else
            {
                localPosition = targetTransform.position;
                newLocalRotation = targetTransform.rotation;
            }

            if (localRotation != newLocalRotation)
            {
                Vector3 newLocalEuler = newLocalRotation.eulerAngles;

                localRotEuler.x += Mathf.DeltaAngle(localRotEuler.x, newLocalEuler.x);
                localRotEuler.y += Mathf.DeltaAngle(localRotEuler.y, newLocalEuler.y);
                localRotEuler.z += Mathf.DeltaAngle(localRotEuler.z, newLocalEuler.z);

                localRotation = newLocalRotation;
            }
        }

        public void Copy(Camera target)
        {
            fov = target.fieldOfView;

            Transform targetTransform = target.transform;
            localPosition = targetTransform.localPosition;

            prepareProperty(targetTransform);
            serializedTargetTransform.UpdateIfRequiredOrScript();
            localRotEuler = serializedEulerHintProp.vector3Value;

            localRotation = targetTransform.localRotation;
        }

        public void Paste(Camera target, bool syncFOV)
        {
            if (syncFOV)
                target.fieldOfView = fov;

            Transform targetTransform = target.transform;
            targetTransform.localPosition = localPosition;

            prepareProperty(targetTransform);
            serializedEulerHintProp.vector3Value = localRotEuler;
            serializedTargetTransform.ApplyModifiedProperties();

            targetTransform.localEulerAngles = localRotEuler;
        }

        void prepareProperty(Transform targetTransform)
        {
            if (serializedTargetTransform != null && serializedTargetTransform.targetObject == targetTransform)
                return;

            serializedTargetTransform = new SerializedObject(targetTransform);
            serializedEulerHintProp = serializedTargetTransform.FindProperty("m_LocalEulerAnglesHint");
        }
    }

    public static class HorizontalLine
    {
        private static GUIStyle m_line = null;

        //constructor
        static HorizontalLine()
        {

            m_line = new GUIStyle("box");
            m_line.border.top = m_line.border.bottom = 1;
            m_line.margin.top = m_line.margin.bottom = 1;
            m_line.padding.top = m_line.padding.bottom = 1;
            m_line.border.left = m_line.border.right = 1;
            m_line.margin.left = m_line.margin.right = 1;
            m_line.padding.left = m_line.padding.right = 1;
        }

        public static void Draw()
        {
            GUILayout.Box(GUIContent.none, m_line, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
        }
    }
}
