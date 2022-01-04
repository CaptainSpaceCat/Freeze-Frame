using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarSolver : MonoBehaviour
{
	class Coords2 {
		public int x;
		public int y;

		public Coords2() {
			x = 0;
			y = 0;
		}

		public Coords2(int _x, int _y) {
			x = _x;
			y = _y;
		}

		public Coords2(Vector2 _vec) {
			x = (int)Mathf.Floor(_vec.x);
			y = (int)Mathf.Floor(_vec.y);
		}

		public static Coords2 operator- (Coords2 a, Coords2 b) {
			Coords2 output = new Coords2();
			output.x = a.x-b.x;
			output.y = a.y-b.y;
			return output;
		}

		public static Coords2 operator+ (Coords2 a, Coords2 b) {
			Coords2 output = new Coords2();
			output.x = a.x+b.x;
			output.y = a.y+b.y;
			return output;
		}

		public Vector2 toVector2() {
			return new Vector2(x, y);
		}
	}

	private Coords2 size;
	private Coords2 bottomLeftCorner;
	private bool[,] grid;
	//private int[,] debugGrid;
	//private Vector2[] debugPath;
    
    //need to add 0.5 to x and to y to get the center of a tile
	private Vector2 deltaVec = new Vector2(0.5f, 0.5f);
	private Coords2[] refTab = {new Coords2(1,0), new Coords2(0,1), new Coords2(-1,0), new Coords2(0,-1)};

    void Start() {
    	Tilemap collision = GameObject.Find("/Grid/Collision").GetComponent<Tilemap>();
    	Tilemap translucent = GameObject.Find("/Grid/Translucent").GetComponent<Tilemap>();

    	collision.CompressBounds();
    	BoundsInt bounds = collision.cellBounds;
    	size = new Coords2(bounds.size.x, bounds.size.y);
    	bottomLeftCorner = new Coords2(bounds.position.x, bounds.position.y);
    	grid = new bool[size.x, size.y];

    	addGrid(collision);
    	addGrid(translucent);
    }

    public void addGrid(Tilemap tmap) {
    	tmap.CompressBounds();
    	BoundsInt bounds = tmap.cellBounds;
    	TileBase[] tbase = tmap.GetTilesBlock(bounds);

    	int offsetX = bounds.position.x - bottomLeftCorner.x;
    	int offsetY = bounds.position.y - bottomLeftCorner.y;

    	for (int x = 0; x < bounds.size.x; x++) {
    		for (int y = 0; y < bounds.size.y; y++) {
    			if (tbase[x + bounds.size.x * y] != null) {
    				grid[x + offsetX, y + offsetY] = true;
    			}
    		}
    	}
    }

    private Coords2 positionToTile(Vector2 position) {
    	Coords2 diff = new Coords2(position) - bottomLeftCorner;
    	return new Coords2((int)diff.x, (int)diff.y);
    }

    public Vector2 getPathSegment(Vector2 target) {
    	Coords2 start = positionToTile(transform.position);
    	Coords2 end = positionToTile(target);

    	Vector2[] path = getPath(start, end);
    	//debugPath = path;

    	int index = 0;
    	for (int i = 1; i < path.Length; i++) {
    		if (!canSee(path[i])) {
    			index = i-1;
    			break;
    		}
    	}
    	return path[index];
    }

    private Vector2[] getPath(Coords2 start, Coords2 end) {
    	int[,] scores = new int[size.x, size.y];
    	scores[start.x, start.y] = 1;

    	int iters = 0;
    	while (scores[end.x, end.y] == 0 && iters < 100) {
    		updateScores(scores);
    		iters++;
    	}
    	//debugGrid = scores;
    	if (iters >= 100) {
    		Debug.Log("Warning: pathing failed");
    	}
    	return backtrack(scores, end);
    }

    private Vector2[] backtrack(int[,] scores, Coords2 end) {
    	//TODO if we do implement A* we'll have to find another way to detect the number of tiles
    	//since scores will include distance values added to the raw number of tiles traversed
    	//making numSteps inaccurate
    	int numSteps = scores[end.x, end.y];

    	Vector2 offsetVec = new Vector2(bottomLeftCorner.x, bottomLeftCorner.y);

    	Coords2 curr = new Coords2(end.x, end.y);
    	Vector2[] result = new Vector2[numSteps];
    	result[numSteps-1] = curr.toVector2() + deltaVec + offsetVec;
    	for (int i = numSteps-2; i >= 0; i--) {
    		curr = backtrackStep(scores, curr);
    		result[i] = curr.toVector2() + deltaVec + offsetVec;
    	}
    	return result;
    }

    private Coords2 backtrackStep(int[,] scores, Coords2 vec) {
    	int targetVal = scores[vec.x, vec.y] - 1;
    	for (int i = 0; i < 4; i++) {
    		Coords2 pos = vec + refTab[i];
    		if (inBounds(pos) && scores[pos.x, pos.y] == targetVal) {
    			return pos;
    		}
    	}
    	Debug.Log("Error - backtracking failed");
    	return new Coords2();
    }

    private void updateScores(int[,] scores) {
    	//TODO make this actually do A*
    	//right now it just does floodFill lmao
        //though to be fair the maps are so small that A* wouldn't be much faster
        //maybe just rename the script and leave it as floodFill for now
    	for (int x = 0; x < size.x; x++) {
    		for (int y = 0; y < size.y; y++) {
    			if (!grid[x, y] && scores[x,y] == 0) {
    				scores[x,y] = getBestAdjacentVal(scores, new Coords2(x, y));
    			}
    		}
    	}
    }

    private int getBestAdjacentVal(int[,] scores, Coords2 vec) {
    	int best = 0;
    	for (int i = 0; i < 4; i++) {
    		Coords2 pos = vec + refTab[i];
    		if (inBounds(pos) && scores[pos.x, pos.y] > 0) {
    			if (best == 0) {
    				best = scores[pos.x, pos.y] + 1;
    			} else {
    				best = Mathf.Min(best, scores[pos.x, pos.y] + 1);
    			}
    		}
    	}
    	return best;
    }

    private bool inBounds(Coords2 vec) {
    	return vec.x >= 0 && vec.x < size.x && vec.y >= 0 && vec.y < size.y;
    }

    private bool canSee(Vector2 target) {
    	LayerMask mask = LayerMask.GetMask("Ignore Raycast", "ActivatorIgnoreRaycast");
		mask = ~mask;
    	RaycastHit2D hit = Physics2D.Raycast(transform.position, target - (Vector2)transform.position, Vector2.Distance(target, (Vector2)transform.position), mask);
    	return hit.collider == null;
    }

    // void OnDrawGizmosSelected() {
    // 	debugGrid[0,0] = 1;
    // 	debugGrid[0,1] = 2;
    // 	debugGrid[0,2] = 3;
    // 	debugGrid[0,4] = 4;
    // 	for (int x = 0; x < size.x; x++) {
    // 		for (int y = 0; y < size.y; y++) {
    // 			if (grid[x, y]) {
    // 				Gizmos.DrawCube(new Vector3(x + bottomLeftCorner.x, y + bottomLeftCorner.y, 3), new Vector3(1,1,1));
    // 			}
    // 			float scale = (float)debugGrid[x,y]/20f;
    // 			Gizmos.DrawSphere(new Vector3(x + bottomLeftCorner.x, y + bottomLeftCorner.y, 3), scale);
    // 		}
    // 	}
    // }

    // void OnDrawGizmos() {
    // 	Gizmos.DrawCube(positionToTile(GameObject.FindWithTag("Player").transform.position).toVector2() +deltaVec + bottomLeftCorner.toVector2(), Vector3.one);
    // 	Gizmos.DrawCube(positionToTile(transform.position).toVector2() +deltaVec + bottomLeftCorner.toVector2(), Vector3.one);
    	
    // 	foreach (Vector2 vec in debugPath) {
    // 		Gizmos.DrawCube(new Vector2(vec.x , vec.y ), new Vector3(1,1,1));
    // 	}
    // }
}
