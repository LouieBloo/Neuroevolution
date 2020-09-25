using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DuelGame : NetworkManager
{


    public GameObject duelerPrefab;

    public List<Transform> spawnPoints;

    public float timeUntilDeathCircle;
    public static float mapSize = 10f;

    Dueler[] duelers;
    int amountOfDuelers = 2;
    int duelerCounter = 0;

    float timer = 0;
    float timeAliveDecreaser = 1000;


    public DeathCircle deathCircle;
    bool runningSimulation = false;

    private static System.Random rng = new System.Random();

    void Start()
    {
        Application.runInBackground = true;

        duelers = new Dueler[amountOfDuelers];
        for (int x = 0; x < amountOfDuelers; x++)
        {
            GameObject dueler = Instantiate(duelerPrefab);
            duelers[x] = dueler.GetComponent<Dueler>();
        }

        
    }

    void Update()
    {
        if (runningSimulation)
        {
            this.timer += Time.deltaTime;
            if (this.timer >= timeUntilDeathCircle && !deathCircle.running)
            {
                deathCircle.startGrowing(Vector2.zero);
            }
        }
    }

    

    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    protected override void testNetworksParallel(List<NeuralNetwork> networks)
    {
        Shuffle(untestedNetworks);
        deployShips();
    }

    void deployShips()
    {
        this.timer = 0;
        this.duelerCounter = 0;
        this.timeAliveDecreaser--;
        this.runningSimulation = true;

        for (int x = 0; x < amountOfDuelers; x++)
        {
            int target = x + 1 >= amountOfDuelers ? 0 : x+1;

            Vector2 spawnPoint;
            if(x == 0)
            {
                spawnPoint = new Vector2(UnityEngine.Random.Range(-mapSize+1,0), UnityEngine.Random.Range(-mapSize+1, mapSize-1));
            }
            else
            {
                spawnPoint = new Vector2(UnityEngine.Random.Range(0,mapSize-1), UnityEngine.Random.Range(-mapSize+1, mapSize-1));
            }
            //Debug.Log(x + " " + networks.Count + " " + target + " " + duelerIndex);
            duelers[x].start(spawnPoint, untestedNetworks[x], networkFinishedTesting, x,duelers[target].gameObject.transform);
        }

    }

    void networkFinishedTesting(NeuralNetwork networkTested, bool won,bool wasShot,int killCount)
    {
        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);
        duelerCounter++;

        
        // 1 left dueler that didnt get the win by a shot, alert them they won
        if(duelerCounter == amountOfDuelers - 1 )
        {
            networkTested.setFitness(fitnessFunc(timer, won, wasShot, killCount));
            //if (wasShot)
            //{
                foreach (Dueler d in duelers)
                {
                    if (d.alive)
                    {
                        d.win();
                    }
                }
           // }
        }
        else if (duelerCounter == amountOfDuelers)
        {
            networkTested.setFitness(fitnessFunc(timer, true, wasShot, killCount));

            this.runningSimulation = false;
            this.deathCircle.stopGrowing();
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (GameObject g in bullets)
            {
                Destroy(g);
            }

            if (untestedNetworks.Count > 0)
            {
                currentChromosome += amountOfDuelers;

                updateTexts();
                StartCoroutine(goNextOrStop(testedNetworks));
            }
            else
            {
                parseTestingResults();
            }
        }
        else
        {
            networkTested.setFitness(fitnessFunc(timer, won, wasShot, killCount));
        }
        
    }

    //let our landers have a frame to reset
    IEnumerator goNextOrStop(List<NeuralNetwork> networks)
    {
        yield return null;

        this.deployShips();
    }

    float fitnessFunc(float aliveTime, bool won, bool wasShot, int killCount)
    {
        float killCountBounty = 300f * killCount;
       
        if (won && killCount < 1)
        {
            return 0; 
        }else if (won)
        {
            return killCountBounty-aliveTime;
        }

        return 0;
    }
}
