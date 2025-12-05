using Convention.Experimental.PublicType;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Convention.Experimental.Modules
{
    public class GameObjectPoolManager : GameModule
    {
        /// <summary>
        /// <para>继承该接口的Component将在回收到对象池时调用<see cref="Release"/></para>
        /// <para><b><see cref="Release"/>中禁止调用<see cref="Unspawn(GameObject)"/></b></para>
        /// </summary>
        public interface IPoolPawn
        {
            void Release();
        }

        private readonly Dictionary<GameObject, Stack<GameObject>> GameObjectPools = new();
        private readonly Dictionary<GameObject, GameObject> LeaveGameObjectPoolObjects = new();
        private bool GameObjectPoolStatus = true;

        /// <summary>
        /// 检查是否存在指定的游戏对象池
        /// </summary>
        /// <param name="prefab">用做键的预制体</param>
        /// <returns></returns>
        public bool Contains(GameObject prefab)
        {
            return GameObjectPools.ContainsKey(prefab);
        }

        /// <summary>
        /// 确保不会嵌套调用
        /// </summary>
        /// <exception cref="GameException"></exception>
        private void EnsureSafeStatus()
        {
            if (GameObjectPoolStatus == false)
            {
                throw new GameException("GameObjectPool is busy now.");
            }
        }

        /// <summary>
        /// 生成游戏对象
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefab)
        {
            EnsureSafeStatus();
            lock (GameObjectPools)
            {
                if (!GameObjectPools.ContainsKey(prefab))
                {
                    GameObjectPools[prefab] = new();
                }

                var pool = GameObjectPools[prefab];
                GameObject instance;
                if (pool.Count > 0)
                {
                    instance = pool.Pop();
                    instance.SetActive(true);
                    instance.transform.SetParent(null);
                }
                else
                {
                    instance = GameObject.Instantiate(prefab);
                }

                lock (LeaveGameObjectPoolObjects)
                {
                    LeaveGameObjectPoolObjects[instance] = prefab;
                }

                return instance;
            }
        }

        /// <summary>
        /// 回收游戏对象
        /// </summary>
        /// <param name="instance"></param>
        /// <exception cref="PublicType.GameException"></exception>
        public void Unspawn(GameObject instance)
        {
            EnsureSafeStatus();
            lock (LeaveGameObjectPoolObjects)
            {
                if (LeaveGameObjectPoolObjects.TryGetValue(instance, out var prefab))
                {
                    var releaser = instance.GetComponents<IPoolPawn>();
                    GameObjectPoolStatus = false;
                    foreach (var r in releaser)
                    {
                        r.Release();
                    }
                    GameObjectPoolStatus = true;
                    instance.SetActive(false);
                    instance.transform.SetParent(ConventionUtility.Singleton.transform);
                    lock (GameObjectPools)
                    {
                        GameObjectPools[prefab].Push(instance);
                        LeaveGameObjectPoolObjects.Remove(instance);
                    }
                }
                else
                {
                    Debug.LogError("This object does not belong to any pool.", instance);
                    throw new PublicType.GameException("This object does not belong to any pool.");
                }
            }
        }

        public void Clear(GameObject prefab)
        {
            EnsureSafeStatus();
            lock (GameObjectPools)
            {
                lock (LeaveGameObjectPoolObjects)
                {
                    // 销毁池中对象
                    if (GameObjectPools.TryGetValue(prefab, out var pool))
                    {
                        while (pool.Count > 0)
                        {
                            var instance = pool.Pop();
                            GameObject.Destroy(instance);
                        }
                    }
                    else
                    {
                        throw new PublicType.GameException("This pool does not exist.");
                    }
                    // 销毁未回收对象
                    var unbackInstances = from item in LeaveGameObjectPoolObjects
                                          where item.Value == prefab
                                          select item.Key;
                    foreach (var instance in unbackInstances)
                    {
                        LeaveGameObjectPoolObjects.Remove(instance);
                    }
                    foreach (var instance in unbackInstances)
                    {
                        GameObject.Destroy(instance);
                    }
                }
            }
        }
    }

    public class ObjectPoolManager
    {
        /// <summary>
        /// <para>继承该接口的Component将在回收到对象池时调用<see cref="Release"/></para>
        /// <para><b><see cref="Release"/>中禁止调用<see cref="Reclaim(object)"/></b></para>
        /// </summary>
        public interface IPoolPawn
        {
            void Release();
        }

        private readonly Dictionary<Type, object> ObjectPools = new();
        private readonly Dictionary<object, Type> LeaveObjectPoolObjects = new();

        private bool GameObjectPoolStatus = true;
        private void EnsureSafeStatus()
        {
            if (GameObjectPoolStatus == false)
            {
                throw new GameException("ObjectPool is busy now.");
            }
        }

        public bool Contains(Type type)
        {
            return ObjectPools.ContainsKey(type);
        }   

        public bool Contains<T>() where T : class, new()
        {
            return ObjectPools.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 插入自行创建的对象到对象池
        /// </summary>
        /// <param name="type">期望类型</param>
        /// <param name="value">实例</param>
        /// <returns></returns>
        /// <exception cref="GameException"></exception>
        public object Insert(Type type, object value)
        {
            if(LeaveObjectPoolObjects.ContainsKey(value))
            {
                throw new PublicType.GameException("This object is already registered in pool");
            }
            if (!ObjectPools.ContainsKey(type))
            {
                ObjectPools[type] = Utility.CreateStack(type);
            }
            var pool = ObjectPools[type];
            var methodInfo = pool.GetType().GetMethod("Push");
            methodInfo.Invoke(pool, new object[] { value });
            return value;
        }

        /// <summary>
        /// 自行创建对象并插入对象池
        /// </summary>
        /// <typeparam name="T">期望类型</typeparam>
        /// <param name="value">实例</param>
        /// <returns></returns>
        /// <exception cref="PublicType.GameException"></exception>
        public T Insert<T>(T value) where T : class
        {
            if(LeaveObjectPoolObjects.ContainsKey(value))
            {
                throw new PublicType.GameException("This object is already registered in pool");
            }
            var type = typeof(T);
            if (!ObjectPools.ContainsKey(type))
            {
                ObjectPools[type] = new Stack<T>();
            }
            var pool = (Stack<T>)ObjectPools[type];
            pool.Push(value);
            return value;
        }

        /// <summary>
        /// 捕获一般对象并返回实例
        /// </summary>
        /// <param name="type">期望类型</param>
        /// <returns></returns>
        public object Summon(Type type)
        {
            if (!ObjectPools.ContainsKey(type))
            {
                ObjectPools[type] = Utility.CreateStack(type);
            }
            var pool = ObjectPools[type];
            var methodInfo = pool.GetType().GetMethod("Pop");
            object instance;
            var countProperty = pool.GetType().GetProperty("Count");
            var count = (int)countProperty.GetValue(pool);
            if (count > 0)
            {
                instance = methodInfo.Invoke(pool, null);
            }
            else
            {
                instance = Activator.CreateInstance(type);
            }
            LeaveObjectPoolObjects[instance] = type;
            return instance;
        }

        /// <summary>
        /// 捕获一般对象并返回实例
        /// </summary>
        /// <typeparam name="T">期望类型</typeparam>
        /// <returns></returns>
        public T Summon<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!ObjectPools.ContainsKey(type))
            {
                ObjectPools[type] = new Stack<T>();
            }
            var pool = (Stack<T>)ObjectPools[type];
            T instance;
            if (pool.Count > 0)
            {
                instance = pool.Pop();
            }
            else
            {
                instance = new T();
            }
            LeaveObjectPoolObjects[instance] = type;
            return instance;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="instance">实例</param>
        /// <exception cref="PublicType.GameException"></exception>
        public void Reclaim(object instance)
        {
            if (LeaveObjectPoolObjects.TryGetValue(instance, out var objType))
            {
                var pool = ObjectPools[objType];
                var methodInfo = pool.GetType().GetMethod("Push");
                if(instance is IPoolPawn pawn)
                {
                    pawn.Release();
                }
                methodInfo.Invoke(pool, new object[] { instance });
                LeaveObjectPoolObjects.Remove(instance);
            }
            else
            {
                throw new GameException("This object does not belong to any pool.");
            }
        }

        public void Clear(Type type)
        {
            if (ObjectPools.TryGetValue(type, out var pool))
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod.Invoke(pool, null);
            }
            else
            {
                throw new PublicType.GameException("This pool does not exist.");
            }
        }

        public void Clear<T>() where T : class, new()
        {
            var type = typeof(T);
            if (ObjectPools.TryGetValue(type, out var pool))
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod.Invoke(pool, null);
            }
            else
            {
                throw new PublicType.GameException("This pool does not exist.");
            }
        }
    }
}
