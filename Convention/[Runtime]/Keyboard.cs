using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Convention
{
    public static class KeyboardUtility
    {
        internal class KeyboardMono:MonoBehaviour
        {
            public Dictionary<Key, HashSet<Action>> actionDict = new();

            private void Update()
            {
                foreach (var (key,actions) in actionDict)
                {
                    if (Keyboard.current[key].wasPressedThisFrame)
                    {
                        foreach (var action in actions)
                        {
                            try
                            {
                                action.Invoke();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }
                }
            }
        }

        private static KeyboardMono mono;
        internal static KeyboardMono instance
        {
            get
            {
                if (mono == null)
                    mono = ConventionUtility.Singleton.AddComponent<KeyboardMono>();
                return mono;
            }
        }

        public static void AddListener(this Key key, Action action)
        {
            if (instance.actionDict.ContainsKey(key) == false)
                instance.actionDict.Add(key, new());
            instance.actionDict[key].Add(action);
        }
    }
}
