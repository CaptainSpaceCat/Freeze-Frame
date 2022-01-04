using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonColliderCounter : MonoBehaviour
{
    public int count;
    public ButtonController BC;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == 8 || other.gameObject.layer == 9) {
            count++;
            if (count == 1) {
                BC.OnButtonActivated();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == 8 || other.gameObject.layer == 9) {
        	count--;
        	if (count == 0) {
        		BC.OnButtonDeactivated();
        	}
        }
    }

    public bool isPressed() {
    	return count > 0;
    }
}
