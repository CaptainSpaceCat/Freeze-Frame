using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class TimeSpeckleShaderController : MonoBehaviour
{

	public Gradient tintGradient;
	public Gradient speckleGradient;
	public Color blockedColor;
	public Color replayColor;

	public AnimationCurve circleSpeedCurve;
	public AnimationCurve spinSpeedCurve;

	private float speedVal = 1f;
	public Shader mainShader;
	private Material mainMaterial;

	public float transparency = .5f;
	public float tintStrength = .5f;
	public float speckleStrength = .8f;
	public float circleFreq = 60f;
	public float circleSlices = 30f;
	public float sliceWidth = .94f;
	public float sliceLength = .94f;
	public float rippleStrength = .01f;
	public float rippleSpeed = 2f;
	public float rippleFreq = 12f;


    // Start is called before the first frame update
    void Start()
    {
        mainMaterial = new Material(mainShader);
        applyPropertiesToShader();
        setInvisible(true);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
    	Graphics.Blit(source, destination, mainMaterial);
    }

    // apply secondary properties (ones that don't generally change)
    // called on Start()
    private void applyPropertiesToShader() {
		mainMaterial.SetFloat("_TintStrength", tintStrength);
    	mainMaterial.SetFloat("_SpeckleStrength", speckleStrength);
    	mainMaterial.SetFloat("_CircleFreq", circleFreq);
    	mainMaterial.SetFloat("_CircleSlices", circleSlices);
    	mainMaterial.SetFloat("_SliceWidth", sliceWidth);
    	mainMaterial.SetFloat("_SliceLength", sliceLength);
    	mainMaterial.SetFloat("_RippleSpeed", rippleSpeed);
    	mainMaterial.SetFloat("_RippleFreq", rippleFreq);
    }

    // apply speed properties to the shader
    // handles spinning and circle motion speeds
    public void setSpeedVal(float val) {
    	mainMaterial.SetFloat("_CircleFreq", circleFreq);
    	// Clamp, then convert to range(0, 1)
    	// the parameter is in range(-1, 1) because that makes more sense intuitively,
    	// but it needs to be between 0 and 1 to work here
    	speedVal = Mathf.Clamp(val, -1f, 1f);
    	speedVal = speedVal*.5f + .5f;

    	mainMaterial.SetFloat("_CircleSpeed", circleSpeedCurve.Evaluate(speedVal));
    	mainMaterial.SetFloat("_SpinSpeed", spinSpeedCurve.Evaluate(speedVal));

    	mainMaterial.SetVector("_TintColor", tintGradient.Evaluate(speedVal));
    	mainMaterial.SetVector("_SpeckleCol", speckleGradient.Evaluate(speedVal));

    	mainMaterial.SetFloat("_RippleStr", rippleStrength);
    }

    // essentially disables/enables the shader
    public void setInvisible(bool state) {
    	if (state) {
			mainMaterial.SetFloat("_Transparency", 0);
    		mainMaterial.SetFloat("_RippleStr", 0);
    	} else {
    		mainMaterial.SetFloat("_Transparency", transparency);
    	}
    	
    }

    // handles whenever the player is in a blockerZone
    public void setBlocked() {
    	mainMaterial.SetVector("_TintColor", blockedColor);
    	mainMaterial.SetFloat("_CircleFreq", 0f);
		mainMaterial.SetFloat("_RippleStr", 0);
    }

    public void setReplay() {
    	mainMaterial.SetVector("_TintColor", replayColor);
    	mainMaterial.SetFloat("_CircleFreq", 0f);
    	mainMaterial.SetFloat("_RippleStr", 0);
    }
}
