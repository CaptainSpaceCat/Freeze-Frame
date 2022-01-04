using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeController : MonoBehaviour
{
	private enum eShaderMode {Frozen, Forward, Reverse, Blocked, Invisible, Replay}

	public bool timeFrozen = false;
	private bool timeBlocked = false;
	private float timeIndex = 0f;
	private float minTime = 0f;
	private float maxTime = 0f;
	private float furthestTime = 1f;
	public float deltaTime;
	private Dictionary<int,Frame> storage;
	private Timeline[] timelines;
	private List<bool> notableFrames = new List<bool>();
	private bool controlsLocked = false;
	public float replayDelayMultiplier;

	//replays are temporarily disabled because Unity WebGL doesn't seem to cooperate with them very well
	private bool skipReplay = true;
	private TemporalBlocker currentBlocker;
	private InputManager inputManager;
	private MusicController musicController;

	public float blueSpeed;
	private float blueMovementTracker = 0f;

	public AudioSource freezeSFX;
	public AudioSource unfreezeSFX;
	public AudioSource forwardSFX;
	public AudioSource reverseSFX;
	public AudioSource frozenSFX;
	public AudioSource blockedSFX;

	public TimeBarShaderController overlay;
	public TimeSpeckleShaderController speckleShader;
	public Kino.AnalogGlitch cameraGlitch;
	public Vector4 neutralGlitch;
	public Vector4 frozenGlitch;
	public Vector4 movingGlitch;

	private int timeDirection = -1000;

	// Start is called before the first frame update
	void Start() {
		inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
		musicController = GameObject.FindWithTag("MusicController").GetComponent<MusicController>();

		storage = new Dictionary<int,Frame>();
		CreateTimelines();
	}

	void Update() {
		overlay.cursorGoal = timeIndex / furthestTime; //Mathf.Clamp(.02f+(timeIndex / furthestTime)*.98f, 0, 1);
		if (timeBlocked) {
			blueMovementTracker += Time.deltaTime * blueSpeed;
	    	overlay.redGoal = timeIndex / furthestTime;
		} else if (timeFrozen) {

		} else {
			overlay.redGoal = minTime / furthestTime;
			overlay.blueGoal = timeIndex / furthestTime;
			blueMovementTracker += Time.deltaTime * blueSpeed;
		}

		overlay.setBlueMovement(blueMovementTracker);

	}

	void FixedUpdate() {
		if (!controlsLocked) {
			if (inputManager.Freeze) {
				if (!timeBlocked) {
					timeFrozen = !timeFrozen;

					if (timeFrozen) {
						// if we just froze time, set the maxTime to the current time
						// and freeze physics and logic calculation for all freezable objects
						setTimeSFX(true, 0);
						maxTime = timeIndex - 1;
						setAllFrozen(true);
						setShaderMode(eShaderMode.Frozen);
					} else {
						// if we just unfroze time, clear all the saved timeslots in the future (referred to as forward history)
						// and unfreeze all freezables
						setTimeSFX(true, 2);
						timeIndex = (float)((int)timeIndex);
						clearAllTimeAfter((int)timeIndex);
						setAllFrozen(false);
						setShaderMode(eShaderMode.Invisible);
					}
				} else {
					//time is blocked, let the blocker know to render the ripple effect thingy
					//also play a sound to tell the player something is blocking them
					currentBlocker.setRippleParams();
					blockedSFX.Play();

				}
			} else if (!timeFrozen) {
				takeAllSnapshots();
				timeIndex += 1f;
				furthestTime = Mathf.Max(furthestTime, timeIndex);
			} else {
				if (inputManager.TimeBack) {
					changeTimeIndex(-deltaTime);
					if (timeIndex == minTime) {
						setTimeSFX(false, 0);
						setShaderMode(eShaderMode.Frozen);
					} else {
						setTimeSFX(false, -1);
						setShaderMode(eShaderMode.Forward);
					}
					restoreAllSnapshots((int)timeIndex);
				} else if (inputManager.TimeForward) {
					changeTimeIndex(deltaTime);
					if (timeIndex == maxTime) {
						setTimeSFX(false, 0);
						setShaderMode(eShaderMode.Frozen);
					} else {
						setTimeSFX(false, 1);
						setShaderMode(eShaderMode.Reverse);
					}
					
					restoreAllSnapshots((int)timeIndex);
				} else {
					//if we aren't moving time, set the shader mode to frozen
					setTimeSFX(false, 0);
					setShaderMode(eShaderMode.Frozen);
				}
			}
		} else {
			//controlsLocked is true
			if (inputManager.Restart) {
				skipReplay = true;
			}
		}
	}

	public void triggerReplay(int sceneIndex) {
		// must clear all time after the current index
		// this is to avoid a bug where the player beats the level while time is frozen with forward history
		// and the replay attempts to play the forward history appended to the rest of the level's history
		clearAllTimeAfter((int)timeIndex);
		// take one more round of snapshots so that the player can see themselves in the goal area during the replay
		// even if time was frozen while they beat the level
		takeAllSnapshots();
		//timeIndex += 1f;

		// handle the replay
		maxTime = timeIndex;
		setShaderMode(eShaderMode.Replay);
		controlsLocked = true;
		setAllEnabled(false);
		GameObject.FindWithTag("Player").GetComponent<CharacterController>().OnReplay();
		StartCoroutine(replayLevel(sceneIndex));
	}

	private IEnumerator replayLevel(int sceneIndex) {
		int index = 0;
		while (index <= maxTime && !skipReplay) {
			restoreAllSnapshots(index);
			index++;
			yield return new WaitForSeconds(Time.deltaTime * replayDelayMultiplier);
		}
		//wait for a bit to show off the goal zone
		if (!skipReplay) {
			yield return new WaitForSeconds(1f);
		}
		//TODO maybe just reference the goal area's scene management controller instead
		//but this is probably fine tbh
		SceneManager.LoadScene(sceneIndex);
	}

	private void changeTimeIndex(float delta) {
		timeIndex += delta;
		timeIndex = Mathf.Clamp(timeIndex, minTime, maxTime);
	}

	private void takeAllSnapshots() {
		bool flag = false;
		foreach (Timeline TL in timelines) {
			bool notable = TL.takeSnapshot();
			flag = flag || notable;
		}
		notableFrames.Add(flag);
	}

	private void restoreAllSnapshots(int timeslot) {
		foreach (Timeline TL in timelines) {
			TL.restoreSnapshot(timeslot);
		}
	}

	private void clearAllTimeAfter(int timeslot) {
		foreach (Timeline TL in timelines) {
			TL.clearTimeAfter(timeslot);
		}
		if (timeslot < notableFrames.Count) {
			notableFrames.RemoveRange(timeslot + 1, notableFrames.Count - timeslot - 1); ;
        }
	}

	private void setAllFrozen(bool state) {
	    //overlay.setBlueMovement(!state);
		foreach (Timeline TL in timelines) {
			TL.setFrozen(state);
		}
	}

	private void setAllEnabled(bool state) {
		foreach (Timeline TL in timelines) {
			TL.setEnabled(state);
		}
	}

    public void setTimeBlocked(bool state, TemporalBlocker blocker) {
    	currentBlocker = blocker;
    	timeBlocked = state;
    	//we should allow the timeBlocked flag to be set if we enter a blocker zone during a cutscene (because i envision that might happen)
    	//but we should't execute any time control functions if the controls are locked
    	if (!controlsLocked) {
	    	if (timeBlocked) {
	    		//if we just blocked time control, we need to unfreeze time (if frozen) and delete the entire timeline up to now
	    		setShaderMode(eShaderMode.Blocked);
	    		//if we just blocked time, we need to set the blue goal to the time index so we show we've lost forward history
	    		overlay.blueGoal = timeIndex / furthestTime;
	    		if (timeFrozen) {
	    			setTimeSFX(true, 2);
	    		}
	    		timeFrozen = false;
	    		setAllFrozen(false);
	    		clearAllTimeAfter((int)timeIndex);
	    	} else {
	    		// if we just unblocked time, this frame is now the earliest we should be allowed to reverse to
	    		minTime = timeIndex;
				setShaderMode(eShaderMode.Invisible);
	    	}
	    }
    }

	// runs on start
	// finds all of the Freezable objects, and creates a Timeline object to watch over each one
	private void CreateTimelines() {
		//TODO: find a way to look up all objects implementing an interface rather than using tags
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Freezable");
		GameObject playerObject = GameObject.FindWithTag("Player");
		timelines = new Timeline[objects.Length + 1];
		int index = 0;
		foreach (GameObject go in objects) {
			timelines[index] = new Timeline(go.GetComponent<IFreezable>());
			index++;
		}
		timelines[index] = new Timeline(playerObject.GetComponent<IFreezable>());
	}


	// ================= MAJOR TODO =================
	// handle destruction of Freezables gracefully
	// if one is instantiated at time 10, destroy it if we go earlier
	// if one is destroyed at time 20, instantiate it if wo go earlier
	// and vice versa
	// this is ONLY relevant for freezable objects that are either instantiated or destroyed after the start of the level
	// no such objects exist in the game currently
	// it may be prudent to simply design the game such that none are ever necessary and this won't be a problem
	// any other objects are already handled perfectly well



	// ===================================== SFX ===================================== //
	//should probably break this out to an SFXController or somn
	private void setTimeSFX(bool transition, int direction) {
		if (timeDirection == direction) {
			return;
		}
		timeDirection = direction;
		/* 0 - frozen
		 * 1 - forward
		 * -1 - backward
		 * 2 - stop
		 */
		//TODO make this an enum -_-

		musicController.setDirection(direction);
		if (direction == 0) {
			if (transition) {
				freezeSFX.Play();
				unfreezeSFX.Stop();
			}
			if (!frozenSFX.isPlaying) {
				frozenSFX.Play();
			}
			reverseSFX.Stop();
			forwardSFX.Stop();
		} else if (direction == 2) {
			if (transition) {
				freezeSFX.Stop();
				unfreezeSFX.Play();
			}
			frozenSFX.Stop();
			reverseSFX.Stop();
			forwardSFX.Stop();
		} else if (direction == -1) {
			frozenSFX.Stop();
			if (!reverseSFX.isPlaying) {
				reverseSFX.Play();
			}
			forwardSFX.Stop();
		} else if (direction == 1) {
			frozenSFX.Stop();
			reverseSFX.Stop();
			if (!forwardSFX.isPlaying) {
				forwardSFX.Play();
			}
		}
	}

	// ===================================== VFX ===================================== //
	// handle changes to the shader that shows up when you freeze time
	private void setShaderMode(eShaderMode mode) {
    	if (mode == eShaderMode.Blocked) {
    		speckleShader.setBlocked();
    	} else if (mode == eShaderMode.Replay){
    		speckleShader.setReplay();
    	}
    	speckleShader.setInvisible(mode == eShaderMode.Invisible);

    	if (mode == eShaderMode.Frozen) {
			speckleShader.setSpeedVal(0f);
    	} else if (mode == eShaderMode.Forward) {
    		speckleShader.setSpeedVal(1f);
    	} else if (mode == eShaderMode.Reverse) {
    		speckleShader.setSpeedVal(-1f);
    	}
    }

    // handle changes to the shader that glitches the camera
    // DEPRECATED
    private void setCameraGlitch(Vector4 parameters) {
    	cameraGlitch.scanLineJitter = parameters.x;
    	cameraGlitch.verticalJump = parameters.y;
    	cameraGlitch.horizontalShake = parameters.z;
    	cameraGlitch.colorDrift = parameters.w;
    }

}
