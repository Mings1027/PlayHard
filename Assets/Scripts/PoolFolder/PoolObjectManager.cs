using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolControl
{
    [Serializable]
    public class Pool
    {
        public PoolObjectKey poolObjectKey;
        public GameObject prefab;
        public byte initSize = 10;
    }

    public class PoolObjectManager : MonoBehaviour
    {
        private static PoolObjectManager _inst;
        private bool _isPlaying;
        private Dictionary<PoolObjectKey, Stack<GameObject>> _poolStackTable;
        private Dictionary<PoolObjectKey, GameObject> _prefabTable;

        [SerializeField] private bool autoPoolCleaner;
        [SerializeField] private float autoCleanerDelay = 3f;
        [SerializeField] private byte poolMaxSize = 30;
        [SerializeField] private Pool[] pools;

        private void OnEnable()
        {
            _isPlaying = true;
        }

        private void Start()
        {
            _inst = this;
            _poolStackTable = new Dictionary<PoolObjectKey, Stack<GameObject>>();
            _prefabTable = new Dictionary<PoolObjectKey, GameObject>();
            PoolInit();

            if (autoPoolCleaner) AutoPoolCleaner().Forget();

#if UNITY_EDITOR
            SortPool();
#endif
        }

        private void OnDisable()
        {
            _isPlaying = false;
        }
#if UNITY_EDITOR
        [ContextMenu("Sort Pool")]
        private void SortPool()
        {
            Array.Sort(pools);
        }

        [ContextMenu("Set Prefab key from Pool")]
        private void MatchPoolKeyToPrefabKey()
        {
            foreach (var t in pools)
            {
                t.prefab.GetComponent<PoolObject>().poolObjKey = t.poolObjectKey;
            }
        }
#endif
        private void PoolInit()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                var t = pools[i];
                _prefabTable.Add(t.poolObjectKey, t.prefab);
                if (t.prefab.TryGetComponent(out PoolObject poolObject))
                {
                    poolObject.poolObjKey = t.poolObjectKey;
                }

                if (t.prefab == null) throw new Exception($"{t.poolObjectKey} doesn't exist");
                _poolStackTable.Add(t.poolObjectKey, new Stack<GameObject>());
                for (var j = 0; j < t.initSize; j++)
                {
                    CreateNewObject(t.poolObjectKey, t.prefab);
                }
            }
        }

        public static void Get(PoolObjectKey poolObjectKey, Transform t) =>
            _inst.Spawn(poolObjectKey, t.position, t.rotation);

        public static void Get(PoolObjectKey poolObjectKey, Vector3 position) =>
            _inst.Spawn(poolObjectKey, position, Quaternion.identity);

        public static void Get(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation) =>
            _inst.Spawn(poolObjectKey, position, rotation);

        public static T Get<T>(PoolObjectKey poolObjectKey, Transform t) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, t.position, t.rotation);
            obj.TryGetComponent(out T component);
            return component;
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Transform t, Vector3 scale) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, t.position, t.rotation, scale);
            obj.TryGetComponent(out T component);
            return component;
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, Quaternion.identity);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation)
            where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, rotation);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static void ReturnToPool(GameObject obj, PoolObjectKey poolObjKey)
        {
            if (!_inst._poolStackTable.TryGetValue(poolObjKey, out var poolStack)) return;
            poolStack.Push(obj);
        }

        private async UniTaskVoid AutoPoolCleaner()
        {
            while (_isPlaying)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(autoCleanerDelay),
                    cancellationToken: _inst.GetCancellationTokenOnDestroy());

                PoolCleaner().Forget();
            }
        }

        private static async UniTaskVoid PoolCleaner()
        {
            foreach (var pool in _inst._poolStackTable)
            {
                while (pool.Value.Count > _inst.poolMaxSize)
                {
                    Destroy(pool.Value.Pop());
                    await UniTask.Delay(1000, cancellationToken: _inst.GetCancellationTokenOnDestroy());
                }
            }
        }

        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation)
        {
            var poolStack = _poolStackTable[poolObjKey];
            if (poolStack.Count <= 0)
            {
                if (_prefabTable.TryGetValue(poolObjKey, out var prefab))
                {
                    CreateNewObject(poolObjKey, prefab);
                }
            }

            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }

        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var poolStack = _poolStackTable[poolObjKey];
            if (poolStack.Count <= 0)
            {
                if (_prefabTable.TryGetValue(poolObjKey, out var prefab))
                {
                    CreateNewObject(poolObjKey, prefab);
                }
            }

            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.transform.localScale = scale;
            poolObj.SetActive(true);
            return poolObj;
        }

        private void CreateNewObject(PoolObjectKey poolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (obj.TryGetComponent(out PoolObject poolObject)) poolObject.poolObjKey = poolObjectKey;
            obj.SetActive(false);
#if UNITY_EDITOR
            if (!obj.TryGetComponent(out PoolObject _))
                throw new Exception($"You have to attach PoolObject.cs in {prefab} prefab");
#endif
        }

#if UNITY_EDITOR
        private void SortObject(GameObject obj)
        {
            var isFind = false;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (i == transform.childCount - 1)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }

                if (transform.GetChild(i).name == obj.name) isFind = true;
                else if (isFind)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }

#endif
    }
}