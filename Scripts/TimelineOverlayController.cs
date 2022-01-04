using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineOverlayController : MonoBehaviour
{
	public RectTransform blueMiddle;
	public RectTransform blueEdge;
	public RectTransform redMiddle;
	public RectTransform redEdge;
	public RectTransform cursor;

	public float cursorGoal;
	public float redGoal;
	public float blueGoal;

	public float leftMin;
	public float rightMax;

	public float timeToMove;

    // Start is called before the first frame update
    void Awake()
    {
        setCursor(1f);
        setBlue(1f);
        setRed(0f);
    }

    void FixedUpdate()
    {
    	moveCursor();
    	moveBlue();
    	moveRed();
    }

    private void moveCursor() {
    	moveSprite(cursor, percentToCoord(cursorGoal));
    }

    private void moveBlue() {
    	moveSprite(blueEdge, percentToCoord(blueGoal));
    	moveSprite(blueMiddle, percentToCoord(blueGoal/2));
    	scaleSprite(blueMiddle, percentToScale(blueGoal));
    }

    private void moveRed() {
    	moveSprite(redEdge, percentToCoord(redGoal));
    	moveSprite(redMiddle, percentToCoord(redGoal/2));
    	scaleSprite(redMiddle, percentToScale(redGoal));
    }

    // this is a tad jank
    // TODO: switch over to Animations
    private void scaleSprite(RectTransform obj, float goal) {
    	float x = obj.rect.width;
    	if (!closeTo(x, goal)) {
	    	float diff = goal - x;
	    	obj.sizeDelta = new Vector2(x + diff * 0.16f, obj.rect.height);
    	} else {
    		obj.sizeDelta = new Vector2(goal, obj.rect.height);
    	}
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
