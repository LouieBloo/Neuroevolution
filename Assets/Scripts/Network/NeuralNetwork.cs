using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeuralNetwork  {

    Layer hiddenLayer;
    Layer outputLayer;

    public float fitness;

    public int id = Random.Range(0, 99999);

    public NeuralNetwork(int inputLayerCount, int hiddenLayerCount, int outputLayerCount)
    {
        createNetwork(inputLayerCount, hiddenLayerCount, outputLayerCount);
    }

    public NeuralNetwork(Layer hiddenLayer,Layer outputLayer)
    {
        this.hiddenLayer = hiddenLayer;
        this.outputLayer = outputLayer;
    }

    public void createNetwork(int inputLayerCount,int hiddenLayerCount,int outputLayerCount)
    {
        hiddenLayer = new Layer(hiddenLayerCount,inputLayerCount);
        outputLayer = new Layer(outputLayerCount,hiddenLayerCount);
    }

    

    public List<float> feedInputs(List<float> inputs)
    {
        if(inputs == null) { Debug.Log("Null inputs to NN"); return null; }
        if (inputs.Count != hiddenLayer.dendriteCount) {
            Debug.Log("==Mismatched input length==\nFOUND: " + inputs.Count + " NEEDED: " + hiddenLayer.nodeCount);
            return null;
        }

        outputLayer.calculateOutput(hiddenLayer.calculateOutput(inputs));

        //hiddenLayer.printOutput();
        //outputLayer.printOutput();

        return outputLayer.lastOutput;
    }

    public void debugInitialization()
    {
        Debug.Log("Hidden:");
        hiddenLayer.printLayer();

        Debug.Log("Output:");
        outputLayer.printLayer();
    }


    
    public Layer getHiddenLayer()
    {
        return hiddenLayer;
    }

    public Layer getOutputLayer()
    {
        return outputLayer;
    }
}
