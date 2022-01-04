using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour, IFreezable, IDroppable
{
	// State Fields
	public float speed;
	private int direction;

    public IGrabbable grabbableObject;
    public bool grabAvailable = false;
    public bool isGrabbing = false;
    public bool isFloating = false;

    private InputManager inputManager;
    public GameObject ghostPrefab;
    private GameObject ghostObject;
    private bool isFrozen = false;

    private bool controlsLocked = false;
    private bool inReplay = false;
	// inventory

    // Start is called before the first frame update
    void Start()
    {
        //TODO: use a shader for this instead, that would be pretty sick dopleberries
        ghostObject = Instantiate(ghostPrefab);
        ghostObject.SetActive(false);

        inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
    }

    void FixedUpdate()
    {
        if (isFloating && transform.parent == null) { //if transform.parent isn't null, it'll be because we're hanging onto a sigil so we don't fall <- this is jank and needs a refactor
            OnDeath();
        }
        if (!controlsLocked) {
            // Interactions
            if (inputManager.Interact) {
                // must be in Update since only the first frame returns true and we might miss it if we're in FixedUpdate
                if (grabAvailable && !isGrabbing) {
                    OnGrab();
                } else if (isGrabbing) {
                    OnRelease();
                }
            }
            if (inputManager.Restart) {
                OnDeath();
            }

            // Movement
            if (inputManager.Right) {
            	transform.Translate(new Vector3(1,0,0) * speed);
            } else if (inputManager.Left) {
            	transform.Translate(new Vector3(-1,0,0) * speed);
            }

            if (inputManager.Forward) {
            	transform.Translate(new Vector3(0,1,0) * speed);
            } else if (inputManager.Back) {
            	transform.Translate(new Vector3(0,-1,0) * speed);
            }
        }
    }

    public void OnDeath() {
        if (!inReplay) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void OnGrab() {
        isGrabbing = true;
        grabAvailable = grabbableObject.OnPlayerGrab();
    }

    public void OnRelease() {
        isGrabbing = false;
        if (grabbableObject != null) { //TODO handle Destroy() lifecycle better than this :/
            grabbableObject.OnPlayerRelease();
            grabbableObject = null;
        }
    }

    public void OnReplay() {
        inReplay = true;
    }

    // TODO: handle when multiple grabbables overlap, which can occasionally happen!!!

    // called by external Grabbable objects, to offer themselves up to the player when the player is close enough to them
    public void offerGrab(IGrabbable grabbable) {
        //if we're already grabbing something, we ignore other grabbing opportunities
        if (!isGrabbing) {
            grabbableObject = grabbable;
            grabAvailable = true;
        }
    }

    // called by external Grabbable objects, to rescind their offer, usually when the player moves away from them
    public void rescindGrab() {
        if (!isGrabbing) {
            grabbableObject = null;
            grabAvailable = false;
        }
    }

    public void setSigilState(bool state) {
        setControlsLocked(state);
    }

    public void setControlsLocked(bool state) {
        controlsLocked = state;
    }

    public void setFloating(bool state) {
        //TODO: make this fancier and a bit different to show it's falling
        isFloating = state;
    }

    // ================================== IFREEZABLE IMPLEMENTATION ================================== //
    public class CharacterFrame : Frame {
        public Vector3 position;
        public int direction;
        // inventory


        public CharacterFrame(Transform _transform, int _direction) {
            position = _transform.position;
            direction = _direction;
        }
    }

    public Frame getFrame() {
        return new CharacterFrame(gameObject.transform, direction);
    }

    public void setFrame(Frame f) {
        CharacterFrame frm = (CharacterFrame)f;
        ghostObject.transform.position = frm.position;
        //if (overridden) {
            //spawn ghost object and manipulate it
        //}
        //CharacterFrame frm = (CharacterFrame)f;
        //gameObject.transform.position = frm.position;

        if (!inReplay) {
            // Do nothing. We explicitly don't want to set anything about the player,
            // since the player is free of temporal influence
        } else {
            // If we're in a cutscene or in a replay, set the player's frame as normal
            transform.position = frm.position;
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
        setControlsLocked(!state);
    }
}
