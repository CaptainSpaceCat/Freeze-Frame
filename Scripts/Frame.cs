using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame
{
	public static Frame destroyed = new Frame();
	public bool notable = false;
	//TODO
	/* implement notability optimization
	 * basically, each frame can decide whether or not it is "notable"
	 * if it is, it gets saved to the timeline
	 * if not, a pointer to the previous frame will get stored instead
	 * this allows for using less memory to store the whole timeline
	 * if an object has not moved or changed state in any way between frames, no need to make a new one
	 * */

	public Frame() {
	}
}
