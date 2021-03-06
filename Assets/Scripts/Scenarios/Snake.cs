﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : NetworkManager
{
    public GameObject snakeGamePrefab;
    GameObject activeSnakeGame;

    public static float frameDelay = 0.01f;

    void Start () {
        Application.runInBackground = true;
	}

    protected override void testNetwork(NeuralNetwork networkToTest)
    {
        activeSnakeGame = Instantiate(snakeGamePrefab);
        activeSnakeGame.GetComponent<SnakeGame>().setup(networkToTest,networkFinishedTesting);
    }

    void networkFinishedTesting(NeuralNetwork networkTested, int foodCount,float timeSurvived)
    {
        //Debug.Log("Dead: " + foodCount);
        networkTested.setFitness(fitnessFunc(foodCount, timeSurvived));

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        Destroy(activeSnakeGame.gameObject);

        if (untestedNetworks.Count > 0)
        {
            currentChromosome++;
            updateTexts();
            testNetwork(untestedNetworks[0]);
        }
        else
        {
            parseTestingResults();
        }
    }

    float fitnessFunc(int foodEaten, float timeSurvived)
    {
        //return Mathf.Exp(foodEaten) + (timeSurvived / 10f);
        //return timeSurvived;
        return foodEaten;
    }

    public void changeFrameTime(float input)
    {
        frameDelay = input;
    }
}
