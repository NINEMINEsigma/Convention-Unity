
using Convention.Experimental.PublicType;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Convention.ReferenceManagement
{
    /// <summary>
    /// 引用接口
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 清理引用
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 引用池
    /// </summary>
    public static partial class ReferencePool
    {
        private static bool m_EnableStrictCheck = false;

        /// <summary>
        /// 获取或设置是否开启强制检查
        /// </summary>
        public static bool EnableStrictCheck
        {
            get
            {
                return m_EnableStrictCheck;
            }
            set
            {
                m_EnableStrictCheck = value;
            }
        }


        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> m_References;
            private readonly Type m_ReferenceType;
            private int m_UsingReferenceCount;
            private int m_AcquireReferenceCount;
            private int m_ReleaseReferenceCount;
            private int m_AddReferenceCount;
            private int m_RemoveReferenceCount;

            public ReferenceCollection(Type referenceType)
            {
                m_References = new Queue<IReference>();
                m_ReferenceType = referenceType;
                m_UsingReferenceCount = 0;
                m_AcquireReferenceCount = 0;
                m_ReleaseReferenceCount = 0;
                m_AddReferenceCount = 0;
                m_RemoveReferenceCount = 0;
            }

            public Type ReferenceType
            {
                get
                {
                    return m_ReferenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return m_References.Count;
                }
            }

            public int UsingReferenceCount
            {
                get
                {
                    return m_UsingReferenceCount;
                }
            }

            public int AcquireReferenceCount
            {
                get
                {
                    return m_AcquireReferenceCount;
                }
            }

            public int ReleaseReferenceCount
            {
                get
                {
                    return m_ReleaseReferenceCount;
                }
            }

            public int AddReferenceCount
            {
                get
                {
                    return m_AddReferenceCount;
                }
            }

            public int RemoveReferenceCount
            {
                get
                {
                    return m_RemoveReferenceCount;
                }
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new GameException("Type is invalid.");
                }

                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T)m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return (IReference)Activator.CreateInstance(m_ReferenceType);
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                lock (m_References)
                {
                    if (m_EnableStrictCheck && m_References.Contains(reference))
                    {
                        throw new GameException("The reference has been released.");
                    }

                    m_References.Enqueue(reference);
                }

                m_ReleaseReferenceCount++;
                m_UsingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new GameException("Type is invalid.");
                }

                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference)Activator.CreateInstance(m_ReferenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    m_RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_References)
                {
                    m_RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }
        }

        private static readonly Dictionary<Type, ReferenceCollection> s_ReferenceCollections = new();

        /// <summary>
        /// 获取引用池的数量。
        /// </summary>
        public static int Count
        {
            get
            {
                return s_ReferenceCollections.Count;
            }
        }

        /// <summary>
        /// 引用池信息
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ReferencePoolInfo
        {
            /// <summary>
            /// 引用池类型
            /// </summary>
            public readonly Type Type;
            /// <summary>
            /// 未使用引用数量
            /// </summary>
            public readonly int UnusedReferenceCount;
            /// <summary>
            /// 正在使用引用数量
            /// </summary>
            public readonly int UsingReferenceCount;
            /// <summary>
            /// 获取引用数量
            /// </summary>
            public readonly int AcquireReferenceCount;
            /// <summary>
            /// 归还引用数量
            /// </summary>
            public readonly int ReleaseReferenceCount;
            /// <summary>
            /// 增加引用数量
            /// </summary>
            public readonly int AddReferenceCount;
            /// <summary>
            /// 移除引用数量
            /// </summary>
            public readonly int RemoveReferenceCount;

            /// <summary>
            /// 初始化引用池信息的新实例。
            /// </summary>
            /// <param name="type">引用池类型。</param>
            /// <param name="unusedReferenceCount">未使用引用数量。</param>
            /// <param name="usingReferenceCount">正在使用引用数量。</param>
            /// <param name="acquireReferenceCount">获取引用数量。</param>
            /// <param name="releaseReferenceCount">归还引用数量。</param>
            /// <param name="addReferenceCount">增加引用数量。</param>
            /// <param name="removeReferenceCount">移除引用数量。</param>
            public ReferencePoolInfo(Type type,
                                     int unusedReferenceCount,
                                     int usingReferenceCount,
                                     int acquireReferenceCount,
                                     int releaseReferenceCount,
                                     int addReferenceCount,
                                     int removeReferenceCount)
            {
                this.Type = type;
                this.UnusedReferenceCount = unusedReferenceCount;
                this.UsingReferenceCount = usingReferenceCount;
                this.AcquireReferenceCount = acquireReferenceCount;
                this.ReleaseReferenceCount = releaseReferenceCount;
                this.AddReferenceCount = addReferenceCount;
                this.RemoveReferenceCount = removeReferenceCount;
            }
        }

        /// <summary>
        /// 获取所有引用池的信息
        /// </summary>
        /// <returns>所有引用池的信息</returns>
        public static ReferencePoolInfo[] GetAllReferencePoolInfos()
        {
            int index = 0;
            ReferencePoolInfo[] results = null;

            lock (s_ReferenceCollections)
            {
                results = new ReferencePoolInfo[s_ReferenceCollections.Count];
                foreach (var (key, value) in s_ReferenceCollections)
                {
                    results[index++] = new ReferencePoolInfo(key,
                                                             value.UnusedReferenceCount,
                                                             value.UsingReferenceCount,
                                                             value.AcquireReferenceCount,
                                                             value.ReleaseReferenceCount,
                                                             value.AddReferenceCount,
                                                             value.RemoveReferenceCount);
                }
            }

            return results;
        }

        /// <summary>
        /// 清除所有引用池
        /// </summary>
        public static void ClearAll()
        {
            lock (s_ReferenceCollections)
            {
                foreach (var (_, value) in s_ReferenceCollections)
                {
                    value.RemoveAll();
                }

                s_ReferenceCollections.Clear();
            }
        }

        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new GameException("ReferenceType is invalid.");
            }

            ReferenceCollection referenceCollection = null;
            lock (s_ReferenceCollections)
            {
                if (!s_ReferenceCollections.TryGetValue(referenceType, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(referenceType);
                    s_ReferenceCollections.Add(referenceType, referenceCollection);
                }
            }

            return referenceCollection;
        }

        private static void InternalCheckReferenceType(Type referenceType)
        {
            if (!m_EnableStrictCheck)
            {
                return;
            }

            if (referenceType == null)
            {
                throw new GameException("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new GameException("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(referenceType))
            {
                throw new GameException($"Reference type '{referenceType.FullName}' is invalid.");
            }
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <returns>引用</returns>
        public static IReference Acquire(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            return GetReferenceCollection(referenceType).Acquire();
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns>引用</returns>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        /// 将引用归还引用池
        /// </summary>
        /// <param name="reference">引用</param>
        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new GameException("Reference is invalid.");
            }

            Type referenceType = reference.GetType();
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Release(reference);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">追加数量</param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <param name="count">追加数量</param>
        public static void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">移除数量</param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <param name="referenceType">引用类型</param>
        /// <param name="count">移除数量</param>
        public static void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
        }
    }
}