using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hydra
{
    /// <summary>
    /// A Pool is a list of prefabs and associated parameters (max spawn count, recycle type, etc)
    /// Any prefab spawned with PoolManager.Spawn() will get added to a default Pool if it's not already there.
    /// </summary>
    public class Pool : ScriptableObject
    {        
        public Transform prefab;
        public int size;
        public bool resize=true;
        public enum DespawnAction
        {
            Deactivate,
            None,
        }
        public DespawnAction onDespawn;

        public enum CapacityReachedAction
        {
            Fail,
            Recycle
        }

        public CapacityReachedAction onCapacityReached;
        

        public delegate void UpdateDelegate(Transform t);

        public delegate bool DespawnTestDelegate(Transform t);
      
    }
}
