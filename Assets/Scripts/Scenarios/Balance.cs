using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balance : NetworkManager
{
    public GameObject balancePrefab;

    List<BalanceObjects> allBalanceObjects = new List<BalanceObjects>();


    void Start()
    {
        for(int x = 0; x < initialPopulation; x++)
        {
            GameObject temp = Instantiate(balancePrefab, Vector2.zero, Quaternion.identity);

            BalanceObjects newBalanceObject = temp.GetComponent<BalanceObjects>();

            foreach (BalanceObjects bo in allBalanceObjects)
            {
                newBalanceObject.setNoCollisionWithBalanceObject(bo);
            }

            allBalanceObjects.Add(newBalanceObject);
        }
    }

    protected override void testNetworksParallel(List<NeuralNetwork> networks)
    {
        for(int x =0; x<allBalanceObjects.Count && x < networks.Count;x++)
        {
            allBalanceObjects[x].setup(networks[x], testCompleted);
        }
    }

    protected override void testNetwork(NeuralNetwork networkToTest)
    {
        Debug.Log("Not implemented");
    }

    void testCompleted(NeuralNetwork networkTested,float timer)
    {
        float fitness = timer;
        networkTested.setFitness(fitness);
        //Debug.Log(untestedNetworks.Count + " :TestComplete");
        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        if (untestedNetworks.Count > 0)
        {
            currentChromosome++;
            //updateTexts();
        }
        else
        {
            resetObjects();
            parseTestingResults();
        }
    }

    void resetObjects()
    {
        foreach(BalanceObjects b in allBalanceObjects)
        {
            b.reset();
        }
    }

}
