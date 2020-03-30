/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
//using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// It is relatively expensive to instantiate new objects so reuse the objects when possible by placing them in a pool.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private static ObjectPool s_Instance;
        private static ObjectPool Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Object Pool").AddComponent<ObjectPool>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        /// <summary>
        /// Specifies a reference to the prefab that should be preloaded along with a preload count.
        /// </summary>
        [Serializable]
        public struct PreloadedPrefab
        {
#pragma warning disable 0649
            [Tooltip("The prefab that should be preloaded.")]
            [SerializeField] private GameObject m_Prefab;
            [Tooltip("Number of prefabs to instantiate upon start.")]
            [SerializeField] private int m_Count;
#pragma warning restore 0649

            public GameObject Prefab { get { return m_Prefab; } }
            public int Count { get { return m_Count; } }
        }

        [Tooltip("Specifies any prefabs that should be preloaded upon start.")]
        [SerializeField] protected PreloadedPrefab[] m_PreloadedPrefabs;

        public PreloadedPrefab[] PreloadedPrefabs { get { return m_PreloadedPrefabs; } }

        private Dictionary<int, Stack<GameObject>> m_GameObjectPool = new Dictionary<int, Stack<GameObject>>();
        private Dictionary<int, int> m_InstantiatedGameObjects = new Dictionary<int, int>();
        private Dictionary<Type, object> m_GenericPool = new Dictionary<Type, object>();
        private Dictionary<int, GameObject> m_OriginalObjectIDs = new Dictionary<int, GameObject>();

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Preloads any prefabs.
        /// </summary>
        private void Start()
        {
            if (m_PreloadedPrefabs != null) {
                var instantiatedObjects = new List<GameObject>();
                for (int i = 0; i < m_PreloadedPrefabs.Length; ++i) {
                    if (m_PreloadedPrefabs[i].Prefab == null || m_PreloadedPrefabs[i].Count == 0) {
                        continue;
                    }

                    // Create and destroy the preloaded prefab so it'll be ready in the pool.
                    for (int j = 0; j < m_PreloadedPrefabs[i].Count; ++j) {
                        if (j < instantiatedObjects.Count) {
                            // Reuse the list if possible.
                            instantiatedObjects[j] = Instantiate(m_PreloadedPrefabs[i].Prefab);
                        } else {
                            instantiatedObjects.Add(Instantiate(m_PreloadedPrefabs[i].Prefab));
                        }
                    }
                    for (int j = 0; j < m_PreloadedPrefabs[i].Count; ++j) {
                        Destroy(instantiatedObjects[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Instantiate a new GameObject. Use the object pool if a previously used GameObject is located in the pool, otherwise instaniate a new GameObject.
        /// </summary>
        /// <param name="original">The original GameObject to pooled a copy of.</param>
        /// <returns>The pooled/instantiated GameObject.</returns>
        public static GameObject Instantiate(GameObject original)
        {
            return Instantiate(original, Vector3.zero, Quaternion.identity, null);
        }

        /// <summary>
        /// Instantiate a new GameObject. Use the object pool if a previously used GameObject is located in the pool, otherwise instaniate a new GameObject.
        /// </summary>
        /// <param name="original">The original GameObject to pooled a copy of.</param>
        /// <param name="position">The position of the pooled GameObject.</param>
        /// <param name="rotation">The rotation of the pooled Gameobject.</param>
        /// <returns>The pooled/instantiated GameObject.</returns>
        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation)
        {
            return Instantiate(original, position, rotation, null);
        }

        /// <summary>
        /// Instantiate a new GameObject. Use the object pool if a previously used GameObject is located in the pool, otherwise instaniate a new GameObject.
        /// </summary>
        /// <param name="original">The original GameObject to pooled a copy of.</param>
        /// <param name="position">The position of the pooled GameObject.</param>
        /// <param name="rotation">The rotation of the pooled Gameobject.</param>
        /// <param name="parent">The parent to assign to the pooled GameObject.</param>
        /// <returns>The pooled/instantiated GameObject.</returns>
        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Instance.InstantiateInternal(original, position, rotation, parent);
        }

        /// <summary>
        /// Internal method to spawn a new GameObject. Use the object pool if a previously used GameObject is located in the pool, otherwise instaniate a new GameObject.
        /// </summary>
        /// <param name="original">The original GameObject to pooled a copy of.</param>
        /// <param name="position">The position of the pooled GameObject.</param>
        /// <param name="rotation">The rotation of the pooled Gameobject.</param>
        /// <param name="parent">The parent to assign to the pooled GameObject.</param>
        /// <returns>The pooled/instantiated GameObject.</returns>
        private GameObject InstantiateInternal(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            var originalInstanceID = original.GetInstanceID();
            var instantiatedObject = ObjectFromPool(originalInstanceID, position, rotation, parent);
            if (instantiatedObject == null) {
                instantiatedObject = GameObject.Instantiate(original, position, rotation, parent);
                if (!m_OriginalObjectIDs.ContainsKey(originalInstanceID)) {
                    m_OriginalObjectIDs.Add(originalInstanceID, original);
                }
            } else {
                instantiatedObject.transform.position = position;
                instantiatedObject.transform.rotation = rotation;
                instantiatedObject.transform.parent = parent;
            }
            // Map the newly instantiated instance ID to the original instance ID so when the object is returned it knows what pool to go to.
            m_InstantiatedGameObjects.Add(instantiatedObject.GetInstanceID(), originalInstanceID);

            return instantiatedObject;
        }

        /// <summary>
        /// An object is trying to be popped from the object pool. Return the pooled object if it exists otherwise null meaning one needs to be insantiated.
        /// </summary>
        /// <param name="originalInstanceID">The instance id of the GameObject trying to be popped from the pool.</param>
        /// <param name="position">The position of the pooled GameObject.</param>
        /// <param name="rotation">The rotation of the pooled Gameobject.</param>
        /// <param name="parent">The parent to assign to the pooled GameObject.</param>
        /// <returns>The pooled GameObject.</returns>
        private GameObject ObjectFromPool(int originalInstanceID, Vector3 position, Quaternion rotation, Transform parent)
        {
            Stack<GameObject> pool;
            if (m_GameObjectPool.TryGetValue(originalInstanceID, out pool)) {
                while (pool.Count > 0) {
                    var instantiatedObject = pool.Pop();
                    // The object may be null if it was removed from an additive scene. Keep popping from the pool until the pool has a valid object or is empty.
                    if (instantiatedObject == null) {
                        continue;
                    }
                    instantiatedObject.transform.position = position;
                    instantiatedObject.transform.rotation = rotation;
                    instantiatedObject.transform.parent = parent;
                    instantiatedObject.SetActive(true);
                    return instantiatedObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Return if the object was instantiated with the ObjectPool.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to check to see if it was instantiated with the ObjectPool.</param>
        /// <returns>True if the object was instantiated with the ObjectPool.</returns>
        public static bool InstantiatedWithPool(GameObject instantiatedObject)
        {
            return Instance.InstantiatedWithPoolInternal(instantiatedObject);
        }

        /// <summary>
        /// Internal method to return if the object was instantiated with the ObjectPool.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to check to see if it was instantiated with the ObjectPool.</param>
        /// <returns>True if the object was instantiated with the ObjectPool.</returns>
        private bool InstantiatedWithPoolInternal(GameObject instantiatedObject)
        {
            return m_InstantiatedGameObjects.ContainsKey(instantiatedObject.GetInstanceID());
        }

        /// <summary>
        /// Return the instance ID of the prefab used to spawn the instantiated object.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to get the original instance ID</param>
        /// <returns>The original instance ID</returns>
        public static int OriginalInstanceID(GameObject instantiatedObject)
        {
            return Instance.OriginalInstanceIDInternal(instantiatedObject);
        }

        /// <summary>
        /// Internal method to return the instance ID of the prefab used to spawn the instantiated object.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to get the original instance ID</param>
        /// <returns>The original instance ID</returns>
        private int OriginalInstanceIDInternal(GameObject instantiatedObject)
        {
            var instantiatedInstanceID = instantiatedObject.GetInstanceID();
            var originalInstanceID = -1;
            if (!m_InstantiatedGameObjects.TryGetValue(instantiatedInstanceID, out originalInstanceID)) {
                Debug.LogError("Unable to get the original instance ID of " + instantiatedObject + ": has the object already been placed in the ObjectPool?");
                return -1;
            }
            return originalInstanceID;
        }

        /// <summary>
        /// Return the specified GameObject back to the ObjectPool.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to return to the pool.</param>
        public static void Destroy(GameObject instantiatedObject)
        {
            // Objects may be wanting to be destroyed as the game is stopping but the ObjectPool has already been destroyed. Ensure the ObjectPool is still valid.
            if (Instance == null) {
                return;
            }

            Instance.DestroyInternal(instantiatedObject);
        }

        /// <summary>
        /// Internal method to return the specified GameObject back to the ObjectPool. Call the corresponding server or client method.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to return to the pool.</param>
        private void DestroyInternal(GameObject instantiatedObject)
        {
            var instantiatedInstanceID = instantiatedObject.GetInstanceID();
            var originalInstanceID = -1;
            if (!m_InstantiatedGameObjects.TryGetValue(instantiatedInstanceID, out originalInstanceID)) { 
                Debug.LogError("Unable to pool " + instantiatedObject.name + " (instance " + instantiatedInstanceID + "): the GameObject was not instantiated with ObjectPool.Instantiate " + Time.time);
                return;
            }

            // Map the instantiated instance ID back to the orignal instance ID so the GameObject can be returned to the correct pool.
            m_InstantiatedGameObjects.Remove(instantiatedInstanceID);
            
            DestroyLocal(instantiatedObject, originalInstanceID);
        }

        /// <summary>
        /// Return the specified GameObject back to the ObjectPool.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject to return to the pool.</param>
        /// <param name="originalInstanceID">The instance ID of the original GameObject.</param>
        private void DestroyLocal(GameObject instantiatedObject, int originalInstanceID)
        {
            // This GameObject may have a collider and that collider may be ignoring the collision with other colliders. Revert this setting because the object is going
            // back into the pool.
            Collider instantiatedObjectCollider;
            //if ((instantiatedObjectCollider = instantiatedObject.GetCachedComponent<Collider>()) != null) {
            //    LayerManager.RevertCollision(instantiatedObjectCollider);
            //}
            instantiatedObject.SetActive(false);
            instantiatedObject.transform.parent = transform;

            Stack<GameObject> pool;
            if (m_GameObjectPool.TryGetValue(originalInstanceID, out pool)) {
                pool.Push(instantiatedObject);
            } else {
                // The pool for this GameObject type doesn't exist yet so it has to be created.
                pool = new Stack<GameObject>();
                pool.Push(instantiatedObject);
                m_GameObjectPool.Add(originalInstanceID, pool);
            }
        }

        /// <summary>
        /// Returns the original GameObject that the specified object was instantiated from.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject that was instantiated.</param>
        /// <returns>The original GameObject that the specified object was instantiated from.</returns>
        public static GameObject OriginalObject(GameObject instantiatedObject)
        {
            // Objects may be wanting to be destroyed as the game is stopping but the ObjectPool has already been destroyed. Ensure the ObjectPool is still valid.
            if (Instance == null) {
                return null;
            }

            return Instance.OriginalObjectInternal(instantiatedObject);
        }

        /// <summary>
        /// Internal method which returns the original GameObject that the specified object was instantiated from.
        /// </summary>
        /// <param name="instantiatedObject">The GameObject that was instantiated.</param>
        /// <returns>The original GameObject that the specified object was instantiated from.</returns>
        private GameObject OriginalObjectInternal(GameObject instantiatedObject)
        {
            var originalInstanceID = -1;
            if (!m_InstantiatedGameObjects.TryGetValue(instantiatedObject.GetInstanceID(), out originalInstanceID)) {
                return null;
            }

            GameObject original;
            if (!m_OriginalObjectIDs.TryGetValue(originalInstanceID, out original)) {
                return null;
            }

            return original;
        }

        /// <summary>
        /// Get a pooled object of the specified type using a generic ObjectPool.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled object of type T.</returns>
        public static T Get<T>()
        {
            return Instance.GetInternal<T>();
        }

        /// <summary>
        /// Internal method to get a pooled object of the specified type using a generic ObjectPool.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled object of type T.</returns>
        private T GetInternal<T>()
        {
            object value;
            if (m_GenericPool.TryGetValue(typeof(T), out value)) {
                var pooledObjects = value as Stack<T>;
                if (pooledObjects.Count > 0) {
                    return pooledObjects.Pop();
                }
            }
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Return the object back to the generic object pool.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        public static void Return<T>(T obj)
        {
            // Objects may be wanting to be returned as the game is stopping but the ObjectPool has already been destroyed. Ensure the ObjectPool is still valid.
            if (Instance == null) {
                return;
            }

            Instance.ReturnInternal<T>(obj);
        }

        /// <summary>
        /// Internal method to return the object back to the generic object pool.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        private void ReturnInternal<T>(T obj)
        {
            if (obj == null) {
                return;
            }

            object value;
            if (m_GenericPool.TryGetValue(typeof(T), out value)) {
                var pooledObjects = value as Stack<T>;
                pooledObjects.Push(obj);
            } else {
                var pooledObjects = new Stack<T>();
                pooledObjects.Push(obj);
                m_GenericPool.Add(typeof(T), pooledObjects);
            }
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
#endif
    }
}