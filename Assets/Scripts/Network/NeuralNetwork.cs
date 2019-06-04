using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeuralNetwork  {

    Layer[] layers;

    float fitness;
    List<float> fitnessHistory = new List<float>();

    public int id = Random.Range(0, 99999);

    public NeuralNetwork(int[] layerCounts)
    {
        this.createNetwork(layerCounts);
    }

    public NeuralNetwork(Layer[] newLayers)
    {
        this.layers = newLayers;
    }

    private void createNetwork(int[] layerCounts)
    {
        if(layerCounts.Length < 1) { Debug.Log("No layers found in createNetwork"); }

        //Dont create the input layer so use count-1
        this.layers = new Layer[layerCounts.Length-1];
        for(int x = 0; x < this.layers.Length; x++)
        {
            this.layers[x] = new Layer(layerCounts[x + 1], layerCounts[x]);
        }
    }

    public void setFitness(float input)
    {
        fitnessHistory.Add(input);
        this.fitness = input;
    }

    public float getFitness()
    {
        float final = 0;
        foreach(float f in fitnessHistory)
        {
            final += f;
        }
        return final;
    }

    public void resetFitness()
    {
        this.fitnessHistory.Clear();
        this.fitness = 0;
    }

    public List<float> feedInputs(List<float> inputs)
    {
        if(inputs == null) { Debug.Log("Null inputs to NN"); return null; }
        if (inputs.Count != this.layers[0].dendriteCount) {
            Debug.Log("==Mismatched input length==\nFOUND: " + inputs.Count + " NEEDED: " + this.layers[0].dendriteCount);
            return null;
        }

        List<float> previousLayerOutput = inputs;
        for (int x = 0; x < this.layers.Length; x++)
        {
            previousLayerOutput = this.layers[x].calculateOutput(previousLayerOutput);
        }

        return this.layers[this.layers.Length-1].lastOutput;
    }

    public void debugInitialization()
    {
        Debug.Log("Hidden:");
        //hiddenLayer.printLayer();

        Debug.Log("Output:");
        //outputLayer.printLayer();
    }

    public Layer[] getLayers()
    {
        return this.layers;
    }

}
