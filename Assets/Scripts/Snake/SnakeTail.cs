using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeTail : MonoBehaviour {

    Transform target;

    Vector3 nextMovePosition;

	// Use this for initialization
	void Start () {
		
	}

    public void setTarget(Transform target)
    {
        this.target = target;
        nextMovePosition = target.position;
    }

    public void moveStep()
    {
        transform.position = nextMovePosition;
        nextMovePosition = target.position;
    }
}
