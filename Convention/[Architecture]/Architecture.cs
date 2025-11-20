using Convention.Experimental.PublicType;
using System;
using System.Collections.Generic;

namespace Convention.Experimental
{
    public static class Architecture
    {
        private static readonly LinkedCacheList<object> s_GameFrameworkModules = new();

        /// <summary>
        /// 创建实例类型
        /// </summary>
        /// <param name="instanceType">要创建的类型</param>
        /// <returns>目标实例</returns>
        private static object Create(Type instanceType, int stackLayer = 0)
        {
            if (stackLayer >= 100)
            {
                throw new GameException($"Create instance '{instanceType.FullName}' failed, recursion too deep, there may be a circular dependency.");
            }

            object instance = Activator.CreateInstance(instanceType);
            if (instance == null)
            {
                throw new GameException($"Can not create instance '{instanceType.FullName}'");
            }

            if (instance is GameModule module)
            {
                // 递归创建依赖模块
                foreach (var dependenceType in module.Dependences())
                    _ = Get(dependenceType, stackLayer + 1);

                var current = s_GameFrameworkModules.First;
                while (current != null)
                {
                    if (current.Value is GameModule nextModule && module.Priority > nextModule.Priority)
                    {
                        break;
                    }

                    current = current.Next;
                }

                if (current != null)
                {
                    s_GameFrameworkModules.AddBefore(current, module);
                }
                else
                {
                    s_GameFrameworkModules.AddLast(module);
                }
            }

            return instance;
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="instanceType">要获取的实例类型</param>
        /// <returns>实例</returns>
        /// <remarks>如果要获取的实例类型不存在，则自动创建该类型的实例。</remarks>
        public static object Get(Type instanceType, int stackLayer = 0)
        {
            foreach (object obj in s_GameFrameworkModules)
            {
                if (obj.GetType() == instanceType)
                {
                    return obj;
                }
            }

            return Create(instanceType, stackLayer + 1);
        }

        /// <summary>
        /// 是否存在实例
        /// </summary>
        /// <param name="type">要检查的实例类型</param>
        /// <returns>实例是否存在</returns>
        public static bool Contains(Type type)
        {
            foreach (object obj in s_GameFrameworkModules)
            {
                if (obj.GetType() == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : GameModule
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (var current = s_GameFrameworkModules.Last; current != null; current = current.Previous)
            {
                if (current.Value is GameModule module)
                    module.Shutdown();
            }

            s_GameFrameworkModules.Clear();
            ReferencePool.ClearAll();
            Utility.Marshal.FreeCachedHGlobal();
        }

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (object obj in s_GameFrameworkModules)
            {
                if (obj is GameModule module)
                    module.Update(elapseSeconds, realElapseSeconds);
            }
        }
    }
}