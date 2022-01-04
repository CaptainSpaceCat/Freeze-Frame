using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSideDetector : MonoBehaviour
{
    public bool entered;

    void OnTriggerEnter2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		entered = true;
    	}
    }

    void OnTriggerExit2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		entered = false;
    	}
    }
}
