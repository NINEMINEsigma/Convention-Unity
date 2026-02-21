using System.Collections.Generic;
using UnityEngine;

namespace Convention
{
    public static class PrefabUtility
    {
        public sealed class PrefabEntry
        {
            private GameObject prefab;
            private Stack<GameObject> pool = new();

            public void Setup(GameObject prefab)
            {
                this.prefab = GameObject.Instantiate(prefab);
                prefab.SetActive(false);
                prefab.transform.SetParent(ConventionUtility.Singleton.transform);
            }

            public GameObject Pop()
            {
                if(pool.TryPop(out var result))
                {
                    result.SetActive(true);
                    return result;
                }
                else
                {
                    result = GameObject.Instantiate(prefab);
                    result.SetActive(true);
                    return result;
                }
            }

            public void Push(GameObject go)
            {
                go.SetActive(false);
                go.transform.SetParent(ConventionUtility.Singleton.transform);
                pool.Push(go);
            }
        }
    }
}
