using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private static MusicController instance;

    public AudioSource forward;
    public AudioSource reverse;

    public float maxVolume;
    public float minVolume;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        forward.Play();
        reverse.Play();

        playForward(maxVolume);
    }

    public void setDirection(int direction) {
        /* 0 - frozen
		 * 1 - forward
		 * -1 - backward
		 * 2 - stop
		 */
        //forward.timeSamples = getFlippedSample(reverse);
        if (direction == 1 || direction == 2) {
            playForward(maxVolume);
        } else if (direction == 0) {
            playForward(minVolume);
        } else if (direction == -1) {
            playReverse(minVolume);
        }
    }

    private void playForward(float volume) {
        forward.volume = volume;
        if (reverse.volume != 0) {
            forward.timeSamples = getFlippedSample(reverse);
        }
        reverse.volume = 0f;
    }

    private void playReverse(float volume) {
        reverse.volume = volume;
        if (forward.volume != 0) {
            reverse.timeSamples = getFlippedSample(forward);
        }
        forward.volume = 0f;
    }

    private int getFlippedSample(AudioSource source) {
        int result = source.clip.samples - source.timeSamples;
        Debug.Log("sample: " + result);
        return result;
    }
}
