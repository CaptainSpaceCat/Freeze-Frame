using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPattern : MonoBehaviour
{
	private Vector3[] positions;
	private bool[] terminalPositions;
	private int[] macroIndices;
	private int count;
	private int currentIndex;
	public bool ready = false;

	void Awake()
	{
		Transform[] allChildren = GetComponentsInChildren<Transform>();
		count = allChildren.Length - 1;
		positions = new Vector3[count];
		terminalPositions = new bool[count];
		int numTerminal = 0;
		// start at 1 so we don't save the transform on the parent object (which gets included by GetComponentsInChildren())
		for (int i = 1; i < count + 1; i++) {
			positions[i - 1] = allChildren[i].position;
			terminalPositions[i - 1] = allChildren[i].GetComponent<PatrolNode>().terminal;
			if (terminalPositions[i - 1]) {
				numTerminal++;
            }
		}

		//set up macro indexing (right now just for sigil coloration)
		macroIndices = new int[count];
		int macroCounter = 0;
		for (int i = 0; i < count; i++) {
			macroIndices[i] = macroCounter;
			if (terminalPositions[i]) {
				macroCounter = (macroCounter + 1) % numTerminal;
			}
		}
		ready = true;
	}

	public Vector3 getClosestPosition(Vector3 pos) {
		float minDistance = Vector3.Distance(pos, positions[0]);
		int index = 0;
		for (int i = 1; i < count; i++) {
			float distance = Vector3.Distance(pos, positions[i]);
			if (distance < minDistance) {
				minDistance = distance;
				index = i;
			}
		}
		// I'm assuming that if the enemy object calls this method,
		// it's going to that index and that index should be the current one
		currentIndex = index;
		return positions[index];
	}

	public Vector3 getNextPosition() {
		currentIndex = (currentIndex + 1) % count;
		return positions[currentIndex];
	}

	public void setIndex(int index) {
		currentIndex = index;
	}

	public int getIndex() {
		return currentIndex;
	}

	//returns the index of the patrol pattern, only counting terminal nodes
	public int getMacroIndex() {
		return macroIndices[currentIndex];
	}

	public bool isTerminalIndex() {
		return terminalPositions[currentIndex];
    }
}
