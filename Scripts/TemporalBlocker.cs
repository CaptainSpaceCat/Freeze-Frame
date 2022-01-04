using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporalBlocker : MonoBehaviour
{
	private TimeController timeControl;
    public Material mat;
    private Material mainMaterial;
    private GameObject playerObject;
    private float sceneTime;

    void Awake() {
        sceneTime = Time.time;
    }

    void Start()
    {
        mainMaterial = new Material(mat);
        GetComponent<Renderer>().material = mainMaterial;
        timeControl = GameObject.FindWithTag("TimeControl").GetComponent<TimeController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
            playerObject = other.gameObject; //might as well nab this bad boy while we got him
    		timeControl.setTimeBlocked(true, this);
    	}
    }

    void OnTriggerExit2D(Collider2D other) {
    	if (other.gameObject.tag == "Player") {
    		timeControl.setTimeBlocked(false, null);
    	}
    }

    private Vector2 getRelativePlayerPos() {
        Vector2 myPos = (Vector2)transform.position;
        Vector2 myScale = (Vector2)transform.localScale/2f;
        Vector2 myOrigin = myPos - myScale;
        Vector2 playerPos = (Vector2)playerObject.transform.position;
        return playerPos - myOrigin;
    }

    public void setRippleParams() {
        mainMaterial.SetFloat("_StartTime", Time.time - sceneTime);
        Vector2 playerPos = getRelativePlayerPos();
        mainMaterial.SetVector("_StartPosition", new Vector4(playerPos.x, playerPos.y, 0f, 0f));
    }
}
