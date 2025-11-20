using Convention.Collections;
using Convention.Collections.Generic;
using Convention.ReferenceManagement;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Convention.TaskManagement
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class TaskBase: IReference
    {
        /// <summary>
        /// 任务默认优先级。
        /// </summary>
        public const int DefaultPriority = 0;

        private int m_SerialId;
        private string m_Tag;
        private int m_Priority;
        private object m_UserData;

        private bool m_Done;

        /// <summary>
        /// 初始化任务基类的新实例
        /// </summary>
        public TaskBase()
        {
            m_SerialId = 0;
            m_Tag = null;
            m_Priority = DefaultPriority;
            m_Done = false;
            m_UserData = null;
        }

        /// <summary>
        /// 获取任务的序列编号
        /// </summary>
        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
        }

        /// <summary>
        /// 获取任务的标签
        /// </summary>
        public string Tag
        {
            get
            {
                return m_Tag;
            }
        }

        /// <summary>
        /// 获取任务的优先级
        /// </summary>
        public int Priority
        {
            get
            {
                return m_Priority;
            }
        }

        /// <summary>
        /// 获取任务的用户自定义数据
        /// </summary>
        public object UserData
        {
            get
            {
                return m_UserData;
            }
        }

        /// <summary>
        /// 获取或设置任务是否完成
        /// </summary>
        public bool Done
        {
            get
            {
                return m_Done;
            }
            set
            {
                m_Done = value;
            }
        }

        /// <summary>
        /// 获取任务描述
        /// </summary>
        public virtual string Description
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 初始化任务基类
        /// </summary>
        /// <param name="serialId">任务的序列编号</param>
        /// <param name="tag">任务的标签</param>
        /// <param name="priority">任务的优先级</param>
        /// <param name="userData">任务的用户自定义数据</param>
        internal void Initialize(int serialId, string tag, int priority, object userData)
        {
            m_SerialId = serialId;
            m_Tag = tag;
            m_Priority = priority;
            m_UserData = userData;
            m_Done = false;
        }

        /// <summary>
        /// 清理任务基类
        /// </summary>
        public virtual void Clear()
        {
            m_SerialId = 0;
            m_Tag = null;
            m_Priority = DefaultPriority;
            m_UserData = null;
            m_Done = false;
        }
    }

    /// <summary>
    /// 开始处理任务的状态
    /// </summary>
    public enum StartTaskStatus : byte
    {
        /// <summary>
        /// 可以立刻处理完成此任务
        /// </summary>
        Done = 0,

        /// <summary>
        /// 可以继续处理此任务
        /// </summary>
        CanResume,

        /// <summary>
        /// 不能继续处理此任务，需等待其它任务执行完成
        /// </summary>
        HasToWait,

        /// <summary>
        /// 不能继续处理此任务，出现未知错误
        /// </summary>
        UnknownError
    }

    /// <summary>
    /// 任务代理接口
    /// </summary>
    /// <typeparam name="T">任务类型</typeparam>
    public interface ITaskAgent<T> where T : TaskBase
    {
        /// <summary>
        /// 获取任务
        /// </summary>
        T Task
        {
            get;
        }

        /// <summary>
        /// 初始化任务代理
        /// </summary>
        void Initialize();

        /// <summary>
        /// 任务代理轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理任务代理
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 开始处理任务
        /// </summary>
        /// <param name="task">要处理的任务</param>
        /// <returns>开始处理任务的状态</returns>
        StartTaskStatus Start(T task);

        /// <summary>
        /// 停止正在处理的任务并重置任务代理
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskStatus : byte
    {
        /// <summary>
        /// 未开始
        /// </summary>
        Todo = 0,

        /// <summary>
        /// 执行中
        /// </summary>
        Doing,

        /// <summary>
        /// 完成
        /// </summary>
        Done
    }

    /// <summary>
    /// 任务信息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TaskInfo
    {
        private readonly bool m_IsValid;
        private readonly int m_SerialId;
        private readonly string m_Tag;
        private readonly int m_Priority;
        private readonly object m_UserData;
        private readonly TaskStatus m_Status;
        private readonly string m_Description;

        /// <summary>
        /// 初始化任务信息的新实例。
        /// </summary>
        /// <param name="serialId">任务的序列编号</param>
        /// <param name="tag">任务的标签</param>
        /// <param name="priority">任务的优先级</param>
        /// <param name="userData">任务的用户自定义数据</param>
        /// <param name="status">任务状态</param>
        /// <param name="description">任务描述</param>
        public TaskInfo(int serialId, string tag, int priority, object userData, TaskStatus status, string description)
        {
            m_IsValid = true;
            m_SerialId = serialId;
            m_Tag = tag;
            m_Priority = priority;
            m_UserData = userData;
            m_Status = status;
            m_Description = description;
        }

        /// <summary>
        /// 获取任务信息是否有效
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_IsValid;
            }
        }

        /// <summary>
        /// 获取任务的序列编号
        /// </summary>
        public int SerialId
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_SerialId;
            }
        }

        /// <summary>
        /// 获取任务的标签
        /// </summary>
        public string Tag
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_Tag;
            }
        }

        /// <summary>
        /// 获取任务的优先级
        /// </summary>
        public int Priority
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_Priority;
            }
        }

        /// <summary>
        /// 获取任务的用户自定义数据
        /// </summary>
        public object UserData
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_UserData;
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        public TaskStatus Status
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_Status;
            }
        }

        /// <summary>
        /// 获取任务描述
        /// </summary>
        public string Description
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new InvalidOperationException("Data is invalid.");
                }

                return m_Description;
            }
        }
    }

    /// <summary>
    /// 任务池
    /// </summary>
    /// <typeparam name="T">任务类型</typeparam>
    public class TaskPool<T> where T : TaskBase
    {
        private readonly Stack<ITaskAgent<T>> m_FreeAgents;
        private readonly LinkedCacheList<ITaskAgent<T>> m_WorkingAgents;
        private readonly LinkedCacheList<T> m_WaitingTasks;
        private bool m_Paused;

        /// <summary>
        /// 初始化任务池的新实例。
        /// </summary>
        public TaskPool()
        {
            m_FreeAgents = new Stack<ITaskAgent<T>>();
            m_WorkingAgents = new LinkedCacheList<ITaskAgent<T>>();
            m_WaitingTasks = new LinkedCacheList<T>();
            m_Paused = false;
        }

        /// <summary>
        /// 获取或设置任务池是否被暂停。
        /// </summary>
        public bool Paused
        {
            get
            {
                return m_Paused;
            }
            set
            {
                m_Paused = value;
            }
        }

        /// <summary>
        /// 获取任务代理总数量。
        /// </summary>
        public int TotalAgentCount
        {
            get
            {
                return FreeAgentCount + WorkingAgentCount;
            }
        }

        /// <summary>
        /// 获取可用任务代理数量。
        /// </summary>
        public int FreeAgentCount
        {
            get
            {
                return m_FreeAgents.Count;
            }
        }

        /// <summary>
        /// 获取工作中任务代理数量。
        /// </summary>
        public int WorkingAgentCount
        {
            get
            {
                return m_WorkingAgents.Count;
            }
        }

        /// <summary>
        /// 获取等待任务数量。
        /// </summary>
        public int WaitingTaskCount
        {
            get
            {
                return m_WaitingTasks.Count;
            }
        }

        /// <summary>
        /// 任务池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_Paused)
            {
                return;
            }

            ProcessRunningTasks(elapseSeconds, realElapseSeconds);
            ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理任务池。
        /// </summary>
        public void Shutdown()
        {
            RemoveAllTasks();

            while (FreeAgentCount > 0)
            {
                m_FreeAgents.Pop().Shutdown();
            }
        }

        /// <summary>
        /// 增加任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddAgent(ITaskAgent<T> agent)
        {
            if (agent == null)
            {
                throw new InvalidOperationException("Task agent is invalid.");
            }

            agent.Initialize();
            m_FreeAgents.Push(agent);
        }

        /// <summary>
        /// 根据任务的序列编号获取任务的信息。
        /// </summary>
        /// <param name="serialId">要获取信息的任务的序列编号。</param>
        /// <returns>任务的信息。</returns>
        public TaskInfo GetTaskInfo(int serialId)
        {
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.SerialId == serialId)
                {
                    return new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
                }
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                if (waitingTask.SerialId == serialId)
                {
                    return new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
                }
            }

            return default(TaskInfo);
        }

        /// <summary>
        /// 根据任务的标签获取任务的信息。
        /// </summary>
        /// <param name="tag">要获取信息的任务的标签。</param>
        /// <returns>任务的信息。</returns>
        public TaskInfo[] GetTaskInfos(string tag)
        {
            List<TaskInfo> results = new List<TaskInfo>();
            GetTaskInfos(tag, results);
            return results.ToArray();
        }

        /// <summary>
        /// 根据任务的标签获取任务的信息。
        /// </summary>
        /// <param name="tag">要获取信息的任务的标签。</param>
        /// <param name="results">任务的信息。</param>
        public void GetTaskInfos(string tag, List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new InvalidOperationException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
                }
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                if (waitingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
                }
            }
        }

        /// <summary>
        /// 获取所有任务的信息。
        /// </summary>
        /// <returns>所有任务的信息。</returns>
        public TaskInfo[] GetAllTaskInfos()
        {
            int index = 0;
            TaskInfo[] results = new TaskInfo[m_WorkingAgents.Count + m_WaitingTasks.Count];
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results[index++] = new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                results[index++] = new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
            }

            return results;
        }

        /// <summary>
        /// 获取所有任务的信息。
        /// </summary>
        /// <param name="results">所有任务的信息。</param>
        public void GetAllTaskInfos(List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new InvalidOperationException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
            }
        }

        /// <summary>
        /// 增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask(T task)
        {
            LinkedListNode<T> current = m_WaitingTasks.Last;
            while (current != null)
            {
                if (task.Priority <= current.Value.Priority)
                {
                    break;
                }

                current = current.Previous;
            }

            if (current != null)
            {
                m_WaitingTasks.AddAfter(current, task);
            }
            else
            {
                m_WaitingTasks.AddFirst(task);
            }
        }

        /// <summary>
        /// 根据任务的序列编号移除任务。
        /// </summary>
        /// <param name="serialId">要移除任务的序列编号。</param>
        /// <returns>是否移除任务成功。</returns>
        public bool RemoveTask(int serialId)
        {
            foreach (T task in m_WaitingTasks)
            {
                if (task.SerialId == serialId)
                {
                    m_WaitingTasks.Remove(task);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            LinkedListNode<ITaskAgent<T>> currentWorkingAgent = m_WorkingAgents.First;
            while (currentWorkingAgent != null)
            {
                LinkedListNode<ITaskAgent<T>> next = currentWorkingAgent.Next;
                ITaskAgent<T> workingAgent = currentWorkingAgent.Value;
                T task = workingAgent.Task;
                if (task.SerialId == serialId)
                {
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    m_WorkingAgents.Remove(currentWorkingAgent);
                    ReferencePool.Release(task);
                    return true;
                }

                currentWorkingAgent = next;
            }

            return false;
        }

        /// <summary>
        /// 根据任务的标签移除任务。
        /// </summary>
        /// <param name="tag">要移除任务的标签。</param>
        /// <returns>移除任务的数量。</returns>
        public int RemoveTasks(string tag)
        {
            int count = 0;

            LinkedListNode<T> currentWaitingTask = m_WaitingTasks.First;
            while (currentWaitingTask != null)
            {
                LinkedListNode<T> next = currentWaitingTask.Next;
                T task = currentWaitingTask.Value;
                if (task.Tag == tag)
                {
                    m_WaitingTasks.Remove(currentWaitingTask);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWaitingTask = next;
            }

            LinkedListNode<ITaskAgent<T>> currentWorkingAgent = m_WorkingAgents.First;
            while (currentWorkingAgent != null)
            {
                LinkedListNode<ITaskAgent<T>> next = currentWorkingAgent.Next;
                ITaskAgent<T> workingAgent = currentWorkingAgent.Value;
                T task = workingAgent.Task;
                if (task.Tag == tag)
                {
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    m_WorkingAgents.Remove(currentWorkingAgent);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWorkingAgent = next;
            }

            return count;
        }

        /// <summary>
        /// 移除所有任务。
        /// </summary>
        /// <returns>移除任务的数量。</returns>
        public int RemoveAllTasks()
        {
            int count = m_WaitingTasks.Count + m_WorkingAgents.Count;

            foreach (T task in m_WaitingTasks)
            {
                ReferencePool.Release(task);
            }

            m_WaitingTasks.Clear();

            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T task = workingAgent.Task;
                workingAgent.Reset();
                m_FreeAgents.Push(workingAgent);
                ReferencePool.Release(task);
            }

            m_WorkingAgents.Clear();

            return count;
        }

        private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<ITaskAgent<T>> current = m_WorkingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done)
                {
                    current.Value.Update(elapseSeconds, realElapseSeconds);
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();
                m_FreeAgents.Push(current.Value);
                m_WorkingAgents.Remove(current);
                ReferencePool.Release(task);
                current = next;
            }
        }

        private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<T> current = m_WaitingTasks.First;
            while (current != null && FreeAgentCount > 0)
            {
                ITaskAgent<T> agent = m_FreeAgents.Pop();
                LinkedListNode<ITaskAgent<T>> agentNode = m_WorkingAgents.AddLast(agent);
                T task = current.Value;
                LinkedListNode<T> next = current.Next;
                StartTaskStatus status = agent.Start(task);
                if (status == StartTaskStatus.Done || status == StartTaskStatus.HasToWait || status == StartTaskStatus.UnknownError)
                {
                    agent.Reset();
                    m_FreeAgents.Push(agent);
                    m_WorkingAgents.Remove(agentNode);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.CanResume || status == StartTaskStatus.UnknownError)
                {
                    m_WaitingTasks.Remove(current);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.UnknownError)
                {
                    ReferencePool.Release(task);
                }

                current = next;
            }
        }
    }
}
