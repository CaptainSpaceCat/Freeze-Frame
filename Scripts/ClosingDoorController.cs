using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingDoorController : MonoBehaviour
{
	private bool isClosing = false;
	private bool isClosed = false;
	public float speed;
	public DoorSideDetector detector;
	public GameObject door1;
	public GameObject door2;

	public float doorHeight = 2.56f; //TODO collect this automatically

	private Vector3 goal1;
	private Vector3 goal2;

    // Start is called before the first frame update
    void Start()
    {
    	Vector3 dir1 = door1.transform.position - gameObject.transform.position;
        Vector3 dir2 = door2.transform.position - gameObject.transform.position;
        goal1 = gameObject.transform.position + (dir1.normalized * (doorHeight/2));
        goal2 = gameObject.transform.position + (dir2.normalized * (doorHeight/2));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isClosing) {
        	Vector3 dir1 = gameObject.transform.position - goal1;
        	Vector3 dir2 = gameObject.transform.position - goal2;
        	door1.transform.Translate(dir1.normalized * speed, Space.World);
        	door2.transform.Translate(dir2.normalized * speed, Space.World);
        	if (closingComplete()) {
        		door1.transform.position = goal1;
        		door2.transform.position = goal2;
        		isClosing = false;
        		isClosed = true;
        	}
        }
    }

    private bool closingComplete() {
    	Vector3 dir1 = door1.transform.position - goal1;
    	Vector3 dir2 = door2.transform.position - goal2;
    	return dir1.magnitude < 0.1 && dir2.magnitude < 0.1;
    }

    void OnTriggerExit2D(Collider2D other) {
    	if (!isClosed && other.gameObject.tag == "Player" && detector.entered) {
    		isClosing = true;
    	}
    }
}
