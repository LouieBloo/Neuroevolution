using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lander : NetworkManager {


    public GameObject landerShipPrefab;
    public Transform target;

    protected override void testNetwork(NeuralNetwork networkToTest)
    {
        //activeSnakeGame = Instantiate(snakeGamePrefab);
        //activeSnakeGame.GetComponent<SnakeGame>().setup(networkToTest, networkFinishedTesting);
        GameObject newLander = Instantiate(landerShipPrefab);
        newLander.GetComponent<LanderShip>().startGame(target, networkToTest, networkFinishedTesting);
    }

    void networkFinishedTesting(NeuralNetwork networkTested, Vector2 position,bool isInLOS)
    {
        //Debug.Log("Dead: " + foodCount);
        networkTested.fitness = fitnessFunc(position, isInLOS);

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        //Destroy(activeSnakeGame.gameObject);

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

    float fitnessFunc(Vector3 finalPosition,bool isInLOS)
    {
        float distanceFromGoal = -Mathf.Abs((target.position - finalPosition).magnitude);
        if (isInLOS)
        {
            distanceFromGoal += 25f;
        }
        return distanceFromGoal;
    }
}
