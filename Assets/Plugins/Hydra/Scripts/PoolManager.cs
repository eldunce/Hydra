using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hydra
{
    public class PoolManager : MonoBehaviour {
        public bool editorDynamicResize = true;

        // fill out in inspector with "magic"
        public PoolSet pools;

        static PoolManager sInstance = null;

        class RuntimePool
        {
            public bool resize = true;
            public Pool basis;
            List<Transform> mInactiveObjects;
            List<Transform> mActiveObjects;            

            public Transform ActivateObject(Vector3 pos, Quaternion rot, Vector3 localScale)
            {
                Transform next = null;
                if (mInactiveObjects.Count > 0)
                {
                    next = mInactiveObjects[mInactiveObjects.Count - 1];
                    mActiveObjects.Add(next);
                    mInactiveObjects.RemoveAt(mInactiveObjects.Count - 1);
                    if (basis.onDespawn == Pool.DespawnAction.Deactivate)
                    {
                        next.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if(Application.isEditor && basis.resize)
                    {
                        basis.size++;
                        next = Instantiate(basis.prefab);
                        mActiveObjects.Add(next);
                    }
                    else if(basis.onCapacityReached == Pool.CapacityReachedAction.Fail)
                    {
                        return null;
                    }
                    else
                    {
                        next = mActiveObjects[0];
                        if(basis.onDespawn == Pool.DespawnAction.Deactivate)
                        {
                            next.gameObject.SetActive(false);
                        }
                        mActiveObjects.RemoveAt(0);
                        if(basis.onDespawn == Pool.DespawnAction.Deactivate)
                        {
                            next.gameObject.SetActive(true);
                        }
                        mActiveObjects.Add(next);
                    }
                }

                next.localScale = new Vector3(1f, 1f, 1f);
                next.rotation = Quaternion.identity;
                next.position = Vector3.zero;

                next.position = pos;
                next.rotation = rot;
                next.localScale = localScale;
                return next;
            }

            public void DeactivateObject(Transform t)
            {
                mActiveObjects.Remove(t);
                mInactiveObjects.Add(t);
                if (basis.onDespawn == Pool.DespawnAction.Deactivate)
                {
                    t.gameObject.SetActive(false);
                }
            }

            public void InitializePool(Pool _basis)
            {
                basis = _basis;
                mInactiveObjects = new List<Transform>(basis.size);
                mActiveObjects = new List<Transform>(basis.size);
                for(int i = 0; i < basis.size; i++)
                {
                    Transform t = Instantiate(basis.prefab);
                    mInactiveObjects.Add(t);
                    if(basis.onDespawn == Pool.DespawnAction.Deactivate)
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }
        }

        Dictionary<Transform, RuntimePool> mRuntimePools;
        Dictionary<Transform, RuntimePool> mSpawnedObjects;

        struct SpawnedUpdate
        {
            public Pool.UpdateDelegate del;
            public Transform t;
        }

        List<SpawnedUpdate> mUpdates;

        struct DespawnCheck
        {
            public Pool.DespawnTestDelegate del;
            public Transform t;
        }

        List<DespawnCheck> mDespawnChecks;

        private void Awake()
        {            
            mRuntimePools = new Dictionary<Transform, RuntimePool>();
            mUpdates = new List<SpawnedUpdate>();
            mDespawnChecks = new List<DespawnCheck>();
            mSpawnedObjects = new Dictionary<Transform, RuntimePool>();
            foreach(var pool in pools.pools)
            {
                RuntimePool runtimePool = new RuntimePool();
                runtimePool.InitializePool(pool);
                mRuntimePools[pool.prefab] = runtimePool;
            }

            if(sInstance == null)
            {
                sInstance = this;
            }            
        }

        // Use this for initialization
        void Start() {
        }

        // Update is called once per frame
        void Update() {
            for(int i = 0; i < mUpdates.Count; i++)
            {
                mUpdates[i].del(mUpdates[i].t);
            }

            for(int i = mDespawnChecks.Count-1; i>=0; i--)
            {
                if(mDespawnChecks[i].del(mDespawnChecks[i].t))
                {
                    Debug.Log("Despawn " + mDespawnChecks[i].t.name);
                    Transform t = mDespawnChecks[i].t;
                    int idx = mUpdates.FindIndex((x) => x.t == t);
                    if(idx != -1)
                        mUpdates.RemoveAt(idx);
                    mSpawnedObjects[t].DeactivateObject(t);
                    mSpawnedObjects.Remove(t);
                    mDespawnChecks.RemoveAt(i);
                }
            }
        }

        void DespawnInternal(Transform t)
        {
            mSpawnedObjects[t].DeactivateObject(t);
            mSpawnedObjects.Remove(t);
            int idx = mUpdates.FindIndex((x) => x.t == t);
            if (idx != -1)
                mUpdates.RemoveAt(idx);
            idx = mDespawnChecks.FindIndex((x) => x.t == t);
            if (idx != -1)
                mDespawnChecks.RemoveAt(idx);
        }

        public static Transform Spawn(Transform prefab, Vector3 pos, Quaternion rot, Vector3 localScale, Pool.UpdateDelegate update = null, Pool.DespawnTestDelegate despawn = null)
        {
            if (sInstance == null)
            {
                Debug.LogError("Hydra.PoolManager: No PoolManager instance.  Add a PoolManager object to the scene.");
                return null;
            }
#if UNITY_EDITOR
            if(sInstance.editorDynamicResize && !sInstance.mRuntimePools.ContainsKey(prefab))
            {
                // make a new pool
                Pool newPool = ScriptableObject.CreateInstance<Pool>();
                newPool.name = "Pool_" + prefab.name;
                newPool.prefab = UnityEditor.PrefabUtility.FindPrefabRoot(prefab.gameObject).transform;
                newPool.size = 1;
                newPool.onDespawn = Pool.DespawnAction.Deactivate;
                newPool.onCapacityReached = Pool.CapacityReachedAction.Recycle;
                sInstance.pools.pools.Add(newPool);
                UnityEditor.AssetDatabase.AddObjectToAsset(newPool, sInstance.pools);
                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(sInstance.pools));
                RuntimePool runtimePool = new RuntimePool();
                runtimePool.InitializePool(newPool);
                sInstance.mRuntimePools[newPool.prefab] = runtimePool;
            }
#endif
            Transform t = sInstance.mRuntimePools[prefab].ActivateObject(pos, rot, localScale);            
            if (t != null)
            {
                sInstance.mSpawnedObjects[t] = sInstance.mRuntimePools[prefab];
                if (update != null)
                {
                    SpawnedUpdate spawnedUpdate = new SpawnedUpdate();
                    spawnedUpdate.del = update;
                    spawnedUpdate.t = t;
                    sInstance.mUpdates.Add(spawnedUpdate);
                }
                if (despawn != null)
                {
                    DespawnCheck check = new DespawnCheck();
                    check.del = despawn;
                    check.t = t;
                    sInstance.mDespawnChecks.Add(check);
                }
            }

            return t;
        }

        public static Transform Spawn(Transform prefab, Transform at, Pool.UpdateDelegate update = null, Pool.DespawnTestDelegate despawn = null)
        {
            return Spawn(prefab, at.position, at.rotation, at.localScale, update, despawn);
        }

        public static void Despawn(Transform prefab)
        {
            if (sInstance == null)
            {
                Debug.LogError("Hydra.PoolManager: No PoolManager instance.  Add a PoolManager object to the scene.");
            }
            else
                sInstance.DespawnInternal(prefab);   
        }
    }
}
