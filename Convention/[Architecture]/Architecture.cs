using Convention;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Convention.Experimental.PublicType;

namespace Convention.Experimental
{
    public static class Architecture
    {
        private static readonly LinkedCacheList<GameModule> s_GameFrameworkModules = new();

        /// <summary>
        /// 创建<see cref="GameModule"/>
        /// </summary>
        /// <param name="moduleType">要创建的<see cref="GameModule"/>类型</param>
        /// <returns>要创建的<see cref="GameModule"/></returns>
        private static GameModule CreateModule(Type moduleType)
        {
            GameModule module = (GameModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new GameException($"Can not create module '{moduleType.FullName}'");
            }

            LinkedListNode<GameModule> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
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

            return module;
        }

        /// <summary>
        /// 获取<see cref="GameModule"/>
        /// </summary>
        /// <param name="moduleType">要获取的<see cref="GameModule"/></param>
        /// <returns>要获取的<see cref="GameModule"/></returns>
        /// <remarks>如果要获取的<see cref="GameModule"/>不存在，则自动创建该<see cref="GameModule"/>实例。</remarks>
        private static GameModule GetModule(Type moduleType)
        {
            foreach (GameModule module in s_GameFrameworkModules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : GameModule
        {
            return (T)GetModule(typeof(T));
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (var current = s_GameFrameworkModules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
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
            foreach (var module in s_GameFrameworkModules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }
    }
}