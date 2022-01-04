using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

	private bool _forward = false;
	public bool Forward {
		get { return _forward; }
	}

	private bool _back = false;
	public bool Back {
		get { return _back; }
	}

	private bool _left = false;
	public bool Left {
		get { return _left; }
	}

	private bool _right = false;
	public bool Right {
		get { return _right; }
	}

	private bool _timeForward = false;
	public bool TimeForward {
		get { return _timeForward; }
	}
	
	private bool _timeBack = false;
	public bool TimeBack {
		get { return _timeBack; }
	}

	private bool _grab = false;
	public bool Grab {
		get { return _grab; }
	}

	//when set to true, stay true until accessed, once accessed set to false again.
	private bool _interact = false;
	public bool Interact {
		get { 
			bool temp = _interact;
			_interact = false;
			return temp;
		}
	}

	//same as interact. this works because the FixedUpdate loop in characterController will check these vars every loop
	//and if they were true from this loop, we'll see that.
	private bool _restart = false;
	public bool Restart {
		get { 
			bool temp = _restart;
			_restart = false;
			return temp;
		}
	}

	private bool _freeze = false;
	public bool Freeze {
		get { 
			bool temp = _freeze;
			_freeze = false;
			return temp;
		}
	}

    void Update()
    {
        _interact = _interact || Input.GetKeyDown(KeyCode.V) || Input.GetMouseButtonDown(0);
        _restart = _restart || Input.GetKeyDown(KeyCode.R);
        _freeze = _freeze || Input.GetKeyDown(KeyCode.C);

        _forward = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        _back = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        _left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        _right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);

        _grab = Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.LeftShift);

        _timeForward = Input.GetKey(KeyCode.X);
        _timeBack = Input.GetKey(KeyCode.Z);
    }
}
