using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGrabZone : MonoBehaviour, IGrabbable
{
	public RigidbodyConstraints2D constraint;
	public GameObject playerOffset;
	public GameObject boundObject;
	private IGrabbableBoundObject boundBox;
	private GameObject playerObject;

	// Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        boundBox = boundObject.GetComponent<IGrabbableBoundObject>();
    }

    public bool OnPlayerGrab() {
    	boundBox.setGrabbed(true, playerOffset.transform.position);
    	lockPlayerAxis(constraint);
    	return false;
    }

    public void OnPlayerRelease() {
    	boundBox.setGrabbed(false, Vector3.zero);
    	freePlayerAxis();
    }

    public void lockPlayerAxis(RigidbodyConstraints2D _constraint) {
        RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation | _constraint;
        playerObject.GetComponent<Rigidbody2D>().constraints = constraints;
    }

    public void freePlayerAxis() {
        playerObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void OnTriggerEnter2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		other.gameObject.GetComponent<CharacterController>().offerGrab(this);
    	}
    }

    public void OnTriggerExit2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		other.gameObject.GetComponent<CharacterController>().rescindGrab();
    	}
    }
}
