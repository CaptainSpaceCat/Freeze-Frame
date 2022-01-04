using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropController : MonoBehaviour
{
    public GameObject boundObject;
    public IDroppable droppableObject;

    private void Start() {
        droppableObject = boundObject.GetComponent<IDroppable>();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Floor") {
            droppableObject.setFloating(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Floor") {
            droppableObject.setFloating(false);
        }
    }
}
