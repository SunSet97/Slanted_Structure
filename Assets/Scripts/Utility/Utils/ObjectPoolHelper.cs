using System.Collections.Generic;
using UnityEngine;

namespace Utility.Utils
{
    public static class ObjectPoolHelper
    {
        private class ObjectPoolProperty
        {
            public readonly List<GameObject> ActiveList;
            public readonly List<GameObject> InactiveList;

            public ObjectPoolProperty()
            {
                ActiveList = new List<GameObject>();
                InactiveList = new List<GameObject>();
            }
        }

        /// <summary>
        /// Prefab, ObjectPoolProperty (Active List, Pool Size, etc...)
        /// </summary>
        private static readonly Dictionary<GameObject, ObjectPoolProperty> ObjectPool =
            new Dictionary<GameObject, ObjectPoolProperty>();

        public static GameObject Get(GameObject prefab)
        {
            if (!ObjectPool.TryGetValue(prefab, out var objectPoolProperty))
            {
                objectPoolProperty = new ObjectPoolProperty();
                ObjectPool.Add(prefab, objectPoolProperty);
            }

            GameObject instance;
            if (objectPoolProperty.InactiveList.Count > 0)
            {
                instance = objectPoolProperty.InactiveList[0];
                objectPoolProperty.InactiveList.RemoveAt(0);
                objectPoolProperty.ActiveList.Add(instance);
            }
            else
            {
                instance = Object.Instantiate(prefab);
                instance.gameObject.SetActive(false);
                objectPoolProperty.ActiveList.Add(instance);
            }

            return instance;
        }

        public static void Release(GameObject prefab, GameObject releaseTarget)
        {
            if (ObjectPool.TryGetValue(prefab, out var list))
            {
                if (list.ActiveList.Contains(releaseTarget))
                {
                    list.ActiveList.Remove(releaseTarget);
                    list.InactiveList.Add(releaseTarget);
                }
                else
                {
                    Debug.LogError($"{prefab} - ReleaseTarget {releaseTarget}이 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"{prefab}이 없습니다.");
            }
        }

        public static void Dispose(GameObject prefab)
        {
            if (ObjectPool.ContainsKey(prefab))
            {
                ObjectPool.Remove(prefab);
            }
            else
            {
                // Debug.LogError($"{prefab}이 없습니다.");
            }
        }

        public static void Clear(GameObject prefab)
        {
            if (ObjectPool.TryGetValue(prefab, out var list))
            {
                list.ActiveList.Clear();
                list.InactiveList.Clear();
            }
            else
            {
                Debug.LogError($"{prefab}이 없습니다.");
            }
        }
    }
}