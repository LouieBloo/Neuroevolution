using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour {

    public GameObject tailPrefab;

    Vector3 moveDirection;
    Vector3 newDestination;

    public enum MovingDirection { Left, Right, Up, Down,Unknown }
    MovingDirection lastMoveDirection = MovingDirection.Unknown;
    
    public List<SnakeTail> tail = new List<SnakeTail>();

	void Start () {
        moveDirection = transform.right;
    }
	
	public void moveStep(MovingDirection directionToMove){

        if (directionToMove == MovingDirection.Left && (lastMoveDirection != MovingDirection.Right))
        {
            moveDirection = -transform.right;
        }
        if (directionToMove == MovingDirection.Right && (lastMoveDirection != MovingDirection.Left))
        {
            moveDirection = transform.right;
        }
        if (directionToMove == MovingDirection.Up && (lastMoveDirection != MovingDirection.Down))
        {
            moveDirection = transform.up;
        }
        if (directionToMove == MovingDirection.Down && (lastMoveDirection != MovingDirection.Up))
        {
            moveDirection = -transform.up;
        }
        //if (Input.GetKey(KeyCode.A))
        //{
        //    if (lastMoveDirection != MovingDirection.Right || tail.Count == 0)
        //    {
        //        moveDirection = -transform.right;
        //    }
        //}
        //else if (Input.GetKey(KeyCode.D))
        //{
        //    if (lastMoveDirection != MovingDirection.Left || tail.Count == 0)
        //    {
        //        moveDirection = transform.right;
        //    }
        //}
        //else if (Input.GetKey(KeyCode.W))
        //{
        //    if (lastMoveDirection != MovingDirection.Down || tail.Count == 0)
        //    {
        //        moveDirection = transform.up;
        //    }
        //}
        //else if (Input.GetKey(KeyCode.S))
        //{
        //    if (lastMoveDirection != MovingDirection.Up || tail.Count == 0)
        //    {
        //        moveDirection = -transform.up;
        //    }
        //}

        newDestination = transform.position + moveDirection;

        lastMoveDirection = determineDirection(newDestination, transform.position);

        transform.position = newDestination;

        //move all tails
        foreach(SnakeTail st in tail)
        {
            st.moveStep();
        }
	}

    public void addToTail()
    {
        if(tail.Count < 1)
        {
            tail.Add(createTailObject(nextPosition(transform.position, lastMoveDirection)));
            tail[0].setTarget(this.transform);
        }
        else if(tail.Count < 2)
        {
            Vector2 spawnPosition = tail[0].transform.position;

            MovingDirection moveDirection = determineDirection(transform.position, spawnPosition);

            tail.Add(createTailObject(nextPosition(spawnPosition, moveDirection)));
            tail[tail.Count-1].setTarget(tail[tail.Count - 2].transform);
        }
        else
        {
            Vector2 spawnPosition = tail[tail.Count-1].transform.position;

            MovingDirection moveDirection = determineDirection(tail[tail.Count - 2].transform.position, spawnPosition);

            tail.Add(createTailObject(nextPosition(spawnPosition, moveDirection)));
            tail[tail.Count - 1].setTarget(tail[tail.Count - 2].transform);
        }
        //GameObject tail = Instantiate(tailPrefab)
    }

    SnakeTail createTailObject(Vector2 spawnPoint)
    {
        GameObject temp = Instantiate(tailPrefab, spawnPoint, Quaternion.identity);
        //temp.transform.SetParent(this.transform, false);
        return temp.GetComponent<SnakeTail>();
    }


    Vector2 nextPosition(Vector2 position, MovingDirection direction)
    {
        switch(direction)
        {
            case MovingDirection.Left://get pos to right of point
                return position + Vector2.right;
            case MovingDirection.Right://get pos to left of point
                return position + Vector2.left;
            case MovingDirection.Up://get pos to down of point
                return position + Vector2.down;
            case MovingDirection.Down://get pos to up of point
                return position + Vector2.up;
            case MovingDirection.Unknown:
                Debug.Log("AHHHHHHHHHHHHH");
                break;
        }

        return Vector2.zero;
    }

    MovingDirection determineDirection(Vector2 currentPos, Vector2 previousPos)
    {
        if(currentPos.x < previousPos.x)
        {
            return MovingDirection.Left;
        }
        else if (currentPos.x > previousPos.x)
        {
            return MovingDirection.Right;
        }
        else if (currentPos.y < previousPos.y)
        {
            return MovingDirection.Down;
        }
        else if (currentPos.y > previousPos.y)
        {
            return MovingDirection.Up;
        }

        return MovingDirection.Unknown;
    }

    public void die()
    {
        foreach(SnakeTail t in tail)
        {
            Destroy(t.gameObject);
        }
    }
}
