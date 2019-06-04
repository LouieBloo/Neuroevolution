using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lander : NetworkManager {


    public GameObject landerShipPrefab;
    public Transform target;

    public List<Transform> spawnPoints;

    public float timeUntilDeathCircle;

    //keeps track of how many spawnPoints we have tested;
    int spawnPointTracker = 0;

    LanderShip[] allShips;

    float timer = 0;
    float timeAliveDecreaser = 1000;

    public DeathCircle deathCircle;
    bool runningSimulation = false;

    void Start()
    {
        Application.runInBackground = true;

        allShips = new LanderShip[100];
        for(int x = 0; x < 100; x++)
        {
            GameObject newLander = Instantiate(landerShipPrefab);
            allShips[x] = newLander.GetComponent<LanderShip>();
        }
    }

    void Update()
    {
        if (runningSimulation)
        {
            this.timer += Time.deltaTime;
            if(this.timer >= timeUntilDeathCircle && !deathCircle.running)
            {
                deathCircle.startGrowing(spawnPoints[spawnPointTracker].position);
            }
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
        this.spawnPointTracker = 0;

        deployShips(networks, spawnPoints[spawnPointTracker]);
    }

    void deployShips(List<NeuralNetwork> networks,Transform spawnPosition)
    {
        this.timer = 0;
        this.timeAliveDecreaser--;
        this.runningSimulation = true;

        for (int x = 0; x < 100; x++)
        {
            allShips[x].startGame(target, networks[x], networkFinishedTesting, spawnPosition);
        }
        
    }

    void networkFinishedTesting(NeuralNetwork networkTested, float aStarDistance,bool isInLOS,float speed)
    {
        //Debug.Log("Dead: " + foodCount);

        networkTested.setFitness(fitnessFunc(aStarDistance, isInLOS,speed));

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
            this.runningSimulation = false;
            this.deathCircle.stopGrowing();
            StartCoroutine(goNextOrStop(testedNetworks));
        }
    }

    //let our landers have a frame to reset
    IEnumerator goNextOrStop(List<NeuralNetwork> networks)
    {
        yield return null;

        spawnPointTracker++;
        //check if its time for the network manager to kill, breed, and mutate our network
        if(spawnPointTracker >= spawnPoints.Count)
        {
            parseTestingResults();
        }
        else
        {
            this.deployShips(networks, spawnPoints[spawnPointTracker]);
            this.resetSoft(networks);
        }
    }

    float fitnessFunc(float aStarDistance, bool isInLOS,float speed)
    {
        float timeAmount = timeAliveDecreaser > 0 ? this.timer / (1f + (1 / timeAliveDecreaser)) : 0;
        if(aStarDistance <= 0)
        {
            return 350f + (75f - speed) + timeAmount;
        }
        return 350f - aStarDistance - speed + timeAmount;
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
