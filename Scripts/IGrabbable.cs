using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
	// returns true if the player can grab other things while this thing is grabbed
	// returns false if grabbing this prevents the player from grabbing anything else
    bool OnPlayerGrab();
    void OnPlayerRelease();
}
