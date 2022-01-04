using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneCollision : MonoBehaviour
{
    void Start()
    {
        Physics.IgnoreLayerCollision(11, 8, true);
    }

    void OnCollisionEnter2D(Collision2D other) {
    	EnemyController EC = other.gameObject.GetComponent<EnemyController>();
    	if (EC != null) {
    		EC.disorient();
    	}
    }
}
