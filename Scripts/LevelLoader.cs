using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public int loadIndex;

    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject.FindObjectOfType<ProgressTracker>().SetLevelIndex(loadIndex);
        SceneManager.LoadScene(loadIndex);
    }
}
