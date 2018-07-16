using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour {


    public Transform toSpawn;
    public Transform atPos;
    public float minSpawnFreq;
    public float maxSpawnFreq;
    public bool follow;
    public float lifetimeMin = -1f;
    public float lifetimeMax = -1f;
	// Use this for initialization
	void Start () {
        StartCoroutine(Spawn());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnFreq, maxSpawnFreq));
            Hydra.Pool.UpdateDelegate del = null;
            if (follow)
                del = (t) => t.position = atPos.position;
            Hydra.Pool.DespawnTestDelegate des = null;
            if (lifetimeMax > 0f && lifetimeMin > 0f)
            {
                float finalTime = Time.realtimeSinceStartup + Random.Range(lifetimeMin, lifetimeMax);
                des = delegate (Transform t){
                    return Time.realtimeSinceStartup> finalTime;
;                }; 
            }
            Hydra.PoolManager.Spawn(toSpawn, atPos, del, des);
        }
    }
}
