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


    public LanderMovement engine;
    public Seeker seeker;

    public EdgeDetector edgeDetector;
    public bool alive = false;

    Transform target;

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
        this.transform.position = startLocation.position;
        this.deathCallback = callback;
        this.target = target;
        this.edgeDetector.target = target;
        this.brain = networkToTest;

        //start game loop
        mainLoop = aliveLoop();
        StartCoroutine(mainLoop);
    }

    IEnumerator aliveLoop()
    {
        this.rigidBody.gravityScale = 1;
        alive = true;

        List<float> inputs;
        List<float> outputs;
        while (alive)
        {

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
        returnInputs.Add(edgeDetector.getLOS());
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

            this.transform.position = new Vector2(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f));
            this.transform.localRotation = Quaternion.identity;

            this.engine.refill();
            edgeDetector.resetClosestDistance();

            deathCallback(brain, aStarDistance, edgeDetector.getLOS() == 1f, edgeDetector.getClosestDitanceToGoal());

            this.gameObject.SetActive(false);
        }
    }

    void stopShip()
    {
        StopCoroutine(mainLoop);

        this.rigidBody.velocity = Vector2.zero;
        this.rigidBody.angularVelocity = 0;
        this.rigidBody.gravityScale = 0;
        this.alive = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col && col.gameObject.CompareTag("Wall"))
        {
            if (alive)
            {
                stopShip();
                seeker.StartPath(transform.position, target.position, pathCalculated);
            }
        }
    }



}
