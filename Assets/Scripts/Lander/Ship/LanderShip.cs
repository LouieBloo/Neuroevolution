using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderShip : MonoBehaviour {

    public Rigidbody2D rigidBody;

    public float middleThrust = 0f;
    public float leftThrust = 0f;
    public float rightThrust = 0f;

    public float test = 0f;

    public LanderMovement engine;

    public EdgeDetector edgeDetector;

    NeuralNetwork brain;
    Action<NeuralNetwork,Vector2,bool> deathCallback;
    IEnumerator mainLoop;

    void Start()
    {
        //StartCoroutine(aliveLoop());
    }

    void Update()
    {
        //move();
    }

    public void startGame(Transform target, NeuralNetwork networkToTest,Action<NeuralNetwork,Vector2,bool> callback)
    {
        this.deathCallback = callback;
        this.edgeDetector.target = target;
        this.brain = networkToTest;

        //start game loop
        mainLoop = aliveLoop();
        StartCoroutine(mainLoop);
    }

    IEnumerator aliveLoop()
    {
        yield return null;//give one frame for our children to load

        List<float> inputs;
        List<float> outputs;
        while (true)
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
        //los returns 1
        returnInputs.Add(edgeDetector.getLOS());
        //distance returns 2
        returnInputs.AddRange(edgeDetector.getDistancesToGoal());

        return returnInputs;
    }

    public void move()
    {
        engine.fireLeftThruster(leftThrust);
        engine.fireRightThruster(rightThrust);
        engine.thrust(middleThrust);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col && col.gameObject.CompareTag("Wall"))
        {
            StopCoroutine(mainLoop);
            deathCallback(brain, this.transform.position,edgeDetector.getLOS() ==1f);
            Destroy(this.gameObject);
        }
    }



}
