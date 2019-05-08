using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public Transform goalTransform;

    public GameObject agentPrefab;

    public Transform agentSpawnPosition;

    List<NeuralNetwork> untestedNetworks = new List<NeuralNetwork>();
    List<NeuralNetwork> testedNetworks = new List<NeuralNetwork>();

    public int initialPopulation = 5;

    int inputLayerCount = 4;
    int hiddenLayerCount = 6;
    int outputLayerCount = 4;

    int generationCount = 1;
    int currentChromosome = 1;

    float crossoverRate = 0.5f;
    float mutationRate = 0.1f;
    float mutationAmount = 0.2f;

    public Text currentChromosomeText;
    public Text generationCountText;

    void Start()
    {
    }

    public void startPressed()
    {
        untestedNetworks.Clear();
        testedNetworks.Clear();

        for (int x = 0;x < initialPopulation; x++)
        {
            untestedNetworks.Add(new NeuralNetwork(inputLayerCount, hiddenLayerCount, outputLayerCount));
        }

        //initial start of testing chain
        if (untestedNetworks.Count > 0)
        {
            foreach(NeuralNetwork n in untestedNetworks)
            {
                testNetwork(n);
            }
        }
    }

    /// <summary>
    /// tell the agent to run with this network
    /// </summary>
    /// <param name="networkToTest"></param>
    void testNetwork(NeuralNetwork networkToTest)
    {
        GameObject agent = Instantiate(agentPrefab, agentSpawnPosition.position, Quaternion.identity);
        agent.GetComponent<Agent>().setup(networkToTest, agentDied);
    }

    /// <summary>
    /// Whenever the agent dies, this function is called. Asses the fitness and test the remaining networks if there is any
    /// </summary>
    /// <param name="networkTested"></param>
    /// <param name="deathPosition"></param>
    public void agentDied(NeuralNetwork networkTested, Vector2 deathPosition,float timeItTookToDie,GameObject agentObject)
    {
        Destroy(agentObject);

        float fitness = distanceFitnessCalculation(deathPosition);
        //float fitness = timeBasedFitnessCalculation(timeItTookToDie);
        //Debug.Log("Agent died with fitness: " + fitness);

        networkTested.setFitness(fitness);

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        if(untestedNetworks.Count > 0)
        {
            //currentChromosome++;
            //updateTexts();

            //testNetwork(untestedNetworks[0]);
        }
        else
        {
            parseTestingResults();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    void parseTestingResults()
    {
        //sort by fitness, best first
        testedNetworks = testedNetworks.OrderByDescending(x => x.getFitness()).ToList();

        //get how many networks are 10% of the total population
        int top10PercentCount = Mathf.RoundToInt ((float)testedNetworks.Count * crossoverRate);

        //create a new list with only our top 10% of networks
        List<NeuralNetwork> top10Networks = new List<NeuralNetwork>();
        for(int x = 0;x < top10PercentCount; x++)
        {
            top10Networks.Add(testedNetworks[x]);
        }


        List<NeuralNetwork> newChildren = new List<NeuralNetwork>();
        int amountOfBreedingNeeded = (testedNetworks.Count - top10PercentCount) / 2;//divide by 2 as each breeding generates 2 children

        for(int x = 0;x < amountOfBreedingNeeded; x++)
        {
            int firstChoice = Random.Range(0, top10Networks.Count);
            int secondChoice = -1;
            while(secondChoice == -1 || secondChoice == firstChoice)
            {
                secondChoice = Random.Range(0, top10Networks.Count);
            }


            newChildren.AddRange(breedCrossoverAndMutateNetworks(top10Networks[firstChoice], top10Networks[secondChoice]));
        }


        //keep the top10Percent from last generation
        newChildren.AddRange(top10Networks);

        resetForNextTest();

        untestedNetworks = newChildren;

        foreach (NeuralNetwork n in untestedNetworks)
        {
            testNetwork(n);
        }
    }

    /// <summary>
    /// Resets our variables for the next test
    /// </summary>
    void resetForNextTest()
    {
        untestedNetworks.Clear();
        testedNetworks.Clear();

        generationCount++;
        currentChromosome = 0;

        updateTexts();
    }

    /// <summary>
    /// Takes in 2 nerual networks and returns 2 children that have been crossedOver
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    List<NeuralNetwork> breedCrossoverAndMutateNetworks(NeuralNetwork a, NeuralNetwork b)
    {
        Layer aHiddenLayer = a.getHiddenLayer().DeepClone();
        Layer bHiddenLayer = b.getHiddenLayer().DeepClone();

        Layer aOutputLayer = a.getOutputLayer().DeepClone();
        Layer bOutputLayer = b.getOutputLayer().DeepClone();

        //crossover our layers
        crossOverLayers(aHiddenLayer, bHiddenLayer);
        crossOverLayers(aOutputLayer, bOutputLayer);

        //mutate layers
        mutateLayer(aHiddenLayer);
        mutateLayer(bHiddenLayer);

        mutateLayer(aOutputLayer);
        mutateLayer(bOutputLayer);

        NeuralNetwork firstChild = new NeuralNetwork(aHiddenLayer, aOutputLayer);
        NeuralNetwork secondChild = new NeuralNetwork(bHiddenLayer, bOutputLayer);


        return new List<NeuralNetwork>() { firstChild, secondChild };
    }

    /// <summary>
    /// Takes in 2 references to layers, crosses them over with 50% chance on each allele
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    void crossOverLayers(Layer a, Layer b)
    {
        for (int x = 0; x < a.nodeCount && x < b.nodeCount; x++)
        {
            float coinFlip = Random.Range(0f, 1f);
            if (coinFlip <= 0.5f)//swap nodes at index
            {
                Node savedNode = a.getNodes[x];
                a.getNodes[x] = b.getNodes[x];
                b.getNodes[x] = savedNode;
            }
            else//dont sawp
            {
            }
        }
    }

    /// <summary>
    /// Tells the layer to mutate itself based on our parameters
    /// </summary>
    /// <param name="a"></param>
    void mutateLayer(Layer a)
    {
        a.mutate(mutationRate, mutationAmount);
    }

    /// <summary>
    /// Fitness calculation based on distance from the goal. Returns a number from 10 to -infinity, 10 is the most bueno
    /// </summary>
    /// <param name="deathPosition"></param>
    /// <returns></returns>
    float distanceFitnessCalculation(Vector2 deathPosition)
    {
        float distance = Vector2.Distance(deathPosition, goalTransform.position);

        return 10f - distance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeItTookToDie"></param>
    /// <returns></returns>
    float timeBasedFitnessCalculation(float timeItTookToDie)
    {
        return timeItTookToDie;
    }

    void updateTexts()
    {
        generationCountText.text = "Generation: " + generationCount;
        currentChromosomeText.text = "Chromosome: " + currentChromosome;
    }

    /// <summary>
    /// Modifies the simulation speed
    /// </summary>
    /// <param name="input"></param>
    public void modifyTimeScale(float input)
    {
        Time.timeScale = input;
    }
}
