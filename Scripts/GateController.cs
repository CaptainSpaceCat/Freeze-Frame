using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
	public GameObject source;
	public GameObject sink;
	public ParticleSystem particles;
	public GameObject collider;

	public bool active = true;

    // Start is called before the first frame update
    void Start()
    {
        // initialize to whatever has been set in the inspector
        setState(active);
    }

    // Update is called once per frame
    void Update()
    {
        if (active) {
            if (!particles.isPlaying) {
                particles.Play();
            }
        } else {
            particles.Stop();
        }
    }

    public void setState(bool state) {
    	active = state;
    	if (active) {
    		collider.SetActive(true);
    	} else {
    		collider.SetActive(false);
    	}
    }
}
