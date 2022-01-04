using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SigilController : MonoBehaviour, IGrabbableObject, IFreezable
{
	private bool isActivated = false;
	public PatrolPattern patrol;
	private Vector2 goalPos;

	public GameObject playerObject;

	public Shader mainShader;
	private Material mainMaterial;

	public Color[] colorList;

	public float delay;
	private float waitTimer;
    public float animationDelay;
    private float animationTimer;
	public AnimationCurve switchCurve;

	private bool isFrozen = false;
	private bool isEnabled = true;

	void Start() {
		mainMaterial = new Material(mainShader);
		GetComponent<SpriteRenderer>().material = mainMaterial;
		goalPos = (Vector2)transform.position;
		playerObject = GameObject.FindWithTag("Player");
        mainMaterial.SetColor("_ColorMid", colorList[patrol.getMacroIndex()]);
    }

    void Update() {
    	if (isActivated && animationTimer > 0f) {
    		if (animationTimer < delay/2f) {
    			mainMaterial.SetColor("_ColorMid", colorList[patrol.getMacroIndex()]);
    		}
    		mainMaterial.SetFloat("_Lightness", switchCurve.Evaluate(1f - animationTimer / animationDelay));
    	}
    }

	void FixedUpdate() {
        if (isActivated) {
            if (animationTimer > 0f) {
                animationTimer -= Time.fixedDeltaTime;
            }
        }
        if (!isFrozen) {
			if (isActivated) {
				if (waitTimer > 0f) {
					waitTimer -= Time.fixedDeltaTime;
				}
			}
			if (waitTimer <= 0f) {
				if (!closeTo((Vector2)transform.position, goalPos)) {
					Vector2 diff = goalPos - (Vector2)transform.position;
					transform.Translate(diff.normalized * 0.1f);
				} else {
                    transform.position = goalPos;
                    if (patrol.isTerminalIndex()) {
                        isActivated = false;
                    } else {
                        goalPos = patrol.getNextPosition();
                    }
				}
			}
		}
	}

	private bool closeTo(Vector2 a, Vector2 b) {
		Vector2 diff = a - b;
		return diff.magnitude < 0.1f;
	}

    public void OnGrab() {
        playerObject.GetComponent<CharacterController>().setSigilState(true);
    	playerObject.transform.SetParent(transform);
    	Activate();
    }

    public void OnRelease() {
    	playerObject.GetComponent<CharacterController>().setSigilState(false);
    	playerObject.transform.SetParent(null);
    }

    public void Activate() {
    	if (!isActivated) {
    		isActivated = true;
    		waitTimer = delay;
            animationTimer = animationDelay;
			goalPos = patrol.getNextPosition();
    	}
    }

    // ================================== IFREEZABLE IMPLEMENTATION ================================== //
    public class SigilFrame : Frame {
    	public bool isActivated;
    	public Vector3 position;
    	public int patrolIndex;
    	public Vector2 goalPos;
    	public float waitTimer;
    	// whether or not the box is grabbed will always be independent of time being frozen
    	// since the player does the grabbing. thus, we don't save it here

    	public SigilFrame(Vector3 _position, bool _isActivated, Vector2 _goalPos, int _patrolIndex, float _waitTimer) {
    		position = _position;
    		isActivated = _isActivated;
    		goalPos = _goalPos;
    		patrolIndex = _patrolIndex;
    		waitTimer = _waitTimer;
    	}
    }

    public Frame getFrame() {
    	return new SigilFrame(transform.position, isActivated, goalPos, patrol.getIndex(), waitTimer);
    }

    public void setFrame(Frame f) {
    	SigilFrame frm = (SigilFrame)f;

    	// interestingly enough, the grabbing mechanic overrides the time mechanic,
    	// as the player should be free to move a box around even while reversing time
    	// thus, we don't set the box's position if the player is grabbing it
        transform.position = frm.position;
    	isActivated = frm.isActivated;
    	if (isActivated) {
    		waitTimer = frm.waitTimer;
    	}
    	goalPos = frm.goalPos;
    	patrol.setIndex(frm.patrolIndex);
        mainMaterial.SetColor("_ColorMid", colorList[patrol.getMacroIndex()]);
    }

    public void setFrozen(bool state) {
    	isFrozen = state;

        if (isFrozen) {
            // ghostObject.SetActive(true);
            // ghostObject.transform.position = transform.position;
        } else {
            // ghostObject.SetActive(false);
        }
    }

    public void setEnabled(bool state) {
        isEnabled = state;
    }
}
