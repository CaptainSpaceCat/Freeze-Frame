using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IFreezable
{
	public bool isPressed = false;
	public bool isEnabled = true;
	private bool isFrozen = false;
	private bool notable = false;
	public ButtonColliderCounter colliderCounter;

	public GameObject upObject;
	public GameObject downObject;

	[System.Serializable]
	public class OnActivate : UnityEvent { }

	[SerializeField]
	private OnActivate _onActivate = new OnActivate();
	public OnActivate onActivate { get { return _onActivate; } set { _onActivate = value; } }

	public void OnButtonActivated() {
		if (!isFrozen && isEnabled) {
			setPressed(true);
		}
	}

	[System.Serializable]
	public class OnDeactivate : UnityEvent { }

	[SerializeField]
	private OnDeactivate _onDeactivate = new OnDeactivate();
	public OnDeactivate onDeactivate { get { return _onDeactivate; } set { _onDeactivate = value; } }

	public void OnButtonDeactivated() {
		if (!isFrozen && isEnabled) {
			setPressed(false);
		}
	}

	void Start() {
		setPressed(isPressed);
	}

	public void setPressed(bool state) {
		if (isPressed != state) {
			notable = true;
        }
		isPressed = state;
		if (state) {
			isPressed = true;
			upObject.SetActive(false);
			downObject.SetActive(true);
			onActivate.Invoke();
		} else {
			isPressed = false;
			upObject.SetActive(true);
			downObject.SetActive(false);
			onDeactivate.Invoke();
		}
	}

	public class ButtonFrame : Frame {
		public bool isPressed;

		public ButtonFrame(bool _isPressed, bool _notable) {
			isPressed = _isPressed;
			notable = _notable;
		}
	}

	public Frame getFrame() {
		bool temp = notable;
		notable = false;
		return new ButtonFrame(isPressed, temp);
	}

	public void setFrame(Frame f) {
		ButtonFrame frm = (ButtonFrame)f;
		
		setPressed(frm.isPressed);
	}

	public void setFrozen(bool state) {
		isFrozen = state;

		if (!state) {
			//if unfreezing, we need to handle abrupt changes to the button's state
			//after all, something that was pressing it might have just been "teleported" away
			setPressed(colliderCounter.isPressed());
		}
	}

	public void setEnabled(bool state) {
        isEnabled = state;
    }
}
