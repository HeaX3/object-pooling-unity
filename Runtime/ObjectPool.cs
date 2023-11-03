using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectPooling
{
    public class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private T Prefab { get; }
        private Transform Parent { get; }
        private Action<T> Initializer { get; }
        private Queue<T> Pool { get; } = new();

        public ObjectPool(T prefab, [CanBeNull] Transform parent, Action<T> initializer = null)
        {
            Prefab = prefab;
            Parent = parent;
            Initializer = initializer;
            if (prefab.gameObject.scene.name != null) prefab.gameObject.SetActive(false);
        }

        public T GetInstance(Action<T> activator = null)
        {
            var result = Pool.TryDequeue(out var pooled) ? pooled : CreateInstance();
            result.gameObject.SetActive(true);
            if (activator != null) activator(result);
            foreach (var component in result.gameObject.GetComponentsInChildren<IPoolable>(true))
            {
                component.Activate();
            }

            return result;
        }

        public void ReturnInstance(T instance)
        {
            foreach (var component in instance.gameObject.GetComponentsInChildren<IPoolable>(true))
            {
                try
                {
                    component.ResetForPool();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            instance.gameObject.SetActive(false);
            Pool.Enqueue(instance);
        }

        private T CreateInstance()
        {
            var result = Object.Instantiate(Prefab.gameObject, Parent, false).GetComponent<T>();
            result.transform.SetAsLastSibling();
            if (Initializer != null) Initializer(result);
            return result;
        }
    }
}