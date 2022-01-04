using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeyBoxController : MonoBehaviour, IFreezable, IGrabbableBoundObject
{
    public enum eExplosionState {Idle, FuseLit, Exploding, Exploded}
    private eExplosionState explosionState = eExplosionState.Idle;
    private bool isFrozen = false;
	private bool isGrabbed = false;
	private Vector3 offset;
	private GameObject playerObject;

    public float timeToExplode = 1f;
    public float fuseTime;
    public float explosionRadius = 2f;
    public AudioSource explosionSound;

    private Animator anim;

    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        anim = GetComponent<Animator>();
        fuseTime = timeToExplode;
    }

    void FixedUpdate()
    {
        if (isGrabbed) {
        	transform.position = playerObject.transform.position + offset;
        }
        if (explosionState == eExplosionState.FuseLit) {
            if (fuseTime <= 0) {
                //if the fuse is active and just got to 0, explode the box
                setExplosionState(eExplosionState.Exploding);
            } else if (!isFrozen) {
                fuseTime -= Time.deltaTime;
                anim.SetInteger("animIndex", getAnimIndex(fuseTime, timeToExplode));
            }
        } else if (explosionState == eExplosionState.Exploding) {
            //if we get to this if loop, it means that it exploded last frame
            //now it's time to set it to be exploded so that it doesn't keep killing the player
            setExplosionState(eExplosionState.Exploded);
        }
    }

    public void setGrabbed(bool state, Vector3 playerOffset) {
    	isGrabbed = state;
    	if (state) {
	    	offset = transform.position - playerOffset;
            if (explosionState != eExplosionState.Exploded) {
                setExplosionState(eExplosionState.FuseLit);
            }
	    }
    }

    private int getAnimIndex(float fuse, float total) {
        if (fuse <= 0f) {
            return -1;
        } else if (fuse >= total) {
            return 8;
        }
    	return (int)Mathf.Floor((fuse/total) * 8);
    }

    private void setExplosionState(eExplosionState state) {
    	explosionState = state;

    	if (explosionState == eExplosionState.Exploding) {
    		if (Vector3.Distance(playerObject.transform.position, gameObject.transform.position) <= explosionRadius) {
        		// if the player is too close, kill them
        		playerObject.GetComponent<CharacterController>().OnDeath();
        	} else {
        		// if they survive, force them to release the now nonexistent box
        		playerObject.GetComponent<CharacterController>().OnRelease();
        	}
        	
            OnDetonate();
    	} else {
    	}
    }

    private void OnDetonate() {
        explosionSound.Play();
    }

    // ================================== IFREEZABLE IMPLEMENTATION ================================== //
    public class ExplodeyBoxFrame : Frame {
        public Vector3 position;
        public float fuseTime;
        public eExplosionState explosionState;
        // whether or not the box is grabbed will always be independent of time being frozen
        // since the player does the grabbing. thus, we don't save it here

        public ExplodeyBoxFrame(Vector3 _position, float _fuseTime, eExplosionState _explosionState) {
            position = _position;
            fuseTime = _fuseTime;
            explosionState = _explosionState;
        }
    }

    public Frame getFrame() {
        return new ExplodeyBoxFrame(gameObject.transform.position, fuseTime, explosionState);
    }

    public void setFrame(Frame f) {
        ExplodeyBoxFrame frm = (ExplodeyBoxFrame)f;

        // interestingly enough, the grabbing mechanic overrides the time mechanic,
        // as the player should be free to move a box around even while reversing time
        // thus, we don't set the box's position if the player is grabbing it
        if (!isGrabbed) {
            gameObject.transform.position = frm.position;
            setExplosionState(frm.explosionState);
        } else {
            //if the box is grabbed, the fuse should of course be active, even if it wasn't before
            setExplosionState(eExplosionState.FuseLit);
        }
        fuseTime = frm.fuseTime;
        anim.SetInteger("animIndex", getAnimIndex(frm.fuseTime, timeToExplode));
    }

    public void setFrozen(bool state) {
        isFrozen = state;
        if (state) {
        } else {
            anim.SetInteger("animIndex", getAnimIndex(fuseTime, timeToExplode));
        }
    }

    public void setEnabled(bool state) {
        //pass
    }
}
