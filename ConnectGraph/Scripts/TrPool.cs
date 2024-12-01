using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ConnectGraph
{
    public interface ITrPoolItem
    {
        public Transform Tr { get; set; }
        public void Start();

        public void OnEnterPool();
    }

    public class TrPool<T> where T : ITrPoolItem, new()
    {
        Queue<ITrPoolItem> pool;
        Transform prefab;
        Dictionary<int, T> prefabScripts;

        public Transform repa;

        public Dictionary<int, T> LivingDic => prefabScripts;

        public TrPool(Transform t, int count = 10)
        {
            prefab = t;
            pool = new Queue<ITrPoolItem>(count);
            prefabScripts = new Dictionary<int, T>(30);
            prefab.gameObject.SetActive(false);
        }

        public Tuple<Transform, T> Getprefab(Transform pa = null)
        {
            if (pool.Count > 0)
            {
                var t = pool.Dequeue();
                t.Tr.gameObject.SetActive(true);
                if (pa)
                {
                    t.Tr.SetParent(pa);
                }
                prefabScripts.Add(t.Tr.GetInstanceID(), (T)t);
                return new Tuple<Transform, T>(t.Tr, (T)t);
            }
            else
            {
                Transform p = pa == null ? prefab.parent : pa;
                var t = UnityEngine.Object.Instantiate(prefab, p);
                t.gameObject.SetActive(true);
                T sp = default(T);
                if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
                {
                    sp = t.GetComponent<T>();
                    sp.Tr = t;
                    prefabScripts.Add(t.GetInstanceID(), sp);
                }
                else
                {
                    sp = new T();
                    sp.Tr = t;
                    sp.Start();
                    prefabScripts.Add(t.GetInstanceID(), sp);
                }
                return new Tuple<Transform, T>(t, sp);
            }
        }

        public void Reprefab(Transform pre)
        {
            if (pool.Any((x) => x.Tr == pre))
            {
                Debug.LogError("重复回收！！！！！！" + pre.name);
                return;
            }
            var a = prefabScripts[pre.GetInstanceID()];
            if (a == null)
            {
                Debug.LogError("无此脚本不能被回收！！！！！！" + pre.name);
                return;
            }
            a.OnEnterPool();
            prefabScripts.Remove(pre.GetInstanceID());
            pre.gameObject.SetActive(false);
            if (repa)
            {
                pre.transform.SetParent(repa);
            }
            pool.Enqueue(a);
        }

        public void Reprefab(ITrPoolItem pre)
        {
            if (pre.Tr)
            {
                Reprefab(pre.Tr);
            }
            else
            {
                Debug.LogError("该物体的Transform组件未赋值或不存在！！！！" + pre.ToString());
                return;
            }

            //if (pool.Contains(pre))
            //{
            //    Debug.LogError("重复回收！！！！！！" + pre.ToString());
            //    return;
            //}
            //pre.OnEnterPool();
            //pre.Tr.gameObject.SetActive(false);
            //if (repa)
            //{
            //    pre.Tr.SetParent(repa);
            //}
            //pool.Enqueue(pre);
        }

        public void Clear()
        {
            foreach (var item in prefabScripts)
                UnityEngine.Object.Destroy(item.Value.Tr.gameObject);
            prefabScripts.Clear();
            foreach (var item in pool)
                UnityEngine.Object.Destroy(item.Tr.gameObject);
            pool.Clear();
        }

        public void ReprefabAll()
        {
            foreach (var item in prefabScripts)
            {
                item.Value.OnEnterPool();
                item.Value.Tr.gameObject.SetActive(false);
                if (repa)
                {
                    item.Value.Tr.SetParent(repa);
                }
                pool.Enqueue(item.Value);
            }
            prefabScripts.Clear();
        }

        public T Script(int trname)
        {
            return prefabScripts[trname];
        }

        public T Script(Transform tr)
        {
            return prefabScripts[tr.GetInstanceID()];
        }
    }
}