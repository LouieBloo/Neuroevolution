using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lander : NetworkManager {


    public GameObject landerShipPrefab;
    public Transform target;

    public List<Transform> spawnPoints;

    LanderShip[] allShips;

    void Start()
    {
        allShips = new LanderShip[100];
        for(int x = 0; x < 100; x++)
        {
            GameObject newLander = Instantiate(landerShipPrefab);
            allShips[x] = newLander.GetComponent<LanderShip>();
        }
    }

    protected override void testNetwork(NeuralNetwork networkToTest)
    {
        //activeSnakeGame = Instantiate(snakeGamePrefab);
        //activeSnakeGame.GetComponent<SnakeGame>().setup(networkToTest, networkFinishedTesting);
        GameObject newLander = Instantiate(landerShipPrefab);
       // newLander.GetComponent<LanderShip>().startGame(target, networkToTest, networkFinishedTesting);
    }

    protected override void testNetworksParallel(List<NeuralNetwork> networks)
    {
        int spawn = Random.Range(0, spawnPoints.Count);
        for (int x = 0; x < 100;x++)
        {
            allShips[x].startGame(target, networks[x], networkFinishedTesting,spawnPoints[spawn]);
        }
    }

    void networkFinishedTesting(NeuralNetwork networkTested, float aStarDistance,bool isInLOS,float closestDistanceToGoal)
    {
        //Debug.Log("Dead: " + foodCount);

        networkTested.fitness = fitnessFunc(aStarDistance, isInLOS,closestDistanceToGoal);

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        //Destroy(activeSnakeGame.gameObject);
        //Debug.Log(untestedNetworks.Count);
        if (untestedNetworks.Count > 0)
        {
            currentChromosome++;
            updateTexts();
            //testNetwork(untestedNetworks[0]);
        }
        else
        {
            StartCoroutine(waitAFrame());
        }
    }

    //let our landers have a frame to reset
    IEnumerator waitAFrame()
    {
        yield return null;
        parseTestingResults();
    }

    float fitnessFunc(float aStarDistance, bool isInLOS,float closestDistanceToGoal)
    {
        return 100f - aStarDistance;
        //float distanceFromGoal = (target.position - finalPosition).magnitude;
        //if (isInLOS)
        //{
        //    distanceFromGoal += 100- distanceFromGoal;
        //}
        //else
        //{
        //    distanceFromGoal += 100 - distanceFromGoal;
        //}

        //distanceFromGoal += 100 - closestDistanceToGoal;

        //return distanceFromGoal;
    }
}
