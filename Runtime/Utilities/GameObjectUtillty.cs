using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP.Utility
{
    public static class GameObjectUtillty
    {
        static void GetComponentsInChildrenDeep<T>(List<T> list, GameObject go, uint maxDepth)
        {
            T componet;
            if (go.TryGetComponent<T>(out componet))
            {
                list.Add(componet);
            }
            var transform = go.transform;
            int count = transform.childCount;
            if (count == 0 || maxDepth == 0)
                return;
            else
            {
                for (int i = 0; i < count; i++)
                {
                    GetComponentsInChildrenDeep<T>(list, transform.GetChild(i).gameObject, maxDepth - 1);
                }
            }

        }
        static void GetComponentsInChildrenDeep<T>(List<T> list, Transform go, uint maxDepth)
        {
            T componet;
            if (go.TryGetComponent<T>(out componet))
            {
                list.Add(componet);
            }
            var transform = go.transform;
            int count = transform.childCount;
            if (count == 0 || maxDepth == 0)
                return;
            else
            {
                for (int i = 0; i < count; i++)
                {
                    GetComponentsInChildrenDeep<T>(list, transform.GetChild(i).gameObject, maxDepth - 1);
                }
            }

        }
        public static T[] GetComponentsInChildrenDeep<T>(this GameObject gameObject, uint maxDepth = 20)
        {
            List<T> list = new List<T>();
            GetComponentsInChildrenDeep(list, gameObject, maxDepth);
            return list.ToArray();
        }
        public static T[] GetComponentsInChildrenDeep<T>(this Transform gameObject, uint maxDepth = 20)
        {
            List<T> list = new List<T>();
            GetComponentsInChildrenDeep(list, gameObject.transform, maxDepth);
            return list.ToArray();
        }
    }
}//namespace JTRP.Utility
