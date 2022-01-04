using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{

    private static ProgressTracker instance;
    private static int levelIndex = -1;
    public Transform[] playerStartPositions;

    void Start()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (levelIndex >= 0) {
            GameObject.FindGameObjectWithTag("Player").transform.position = playerStartPositions[levelIndex].position;
        }
    }


    public void SetLevelIndex(int idx) {
        //every time the player enters a level, the index is saved in this static persistent object
        //the index is offset by 1 because index 0 is reserved for the menu scene
        //then when the main menu is loaded again, this object will place the player outside the level they just entered
        levelIndex = idx - 1;
	}
}
