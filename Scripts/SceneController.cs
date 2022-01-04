using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	private int sceneIndex;

    void Start() {
        sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
    }

	//flag exists to stop goal area from loading a second replay during the end of the first replay
	private bool completed = false;

    void OnTriggerEnter2D(Collider2D other) {
    	if (!completed && other.gameObject.tag == "Player") {
    		completed = true;
    		TimeController TC = GameObject.FindObjectsOfType<TimeController>()[0];
    		TC.triggerReplay(0);
    	}
    }

    private void loadLevel(int index) {
    	SceneManager.LoadScene(index);
    }
}
