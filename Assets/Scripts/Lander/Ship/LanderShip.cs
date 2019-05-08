using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderShip : MonoBehaviour {

    public Rigidbody2D rigidBody;

    public float middleThrust = 0f;
    public float leftThrust = 0f;
    public float rightThrust = 0f;
    public float gravityScale = 0.163f;
    public float minimumMovementTimeUntilDead = 2f;
    public float minimumMovementDistanceUntilDead = 0.85f;
    float lastSpeed = 0;
    float movementTimer = 0;

    public LanderMovement engine;
    public Seeker seeker;

    public EdgeDetector edgeDetector;
    public bool alive = false;

    Transform target;
    Vector3 startLocation;

    NeuralNetwork brain;
    Action<NeuralNetwork,float,bool,float> deathCallback;
    IEnumerator mainLoop;

    int test = 0;

    void Start()
    {
        //StartCoroutine(aliveLoop());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            engine.thrust(middleThrust);
        }
        if (Input.GetKey(KeyCode.D))
        {
            engine.fireLeftThruster(leftThrust);
        }
        if (Input.GetKey(KeyCode.A))
        {
            engine.fireRightThruster(rightThrust);
        }
    }


    public void startGame(Transform target, NeuralNetwork networkToTest,Action<NeuralNetwork,float,bool,float> callback,Transform startLocation)
    {
        this.gameObject.SetActive(true);
        this.startLocation = startLocation.position;
        this.transform.position = startLocation.position;
        this.deathCallback = callback;
        this.target = target;
        this.edgeDetector.target = target;
        this.brain = networkToTest;

        this.lastSpeed = 0;
        this.movementTimer = 0;

        this.rigidBody.gravityScale = gravityScale;
        this.alive = true;


        //start game loop
        mainLoop = aliveLoop();
        StartCoroutine(mainLoop);
    }

    IEnumerator aliveLoop()
    {
        List<float> inputs;
        List<float> outputs;
        while (alive)
        {
            movementTimer += Time.deltaTime;

            if(movementTimer >= minimumMovementTimeUntilDead)
            {
                //check if we haven't moved a minimum distance yet
                if(distanceFromStart() <= minimumMovementDistanceUntilDead)
                {
                    dieFromWall();
                }
                else
                {
                    movementTimer = -99999f;//jank but works so we dont have to check this everytime
                }
            }
            inputs = generateNetworkInputs();
            outputs = brain.feedInputs(inputs);

            leftThrust = outputs[0];
            rightThrust = outputs[1];
            middleThrust = outputs[2];

            move();

            yield return null;    
        }
    }

    public List<float> generateNetworkInputs()
    {
        //edge returns 8
        List<float> returnInputs = edgeDetector.getEdges();
        //add currentRotation
        returnInputs.Add(transform.rotation.z);
        //los returns 1
        //returnInputs.Add(edgeDetector.getLOS());
        //distance returns 2
        returnInputs.AddRange(edgeDetector.getXYDistancesToGoal());

        return returnInputs;
    }

    public void move()
    {
        engine.fireLeftThruster(leftThrust);
        engine.fireRightThruster(rightThrust);
        engine.thrust(middleThrust);
    }

    void pathCalculated(Path p)
    {
        if (p.error)
        {
            Debug.Log("ERROR finding path!");
        }
        else
        {
            float aStarDistance = p.GetTotalLength();

            resetShip();

            deathCallback(brain, aStarDistance, edgeDetector.getLOS() == 1f, lastSpeed);

            this.gameObject.SetActive(false);
        }
    }

    void resetShip()
    {
        this.transform.position = new Vector2(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f));
        this.transform.localRotation = Quaternion.identity;

        this.engine.refill();
        edgeDetector.resetClosestDistance();
    }

    void stopShip()
    {
        StopCoroutine(mainLoop);

        this.rigidBody.velocity = Vector2.zero;
        this.rigidBody.angularVelocity = 0;
        this.rigidBody.gravityScale = 0;
        this.alive = false;
    }

    float getSpeed()
    {
        return this.rigidBody.velocity.magnitude;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col && col.gameObject.CompareTag("Wall"))
        {
            if (alive)
            {
                dieFromWall();
            }
        }else if(col && col.gameObject.CompareTag("Target"))
        {
            if (alive)
            {
                dieFromTarget();
            }
        }
    }

    void dieFromWall()
    {
        lastSpeed = getSpeed();
        stopShip();
        seeker.StartPath(transform.position, target.position, pathCalculated);
    }

    void dieFromTarget()
    {
        lastSpeed = getSpeed();
        deathCallback(brain, 0, edgeDetector.getLOS() == 1f, lastSpeed);
        stopShip();
        resetShip();
    }

    float distanceFromStart()
    {
        return (transform.position - startLocation).magnitude;
    }


}
