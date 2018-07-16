using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAndDespawn : MonoBehaviour {

    public Transform prefab;
    public int maxSpawn = 10;

    List<Transform> mSpawnedPrefabs;


	// Use this for initialization
	void Start () {
        mSpawnedPrefabs = new List<Transform>();
        StartCoroutine(SpawnRoutine());
	}
	
	IEnumerator SpawnRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(.5f, 1.5f));
            if(mSpawnedPrefabs.Count < maxSpawn)
            {
                mSpawnedPrefabs.Add(Hydra.PoolManager.Spawn(prefab, transform));
            }
            else
            {
                int idx = (int)Random.Range(0, mSpawnedPrefabs.Count);
                Transform t = mSpawnedPrefabs[idx];
                mSpawnedPrefabs.RemoveAt(idx);
                Hydra.PoolManager.Despawn(t);
            }
        }
    }
}
