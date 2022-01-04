using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyController : MonoBehaviour, IFreezable
{
	private GameObject playerObject;
	public float speed;
	public AudioSource alertBeep;

	//gets automatically set the the starting position, for guards
	private Vector3 homePosition;

	//finalTargetPos stores final target, immediateTargetPos stores intermediate target for A* pathing
	private Vector3 immediateTargetPos;
	private Vector3 finalTargetPos;

	//optional patrolPattern object that defines where to patrol
	public PatrolPattern patrolPattern;
	private float waitingTicks;

	public bool isFrozen = false;
	public bool isEnabled = true;

	public LineRenderer potentialLine;
	public LineRenderer actualLine;

	public enum enEnemyMode {Guard, Roam, Patrol}
	public enEnemyMode mode;

	public enum enEnemyState {Idle, Pursuing, Waiting, Returning}
	public enEnemyState state = enEnemyState.Idle;

	private AStarSolver aStar;

	// Start is called before the first frame update
	void Start()
	{
		aStar = GetComponent<AStarSolver>();
		playerObject = GameObject.FindWithTag("Player");
		if (mode == enEnemyMode.Guard) {
			homePosition = transform.position;
		} else if (mode == enEnemyMode.Patrol) {
			//set the initial target for the patroller
			Assert.IsTrue(patrolPattern.ready);
			setTarget(patrolPattern.getClosestPosition(transform.position));
		}
	}

	void Update() {
		visualizeTarget(state == enEnemyState.Pursuing);
	}

	void FixedUpdate()
	{
		if (isEnabled) {
			RaycastHit2D hit = raycastToPlayer();

			if (!isFrozen) {
				if (hit.collider != null && hit.collider.gameObject == playerObject) {
					if (state != enEnemyState.Pursuing || Vector3.Distance(playerObject.transform.position, finalTargetPos) > 0.3f) {
						alertBeep.Play();
					}
					_pursue(playerObject.transform.position);
				}

				if (state == enEnemyState.Idle) {
					// for patrollers, make them patrol
					if (mode == enEnemyMode.Patrol) {
						if (reachedFinalTarget()) {
							//TODO what happens if potentially the player blocks off part of the patrol pattern???
							//TODO do we include A* for patrolling?
							setTarget(patrolPattern.getNextPosition());
						} else {
							moveTowardTarget();
						}
					}
				} else if (state == enEnemyState.Pursuing) { // Handle the pursuit of the player!
					if (reachedFinalTarget()) {
						if (mode == enEnemyMode.Guard || mode == enEnemyMode.Patrol) {
							_wait(Random.Range(80, 100));
						} else if (mode == enEnemyMode.Roam) {
							_idle();
						}
					} else {
						moveTowardTarget();
					}
				} else if (state == enEnemyState.Waiting) { //Handle the boring waits...
					if (waitingTicks > 0) {
						waitingTicks--;
					} else {
						_return();
					}
				} else if (state == enEnemyState.Returning) {
					if (reachedFinalTarget()) {
	        			_idle();
	        		} else {
	        			moveTowardTarget();
	        		}
				}
			} else {
				if (hit.collider != null && hit.collider.gameObject == playerObject) {
					visualizePotentialTarget(true, playerObject.transform.position);
				} else {
					visualizePotentialTarget(false, Vector3.zero);
				}
			}
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.gameObject == playerObject) {
			playerObject.GetComponent<CharacterController>().OnDeath();
		}
	}

	//raycast that doesn't ignore objects on the translucent layer
	//this is used to detect if the enemy has a direct path to its target, or if it needs to use A*
	private RaycastHit2D raycastOpaque(Vector3 target) {
		LayerMask mask = LayerMask.GetMask("Ignore Raycast", "ActivatorIgnoreRaycast");
		mask = ~mask;
		return Physics2D.Raycast(transform.position, target - transform.position, Vector2.Distance(transform.position, target), mask);
	}

	//raycast directly to the player
	private RaycastHit2D raycastToPlayer() {
		Vector3 raycastDir = playerObject.transform.position - transform.position;
		LayerMask mask = LayerMask.GetMask("Ignore Raycast", "ActivatorIgnoreRaycast", "Translucent");
		mask = ~mask;
		return Physics2D.Raycast(transform.position, raycastDir, Mathf.Infinity, mask);
	}

	private bool setTarget(Vector3 position) {
		finalTargetPos = position;

		RaycastHit2D hit = raycastOpaque(position);
		if (hit.collider == null || hit.collider.gameObject == playerObject) {
			//if we've got a clear path to our target or the player, just go for it
			immediateTargetPos = position;
			return true;
		} else {
			//if some translucent object is blocking us from just moving right to the player, try and path around it
			Vector3 pos = aStar.getPathSegment(position);
			if (pos != null) {
				immediateTargetPos = pos;
				return true;
			}
		}
		return false;
	}

	private void moveTowardTarget() {
		if (reachedImmediateTarget()) {
			if (!setTarget(finalTargetPos)) {
				//TODO decide what to do if we can't reach our target
			}
		}
		Vector3 pursuitDir = immediateTargetPos - transform.position;
		transform.Translate(pursuitDir.normalized * speed);
	}

	private bool reachedFinalTarget() {
		Vector3 pursuitDir = finalTargetPos - transform.position;
		return pursuitDir.magnitude < 0.3;
	}

	private bool reachedImmediateTarget() {
		Vector3 pursuitDir = immediateTargetPos - transform.position;
		return pursuitDir.magnitude < 0.3;
	}

	// Runs whenever time is unfrozen and the enemy can see the player
	private void visualizeTarget(bool _state) {
		if (!_state) {
			actualLine.enabled = false;
		} else {
			actualLine.enabled = true;
			actualLine.positionCount = 2;
			Vector3[] pos = new Vector3[2] {transform.position, finalTargetPos};
			actualLine.SetPositions(pos);
		}
	}

	// Runs whenever time is frozen the enemy would be able to see the player if time were unfrozen
	private void visualizePotentialTarget(bool _state, Vector3 potentialTargetPos) {
		if (!_state) {
			potentialLine.enabled = false;
		} else {
			potentialLine.enabled = true;
			potentialLine.positionCount = 2;
			Vector3[] pos = new Vector3[2] {transform.position, potentialTargetPos};
			potentialLine.SetPositions(pos);
		}
	}

	// This method is for safeZones to call, when enemies enter safezones, we can disorient them so they return home and dont get stuck
	public void disorient() {
		_wait(Random.Range(100, 160));
	}

	// ================================== ENEMYSTATE HANDLING ================================== //
	private void _idle() {
		state = enEnemyState.Idle;
	}

	private void _pursue(Vector3 playerPos) {
		state = enEnemyState.Pursuing;
		setTarget(playerPos);
	}

	private void _wait(float ticks) {
		state = enEnemyState.Waiting;
		waitingTicks = ticks;
	}

	private void _return() {
		state = enEnemyState.Returning;
		if (mode == enEnemyMode.Guard) {
			//guards will return to their guard position
			setTarget(homePosition);
		} else if (mode == enEnemyMode.Patrol) {
			//patrollers will go to the closest patrol node on their path and continue patrolling
			setTarget(patrolPattern.getClosestPosition(transform.position));
		} else if (mode == enEnemyMode.Roam) {
			//roamers don't actually have a home, they just chill out wherever they last saw ya!
			_idle();
		}
	}

	// ================================== IFREEZABLE IMPLEMENTATION ================================== //
	public class EnemyFrame : Frame {
		public Vector3 position;
		public Vector3 finalTargetPos;
		public float waitingTicks;
		public EnemyController.enEnemyState state;
		public int patrolIndex;

		public EnemyFrame(Transform _transform, Vector3 _finalTargetPos, float _waitingTicks, EnemyController.enEnemyState _state, int _patrolIndex) {
			position = _transform.position;
			finalTargetPos = _finalTargetPos;
			waitingTicks = _waitingTicks;
			state = _state;
			patrolIndex = _patrolIndex;
		}
	}

	public Frame getFrame() {
		if (mode == enEnemyMode.Patrol) {
			return new EnemyFrame(transform, finalTargetPos, waitingTicks, state, patrolPattern.getIndex());
		}
		return new EnemyFrame(transform, finalTargetPos, waitingTicks, state, 0);
	}

	public void setFrame(Frame f) {
		EnemyFrame frm = (EnemyFrame)f;
		transform.position = frm.position;

		if (mode == enEnemyMode.Patrol) {
			patrolPattern.setIndex(frm.patrolIndex);
			setTarget(frm.finalTargetPos);
		}

		if (frm.state == enEnemyState.Idle) {
			_idle();
		} else if (frm.state == enEnemyState.Pursuing) {
			_pursue(frm.finalTargetPos);
		} else if (frm.state == enEnemyState.Waiting) {
			_wait(frm.waitingTicks);
		} else if (frm.state == enEnemyState.Returning) {
			_return();
		}
	}

	public void setFrozen(bool _state) {
		isFrozen = _state;
		if (!isFrozen) {
			visualizePotentialTarget(false, Vector3.zero);
		}
	}

	public void setEnabled(bool state) {
        isEnabled = state;
        if (!isEnabled) {
        	visualizePotentialTarget(false, Vector2.zero);
        	visualizeTarget(false);
        }
    }
}
