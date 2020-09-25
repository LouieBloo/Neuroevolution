using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class Agent : MonoBehaviour {

    public float moveSpeed = 1;
    public float distanceToBeConsideredStuck;

    float timer;

    float deathTimer = 10;

    //Neural Network
    NeuralNetwork network;
    List<float> networkInputs = new List<float>();//inputs we are going to give to the neural network each frame

    Vector2 nextTopPosition;
    Vector2 nextRightPosition;
    Vector2 nextBottomPosition;
    Vector2 nextLeftPosition;

    Action<NeuralNetwork,Vector2,float,GameObject> callbackWhenDead;

    Vector2 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    public void setup(NeuralNetwork network,Action<NeuralNetwork, Vector2,float,GameObject> callbackWhenDead)
    {
        this.network = network;
        this.callbackWhenDead = callbackWhenDead;


        StartCoroutine(moveRoutine());
        StartCoroutine(checkIfStuckRoutine());
        StartCoroutine(keepTimeRoutine());
    }


    IEnumerator moveRoutine()
    {
        yield return null;//we need this here so all of the agents are spawned before we start

        while(!checkIfPointTouchesWallOrGoal(transform.position))
        {
            float moveStep = Time.deltaTime * moveSpeed;//how far we will move this frame

            ////check if wall is to our top
            //nextTopPosition = transform.position + (transform.up * moveStep);//vector2 of our left side
            //float isWallTop = checkIfPointTouchesWall(nextTopPosition) ? 1f : 0f;

            ////check if wall is to our right
            //nextRightPosition = transform.position + (transform.right * moveStep);//vector2 of our left side
            //float isWallRight = checkIfPointTouchesWall(nextRightPosition) ? 1f : 0f;

            ////check if wall is to our bottom
            //nextBottomPosition = transform.position + (-transform.up * moveStep);//vector2 of our left side
            //float isWallBottom = checkIfPointTouchesWall(nextBottomPosition) ? 1f : 0f;

            ////check if wall is to our left
            //nextLeftPosition = transform.position + (-transform.right * moveStep);
            //float isWallLeft = checkIfPointTouchesWall(nextLeftPosition) ? 1f : 0f;

            //All raycasts
            //Get the distance to the nearest wall in front of us
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up);
            float topDistanceToWall = normalizePoint(hit);

            //right of us
            hit = Physics2D.Raycast(transform.position, Vector2.right);
            float rightDistanceToWall = normalizePoint(hit);

            //below us
            hit = Physics2D.Raycast(transform.position, Vector2.down);
            float bottomDistanceToWall = normalizePoint(hit);

            //right of us
            hit = Physics2D.Raycast(transform.position, Vector2.left);
            float leftDistanceToWall = normalizePoint(hit);

            //create network inputs
            networkInputs = new List<float>() { topDistanceToWall, rightDistanceToWall, bottomDistanceToWall, leftDistanceToWall };

            //run against network
            List<float> networkOutput = network.feedInputs(networkInputs);

            if (networkOutput == null || networkOutput.Count != 4) { Debug.Log("Its broken"); yield break; }

            //find highest index, aka our chosen direction
            int selectedIndex = highestIndex(networkOutput);

            Vector3 moveDirection = transform.up;
            switch (selectedIndex)
            {
                case 0:
                    moveDirection = transform.up;
                    break;
                case 1:
                    moveDirection = transform.right;
                    break;
                case 2:
                    moveDirection = -transform.up;
                    break;
                case 3:
                    moveDirection = -transform.right;
                    break;
            }


            transform.position += moveDirection * moveStep;

            yield return null;
        }

        
        callbackWhenDead(network, transform.position,timer,this.gameObject);
    }

    IEnumerator checkIfStuckRoutine()
    {
        Vector2 lastPosition;
        while(true)
        {
            lastPosition = transform.position;
            yield return new WaitForSeconds(2.5f);

            if(Vector2.Distance(lastPosition,transform.position) <= distanceToBeConsideredStuck)
            {
                callbackWhenDead(network, transform.position,timer,this.gameObject);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator keepTimeRoutine()
    {
        timer = 0;
        while(true)
        {
            timer += Time.deltaTime;
            //if(timer >= deathTimer)
            //{
            //    callbackWhenDead(network, transform.position, timer, this.gameObject);
            //    yield break;
            //}
            yield return null;
        }
    }

    public void reset()
    {
        transform.position = startPos;

        StopAllCoroutines();
    }

    //5 is really close to us, -5 is max distance away
    float normalizePoint(RaycastHit2D hit)
    {
        if(!hit)
        {
            return -5f;
        }

        float distance = hit.distance > 2f ? 2f : hit.distance;
        return 5f - (distance * 5f);
    }

    bool checkIfPointTouchesWallOrGoal(Vector2 point)
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(point, Vector2.zero);
        foreach (RaycastHit2D h in hit)
        {
            if(h.transform.CompareTag("Wall") || h.transform.CompareTag("Goal"))
            {
                return true;
            }
        }

        return false;
    }

    int highestIndex(List<float> inputs)
    {
        float highest = inputs[0];
        int index = 0;

        for(int x =0;x < inputs.Count;x++)
        {
            if(inputs[x] > highest)
            {
                highest = inputs[x];
                index = x;
            }
        }

        return index;
    }
}
