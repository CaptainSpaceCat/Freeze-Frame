using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerController : MonoBehaviour, IFreezable
{
	public float speed;
	public Vector3 direction;
	private bool isFrozen = false;

    void OnTriggerStay2D(Collider2D other) {
    	if (!isFrozen) {
    		if (!other.isTrigger) {
    			other.transform.Translate(direction * speed);
    		}
    	}

        //TODO if time is reversing or forwarding apply the movement of the conveyer to objects on it
    }

    // ================================== IFREEZABLE IMPLEMENTATION ================================== //
    public class ConveyerFrame : Frame {
    	//pass for now
    	//TODO: include direction of conveyance

        public ConveyerFrame() {
        }
    }

    public Frame getFrame() {
        return new ConveyerFrame();
    }

    public void setFrame(Frame f) {
    	ConveyerFrame frm = (ConveyerFrame)f;
        //pass for now
    }

    public void setFrozen(bool state) {
        isFrozen = state;
    }

    public void setEnabled(bool state) {
        //pass
    }
}
