using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colleded with " + other.name);
    }
}
