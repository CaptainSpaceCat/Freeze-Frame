using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBarShaderController : MonoBehaviour
{
	public RectTransform cursor;
	public Material mainMaterial;

	public float cursorGoal;
	private float redVal;
	public float redGoal;
	private float blueVal;
	public float blueGoal;

	public float blueMovementSpeed;

	public float leftMin;
	public float rightMax;

	public float timeToMove;
	private float kTimeNumerator = 0.012f;


    void Awake()
    {
        setCursor(1f);
        setBlue(1f);
        setRed(0f);
    }

    void Update()
    {
    	moveCursor();
    	moveRed();
    	moveBlue();
    }

    private void moveCursor() {
    	moveSprite(cursor, percentToCoord(cursorGoal));
    }

    private void moveRed() {
    	float diff = redGoal - redVal;
    	redVal += diff * kTimeNumerator / timeToMove;
    	mainMaterial.SetFloat("_RedPercent", redVal);
    }

    private void moveBlue() {
    	float diff = blueGoal - blueVal;
    	blueVal += diff * kTimeNumerator / timeToMove;
    	mainMaterial.SetFloat("_BluePercent", blueVal);
    }

    public void setBlueMovement(float t) {
    	// if (state) {
    	// 	mainMaterial.SetFloat("_BlueOffset", blueMovementSpeed);
    	// } else {
    		mainMaterial.SetFloat("_BlueOffset", t);
    	//}
    }

    private void moveSprite(RectTransform obj, float goal) {
    	float x = obj.localPosition.x;
    	if (!closeTo(x, goal)) {
	    	float diff = goal - x;
	    	obj.Translate(Vector3.right * diff * 0.004f);
    	} else {
    		obj.localPosition = new Vector2(x, obj.localPosition.y);
    	}
    }

    private bool closeTo(float a, float b) {
    	return Mathf.Abs(a-b) <= 0.1;
    }

    private float percentToCoord(float percent) {
    	return percent * (rightMax - leftMin) + leftMin;
    }

    private float percentToScale(float percent) {
    	return percent * (rightMax - leftMin);
    }

    public void setCursor(float percent) {
    	cursorGoal = percent;
    }

    public void setBlue(float percent) {
    	blueGoal = percent;
    }

    public void setRed(float percent) {
    	redGoal = percent;
    }
}
