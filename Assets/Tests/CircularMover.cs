using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMover : MonoBehaviour {
    public float radius;
    public float degreesPerSecond;
    public Vector3 axis;
    // Use this for initialization
    float mCurrentAngle = 0f;
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        mCurrentAngle += Time.deltaTime * degreesPerSecond;
        transform.position = Vector3.zero + Quaternion.AngleAxis(mCurrentAngle, axis) * new Vector3(0f, 0f, 1f) * radius;
    }
}
