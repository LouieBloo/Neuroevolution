using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour {


    public enum direction { LeftRight,UpDown};
    public direction wallDirection;

    public float moveSpeed;

    public float maxMoveDistance;

    Vector3 moveDirection;

	// Use this for initialization
	void Start () {


        //if (Random.Range(0f, 1f) <= 0.5f)
        //{
        //    wallDirection = direction.LeftRight;
        //}
        //else
        //{
        //    wallDirection = direction.UpDown;
        //}

        moveSpeed = Random.Range(0.75f,3f);

        switch (wallDirection)
        {
            case direction.LeftRight:
                StartCoroutine(leftRightMovement());
                break;
            case direction.UpDown:
                StartCoroutine(upDownMovement());
                break;
        }
        
	}
	

    IEnumerator leftRightMovement()
    {
        float diceRoll = Random.Range(0f, 1f);

        if(diceRoll <= 0.5f)
        {
            moveDirection = -transform.right;
        }
        else
        {
            moveDirection = transform.right;
        }

        float moveDistance = 0;

        while (true)
        {
            float moveStep = Time.deltaTime * moveSpeed;//how far we will move this frame
            transform.position += moveDirection * moveStep;

            moveDistance += moveStep;

            if(Mathf.Abs(moveDistance) >= maxMoveDistance)
            {
                moveDirection *= -1;
                moveDistance = 0;
            }

            yield return null;
        }
    }

    IEnumerator upDownMovement()
    {
        float diceRoll = Random.Range(0f, 1f);

        if (diceRoll <= 0.5f)
        {
            moveDirection = transform.up;
        }
        else
        {
            moveDirection = -transform.up;
        }

        float moveDistance = 0;

        while (true)
        {
            float moveStep = Time.deltaTime * moveSpeed;//how far we will move this frame
            transform.position += moveDirection * moveStep;

            moveDistance += moveStep;

            if (Mathf.Abs(moveDistance) >= maxMoveDistance)
            {
                moveDirection *= -1;
                moveDistance = 0;
            }

            yield return null;
        }
    }
}
