using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabZone : MonoBehaviour
{
	private InputManager inputManager;
	private bool playerPresent = false;
	private bool isPlayerGrabbing = false;
	public GameObject boundObject;
	private IGrabbableObject boundGrabbable;

    void Start()
    {
        inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
        boundGrabbable = boundObject.GetComponent<IGrabbableObject>();
    }

    void Update() {
    	if (inputManager.Grab) {
    		if (!isPlayerGrabbing && playerPresent) {
	    		isPlayerGrabbing = true;
	    		boundGrabbable.OnGrab();
	    	}
    	} else if (!inputManager.Grab) {
    		if (isPlayerGrabbing) {
	    		isPlayerGrabbing = false;
	    		boundGrabbable.OnRelease();
	    	}
    	}
    }

    public void OnTriggerEnter2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		playerPresent = true;
    	}
    }

    public void OnTriggerExit2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		if (isPlayerGrabbing) {
    			boundGrabbable.OnRelease();
    		}
    		playerPresent = false;
    	}
    }
}
