using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline
{
	private Frame buffer;
	private List<Frame> timeline = new List<Frame>();
	private IFreezable boundObject;

	public Timeline(IFreezable script) {
		boundObject = script;
	}

	// takeSnapshot will grab a frame from its associated object
	// returns true if the frame is notable, false otherwise
	public bool takeSnapshot() {
		Frame newFrame;
		if (boundObject == null) {
			//if the object has been destroyed (maybe it exploded) then set future frames to Frame.destroyed
			newFrame = Frame.destroyed;
		} else {
			newFrame = boundObject.getFrame();
		}
		timeline.Add(newFrame);
		return newFrame.notable;
	}

	public void restoreSnapshot(int timeslot) {
		Frame oldFrame = timeline[timeslot];
		
		if (oldFrame == Frame.destroyed) {
			// if the frame we're restoring is Frame.destroyed, we should destroy the existing object if needed
			if (boundObject != null) {
				//GameObject.Destroy(boundObject);
			}
		} else {
			// TODO instantiate boundObject if boundObject == null
			boundObject.setFrame(oldFrame);
		}
	}

	public void clearTimeAfter(int timeslot) {
		if (timeslot < timeline.Count) {
			timeline.RemoveRange(timeslot + 1, timeline.Count - timeslot - 1);
		}
	}

	public void setFrozen(bool state) {
		boundObject.setFrozen(state);
	}

	public void setEnabled(bool state) {
		boundObject.setEnabled(state);
	}

}
