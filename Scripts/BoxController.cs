using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour, IFreezable, IGrabbableBoundObject
{
	private bool isFrozen = false;
    private bool isEnabled = true;
	private bool isGrabbed = false;
	private Vector3 offset;
	private GameObject playerObject;
    public GameObject ghostPrefab;
    private GameObject ghostObject;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        ghostObject = Instantiate(ghostPrefab);
        ghostObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isGrabbed) {
        	transform.position = playerObject.transform.position + offset;
        }
    }

    // called by one of the GrabbableZones associated with this Grabbable object
    public void setGrabbed(bool state, Vector3 playerOffset) {
    	isGrabbed = state;
    	offset = transform.position - playerOffset;
    }

    // ================================== IFREEZABLE IMPLEMENTATION ================================== //
    public class BoxFrame : Frame {
    	public Vector3 position;
    	// whether or not the box is grabbed will always be independent of time being frozen
    	// since the player does the grabbing. thus, we don't save it here

    	public BoxFrame(Vector3 _position) {
    		position = _position;
    	}
    }

    public Frame getFrame() {
    	return new BoxFrame(gameObject.transform.position);
    }

    public void setFrame(Frame f) {
    	BoxFrame frm = (BoxFrame)f;

    	// interestingly enough, the grabbing mechanic overrides the time mechanic,
    	// as the player should be free to move a box around even while reversing time
    	// thus, we don't set the box's position if the player is grabbing it
        ghostObject.transform.position = frm.position;
    	if (!isGrabbed) {
    		gameObject.transform.position = frm.position;
    	}
    }

    public void setFrozen(bool state) {
    	isFrozen = state;

        if (isFrozen) {
            ghostObject.SetActive(true);
            ghostObject.transform.position = transform.position;
        } else {
            ghostObject.SetActive(false);
        }
    }

    public void setEnabled(bool state) {
        isEnabled = state;
    }
}
