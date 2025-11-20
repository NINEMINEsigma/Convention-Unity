using System;
using System.Collections.Generic;

namespace Convention.Experimental.Modules
{
    public partial class ActionManager : PublicType.GameModule
    {
        public interface IActor
        {
            void Execute();
            void Undo();
            string Description { get; }
        }

        private readonly object m_lock = new(); 
        private readonly LinkedList<IActor> m_history = new();
        private readonly Stack<IActor> m_redoStack = new();
        private int m_historyLimit = 100;
        public int HistoryLimit
        {
            get => m_historyLimit;
            set
            {
                m_historyLimit = value;
                while (m_history.Count > m_historyLimit)
                {
                    m_history.RemoveFirst();
                }
            }
        }

        public bool CanUndo => m_history.Count > 0;

        public int UndoCount => m_history.Count;

        public List<string> GetHistoryDescription()
        {
            lock (m_lock)
            {
                var descriptions = new List<string>();
                foreach (var actor in m_history)
                {
                    descriptions.Add(actor.Description);
                }
                return descriptions;
            }
        }

        public bool CanRedo => m_redoStack.Count > 0;

        public int RedoCount => m_redoStack.Count;

        public List<string> GetRedoDescription()
        {
            lock (m_lock)
            {
                var descriptions = new List<string>();
                foreach (var actor in m_redoStack)
                {
                    descriptions.Add(actor.Description);
                }
                return descriptions;
            }
        }

        public void Execute(IActor actor)
        {
            lock (m_lock)
            {
                actor.Execute();
                m_history.AddLast(actor);
                m_redoStack.Clear();
                if (m_history.Count > m_historyLimit)
                {
                    m_history.RemoveFirst();
                }
            }
        }

        public bool Undo()
        {
            lock (m_lock)
            {
                if (CanUndo)
                {
                    var actor = m_history.Last.Value;
                    m_history.RemoveLast();
                    actor.Undo();
                    m_redoStack.Push(actor);
                    return true;
                }
                return false;
            }
        }

        public bool Redo()
        {
            lock (m_lock)
            {
                if (CanRedo)
                {
                    var actor = m_redoStack.Pop();
                    actor.Execute();
                    m_history.AddLast(actor);
                    return true;
                }
                return false;
            }
        }
    }
}
