using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace Convention.Collections
{
    namespace Generic
    {
        /// <summary>
        /// 附带有缓存机制的链表类
        /// </summary>
        /// <typeparam name="T">指定链表的元素类型</typeparam>
        public sealed class LinkedCacheList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
        {
            private readonly LinkedList<T> m_LinkedList;
            /// <summary>
            /// 之所以需要这套缓存机制，主要有三点好处：
            /// <list dataType="bullet">减少 GC 压力：大量频繁的插入/删除会产生很多短生命周期的节点对象，通过复用可以显著降低托管堆的分配与回收次数</list>
            /// <list dataType="bullet">提升性能：避免频繁 new 和 GC，能减少停顿时间，提高链表在高频操作场景下的吞吐</list>
            /// <list dataType="bullet">便于观察与管理：类中还提供了 CachedNodeCount 方便调试统计缓存规模，必要时可通过 ClearCachedNodes 主动释放</list>
            /// 适用场景是节点使用模式“高频增删但总量有限”，此时缓存能稳定性能；若链表规模始终在增长且很少释放，缓存收益会较低。
            /// </summary>
            private readonly Queue<LinkedListNode<T>> m_CachedNodes;

            /// <summary>
            /// 初始化游戏框架链表类的新实例
            /// </summary>
            public LinkedCacheList()
            {
                m_LinkedList = new LinkedList<T>();
                m_CachedNodes = new Queue<LinkedListNode<T>>();
            }

            /// <summary>
            /// 获取链表中实际包含的结点数量
            /// </summary>
            public int Count
            {
                get
                {
                    return m_LinkedList.Count;
                }
            }

            /// <summary>
            /// 获取链表结点缓存数量
            /// </summary>
            public int CachedNodeCount
            {
                get
                {
                    return m_CachedNodes.Count;
                }
            }

            /// <summary>
            /// 获取链表的第一个结点
            /// </summary>
            public LinkedListNode<T> First
            {
                get
                {
                    return m_LinkedList.First;
                }
            }

            /// <summary>
            /// 获取链表的最后一个结点
            /// </summary>
            public LinkedListNode<T> Last
            {
                get
                {
                    return m_LinkedList.Last;
                }
            }

            /// <summary>
            /// 获取一个值，该值指示 <see cref="ICollection{T}"/> 是否为只读
            /// </summary>
            public bool IsReadOnly
            {
                get
                {
                    return ((ICollection<T>)m_LinkedList).IsReadOnly;
                }
            }

            /// <summary>
            /// 获取可用于同步对 <see cref="ICollection"/> 的访问的对象。
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    return ((ICollection)m_LinkedList).SyncRoot;
                }
            }

            /// <summary>
            /// 获取一个值，该值指示是否同步对 <see cref="ICollection"/> 的访问（线程安全）。
            /// </summary>
            public bool IsSynchronized
            {
                get
                {
                    return ((ICollection)m_LinkedList).IsSynchronized;
                }
            }

            /// <summary>
            /// 在链表中指定的现有结点后添加包含指定值的新结点
            /// </summary>
            /// <param name="node">指定的现有结点</param>
            /// <param name="value">指定值</param>
            /// <returns>包含指定值的新结点</returns>
            public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
            {
                LinkedListNode<T> newNode = AcquireNode(value);
                m_LinkedList.AddAfter(node, newNode);
                return newNode;
            }

            /// <summary>
            /// 在链表中指定的现有结点后添加指定的新结点
            /// </summary>
            /// <param name="node">指定的现有结点</param>
            /// <param name="newNode">指定的新结点</param>
            public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
            {
                m_LinkedList.AddAfter(node, newNode);
            }

            /// <summary>
            /// 在链表中指定的现有结点前添加包含指定值的新结点
            /// </summary>
            /// <param name="node">指定的现有结点</param>
            /// <param name="value">指定值</param>
            /// <returns>包含指定值的新结点</returns>
            public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
            {
                LinkedListNode<T> newNode = AcquireNode(value);
                m_LinkedList.AddBefore(node, newNode);
                return newNode;
            }

            /// <summary>
            /// 在链表中指定的现有结点前添加指定的新结点
            /// </summary>
            /// <param name="node">指定的现有结点</param>
            /// <param name="newNode">指定的新结点</param>
            public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
            {
                m_LinkedList.AddBefore(node, newNode);
            }

            /// <summary>
            /// 在链表的开头处添加包含指定值的新结点
            /// </summary>
            /// <param name="value">指定值</param>
            /// <returns>包含指定值的新结点</returns>
            public LinkedListNode<T> AddFirst(T value)
            {
                LinkedListNode<T> node = AcquireNode(value);
                m_LinkedList.AddFirst(node);
                return node;
            }

            /// <summary>
            /// 在链表的开头处添加指定的新结点
            /// </summary>
            /// <param name="node">指定的新结点</param>
            public void AddFirst(LinkedListNode<T> node)
            {
                m_LinkedList.AddFirst(node);
            }

            /// <summary>
            /// 在链表的结尾处添加包含指定值的新结点
            /// </summary>
            /// <param name="value">指定值</param>
            /// <returns>包含指定值的新结点</returns>
            public LinkedListNode<T> AddLast(T value)
            {
                LinkedListNode<T> node = AcquireNode(value);
                m_LinkedList.AddLast(node);
                return node;
            }

            /// <summary>
            /// 在链表的结尾处添加指定的新结点
            /// </summary>
            /// <param name="node">指定的新结点</param>
            public void AddLast(LinkedListNode<T> node)
            {
                m_LinkedList.AddLast(node);
            }

            /// <summary>
            /// 从链表中移除所有结点
            /// </summary>
            public void Clear()
            {
                LinkedListNode<T> current = m_LinkedList.First;
                while (current != null)
                {
                    ReleaseNode(current);
                    current = current.Next;
                }

                m_LinkedList.Clear();
            }

            /// <summary>
            /// 清除链表结点缓存
            /// </summary>
            public void ClearCachedNodes()
            {
                m_CachedNodes.Clear();
            }

            /// <summary>
            /// 确定某值是否在链表中
            /// </summary>
            /// <param name="value">指定值</param>
            /// <returns>某值是否在链表中</returns>
            public bool Contains(T value)
            {
                return m_LinkedList.Contains(value);
            }

            /// <summary>
            /// 从目标数组的指定索引处开始将整个链表复制到兼容的一维数组
            /// </summary>
            /// <param name="array">一维数组，它是从链表复制的元素的目标。数组必须具有从零开始的索引</param>
            /// <param name="index">array 中从零开始的索引，从此处开始复制</param>
            public void CopyTo(T[] array, int index)
            {
                m_LinkedList.CopyTo(array, index);
            }

            /// <summary>
            /// 从特定的 ICollection 索引开始，将数组的元素复制到一个数组中
            /// </summary>
            /// <param name="array">一维数组，它是从 ICollection 复制的元素的目标。数组必须具有从零开始的索引</param>
            /// <param name="index">array 中从零开始的索引，从此处开始复制</param>
            public void CopyTo(Array array, int index)
            {
                ((ICollection)m_LinkedList).CopyTo(array, index);
            }

            /// <summary>
            /// 查找包含指定值的第一个结点
            /// </summary>
            /// <param name="value">要查找的指定值</param>
            /// <returns>包含指定值的第一个结点</returns>
            public LinkedListNode<T> Find(T value)
            {
                return m_LinkedList.Find(value);
            }

            /// <summary>
            /// 查找包含指定值的最后一个结点
            /// </summary>
            /// <param name="value">要查找的指定值</param>
            /// <returns>包含指定值的最后一个结点</returns>
            public LinkedListNode<T> FindLast(T value)
            {
                return m_LinkedList.FindLast(value);
            }

            /// <summary>
            /// 从链表中移除指定值的第一个匹配项
            /// </summary>
            /// <param name="value">指定值</param>
            /// <returns>是否移除成功</returns>
            public bool Remove(T value)
            {
                LinkedListNode<T> node = m_LinkedList.Find(value);
                if (node != null)
                {
                    m_LinkedList.Remove(node);
                    ReleaseNode(node);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 从链表中移除指定的结点
            /// </summary>
            /// <param name="node">指定的结点</param>
            public void Remove(LinkedListNode<T> node)
            {
                m_LinkedList.Remove(node);
                ReleaseNode(node);
            }

            /// <summary>
            /// 移除位于链表开头处的结点
            /// </summary>
            public void RemoveFirst()
            {
                LinkedListNode<T> first = m_LinkedList.First;
                if (first == null)
                {
                    throw new InvalidOperationException("Rmove first is invalid.");
                }

                m_LinkedList.RemoveFirst();
                ReleaseNode(first);
            }

            /// <summary>
            /// 移除位于链表结尾处的结点。
            /// </summary>
            public void RemoveLast()
            {
                LinkedListNode<T> last = m_LinkedList.Last;
                if (last == null)
                {
                    throw new InvalidOperationException("Remove last is invalid.");
                }

                m_LinkedList.RemoveLast();
                ReleaseNode(last);
            }

            /// <summary>
            /// 返回循环访问集合的枚举数
            /// </summary>
            /// <returns>循环访问集合的枚举数</returns>
            public Enumerator GetEnumerator()
            {
                return new Enumerator(m_LinkedList);
            }

            private LinkedListNode<T> AcquireNode(T value)
            {
                LinkedListNode<T> node = null;
                if (m_CachedNodes.Count > 0)
                {
                    node = m_CachedNodes.Dequeue();
                    node.Value = value;
                }
                else
                {
                    node = new LinkedListNode<T>(value);
                }

                return node;
            }

            private void ReleaseNode(LinkedListNode<T> node)
            {
                node.Value = default(T);
                m_CachedNodes.Enqueue(node);
            }

            /// <summary>
            /// 将值添加到 ICollection`1 的结尾处
            /// </summary>
            /// <param name="value">要添加的值</param>
            void ICollection<T>.Add(T value)
            {
                AddLast(value);
            }

            /// <summary>
            /// 返回循环访问集合的枚举数
            /// </summary>
            /// <returns>循环访问集合的枚举数</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// 返回循环访问集合的枚举数
            /// </summary>
            /// <returns>循环访问集合的枚举数</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// 循环访问集合的枚举数
            /// </summary>
            [StructLayout(LayoutKind.Auto)]
            public struct Enumerator : IEnumerator<T>, IEnumerator
            {
                private LinkedList<T>.Enumerator m_Enumerator;

                internal Enumerator(LinkedList<T> linkedList)
                {
                    if (linkedList == null)
                    {
                        throw new InvalidOperationException("Linked list is invalid.");
                    }

                    m_Enumerator = linkedList.GetEnumerator();
                }

                /// <summary>
                /// 获取当前结点
                /// </summary>
                public T Current
                {
                    get
                    {
                        return m_Enumerator.Current;
                    }
                }

                /// <summary>
                /// 获取当前的枚举数
                /// </summary>
                object IEnumerator.Current
                {
                    get
                    {
                        return m_Enumerator.Current;
                    }
                }

                /// <summary>
                /// 清理枚举数
                /// </summary>
                public void Dispose()
                {
                    m_Enumerator.Dispose();
                }

                /// <summary>
                /// 获取下一个结点
                /// </summary>
                /// <returns>返回下一个结点</returns>
                public bool MoveNext()
                {
                    return m_Enumerator.MoveNext();
                }

                /// <summary>
                /// 重置枚举数
                /// </summary>
                void IEnumerator.Reset()
                {
                    ((IEnumerator<T>)m_Enumerator).Reset();
                }
            }
        }
    }

#if UNITY_6000_0_OR_NEWER
    namespace Cache
    {
        /// <summary>
        /// Native LRUCache - 基于Unity.Collections的高性能最近最少使用缓存
        /// Burst兼容，需要手动Dispose
        /// </summary>
        /// <typeparam name="TKey">键类型（必须是unmanaged类型）</typeparam>
        /// <typeparam name="TValue">值类型（必须是unmanaged类型）</typeparam>
        public struct NativeLRUCache<TKey, TValue> : IDisposable
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            // 链表节点
            private struct Node
            {
                public TKey key;
                public TValue value;
                public int prevIndex;  // 前一个节点索引
                public int nextIndex;  // 后一个节点索引
            }

            private NativeList<Node> nodes;                     // 节点池
            private readonly NativeHashMap<TKey, int> hashMap;  // key -> 节点索引
            private readonly NativeList<int> freeList;          // 空闲节点索引池

            private readonly int capacity;
            private int headIndex;                         // 链表头（最久未使用）
            private int tailIndex;                         // 链表尾（最近使用）
            private int count;

            // 是否为有效节点索引
            private readonly bool IsValidIndex(int index) => index >= 0;

            public NativeLRUCache(int capacity, Allocator allocator)
            {
                this.capacity = capacity;
                this.count = 0;
                this.headIndex = -1;
                this.tailIndex = -1;

                hashMap = new NativeHashMap<TKey, int>(capacity, allocator);
                nodes = new NativeList<Node>(capacity, allocator);
                freeList = new NativeList<int>(capacity, allocator);

                // 预分配节点池
                nodes.Length = capacity;
                for (int i = capacity - 1; i >= 0; i--)
                {
                    freeList.Add(i);
                }
            }

            /// <summary>
            /// 获取缓存值
            /// </summary>
            public bool TryGetValue(TKey key, out TValue value)
            {
                if (hashMap.TryGetValue(key, out int nodeIndex))
                {
                    MoveToTail(nodeIndex);  // 移动到尾部（最近使用）
                    value = nodes[nodeIndex].value;
                    return true;
                }

                value = default;
                return false;
            }

            /// <summary>
            /// 添加或更新缓存
            /// </summary>
            public void Put(TKey key, TValue value)
            {
                // 已存在则更新并移动到尾部
                if (hashMap.TryGetValue(key, out int nodeIndex))
                {
                    Node node = nodes[nodeIndex];
                    node.value = value;
                    nodes[nodeIndex] = node;
                    MoveToTail(nodeIndex);
                    return;
                }

                // 容量已满，淘汰头部节点
                if (count >= capacity)
                {
                    EvictHead();
                }

                // 创建新节点
                nodeIndex = AllocateNode();
                nodes[nodeIndex] = new Node
                {
                    key = key,
                    value = value,
                    prevIndex = tailIndex,
                    nextIndex = -1
                };

                hashMap.Add(key, nodeIndex);
                AddToTail(nodeIndex);
                count++;
            }

            /// <summary>
            /// 删除缓存项
            /// </summary>
            public bool Remove(TKey key)
            {
                if (!hashMap.TryGetValue(key, out int nodeIndex))
                    return false;

                hashMap.Remove(key);
                RemoveFromList(nodeIndex);
                FreeNode(nodeIndex);
                count--;
                return true;
            }

            /// <summary>
            /// 清空缓存
            /// </summary>
            public void Clear()
            {
                hashMap.Clear();

                // 重置所有节点到空闲列表
                freeList.Clear();
                for (int i = 0; i < capacity; i++)
                {
                    freeList.Add(i);
                }

                headIndex = -1;
                tailIndex = -1;
                count = 0;
            }

            // 分配节点索引
            private readonly int AllocateNode()
            {
                int index = freeList[^1];
                freeList.RemoveAtSwapBack(freeList.Length - 1);
                return index;
            }

            // 释放节点索引
            private readonly void FreeNode(int index)
            {
                freeList.Add(index);
            }

            // 将节点移动到链表尾部（标记为最近使用）
            private void MoveToTail(int nodeIndex)
            {
                if (nodeIndex == tailIndex)
                    return;

                RemoveFromList(nodeIndex);
                AddToTail(nodeIndex);
            }

            // 将节点添加到尾部
            private void AddToTail(int nodeIndex)
            {
                Node node = nodes[nodeIndex];
                node.prevIndex = tailIndex;
                node.nextIndex = -1;
                nodes[nodeIndex] = node;

                if (IsValidIndex(tailIndex))
                {
                    Node tail = nodes[tailIndex];
                    tail.nextIndex = nodeIndex;
                    nodes[tailIndex] = tail;
                }

                tailIndex = nodeIndex;

                // 如果链表为空，头尾指向同一节点
                if (!IsValidIndex(headIndex))
                {
                    headIndex = nodeIndex;
                }
            }

            // 从链表中移除节点
            private void RemoveFromList(int nodeIndex)
            {
                Node node = nodes[nodeIndex];

                // 更新前一个节点的next指针
                if (IsValidIndex(node.prevIndex))
                {
                    Node prev = nodes[node.prevIndex];
                    prev.nextIndex = node.nextIndex;
                    nodes[node.prevIndex] = prev;
                }
                else
                {
                    // 没有前一个节点，说明是头节点
                    headIndex = node.nextIndex;
                }

                // 更新后一个节点的prev指针
                if (IsValidIndex(node.nextIndex))
                {
                    Node next = nodes[node.nextIndex];
                    next.prevIndex = node.prevIndex;
                    nodes[node.nextIndex] = next;
                }
                else
                {
                    // 没有后一个节点，说明是尾节点
                    tailIndex = node.prevIndex;
                }
            }

            // 淘汰头部节点（最久未使用）
            private void EvictHead()
            {
                if (!IsValidIndex(headIndex)) return;

                int oldHead = headIndex;
                Node headNode = nodes[oldHead];

                hashMap.Remove(headNode.key);
                headIndex = headNode.nextIndex;

                if (IsValidIndex(headIndex))
                {
                    Node newHead = nodes[headIndex];
                    newHead.prevIndex = -1;
                    nodes[headIndex] = newHead;
                }
                else
                {
                    tailIndex = -1;  // 链表为空
                }

                FreeNode(oldHead);
                count--;
            }

            public void Dispose()
            {
                if (hashMap.IsCreated) hashMap.Dispose();
                if (nodes.IsCreated) nodes.Dispose();
                if (freeList.IsCreated) freeList.Dispose();

                headIndex = -1;
                tailIndex = -1;
                count = 0;
            }

            // 属性访问器
            public readonly int Count => count;
            public readonly int Capacity => capacity;
            public readonly bool IsCreated => hashMap.IsCreated;
        }

        /// <summary>
        /// Native LFUCache - 基于Unity.Collections的高性能最不频繁使用缓存
        /// Burst兼容，需要手动Dispose
        /// </summary>
        /// <typeparam name="TKey">键类型（必须是unmanaged类型）</typeparam>
        /// <typeparam name="TValue">值类型（必须是unmanaged类型）</typeparam>
        public struct NativeLFUCache<TKey, TValue> : IDisposable
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            // 缓存节点
            private struct Node
            {
                public TKey key;
                public TValue value;
                public int frequency;      // 访问频率
                public int prevIndex;      // 同频率链表中的前一个节点
                public int nextIndex;      // 同频率链表中的后一个节点
            }

            // 频率链表头尾
            private struct FrequencyList
            {
                public int headIndex;  // 该频率链表的头节点（最久未使用）
                public int tailIndex;  // 该频率链表的尾节点（最近使用）

                public readonly bool IsEmpty => headIndex == -1;
            }

            private readonly NativeHashMap<TKey, int> keyToNodeIndex;   // key -> nodeIndex
            private NativeList<Node> nodes;                             // 节点池
            private readonly NativeList<int> freeList;                  // 空闲节点索引
            private NativeHashMap<int, FrequencyList> freqToList;       // frequency -> FrequencyList

            private readonly int capacity;
            private int count;
            private int minFrequency;  // 当前最小频率

            public NativeLFUCache(int capacity, Allocator allocator)
            {
                this.capacity = capacity;
                this.count = 0;
                this.minFrequency = 0;

                keyToNodeIndex = new NativeHashMap<TKey, int>(capacity, allocator);
                nodes = new NativeList<Node>(capacity, allocator);
                freeList = new NativeList<int>(capacity, allocator);
                freqToList = new NativeHashMap<int, FrequencyList>(capacity, allocator);

                // 预分配节点池
                nodes.Length = capacity;
                for (int i = capacity - 1; i >= 0; i--)
                {
                    freeList.Add(i);
                }
            }

            /// <summary>
            /// 获取缓存值并增加频率
            /// </summary>
            public bool TryGetValue(TKey key, out TValue value)
            {
                if (keyToNodeIndex.TryGetValue(key, out int nodeIndex))
                {
                    // 增加访问频率
                    IncreaseFrequency(nodeIndex);

                    value = nodes[nodeIndex].value;
                    return true;
                }

                value = default;
                return false;
            }

            /// <summary>
            /// 添加或更新缓存
            /// </summary>
            public void Put(TKey key, TValue value)
            {
                // 更新现有值
                if (keyToNodeIndex.TryGetValue(key, out int nodeIndex))
                {
                    Node node = nodes[nodeIndex];
                    node.value = value;
                    nodes[nodeIndex] = node;

                    IncreaseFrequency(nodeIndex);
                    return;
                }

                // 容量已满，淘汰最小频率的节点
                if (count >= capacity)
                {
                    EvictMinFrequencyNode();
                }

                // 创建新节点，初始频率为1
                nodeIndex = AllocateNode();
                nodes[nodeIndex] = new Node
                {
                    key = key,
                    value = value,
                    frequency = 1,
                    prevIndex = -1,
                    nextIndex = -1
                };

                keyToNodeIndex.Add(key, nodeIndex);
                AddToFrequencyList(nodeIndex, 1);

                // 更新最小频率
                if (minFrequency == 0)
                {
                    minFrequency = 1;
                }

                count++;
            }

            /// <summary>
            /// 删除缓存项
            /// </summary>
            public bool Remove(TKey key)
            {
                if (!keyToNodeIndex.TryGetValue(key, out int nodeIndex))
                    return false;

                RemoveNode(nodeIndex);
                return true;
            }

            /// <summary>
            /// 清空缓存
            /// </summary>
            public void Clear()
            {
                keyToNodeIndex.Clear();
                freqToList.Clear();

                // 重置所有节点到空闲列表
                freeList.Clear();
                for (int i = 0; i < capacity; i++)
                {
                    freeList.Add(i);
                }

                minFrequency = 0;
                count = 0;
            }

            /// <summary>
            /// 释放所有内存
            /// </summary>
            public void Dispose()
            {
                if (keyToNodeIndex.IsCreated) keyToNodeIndex.Dispose();
                if (nodes.IsCreated) nodes.Dispose();
                if (freeList.IsCreated) freeList.Dispose();
                if (freqToList.IsCreated) freqToList.Dispose();

                minFrequency = 0;
                count = 0;
            }

            #region 内部辅助方法

            private int AllocateNode()
            {
                int index = freeList[^1];
                freeList.RemoveAtSwapBack(freeList.Length - 1);
                return index;
            }

            private void FreeNode(int index)
            {
                freeList.Add(index);
            }

            /// <summary>
            /// 增加节点的访问频率
            /// </summary>
            private void IncreaseFrequency(int nodeIndex)
            {
                Node node = nodes[nodeIndex];
                int oldFreq = node.frequency;
                int newFreq = oldFreq + 1;

                // 从旧频率链表移除
                RemoveFromFrequencyList(nodeIndex, oldFreq);

                // 检查是否需要更新最小频率
                if (oldFreq == minFrequency && GetFrequencyListHead(oldFreq) == -1)
                {
                    minFrequency = newFreq;
                }

                // 更新节点频率
                node.frequency = newFreq;
                nodes[nodeIndex] = node;

                // 添加到新频率链表
                AddToFrequencyList(nodeIndex, newFreq);
            }

            /// <summary>
            /// 从频率链表中移除节点
            /// </summary>
            private void RemoveFromFrequencyList(int nodeIndex, int frequency)
            {
                if (!freqToList.TryGetValue(frequency, out var list))
                    return;

                Node node = nodes[nodeIndex];

                // 如果是头节点
                if (node.prevIndex == -1)
                {
                    list.headIndex = node.nextIndex;
                }
                else
                {
                    // 更新前一个节点的next指针
                    Node prevNode = nodes[node.prevIndex];
                    prevNode.nextIndex = node.nextIndex;
                    nodes[node.prevIndex] = prevNode;
                }

                // 如果是尾节点
                if (node.nextIndex == -1)
                {
                    list.tailIndex = node.prevIndex;
                }
                else
                {
                    // 更新后一个节点的prev指针
                    Node nextNode = nodes[node.nextIndex];
                    nextNode.prevIndex = node.prevIndex;
                    nodes[node.nextIndex] = nextNode;
                }

                // 如果链表变空，移除该频率
                if (list.headIndex == -1)
                {
                    freqToList.Remove(frequency);
                }
                else
                {
                    freqToList[frequency] = list;
                }
            }

            /// <summary>
            /// 将节点添加到频率链表尾部
            /// </summary>
            private void AddToFrequencyList(int nodeIndex, int frequency)
            {
                if (!freqToList.TryGetValue(frequency, out var list))
                {
                    // 创建新的频率链表
                    list = new FrequencyList { headIndex = nodeIndex, tailIndex = nodeIndex };
                    freqToList.Add(frequency, list);

                    // 设置节点指针
                    Node node = nodes[nodeIndex];
                    node.prevIndex = -1;
                    node.nextIndex = -1;
                    nodes[nodeIndex] = node;
                }
                else
                {
                    // 添加到链表尾部
                    int oldTail = list.tailIndex;

                    // 更新原尾节点
                    Node oldTailNode = nodes[oldTail];
                    oldTailNode.nextIndex = nodeIndex;
                    nodes[oldTail] = oldTailNode;

                    // 设置新节点
                    Node newNode = nodes[nodeIndex];
                    newNode.prevIndex = oldTail;
                    newNode.nextIndex = -1;
                    nodes[nodeIndex] = newNode;

                    // 更新链表尾
                    list.tailIndex = nodeIndex;
                    freqToList[frequency] = list;
                }
            }

            /// <summary>
            /// 获取频率链表的头节点
            /// </summary>
            private int GetFrequencyListHead(int frequency)
            {
                if (freqToList.TryGetValue(frequency, out var list))
                {
                    return list.headIndex;
                }
                return -1;
            }

            /// <summary>
            /// 淘汰最小频率的节点（淘汰该频率链表的头节点）
            /// </summary>
            private void EvictMinFrequencyNode()
            {
                // 找到有节点的最小频率
                while (minFrequency <= capacity && GetFrequencyListHead(minFrequency) == -1)
                {
                    minFrequency++;
                }

                if (minFrequency > capacity) return;

                int headIndex = GetFrequencyListHead(minFrequency);
                if (headIndex == -1) return;

                RemoveNode(headIndex);
            }

            /// <summary>
            /// 移除完整节点（从所有数据结构中删除）
            /// </summary>
            private void RemoveNode(int nodeIndex)
            {
                Node node = nodes[nodeIndex];

                // 从频率链表移除
                RemoveFromFrequencyList(nodeIndex, node.frequency);

                // 从主HashMap移除
                keyToNodeIndex.Remove(node.key);

                // 回收节点
                FreeNode(nodeIndex);
                count--;
            }

            #endregion

            // 属性访问器
            public readonly int Count => count;
            public readonly int Capacity => capacity;
            public readonly bool IsCreated => keyToNodeIndex.IsCreated;
        }

        public struct NativeARCCache<TKey, TValue> : IDisposable
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            private const int LIST_T1 = 0, LIST_T2 = 1, LIST_B1 = 2, LIST_B2 = 3;

            private struct Node
            {
                public TKey key;
                public TValue value;
                public byte listType;
                public int prevIndex;
                public int nextIndex;
            }

            private NativeHashMap<TKey, int> keyToNode;
            private NativeList<Node> nodes;
            private NativeList<int> freeList;

            // 四个链表头尾指针
            private int t1Head, t1Tail, t2Head, t2Tail, b1Head, b1Tail, b2Head, b2Tail;

            private int capacity, t1Count, t2Count, b1Count, b2Count, p;

            public NativeARCCache(int capacity, Allocator allocator)
            {
                this.capacity = capacity;
                this.t1Count = this.t2Count = this.b1Count = this.b2Count = 0;
                this.p = 0;

                t1Head = t1Tail = t2Head = t2Tail = -1;
                b1Head = b1Tail = b2Head = b2Tail = -1;

                int totalNodes = capacity * 2;
                keyToNode = new NativeHashMap<TKey, int>(totalNodes, allocator);
                nodes = new NativeList<Node>(totalNodes, allocator);
                freeList = new NativeList<int>(totalNodes, allocator);

                nodes.Length = totalNodes;
                for (int i = totalNodes - 1; i >= 0; i--)
                {
                    freeList.Add(i);
                }
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                if (keyToNode.TryGetValue(key, out int nodeIndex))
                {
                    Node node = nodes[nodeIndex]; // 先读取

                    if (node.listType == LIST_T1 || node.listType == LIST_T2)
                    {
                        MoveToMRU(nodeIndex, LIST_T2);
                        value = node.value;
                        return true;
                    }

                    if (node.listType == LIST_B1)
                    {
                        node = ComputeNewPAndReplace(node, b2Count, b1Count, true);
                        b1Count--; t2Count++;
                        value = node.value;
                        return true;
                    }

                    if (node.listType == LIST_B2)
                    {
                        node = ComputeNewPAndReplace(node, b1Count, b2Count, false);
                        b2Count--; t2Count++;
                        value = node.value;
                        return true;
                    }
                }

                value = default;
                return false;
            }

            public void Put(TKey key, TValue value)
            {
                if (keyToNode.TryGetValue(key, out int nodeIndex))
                {
                    Node node = nodes[nodeIndex];
                    node.value = value;

                    if (node.listType == LIST_T1 || node.listType == LIST_T2)
                    {
                        MoveToMRU(nodeIndex, LIST_T2);
                    }
                    else if (node.listType == LIST_B1)
                    {
                        node = ComputeNewPAndReplace(node, b2Count, b1Count, true);
                        b1Count--; t2Count++;
                    }
                    else if (node.listType == LIST_B2)
                    {
                        node = ComputeNewPAndReplace(node, b1Count, b2Count, false);
                        b2Count--; t2Count++;
                    }

                    nodes[nodeIndex] = node; // 写回
                    return;
                }

                // 容量满，执行Replace
                int totalSize = t1Count + t2Count;
                if (totalSize >= capacity)
                {
                    Replace();
                }

                // 创建新节点到T1
                nodeIndex = AllocateNode();
                nodes[nodeIndex] = new Node
                {
                    key = key,
                    value = value,
                    listType = LIST_T1,
                    prevIndex = -1,
                    nextIndex = -1
                };

                keyToNode.Add(key, nodeIndex);
                AddToHead(nodeIndex, LIST_T1);
                t1Count++;
            }

            private Node ComputeNewPAndReplace(Node node, int oppositeCount, int sameCount, bool isB1)
            {
                int delta = (oppositeCount >= sameCount) ? 1 : oppositeCount / sameCount + 1;

                // 先修改p值
                if (isB1) p = Math.Min(p + delta, capacity);
                else p = Math.Max(p - delta, 0);

                // 执行替换
                Replace();

                // 移动节点
                MoveBetweenLists(node.prevIndex, node.listType, LIST_T2);
                node.listType = LIST_T2;

                return node;
            }

            private void Replace()
            {
                if (t1Count > 0 && (t1Count > p || (b2Count > 0 && t1Count == p)))
                {
                    MoveBetweenLists(t1Head, LIST_T1, LIST_B1);
                    t1Count--; b1Count++;

                    if (b1Count > capacity)
                    {
                        int old = b1Head; Node n = nodes[old];
                        RemoveFromList(old);
                        keyToNode.Remove(n.key);
                        FreeNode(old);
                        b1Count--;
                    }
                }
                else
                {
                    MoveBetweenLists(t2Head, LIST_T2, LIST_B2);
                    t2Count--; b2Count++;

                    if (b2Count > capacity)
                    {
                        int old = b2Head; Node n = nodes[old];
                        RemoveFromList(old);
                        keyToNode.Remove(n.key);
                        FreeNode(old);
                        b2Count--;
                    }
                }
            }

            #region 链表操作

            private void MoveToMRU(int nodeIndex, byte targetList)
            {
                RemoveFromList(nodeIndex);
                AddToHead(nodeIndex, targetList);
            }

            private void MoveBetweenLists(int nodeIndex, byte fromList, byte toList)
            {
                RemoveFromList(nodeIndex);
                AddToHead(nodeIndex, toList);
            }

            private void AddToHead(int nodeIndex, byte listType)
            {
                Node node = nodes[nodeIndex]; // 读取
                node.listType = listType;
                node.prevIndex = -1;

                switch (listType)
                {
                    case LIST_T1:
                        node.nextIndex = t1Head;
                        nodes[nodeIndex] = node; // 写回
                        if (t1Head != -1)
                        {
                            Node headNode = nodes[t1Head];
                            headNode.prevIndex = nodeIndex;
                            nodes[t1Head] = headNode; // 写回
                        }
                        t1Head = nodeIndex;
                        if (t1Tail == -1) t1Tail = nodeIndex;
                        break;

                    case LIST_T2:
                        node.nextIndex = t2Head;
                        nodes[nodeIndex] = node; // 写回
                        if (t2Head != -1)
                        {
                            Node headNode = nodes[t2Head];
                            headNode.prevIndex = nodeIndex;
                            nodes[t2Head] = headNode; // 写回
                        }
                        t2Head = nodeIndex;
                        if (t2Tail == -1) t2Tail = nodeIndex;
                        break;

                    case LIST_B1:
                        node.nextIndex = b1Head;
                        nodes[nodeIndex] = node; // 写回
                        if (b1Head != -1)
                        {
                            Node headNode = nodes[b1Head];
                            headNode.prevIndex = nodeIndex;
                            nodes[b1Head] = headNode; // 写回
                        }
                        b1Head = nodeIndex;
                        if (b1Tail == -1) b1Tail = nodeIndex;
                        break;

                    case LIST_B2:
                        node.nextIndex = b2Head;
                        nodes[nodeIndex] = node; // 写回
                        if (b2Head != -1)
                        {
                            Node headNode = nodes[b2Head];
                            headNode.prevIndex = nodeIndex;
                            nodes[b2Head] = headNode; // 写回
                        }
                        b2Head = nodeIndex;
                        if (b2Tail == -1) b2Tail = nodeIndex;
                        break;
                }
            }

            private void RemoveFromList(int nodeIndex)
            {
                Node node = nodes[nodeIndex]; // 读取
                int prev = node.prevIndex;
                int next = node.nextIndex;

                // 根据类型更新头尾指针
                switch (node.listType)
                {
                    case LIST_T1:
                        if (nodeIndex == t1Head) t1Head = next;
                        if (nodeIndex == t1Tail) t1Tail = prev;
                        break;
                    case LIST_T2:
                        if (nodeIndex == t2Head) t2Head = next;
                        if (nodeIndex == t2Tail) t2Tail = prev;
                        break;
                    case LIST_B1:
                        if (nodeIndex == b1Head) b1Head = next;
                        if (nodeIndex == b1Tail) b1Tail = prev;
                        break;
                    case LIST_B2:
                        if (nodeIndex == b2Head) b2Head = next;
                        if (nodeIndex == b2Tail) b2Tail = prev;
                        break;
                }

                // 更新邻居
                if (prev != -1)
                {
                    Node prevNode = nodes[prev];
                    prevNode.nextIndex = next;
                    nodes[prev] = prevNode; // 写回
                }
                if (next != -1)
                {
                    Node nextNode = nodes[next];
                    nextNode.prevIndex = prev;
                    nodes[next] = nextNode; // 写回
                }

                node.prevIndex = node.nextIndex = -1;
                nodes[nodeIndex] = node; // 写回
            }

            #endregion

            private int AllocateNode()
            {
                int index = freeList[^1];
                freeList.RemoveAtSwapBack(freeList.Length - 1);
                return index;
            }

            private void FreeNode(int index)
            {
                freeList.Add(index);
            }

            public bool Remove(TKey key)
            {
                if (!keyToNode.TryGetValue(key, out int nodeIndex))
                    return false;

                Node node = nodes[nodeIndex]; // 读取

                // 更新计数
                if (node.listType == LIST_T1) t1Count--;
                else if (node.listType == LIST_T2) t2Count--;
                else if (node.listType == LIST_B1) b1Count--;
                else if (node.listType == LIST_B2) b2Count--;

                RemoveFromList(nodeIndex);
                keyToNode.Remove(key);
                FreeNode(nodeIndex);

                return true;
            }

            public void Clear()
            {
                keyToNode.Clear();

                freeList.Clear();
                for (int i = 0; i < nodes.Length; i++)
                {
                    freeList.Add(i);
                }

                t1Head = t1Tail = t2Head = t2Tail = -1;
                b1Head = b1Tail = b2Head = b2Tail = -1;

                t1Count = t2Count = b1Count = b2Count = 0;
                p = 0;
            }

            public void Dispose()
            {
                if (keyToNode.IsCreated) keyToNode.Dispose();
                if (nodes.IsCreated) nodes.Dispose();
                if (freeList.IsCreated) freeList.Dispose();

                t1Head = t1Tail = t2Head = t2Tail = -1;
                b1Head = b1Tail = b2Head = b2Tail = -1;

                t1Count = t2Count = b1Count = b2Count = 0;
                p = 0;
            }

            public int Count => t1Count + t2Count;
            public int Capacity => capacity;
            public bool IsCreated => keyToNode.IsCreated;
            public int T1Count => t1Count;
            public int T2Count => t2Count;
            public int PValue => p;
        }
    }
#endif
}

namespace Convention
{
    public static class EasyManifest
    {
        public enum Format
        {
            Default
        }

        public static void MakeMarkdownManifest(object manifestData, StreamWriter writer, int initDepth, int maxDepth = 10)
        {
            var fields = manifestData.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            var contentField = from field in fields where field.HasAttribute<ContentAttribute>() select field;
            var resourcesField = from field in fields where field.HasAttribute<ResourcesAttribute>() select field;
            var settingField = from field in fields where field.HasAttribute<SettingAttribute>() select field;
            fields.RemoveAll(x => x.HasAttribute<ProjectContextLabelAttribute>());

            void DrawItem([Opt] string name, int depth, object data)
            {
                for (int i = depth; i-- > 0;)
                    writer.Write("  ");
                writer.Write("- ");
                if (name != null)
                {
                    writer.Write($"[{name}]");
                }
                if (data == null)
                {
                    writer.WriteLine($"null");
                }
                else if(data.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    writer.WriteLine($"{data}(UObject)");
                }
                else if (data.GetType() == typeof(string))
                {
                    writer.WriteLine($"\"{data}\"");
                }
                else if (data.GetType().IsValueType == false && depth < maxDepth)
                {
                    writer.Write("\n");
                    if (data is IEnumerable enumer)
                    {
                        foreach (var iter in enumer)
                        {
                            DrawItem(null, depth + 1, iter);
                        }
                    }
                    else
                    {
                        foreach (var iter in data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            DrawItem(iter.Name, depth + 1, iter.GetValue(data));
                        }
                    }
                }
                else
                {
                    writer.WriteLine($"{data}");
                }
            }

            void Draw(object item, IEnumerable<FieldInfo> fields)
            {
                foreach (FieldInfo field in fields)
                {
                    DrawItem(field.Name, initDepth, field.GetValue(item));
                }
            }

            var ctype = manifestData.GetType();
            if (ctype == typeof(string))
            {
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("  ");
                writer.WriteLine(manifestData.ToString());
                return;
            }
            else if (ctype.IsValueType)
            {
                DrawItem(null, initDepth, manifestData);
                return;
            }

            if (contentField.Count() > 0)
            {
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("  ");
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("#");
                writer.WriteLine("# Content");
                Draw(manifestData, contentField);
            }
            if (resourcesField.Count() > 0)
            {
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("  ");
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("#");
                writer.WriteLine("# Resources");
                Draw(manifestData, resourcesField);
            }
            if (settingField.Count() > 0)
            {
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("  ");
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("#");
                writer.WriteLine("# Setting");
                Draw(manifestData, settingField);
            }
            if (fields.Count() > 0)
            {
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("  ");
                for (int depth = initDepth; depth-- > 0;)
                    writer.Write("#");
                if (contentField.Count() + resourcesField.Count() + settingField.Count() > 0)
                {
                    writer.WriteLine("# Others");
                }
                else
                {
                    writer.WriteLine("# Items");
                }
                Draw(manifestData, fields);
            }
        }

        public static void MakeManifest(object data, StreamWriter writer, int depth, int maxDepth = 10, Format format = Format.Default)
        {
            switch (format)
            {
                case Format.Default:
                    MakeMarkdownManifest(data, writer, depth, maxDepth);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}