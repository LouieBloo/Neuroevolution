using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    //public Transform

    protected List<NeuralNetwork> untestedNetworks = new List<NeuralNetwork>();
    protected List<NeuralNetwork> testedNetworks = new List<NeuralNetwork>();

    public int initialPopulation = 100;

    public int inputLayerCount;
    public int hiddenLayerCount;
    public int outputLayerCount;

    protected int generationCount = 1;
    protected int currentChromosome = 1;

    public float crossoverRate = 0.5f;
    public float mutationRate = 0.1f;
    public float mutationAmount = 0.2f;

    protected float lastAverageFitenss;
    protected float lastBestFitenss;
    protected float lastWorstFitness;

    public enum SpawnBehavior { Single,Parallel};
    public SpawnBehavior spawnBehavior;

    protected float totalTimeRunningGeneration;

    public static float timeScale;

    public Text currentChromosomeText;
    public Text generationCountText;
    public Text lastAverageFitnessText;

    System.DateTime howLong;

    void Start()
    {
    }

    public void startPressed()
    {
        untestedNetworks.Clear();
        testedNetworks.Clear();

        for (int x = 0; x < initialPopulation; x++)
        {
            untestedNetworks.Add(new NeuralNetwork(inputLayerCount, hiddenLayerCount, outputLayerCount));
        }

        startTestingGeneration();
    }

    protected virtual void startTestingGeneration()
    {
        if(spawnBehavior == SpawnBehavior.Parallel)
        {
            //initial start of testing chain
            if (untestedNetworks.Count > 0)
            {
                testNetworksParallel(untestedNetworks);
            }
        }
        else if(spawnBehavior == SpawnBehavior.Single)
        {
            //initial start of testing chain
            if (untestedNetworks.Count > 0)
            {
                testNetwork(untestedNetworks[0]);
            }
        }
    }

    protected virtual void testNetworksParallel(List<NeuralNetwork> networks)
    {

    }

    /// <summary>
    /// tell the agent to run with this network
    /// </summary>
    /// <param name="networkToTest"></param>
    protected virtual void testNetwork(NeuralNetwork networkToTest)
    {
        //GameObject agent = Instantiate(agentPrefab, agentSpawnPosition.position, Quaternion.identity);
        //agent.GetComponent<Agent>().setup(networkToTest, agentDied);
    }

    /// <summary>
    /// Whenever the agent dies, this function is called. Asses the fitness and test the remaining networks if there is any
    /// </summary>
    /// <param name="networkTested"></param>
    /// <param name="deathPosition"></param>
    protected virtual void agentDied(NeuralNetwork networkTested, Vector2 deathPosition, float timeItTookToDie, GameObject agentObject)
    {
        Destroy(agentObject);

        float fitness = distanceFitnessCalculation(deathPosition);
        //float fitness = timeBasedFitnessCalculation(timeItTookToDie);
        //Debug.Log("Agent died with fitness: " + fitness);

        networkTested.fitness = fitness;

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);

        if (untestedNetworks.Count > 0)
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
    protected virtual void  parseTestingResults()
    {
        howLong = System.DateTime.Now;
        //sort by fitness, best first
        testedNetworks = testedNetworks.OrderByDescending(x => x.fitness).ToList();

        //calculate average,best,worst fitness of this generation
        lastAverageFitenss = 0;
        lastBestFitenss = 0;
        lastWorstFitness = 999999;
        foreach(NeuralNetwork n in testedNetworks)
        {
            if(n.fitness > lastBestFitenss)//best fitness
            {
                lastBestFitenss = n.fitness;
            }
            else if(n.fitness < lastWorstFitness)//worst
            {
                lastWorstFitness = n.fitness;
            }

            lastAverageFitenss += n.fitness;//average
        }

        lastAverageFitenss /= testedNetworks.Count;

        //Debug.Log("Fitness: " + (System.DateTime.Now - howLong).TotalMilliseconds);

        //get how many networks are crossoverRate% of the total population
        int totalEliteChromosomeCount = Mathf.RoundToInt((float)testedNetworks.Count * crossoverRate);
        //Debug.Log("Tested: " + testedNetworks.Count);
        //Debug.Log("Top 10 total: " + totalEliteChromosomeCount);

        //create a new list with only our top x% of networks
        List<NeuralNetwork> topNetworks = new List<NeuralNetwork>();
        for (int x = 0; x < totalEliteChromosomeCount; x++)
        {
            topNetworks.Add(testedNetworks[x]);
        }


        List<NeuralNetwork> newChildren = new List<NeuralNetwork>();
        int amountOfBreedingNeeded = (testedNetworks.Count - totalEliteChromosomeCount) / 2;//divide by 2 as each breeding generates 2 children
        //Debug.Log("Breeding: " + amountOfBreedingNeeded);
        //Debug.Log("Before breeding: " + (System.DateTime.Now - howLong).TotalMilliseconds);
        for (int x = 0; x < amountOfBreedingNeeded; x++)
        {
            int firstChoice = Random.Range(0, topNetworks.Count);
            int secondChoice = -1;
            while (secondChoice == -1 || secondChoice == firstChoice)
            {
                secondChoice = Random.Range(0, topNetworks.Count);
            }


            newChildren.AddRange(breedCrossoverAndMutateNetworks(topNetworks[firstChoice], topNetworks[secondChoice]));
        }
        //Debug.Log("After breeding: " + (System.DateTime.Now - howLong).TotalMilliseconds);

        //keep the top10Percent from last generation
        newChildren.AddRange(topNetworks);

        resetForNextTest();

        untestedNetworks = newChildren;

        //Debug.Log("Untested: " + untestedNetworks.Count);

        startTestingGeneration();
    }

    /// <summary>
    /// Resets our variables for the next test
    /// </summary>
    protected virtual void resetForNextTest()
    {
        untestedNetworks.Clear();
        testedNetworks.Clear();

        generationCount++;
        currentChromosome = 0;

        totalTimeRunningGeneration = 0;

        updateTexts();
    }

    /// <summary>
    /// Takes in 2 nerual networks and returns 2 children that have been crossedOver
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    protected virtual List<NeuralNetwork> breedCrossoverAndMutateNetworks(NeuralNetwork a, NeuralNetwork b)
    {
        //Layer aHiddenLayer = a.getHiddenLayer().DeepClone();
        //Layer bHiddenLayer = b.getHiddenLayer().DeepClone();
        Layer aHiddenLayer = a.getHiddenLayer().copy();//better copy
        Layer bHiddenLayer = b.getHiddenLayer().copy();

        //Layer aOutputLayer = a.getOutputLayer().DeepClone();
        //Layer bOutputLayer = b.getOutputLayer().DeepClone();
        Layer aOutputLayer = a.getOutputLayer().copy();//better copy
        Layer bOutputLayer = b.getOutputLayer().copy();
        //Debug.Log("After cloning: " + (System.DateTime.Now - howLong).TotalMilliseconds);
        //crossover our layers
        crossOverLayers(aHiddenLayer, bHiddenLayer);
        crossOverLayers(aOutputLayer, bOutputLayer);
        //Debug.Log("After crossover: " + (System.DateTime.Now - howLong).TotalMilliseconds);
        //mutate layers
        mutateLayer(aHiddenLayer);
        mutateLayer(bHiddenLayer);

        mutateLayer(aOutputLayer);
        mutateLayer(bOutputLayer);
        //Debug.Log("After mutate: " + (System.DateTime.Now - howLong).TotalMilliseconds);
        NeuralNetwork firstChild = new NeuralNetwork(aHiddenLayer, aOutputLayer);
        NeuralNetwork secondChild = new NeuralNetwork(bHiddenLayer, bOutputLayer);


        return new List<NeuralNetwork>() { firstChild, secondChild };
    }

    /// <summary>
    /// Takes in 2 references to layers, crosses them over with 50% chance on each allele
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    protected virtual void crossOverLayers(Layer a, Layer b)
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
    protected virtual void mutateLayer(Layer a)
    {
        a.mutate(mutationRate, mutationAmount);
    }

    /// <summary>
    /// Fitness calculation based on distance from the goal. Returns a number from 10 to -infinity, 10 is the most bueno
    /// </summary>
    /// <param name="deathPosition"></param>
    /// <returns></returns>
    protected virtual float distanceFitnessCalculation(Vector2 deathPosition)
    {
        //float distance = Vector2.Distance(deathPosition, goalTransform.position);
        float distance = 0;

        return 10f - distance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeItTookToDie"></param>
    /// <returns></returns>
    protected virtual float timeBasedFitnessCalculation(float timeItTookToDie)
    {
        return timeItTookToDie;
    }

    protected virtual void updateTexts()
    {
        generationCountText.text = "Generation: " + generationCount;
        currentChromosomeText.text = "Chromosome: " + currentChromosome;
        lastAverageFitnessText.text = "Worst: " + lastWorstFitness + "     Average: " + lastAverageFitenss + "     Best: " + lastBestFitenss;
    }


    protected IEnumerator keepTimeRoutine()
    {
        totalTimeRunningGeneration = 0;
        while (true)
        {
            totalTimeRunningGeneration += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Modifies the simulation speed
    /// </summary>
    /// <param name="input"></param>
    public void modifyTimeScale(float input)
    {
        //Time.timeScale = input;
        timeScale = input / 100f;
        Debug.Log(timeScale);
    }
}
