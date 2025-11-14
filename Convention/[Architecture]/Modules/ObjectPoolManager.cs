using System.Collections.Generic;
using UnityEngine;

namespace Convention.Experimental.Modules
{
    public class ObjectPoolManager : PublicType.GameModule
    {
        public interface IPawn
        {
            void Release();
        }

        private readonly Dictionary<GameObject, Stack<GameObject>> ObjectPools = new();
        private readonly Dictionary<GameObject, GameObject> LeavePoolObjects = new();

        public GameObject Spawn(GameObject prefab)
        {
            if (!ObjectPools.ContainsKey(prefab))
            {
                ObjectPools[prefab] = new();
            }
            var pool = ObjectPools[prefab];
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
            LeavePoolObjects[instance] = prefab;
            return instance;
        }

        public void BackPool(GameObject instance)
        {
            if(LeavePoolObjects.TryGetValue(instance,out var prefab))
            {
                var releaser = instance.GetComponents<IPawn>();
                foreach (var r in releaser)
                {
                    r.Release();
                }
                instance.SetActive(false);
                instance.transform.SetParent(ConventionUtility.Singleton.transform);
                ObjectPools[prefab].Push(instance);
                LeavePoolObjects.Remove(instance);
            }
            else
            {
                throw new PublicType.GameException("This object does not belong to any pool.");
            }
        }

        public void ClearPool(GameObject prefab)
        {
            if (ObjectPools.TryGetValue(prefab, out var pool))
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
        }
    }
}
