using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Rcade
{
    public interface IPoolable<T> where T : MonoBehaviour
    {
        System.Action<T, int> disableAction { get; set; }  
    }

    public class PoolManager<T> where T : MonoBehaviour
    {
        private static Transform _PARENT_TRANSFORM;
        public static Transform PARENT_TRANSFORM
        {
            get
            {
                if (_PARENT_TRANSFORM == null)
                {
                    _PARENT_TRANSFORM = new GameObject("Pool").transform;
                }
                return _PARENT_TRANSFORM;
            }
        }

        private List<T> m_activeObjectList = new List<T>();
        private List<T> m_inactiveObjectList = new List<T>();

        public T Spawn(T prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            T obj;

            if (this.m_inactiveObjectList.Count > 0)
            {
                // Reuse an existing inactive object from the pool
                obj = this.m_inactiveObjectList[0];
                this.m_inactiveObjectList.RemoveAt(0);
                obj.gameObject.SetActive(true);
            }
            else
            {
                // Instantiate a new object since there are no inactive objects
                obj = Object.Instantiate(prefab, position, rotation);

                IPoolable<T> poolable = obj.GetComponent<IPoolable<T>>();

                if (poolable != null)
                {
                    poolable.disableAction = this.Despawn;
                }

            }

            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            this.m_activeObjectList.Add(obj);

            return obj;
        }

        public T Spawn(T prefab, Vector3 position, Quaternion rotation)
        {
            T obj;

            if (this.m_inactiveObjectList.Count > 0)
            {
                // Reuse an existing inactive object from the pool
                obj = this.m_inactiveObjectList[0];
                this.m_inactiveObjectList.RemoveAt(0);
                obj.gameObject.SetActive(true);
            }
            else
            {
                // Instantiate a new object since there are no inactive objects
                obj = Object.Instantiate(prefab, position, rotation);

                IPoolable<T> poolable = obj.GetComponent<IPoolable<T>>();

                if (poolable != null)
                {
                    poolable.disableAction = this.Despawn;
                }

                obj.transform.SetParent(PARENT_TRANSFORM);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;

            this.m_activeObjectList.Add(obj);

            return obj;
        }

        public void Despawn(T obj, int limit)
        {
            bool poolIsFull = this.m_inactiveObjectList.Count >= limit;

            if (this.m_activeObjectList.Contains(obj))
            {
                this.m_activeObjectList.Remove(obj);
                if (!poolIsFull)
                {
                    this.m_inactiveObjectList.Add(obj);
                }
            }

            if (!poolIsFull)
            {
                if (obj.gameObject.activeSelf && obj.transform.parent != PARENT_TRANSFORM)
                {
                    obj.transform.SetParent(PARENT_TRANSFORM);
                }
                obj.gameObject.SetActive(false);
            }
            else
            {
                Object.Destroy(obj.gameObject);
            }
        }
    }
}
