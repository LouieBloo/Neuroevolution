using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.IO;

public class NetworkManager : MonoBehaviour
{
    //public Transform

    protected List<NeuralNetwork> untestedNetworks = new List<NeuralNetwork>();
    protected List<NeuralNetwork> testedNetworks = new List<NeuralNetwork>();

    public int initialPopulation = 100;

    public int[] layerCounts;

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
    public Text timeScaleText;

    System.DateTime howLong;

    //write average to file
    public bool logAverages;
    StreamWriter writer;

    void Start()
    {
       
    }

    void OnDestroy()
    {
        writer.Close();
    }

    public void startPressed()
    {
        untestedNetworks.Clear();
        testedNetworks.Clear();

        writer = new StreamWriter("averages.txt", true);
        //11 30 3
        for (int x = 0; x < initialPopulation; x++)
        {
            untestedNetworks.Add(new NeuralNetwork(layerCounts));
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

        networkTested.setFitness(fitness);

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
        testedNetworks = testedNetworks.OrderByDescending(x => x.getFitness()).ToList();

        //calculate average,best,worst fitness of this generation
        lastAverageFitenss = 0;
        lastBestFitenss = -999999;
        lastWorstFitness = 999999;
        foreach(NeuralNetwork n in testedNetworks)
        {
            float networkFitness = n.getFitness();
            if(networkFitness > lastBestFitenss)//best fitness
            {
                lastBestFitenss = networkFitness;
            }
            else if(networkFitness < lastWorstFitness)//worst
            {
                lastWorstFitness = networkFitness;
            }

            lastAverageFitenss += networkFitness;//average
           // Debug.Log(n.id + " : " + n.fitness);
        }

        lastAverageFitenss /= testedNetworks.Count;

        //get how many networks are crossoverRate% of the total population
        int totalEliteChromosomeCount = Mathf.RoundToInt((float)testedNetworks.Count * crossoverRate);

        //create a new list with only our top X % of networks
        List<NeuralNetwork> topNetworks = new List<NeuralNetwork>();
        for (int x = 0; x < totalEliteChromosomeCount; x++)
        {
            topNetworks.Add(testedNetworks[x]);
            //also wipe the networks fitness memory
            testedNetworks[x].resetFitness();
        }

        //choose 2 random networks from our top networks and breed them
        List<NeuralNetwork> newChildren = new List<NeuralNetwork>();
        //keep the top10Percent from last generation
        newChildren.AddRange(topNetworks);
        //divide by 2 as each breeding generates 2 children
        int amountOfBreedingNeeded = (testedNetworks.Count - totalEliteChromosomeCount) / 2;
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

        

        resetForNextTest();

        untestedNetworks = newChildren;

        if (logAverages)
        {
            writer.Write(lastAverageFitenss + ",");
            writer.Flush();
        }

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
    /// Used for when we have mutliple rounds of judging before a breed, mutate, etc
    /// </summary>
    /// <param name="networks"></param>
    protected virtual void resetSoft(List<NeuralNetwork> networks)
    {
        untestedNetworks = networks;

        testedNetworks = new List<NeuralNetwork>();
    }

    /// <summary>
    /// Takes in 2 nerual networks and returns 2 children that have been crossedOver
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    protected virtual List<NeuralNetwork> breedCrossoverAndMutateNetworks(NeuralNetwork a, NeuralNetwork b)
    {
        
        Layer[] aLayers = a.getLayers();
        Layer[] bLayers = b.getLayers();

        //need to store each crosssed over and mutated layer to make a final network at the end
        Layer[] aFinalLayers = new Layer[aLayers.Length];
        Layer[] bFinalLayers = new Layer[bLayers.Length];

        for (int x = 0; x < aLayers.Length; x++)
        {
            //copy each layer as we dont want to mess with the original parents since they will be tested in the next trial
            Layer aNewLayer = aLayers[x].copy();
            Layer bNewLayer = bLayers[x].copy();

            //crossover our layers
            crossOverLayers(aNewLayer, bNewLayer);

            //mutate layers
            mutateLayer(aNewLayer);
            mutateLayer(bNewLayer);

            //add these finalized layers to our final array
            aFinalLayers[x] = aNewLayer;
            bFinalLayers[x] = bNewLayer;
        }

        //actually create the offspring
        NeuralNetwork aChild = new NeuralNetwork(aFinalLayers);
        NeuralNetwork bChild = new NeuralNetwork(bFinalLayers);

        return new List<NeuralNetwork>() { aChild, bChild };

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
        Time.timeScale = input;
        this.timeScaleText.text = input + "x real speed";
    }
}
